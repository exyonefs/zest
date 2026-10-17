// _finalize.fsx — Zest post-build script.
//
// Runs once per build, after every page and asset has been written to the
// output directory. It must sit at the project root under exactly this name;
// it is never discovered in subdirectories, never routed, and never published.
//
// Its job is the finished output: inspect it, index it, reshape it. It cannot
// feed values back into templates (that is _prebuild.fsx, which runs before
// anything is rendered), and it may only write inside the output directory, so
// a mistake here can never reach the sources.
//
// Available helpers include formatHtml, minifyHtml, formatCss, minifyCss,
// formatJs, minifyJs, md, imageDefaults, processImages, rewriteFiles,
// writeFile, readFile, exists, listFiles, sizeOf, toJson and setFailOnError.

// A layout is F#, so it composes markup as strings: nesting is carried by the
// tags themselves, not by the indentation around them, and the written HTML
// comes out as one long line per element. That is fine for a browser and bleak
// for anyone reading `_site/` or reviewing a diff — which is why the last thing
// this build does is pretty-print the HTML it just wrote.
//
// `rewriteFiles` walks the whole output directory rather than a list of pages,
// so anything added later is covered without naming it. It rewrites only files
// whose text actually changed, so a rebuild of an unchanged site leaves every
// timestamp — and every incremental cache — alone.
rewriteFiles ".html" formatHtml

// More examples — uncomment what you need.
//
// Compress instead of pretty-print, for a deployed copy:
//
//   rewriteFiles ".html" minifyHtml
//
// Fail the build when a page links to something that was never written:
//
//   let routes = pages |> List.map (fun p -> p.route) |> Set.ofList
//   for page in pages do
//       let html = readFile page.output
//       for m in Regex.Matches(html, "href=\"(/[^\"]*)\"") do
//           let href = m.Groups.[1].Value
//           if not (routes.Contains href) && not (exists ("_site" + href)) then
//               eprintfn "  dead link on %s: %s" page.route href
//
// Emit a build manifest next to the site:
//
//   writeFile "build.json" (toJson {| pages = build.page_count; bytes = build.output_bytes |})
