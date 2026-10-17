// Media/ImageProcessor.fs
//
// The built-in image processor: compress, resize, convert and thumbnail the
// images a site references, then rewrite every reference so the site keeps
// working. It is the engine behind the `processImages` entry point that
// _finalize.fsx exposes.
//
// It runs after the site is written, when every image and every reference to
// it exists. A run does four things, in order:
//   1. discover the work — every processable image already in the output
//      tree, plus every processable image in the source tree (a picture
//      sitting next to its Markdown page, which the build itself never
//      copies),
//   2. downscale, re-encode and variant each one into OutputDir (or in
//      place), deleting the output copy of the source when ReplaceSource is
//      set — source-tree files are never deleted,
//   3. rewrite <img src>, og:image / twitter:image and CSS url() references
//      to the processed files: local paths, or BaseUrl-prefixed links when
//      an image host is configured,
//   4. report what happened, including per-image failures.
//
// Dependencies: SixLabors.ImageSharp

namespace Zest.Compiler.Media
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Net
open System.Text.RegularExpressions
open System.Threading.Tasks
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Formats
open SixLabors.ImageSharp.Formats.Gif
open SixLabors.ImageSharp.Formats.Jpeg
open SixLabors.ImageSharp.Formats.Png
open SixLabors.ImageSharp.Formats.Webp

/// Target format for processed images. `KeepSource` re-encodes in each source
/// file's own format, changing nothing but the compression.
[<RequireQualifiedAccess>]
type ImageOutputFormat =
    | KeepSource
    | WebP
    | Jpeg
    | Png

/// One extra rendering of an image at a fixed width, e.g. a 480px thumbnail
/// written beside the main file as `name-thumb.webp`.
type ImageVariant = {
    /// Width in pixels. Wider sources are downscaled to it; narrower ones are
    /// left at their own width.
    Width: int
    /// Inserted before the extension, e.g. "-thumb".
    Suffix: string
}

/// Everything the processor needs — one record, one entry point.
type ImageOptions = {
    /// Directory the processor reads from and writes under: the build output.
    RootDir: string
    /// Source tree the processor also scans: normally the content directory,
    /// so an image sitting next to its Markdown page is pulled into the
    /// output even though the build itself never copies it. Empty disables
    /// the scan: only images already in the output are processed.
    SourceDir: string
    /// Sub-directory of RootDir that collects the processed images, preserving
    /// each source's path relative to its base directory. Empty means
    /// "in place".
    OutputDir: string
    /// Encoder quality, 1–100. Applies to WebP and JPEG; clamped on entry.
    Quality: int
    /// Sources wider than this are downscaled to it (never upscaled).
    /// Zero or less disables resizing.
    MaxWidth: int
    /// Extra widths rendered beside the main image.
    Variants: ImageVariant list
    /// Output format for the main image and every variant.
    Format: ImageOutputFormat
    /// Image-host / CDN base URL. When set, rewritten references point at
    /// `<BaseUrl>/<path>` instead of a local file — the "图床 link" mode.
    BaseUrl: string
    /// Extensions processed (lowercase, leading dot optional). SVG is always
    /// skipped — ImageSharp cannot decode it — and an animated GIF loses its
    /// animation when re-encoded, so `.gif` stays out of the default set.
    Extensions: string list
    /// Delete the output copy of a source file after a successful conversion.
    /// Off keeps both. Files in the source tree are never deleted.
    ReplaceSource: bool
}

/// What one run did.
type ImageReport = {
    /// Images re-encoded this run (not counting skipped ones).
    ImagesProcessed: int
    /// Variant files written.
    VariantsWritten: int
    /// Images left untouched because the target was already up to date.
    ImagesSkipped: int
    /// Total size of the sources that were processed.
    BytesBefore: int64
    /// Total size of the files written in their place.
    BytesAfter: int64
    /// Written files, site-relative with forward slashes.
    Files: string list
    /// Per-image failures. The run keeps going past them; a hook that cares
    /// inspects this list and decides whether to fail the build.
    Errors: string list
}

