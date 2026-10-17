+++
title = "Static by design"
layout = "post"
date = 2026-06-30
description = "What a static site promises, what the build outputs, and what this one deliberately leaves out."
permalink = "/posts/2026-06-30-static-by-design.html"
+++

A static site is a promise: the bytes on disk are the bytes the reader gets. No server renders the page, no database is queried, no script reflows the content after it arrives. This post is about what that promise buys, and what it costs.

## The whole site is a folder

`zest build` writes everything into `_site/`. That folder is the deployable artifact:

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
    ├── 2026-06-30-static-by-design.html
    ├── 2026-07-21-writing-in-markdown.html
    └── 2026-08-02-welcome-to-zest.html
```

Copy that folder to a host and the site is live. There is no runtime to keep alive, no dependency to audit, and nothing to migrate when the host changes.

## What the page costs to load

One HTML document, one stylesheet, and system fonts. No JavaScript executes, no request leaves for a third party, and no cookie is set. On a slow connection the page is readable almost immediately, and it prints cleanly.

## What is not here, on purpose

- **Comments.** A comments system needs a backend or a third party. An email link needs neither.
- **Search.** The browser's find is instant and private; a search index can be added later as a static JSON file if the archive grows.
- **Tags and categories.** Directories and links carry the same information at this size.
- **Pagination.** The listing is one page until it is not; splitting it is a later change.
- **Dark mode.** The browser's reader mode already does this well.

Everything on that list is a decision that can be revisited. None of them is load-bearing for a site that is read.

## Backups

The sources are plain text, so Git is a complete backup: `git commit` after every post, and the history is the archive. The build is reproducible from the sources alone, which means the deployable artifact never has to be backed up — only written.
