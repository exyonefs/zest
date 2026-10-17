# Zest Starter

The site produced by `zest init`.

A flat, minimal blog: every page is one static HTML file, every URL is that
file's path, and nothing runs in the browser. No JavaScript, no webfonts, no
analytics, no third-party requests.

## Layout

```
.
├── _config.toml              # optional: site metadata
├── _prebuild.fsx             # optional: pre-build script (global data)
├── _finalize.fsx             # optional: post-build script (shape output)
├── _layouts/                 # layouts: .zest.fsx (F#)
│   ├── shell.zest.fsx        # the document: <html>, <head>, header, footer
│   ├── default.zest.fsx      # an ordinary page; the engine's fallback layout
│   └── post.zest.fsx         # a dated entry: title, date, body, back link
├── assets/css/style.zcss     # the stylesheet the pages link, in one file
└── content/                  # pages (.md, .zest.fsx)
    ├── index.zest.fsx        # the home listing
    ├── about.md              # → /about.html
    ├── archive.zest.fsx      # → /archive.html, every post by year
    ├── 404.md                # → /404.html
    ├── feed.zest.fsx         # → /feed.xml (RSS 2.0)
    ├── atom.zest.fsx         # → /atom.xml (Atom 1.0)
    ├── robots.zest.fsx       # → /robots.txt
    ├── sitemap.zest.fsx      # → /sitemap.xml
    └── posts/                # one Markdown file per post
```

`_config.toml`, `_prebuild.fsx` and `_finalize.fsx` are the only three special
files Zest recognises, and all live at the project root. Everything else is
convention: delete `_config.toml` and the site still builds.

## The built site

`zest build` writes the flat site into `_site/`:

```text
_site/
├── index.html
├── about.html
├── archive.html
├── 404.html
├── assets/css/style.css
├── feed.xml
├── atom.xml
├── sitemap.xml
├── robots.txt
└── posts/
    └── 2026-08-02-welcome-to-zest.html
```

Every URL is a file path: the home page is `/`, the about page is
`/about.html`, and a post is `/posts/<file-name>.html`. A `permalink` in the
front matter sets that path explicitly.

The stylesheet is ZCSS — a CSS superset with `let` bindings — compiled to
plain CSS at build time. One file in, one file out: the browser downloads
`style.css` and nothing else.

## Writing a post

Create `content/posts/2026-10-17-hello.md`:

```toml
+++
title = "Hello"
layout = "post"
date = 2026-10-17
description = "One line for the listing and the feeds."
permalink = "/posts/2026-10-17-hello.html"
+++

The body is Markdown. Raw HTML passes through untouched.
```

That is the whole workflow. The home listing, the archive, both feeds and the
sitemap are generated from the front matter at build time, so a new post never
means editing another file.

## Publishing

```bash
zest build          # static output into _site/
```

Copy `_site/` to any static host, or push the repository to one that builds
from source. Git is the backup: the sources are plain text, and the build is
reproducible from them alone.

## What is not here

Comments, search, tags, pagination, an admin screen, analytics, external
fonts — none of them ship, and none of them is needed to start. The design
post that ships with the template explains each omission.

## Workflow

```bash
zest init my-site     # scaffold this starter
zest init --empty     # scaffold only the directory layout
cd my-site
zest serve            # dev server with live reload
zest build            # static output into _site/
zest clean            # remove build output
```
