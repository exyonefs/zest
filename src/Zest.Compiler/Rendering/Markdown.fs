namespace Zest.Compiler.Rendering
open System
open System.Text.RegularExpressions
open Markdig

// ============================================================
// Markdown → HTML
// ============================================================
// Parsing is delegated to Markdig — CommonMark plus AdvancedExtensions:
// pipe tables, task lists, footnotes, auto-identifiers (the heading anchors
// templates link to), autolinks, definition lists, figures, custom
// containers, emphasis extras, and raw inline HTML. Fenced code keeps its
// `language-*` class, which is the hand-off point for a client-side
// highlighter (highlight.js / Prism) that _finalize.fsx can inject.
//
// Markdig emits author-provided URLs verbatim, so the finished HTML still
// passes through the scheme denylist below: a pasted snippet must not
// smuggle an executable URL into the output.
//
// `toHtml` is the single entry point every caller already uses (the `md` /
// `mdDedent` DSL helpers, PageBuilder, Authoring, shortcodes, the page
// pipeline), so upgrading the parser upgrades all of them at once.
// ============================================================

module Markdown =

    /// One pipeline for the whole process. Markdig pipelines are immutable
    /// once built and safe to share across threads.
    let private pipeline =
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build()

    // Schemes that execute script (or otherwise escape the document origin)
    // when a link/image is activated. Case- and whitespace-insensitive so
    // `JaVaScRiPt:` and ` javascript:` are both caught.
    let private dangerousHrefPat =
        Regex(@"(?<attr>\bhref|\baction|\bformaction)\s*=\s*""\s*(?:(?:javascript|vbscript|data|file|blob)\s*:[^""]*)""",
              RegexOptions.IgnoreCase)
    let private dangerousSrcPat =
        Regex(@"(?<attr>\bsrc)\s*=\s*""\s*(?:(?:javascript|vbscript|file)\s*:[^""]*)""",
              RegexOptions.IgnoreCase)

    let private sanitizeUrls (html: string) =
        let afterHref =
            dangerousHrefPat.Replace(html, fun m -> sprintf "%s=\"#\"" m.Groups.["attr"].Value)
        dangerousSrcPat.Replace(afterHref, fun m -> sprintf "%s=\"\"" m.Groups.["attr"].Value)

    /// Render Markdown to HTML. An empty (or null) source renders to "".
    let toHtml (markdown: string) : string =
        if String.IsNullOrEmpty markdown then ""
        else
            Markdig.Markdown.ToHtml(markdown, pipeline) |> sanitizeUrls
