+++
title = "Writing in Markdown"
layout = "post"
date = 2026-07-21
description = "Headings, tables, code, quotations and lists — the palette of a post body."
tags = ["markdown", "writing"]
categories = ["writing"]
+++

The body of a post is ordinary Markdown. This page is a reference for what the renderer understands, so the pieces can be copied instead of guessed.

## Headings

`##` and `###` nest as expected, and the stylesheet keeps them in the same measure as the body text. Never type a number yourself — headings are structure, not sequence.

### A third-level heading

Long arguments usually live here.

## Emphasis and code

| Written as | Rendered as |
| --- | --- |
| `*text*` | italic |
| `**text**` | **bold** |
| `` `code` `` | monospace on a quiet fill |
| `[label](url)` | a link |

Inline code such as `zest build` sits on the same fill, so it never competes with the body text. Fenced blocks keep their whitespace and scroll sideways on a narrow screen:

```fsharp
let posts =
    sitePages ()
    |> Array.filter (fun p -> p.date <> "")
    |> Array.sortByDescending (fun p -> p.date)
```

## Quotations and lists

A quotation is set apart with a rule on the left:

> Everything should be made as simple as possible, but not simpler.

Ordered lists are for genuine sequence:

1. create a file under `content/posts/`
2. give it a `title`, a `date`, some `tags` and a `category`
3. run `zest build`

And unordered lists for the remainder:

- front matter is TOML between `+++` fences
- layouts are `.zest.fsx`, `.html` or `.ztk`
- styles are ZCSS, compiled to plain CSS

## Raw HTML

Anything Markdown does not cover can be written as HTML and is passed through untouched — that is how this site's own pages stay valid without a single shortcode.