module ImageProcessor =

    /// ImageSharp cannot decode SVG — it is vector, not raster — so it is
    /// never processed even when an author lists the extension.
    let private skippableExtensions = set [ ".svg" ]

    /// URL schemes that are external references, never local files.
    let private externalSchemes =
        set [ "http"; "https"; "data"; "blob"; "mailto"; "tel"; "ftp" ]

    // ── Reference patterns ────────────────────────────────────────────
    // Scoped so only the intended references are rewritten: a bare `src=`
    // would also hit <script src> and <audio src>. The `q`/`url` groups
    // carry the raw URL; the quote style is matched by backreference.
    let private imgTagPat =
        Regex("""<img\b[^>]*?\bsrc\s*=\s*(?<q>["'])(?<url>[^"']*)\k<q>[^>]*>""", RegexOptions.IgnoreCase)
    let private ogImagePat =
        Regex("""<meta\b[^>]*?\b(?:property|name)\s*=\s*(?<q>["'])(?:og:image|twitter:image)(?::url)?\k<q>[^>]*?\bcontent\s*=\s*(?<q>["'])(?<url>[^"']*)\k<q>[^>]*>""",
              RegexOptions.IgnoreCase)
    let private cssUrlPat =
        Regex("""url\(\s*(?<q>["']?)(?<url>[^"')]*)\k<q>\s*\)""", RegexOptions.IgnoreCase)

    // ── Path helpers ──────────────────────────────────────────────────

    let private toSlashes (path: string) = path.Replace('\\', '/')

    /// True when `path` sits strictly inside `dir`.
    let private isWithin (dir: string) (path: string) =
        (Path.GetFullPath path).StartsWith(
            (Path.GetFullPath dir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + string Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)

    /// Percent-encode each path segment, leaving the separators intact.
    let private encodeUrlPath (relPath: string) =
        relPath.Split('/') |> Array.map Uri.EscapeDataString |> String.concat "/"

    let private withSuffix (path: string) (suffix: string) =
        if String.IsNullOrEmpty suffix then path
        else
            let dir = Path.GetDirectoryName path
            let name = Path.GetFileNameWithoutExtension path
            let ext = Path.GetExtension path
            let combined = name + suffix + ext
            if String.IsNullOrEmpty dir then combined else Path.Combine(dir, combined)

    /// `withSuffix` for a site-relative path.
    let private withSuffixRel (relPath: string) (suffix: string) =
        if String.IsNullOrEmpty suffix then relPath
        else
            let ext = Path.GetExtension relPath
            relPath.Substring(0, relPath.Length - ext.Length) + suffix + ext

    let private extensionFor (format: ImageOutputFormat) (sourceExt: string) : string =
        match format with
        | ImageOutputFormat.KeepSource -> sourceExt
        | ImageOutputFormat.WebP -> ".webp"
        | ImageOutputFormat.Jpeg -> ".jpg"
        | ImageOutputFormat.Png -> ".png"

    /// The encoder for a target extension, or None when the extension has no
    /// encoder (an author-listed format the library cannot write).
    let private encoderFor (targetExt: string) (quality: int) : IImageEncoder option =
        match targetExt with
        | ".webp" -> Some (WebpEncoder(Quality = quality) :> IImageEncoder)
        | ".jpg" | ".jpeg" -> Some (JpegEncoder(Quality = quality) :> IImageEncoder)
        | ".png" -> Some (PngEncoder() :> IImageEncoder)
        | ".gif" -> Some (GifEncoder() :> IImageEncoder)
        | _ -> None

    // ── Discovery ─────────────────────────────────────────────────────

    /// Collect processable images under `root`, skipping reparse points and
    /// every directory `skipDir` rejects. Unreadable directories are skipped
    /// rather than aborting the run — one locked folder must not cost the
    /// site its image pipeline.
    let private collectImages (root: string) (skipDir: string -> bool) (extensions: Set<string>) : string list =
        let results = ResizeArray<string>()
        let rec walk (dir: string) =
            try
                for f in Directory.EnumerateFiles dir do
                    let info = FileInfo f
                    if not (info.Attributes.HasFlag FileAttributes.ReparsePoint) then
                        let ext = info.Extension.ToLowerInvariant()
                        if extensions.Contains ext && not (skippableExtensions.Contains ext) then
                            results.Add f
            with _ -> ()
            try
                for d in Directory.EnumerateDirectories dir do
                    let info = DirectoryInfo d
                    if not (info.Attributes.HasFlag FileAttributes.ReparsePoint) && not (skipDir d) then
                        walk d
            with _ -> ()
        if Directory.Exists root then walk root
        List.ofSeq results

    /// Every processable image already in the output tree, skipping the
    /// output directory itself (those are our own products).
    let private enumerateImages (rootFull: string) (outputRel: string) (extensions: Set<string>) : string list =
        let outputFull =
            if String.IsNullOrEmpty outputRel then "" else Path.GetFullPath(Path.Combine(rootFull, outputRel))
        collectImages rootFull (fun dir ->
            outputFull <> "" &&
            (Path.GetFullPath dir).StartsWith(outputFull + string Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            extensions

    /// Every processable image in the source tree.
    ///
    /// A full scan, not a reference-driven one: once a reference has been
    /// rewritten to the processed file, the original name no longer appears
    /// in any page, so a reference-driven scan would stop finding the source
    /// — and an edit to it would never reach the output again. Scanning
    /// everything also means an image that only a layout or a stylesheet
    /// references is still collected.
    ///
    /// The exclusions mirror the content pipeline: dot- and underscore-
    /// prefixed directories (`_layouts`, `_data`, `.git`) are conventions,
    /// not content, and the output directory is never a source even when the
    /// content directory is the project root.
    let private enumerateSourceImages (sourceFull: string) (rootFull: string) (extensions: Set<string>) : string list =
        let skipDir (dir: string) =
            let name = Path.GetFileName dir
            name.StartsWith(".", StringComparison.Ordinal)
            || name.StartsWith("_", StringComparison.Ordinal)
            || (Path.GetFullPath dir).StartsWith(
                (Path.GetFullPath rootFull).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + string Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
        collectImages sourceFull skipDir extensions

    /// HTML and CSS files are where references live; anything else (feeds,
    /// JSON manifests, scripts) is left alone.
    let private isReferenceFile (file: string) =
        let ext = (Path.GetExtension file).ToLowerInvariant()
        ext = ".html" || ext = ".htm" || ext = ".css"

    let private enumerateReferenceFiles (rootFull: string) : string list =
        if not (Directory.Exists rootFull) then []
        else
            try
                Directory.EnumerateFiles(rootFull, "*", SearchOption.AllDirectories)
                |> Seq.filter isReferenceFile
                |> List.ofSeq
            with _ -> []

    // ── Reference resolution ──────────────────────────────────────────

    /// True when a URL points somewhere else entirely (another host, a data
    /// URI, a protocol-relative URL) rather than at this site. A single-letter
    /// "scheme" is a Windows drive letter, not a URL.
    let private isExternalUrl (url: string) : bool =
        if url.StartsWith "//" then true
        else
            let ok, uri = Uri.TryCreate(url, UriKind.Absolute)
            ok && uri.Scheme.Length > 1 && externalSchemes.Contains(uri.Scheme.ToLowerInvariant())

    /// The local files a reference URL could name, in priority order: the
    /// output tree first, then the source tree. A root-relative URL is tried
    /// against both roots; a relative one is tried against the directory of
    /// the file that carries it, then against the matching directory in the
    /// source tree — `_site/posts/a/index.html` referencing `img/cat.jpg`
    /// looks for `content/posts/a/img/cat.jpg`, because the output mirrors
    /// the content. External URLs name nothing local.
    let private referenceCandidates
        (rootFull: string)
        (sourceFull: string)
        (referencingFile: string)
        (rawUrl: string)
        : string list =
        let url = (WebUtility.HtmlDecode rawUrl).Trim()
        if url.Length = 0 || url.StartsWith "#" || isExternalUrl url then []
        else
            let pathOnly = (url.Split([| '?'; '#' |]).[0]).Trim()
            if pathOnly.Length = 0 then []
            elif pathOnly.StartsWith("/") || pathOnly.StartsWith("\\") then
                let trimmed = pathOnly.TrimStart('/', '\\')
                [ Path.Combine(rootFull, trimmed); Path.Combine(sourceFull, trimmed) ]
                |> List.map Path.GetFullPath
            else
                let mirroredDir =
                    if sourceFull = "" then ""
                    else Path.Combine(sourceFull, Path.GetRelativePath(rootFull, Path.GetDirectoryName referencingFile))
                [ Path.Combine(Path.GetDirectoryName referencingFile, pathOnly); Path.Combine(mirroredDir, pathOnly) ]
                |> List.map Path.GetFullPath

    /// Resolve a reference to a file this run processed. Deliberately does
    /// not touch the file system: `ReplaceSource` deletes the output copy
    /// after a successful conversion, and a reference to it must still be
    /// rewritten.
    let private resolveProcessed
        (rootFull: string)
        (sourceFull: string)
        (referencingFile: string)
        (map: ConcurrentDictionary<string, string>)
        (rawUrl: string)
        : string option =
        referenceCandidates rootFull sourceFull referencingFile rawUrl
        |> List.tryPick (fun candidate ->
            match map.TryGetValue candidate with
            | true, targetRel -> Some targetRel
            | _ -> None)

    /// Path equality that survives mixed separators: `Path.Combine` keeps the
    /// forward slashes of a site-relative path, so a raw string compare of
    /// `_site\assets/a.webp` against `_site/assets/a.webp` would call two
    /// spellings of one file different — and an in-place conversion would
    /// then delete the very file it just wrote.
    let private samePath (a: string) (b: string) =
        String.Equals(Path.GetFullPath a, Path.GetFullPath b, StringComparison.OrdinalIgnoreCase)

    // ── Processing ────────────────────────────────────────────────────

    /// Where a processed image lands: under OutputDir with its path relative
    /// to its base directory (the output tree, or the source tree for images
    /// pulled in from the content), or in place. A source already living
    /// under OutputDir is processed in place, so the prefix never nests into
    /// itself across runs.
    let private targetRelPath (options: ImageOptions) (rootFull: string) (sourceFull: string) (source: string) (targetExt: string) : string =
        let baseDir = if isWithin rootFull source then rootFull else sourceFull
        let relFromBase = toSlashes (Path.GetRelativePath(baseDir, source))
        let outputRel = (if isNull options.OutputDir then "" else options.OutputDir).Trim('/', '\\')
        let underOutput =
            outputRel <> "" &&
            (relFromBase.Equals(outputRel, StringComparison.OrdinalIgnoreCase)
             || relFromBase.StartsWith(outputRel + "/", StringComparison.OrdinalIgnoreCase))
        let baseRel =
            if outputRel = "" || underOutput then relFromBase
            else outputRel + "/" + relFromBase
        Path.ChangeExtension(baseRel, targetExt)

    /// Decode, downscale, re-encode and variant one image. Returns the
    /// site-relative target path plus the site-relative paths of the variants
    /// written beside it.
    let private processImage (options: ImageOptions) (rootFull: string) (sourceFull: string) (source: string) : (string * string list) =
        let sourceExt = (Path.GetExtension source).ToLowerInvariant()
        let targetExt = extensionFor options.Format sourceExt
        match encoderFor targetExt options.Quality with
        | None ->
            failwithf "no encoder for '%s'" targetExt
        | Some encoder ->
            let targetRel = targetRelPath options rootFull sourceFull source targetExt
            let targetFull = Path.GetFullPath(Path.Combine(rootFull, targetRel))
            use image = Image.Load(source)
            if options.MaxWidth > 0 && image.Width > options.MaxWidth then
                image.Mutate(fun ctx ->
                    ctx.Resize(ResizeOptions(Size = Size(options.MaxWidth, 0), Mode = ResizeMode.Max)) |> ignore)
            let targetDir = Path.GetDirectoryName targetFull
            if not (String.IsNullOrEmpty targetDir) then Directory.CreateDirectory targetDir |> ignore
            image.Save(targetFull, encoder)
            let variantRels = ResizeArray<string>()
            for v in options.Variants do
                if v.Width > 0 && v.Width < image.Width then
                    use variant =
                        image.Clone(fun ctx ->
                            ctx.Resize(ResizeOptions(Size = Size(v.Width, 0), Mode = ResizeMode.Max)) |> ignore)
                    variant.Save(withSuffix targetFull v.Suffix, encoder)
                    variantRels.Add (withSuffixRel targetRel v.Suffix)
            targetRel, List.ofSeq variantRels

    // ── Reference rewriting ───────────────────────────────────────────

    /// The URL a rewritten reference should carry. An image host prefixes
    /// everything; otherwise the reference keeps its original style
    /// (root-relative stays root-relative, a relative one stays relative to
    /// the file that holds it).
    let private referenceUrl
        (options: ImageOptions)
        (rootFull: string)
        (referencingFile: string)
        (targetRel: string)
        (originalUrl: string)
        : string =
        let encoded = encodeUrlPath targetRel
        if not (String.IsNullOrWhiteSpace options.BaseUrl) then
            options.BaseUrl.TrimEnd('/', '\\') + "/" + encoded
        elif originalUrl.TrimStart().StartsWith("/") then
            "/" + encoded
        else
            let dir = Path.GetDirectoryName referencingFile
            toSlashes (Path.GetRelativePath(dir, Path.Combine(rootFull, encoded)))

    /// Rebuild one match with its URL group swapped for `newUrl`.
    let private replaceMatchUrl (m: Match) (newUrl: string) : string =
        let value = m.Value
        let g = m.Groups.["url"]
        let start = g.Index - m.Index
        value.Substring(0, start) + newUrl + value.Substring(start + g.Length)

    let private rewriteReferences
        (options: ImageOptions)
        (rootFull: string)
        (sourceFull: string)
        (map: ConcurrentDictionary<string, string>)
        (file: string)
        : unit =
        let text = File.ReadAllText file
        let rewriteWith (pattern: Regex) (input: string) : string =
            pattern.Replace(input, fun m ->
                let raw = m.Groups.["url"].Value
                match resolveProcessed rootFull sourceFull file map raw with
                | Some targetRel -> replaceMatchUrl m (referenceUrl options rootFull file targetRel raw)
                | None -> m.Value)
        let updated =
            text
            |> rewriteWith imgTagPat
            |> rewriteWith ogImagePat
            |> rewriteWith cssUrlPat
        if updated <> text then File.WriteAllText(file, updated, Text.Encoding.UTF8)

    // ── Entry point ───────────────────────────────────────────────────

    /// Per-image accounting collected from the parallel workers and summed
    /// into the report afterwards, so no shared counters are needed.
    type private Outcome = {
        Variants: int
        BytesBefore: int64
        BytesAfter: int64
    }

    /// One unit of work: the file to write and every source that maps to it.
    /// Several sources can share a target — the output copy and the source
    /// copy of one image, or a pre-conversion file and its already-processed
    /// replacement — so exactly one of them writes while all of them keep
    /// their mapping and take part in the cleanup.
    type private Plan = {
        TargetRel: string
        /// The source that writes the target: the newest of the group, so an
        /// edited source image beats the stale output copy sharing its target.
        Writer: string
        Sources: string list
    }

    /// Process every image the site references and rewrite the references to
    /// them. One call is the whole feature; a missing RootDir is reported as
    /// a single error rather than thrown, so a hook can react to it.
    let processImages (options: ImageOptions) : ImageReport =
        let rootFull = Path.GetFullPath options.RootDir
        if not (Directory.Exists rootFull) then
            { ImagesProcessed = 0; VariantsWritten = 0; ImagesSkipped = 0
              BytesBefore = 0L; BytesAfter = 0L; Files = []
              Errors = [ sprintf "image root '%s' does not exist" rootFull ] }
        else
            let options = { options with Quality = min 100 (max 1 options.Quality) }
            let sourceFull =
                if String.IsNullOrWhiteSpace options.SourceDir then "" else Path.GetFullPath options.SourceDir
            let extensions =
                options.Extensions
                |> List.map (fun e -> (if e.StartsWith "." then e else "." + e).ToLowerInvariant())
                |> Set.ofList
            let outputRel = (if isNull options.OutputDir then "" else options.OutputDir).Trim('/', '\\')

            // ── Discovery ──
            // Images already in the output, plus the ones the built pages
            // reference that only exist in the source tree. Each is paired
            // with the site-relative path it will be written to; the pairing
            // is what lets several sources of one image converge.
            let referenceFiles = enumerateReferenceFiles rootFull
            let discovered = ResizeArray<string * string>()
            let seenSources = HashSet<string>(StringComparer.OrdinalIgnoreCase)
            let tryAdd (candidate: string) =
                let ext = (Path.GetExtension candidate).ToLowerInvariant()
                if extensions.Contains ext && not (skippableExtensions.Contains ext) && seenSources.Add candidate then
                    let targetRel = targetRelPath options rootFull sourceFull candidate (extensionFor options.Format ext)
                    discovered.Add(candidate, targetRel)
            for image in enumerateImages rootFull outputRel extensions do tryAdd image
            if sourceFull <> "" &&
               Directory.Exists sourceFull &&
               not (String.Equals(Path.GetFullPath sourceFull, rootFull, StringComparison.OrdinalIgnoreCase)) then
                for image in enumerateSourceImages sourceFull rootFull extensions do tryAdd image

            // Every source maps to its target from the start, so the rewrite
            // pass resolves whichever copy a reference names — including a
            // source that never wrote because another claimed the target.
            let map = ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            for (source, targetRel) in discovered do map.[source] <- targetRel

            // One plan per target. Grouping is what keeps two sources from
            // writing the same file concurrently, and picking the newest
            // source as the writer is what makes an edited source image win
            // over the stale output copy sharing its target.
            let plans =
                discovered
                |> List.ofSeq
                // Grouped case-insensitively: on a case-insensitive file
                // system, "A.webp" and "a.webp" are one file, and two plans
                // writing it would race.
                |> List.groupBy (fun (_, t) -> t.ToLowerInvariant())
                |> List.map (fun (_, sources) ->
                    let writer =
                        sources
                        |> List.maxBy (fun (s, _) -> File.GetLastWriteTimeUtc s)
                        |> fst
                    { TargetRel = sources |> List.head |> snd
                      Writer = writer
                      Sources = sources |> List.map fst })

            // ── Process ──
            let errors = ConcurrentBag<string>()
            let written = ConcurrentBag<string>()
            let outcomes = ConcurrentBag<Outcome>()
            let skippedPlans = ConcurrentBag<string>()

            Parallel.ForEach(plans, fun plan ->
                let targetFull = Path.GetFullPath(Path.Combine(rootFull, plan.TargetRel))
                // Only output-tree copies are replaced; deleting a source-tree
                // image would reach outside the output directory, which a
                // post-build hook must never do.
                let replaceCopies () =
                    if options.ReplaceSource then
                        for src in plan.Sources do
                            if isWithin rootFull src && not (samePath targetFull src) then
                                try File.Delete src with _ -> ()
                try
                    let sourceExt = (Path.GetExtension plan.Writer).ToLowerInvariant()
                    let targetExt = extensionFor options.Format sourceExt
                    if encoderFor targetExt options.Quality |> Option.isNone then
                        errors.Add (sprintf "%s: no encoder for '%s'" (Path.GetFileName plan.Writer) targetExt)
                    elif File.Exists targetFull && File.GetLastWriteTimeUtc targetFull >= File.GetLastWriteTimeUtc plan.Writer then
                        // Up to date already — but the pre-conversion copies
                        // still have to go.
                        replaceCopies ()
                        skippedPlans.Add plan.TargetRel
                    else
                        let rel, variantRels = processImage options rootFull sourceFull plan.Writer
                        written.Add rel
                        for vrel in variantRels do written.Add vrel
                        outcomes.Add { Variants = variantRels.Length
                                       BytesBefore = FileInfo(plan.Writer).Length
                                       BytesAfter = FileInfo(targetFull).Length }
                        replaceCopies ()
                with ex ->
                    errors.Add (sprintf "%s: %s" (Path.GetFileName plan.Writer) ex.Message))
            |> ignore

            // ── Rewrite references ──
            for file in referenceFiles do
                try rewriteReferences options rootFull sourceFull map file
                with ex -> errors.Add (sprintf "%s: %s" (Path.GetFileName file) ex.Message)

            { ImagesProcessed = outcomes.Count
              VariantsWritten = outcomes |> Seq.sumBy (fun o -> o.Variants)
              ImagesSkipped = skippedPlans.Count
              BytesBefore = outcomes |> Seq.sumBy (fun o -> o.BytesBefore)
              BytesAfter = outcomes |> Seq.sumBy (fun o -> o.BytesAfter)
              Files = written |> Seq.distinct |> Seq.sort |> List.ofSeq
              Errors = List.ofSeq errors }
