# Zest Starter

The site produced by `zest init`.

A complete personal blog: paginated listing, tags, categories, a year archive,
RSS and Atom feeds, a sitemap, a 404 page — and nothing that runs in the
browser. No JavaScript, no webfonts, no analytics, no third-party requests.

## Layers

Three directories, one reference direction:

```
content/  →  _layouts/  →  _includes/
```

- **`_includes/`** — leaf components (`header`, `footer`, `post-list`,
  `pager`). A component may read the site/page context, never a layout.
- **`_layouts/`** — page frames. A frame may `{% include %}` components and
  `{% extends %}` another frame; components never point back at frames.
- **`content/`** — pages. A page chooses a layout and renders its body only;
  it never reaches into either directory.

```
.
├── _config.toml              # site metadata + [pagination] per_page
├── _prebuild.fsx             # optional: pre-build script (global data)
├── _finalize.fsx             # optional: post-build script (shape output)
├── _includes/                # components (Zestucks .html), used by layouts
│   ├── header.html           #   brand + navigation
│   ├── footer.html           #   copyright + feed link
│   ├── post-list.html        #   listing entries, driven by `posts`
│   └── pager.html            #   newer / page N of M / older
├── _layouts/                 # page frames (Zestucks .html)
│   ├── shell.html            #   the document: head, header, main, footer
│   ├── default.html          #   an ordinary page; the engine's fallback
│   ├── post.html             #   a dated entry: date, tags, categories
│   ├── tag.html              #   /tags/<term>/      (taxonomy generator)
│   ├── tags.html             #   /tags/             (taxonomy generator)
│   ├── category.html         #   /categories/<term>/
│   └── categories.html       #   /categories/
├── assets/css/style.zcss     # the stylesheet the pages link, in one file
└── content/                  # pages (.md, .zest.fsx)
    ├── index.zest.fsx        # the home listing, five posts per window
    ├── about.md              # → /about/
    ├── archive.zest.fsx      # → /archive/, every post by year
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

`index.zest.fsx` and `archive.zest.fsx` stay in F# because they compute
(pagination windows, grouping by year); the template layer is Zestucks, so the
listing markup they emit reuses the same class names — and therefore the same
styles — as `_includes/post-list.html`.

## The built site

`zest build` writes the flat site into `_site/`:

```text
_site/
├── index.html                  the home listing, first window
├── page/2/index.html           the second window
├── about/index.html
├── archive/index.html
├── tags/index.html             + one page per tag
├── categories/index.html       + one page per category
├── posts/<slug>/index.html     one directory per post
├── 404.html
├── assets/css/style.css
├── feed.xml
├── atom.xml
├── sitemap.xml
└── robots.txt
```

Every URL is a directory with an `index.html`, so the path on disk is the path
in the browser. Only the pages whose URLs the format itself fixes declare a
`permalink`: `/404.html` for the not-found page, and the feed, sitemap and
robots files.

The stylesheet is ZCSS — a CSS superset with `let` bindings — compiled to
plain CSS at build time. One file in, one file out: the browser downloads
`style.css` and nothing else.

## Style conventions

Class names are BEM: a block (`header`, `entry`, `post-list`, `pager`,
`term-list`, `archive`), an element (`block__part`), and a state modifier
(`block__part--disabled`). Every class is emitted by exactly one place —
a component, a layout, or a content page — so a rename has a single home.

Tokens are named for purpose, not appearance: `color-bg`, `color-text`,
`color-muted`, `color-accent`, `color-surface`, `color-border`, `font-body`,
`font-code`, `width-content`.

## Writing a post

Create `content/posts/2026-10-17-hello.md`:

```toml
+++
title = "Hello"
layout = "post"
date = 2026-10-17
description = "One line for the listing and the feeds."
tags = ["hello", "writing"]
categories = ["writing"]
+++

The body is Markdown. Raw HTML passes through untouched.
```

That is the whole workflow. The home listing, the pager, the archive, the tag
and category pages, both feeds and the sitemap are generated from the front
matter at build time, so a new post never means editing another file.

## Publishing

```bash
zest build          # static output into _site/
```

Copy `_site/` to any static host, or push the repository to one that builds
from source. Git is the backup: the sources are plain text, and the build is
reproducible from them alone.

## What is not here

Comments, search, an admin screen, analytics, external fonts — none of them
ship, and none of them is needed to start. Each can be added later without
changing how the site is built.

## Workflow

```bash
zest init my-site     # scaffold this starter
zest init --empty     # scaffold only the directory layout
cd my-site
zest serve            # dev server with live reload
zest build            # static output into _site/
zest clean            # remove build output
```
