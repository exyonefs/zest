+++
title = "Static by design"
layout = "post"
date = 2026-06-30
description = "What a static site promises, what the build outputs, and what this one deliberately leaves out."
tags = ["static", "engineering"]
categories = ["engineering"]
+++

A static site is a promise: the bytes on disk are the bytes the reader gets. No server renders the page, no database is queried, no script reflows the content after it arrives. This post is about what that promise buys, and what it costs.

## The whole site is a folder

`zest build` writes everything into `_site/`. That folder is the deployable artifact:

```text
_site/
├── index.html                  the home listing, first window
├── page/2/index.html           the second window
├── about/index.html
├── archive/index.html
├── tags/index.html             one page per tag below it
├── categories/index.html      one page per category below it
├── posts/
│   ├── 2026-02-14-why-static/index.html
│   ├── 2026-03-08-the-shape-of-a-url/index.html
│   └── …
├── 404.html
├── assets/css/style.css
├── feed.xml
├── atom.xml
├── sitemap.xml
└── robots.txt
```

Copy that folder to a host and the site is live. There is no runtime to keep alive, no dependency to audit, and nothing to migrate when the host changes.

## What the page costs to load

One HTML document, one stylesheet, and system fonts. No JavaScript executes, no request leaves for a third party, and no cookie is set. On a slow connection the page is readable almost immediately, and it prints cleanly.

## What is not here, on purpose

- **Comments.** A comments system needs a backend or a third party. An email link needs neither.
- **Search.** The browser's find is instant and private; a static JSON index can be added later if the archive grows.
- **Dark mode.** The browser's reader mode already does this well.
- **Analytics.** The server log is the honest version, and it does not follow the reader home.

Everything on that list is a decision that can be revisited. None of them is load-bearing for a site that is read.

## Backups

The sources are plain text, so Git is a complete backup: `git commit` after every post, and the history is the archive. The build is reproducible from the sources alone, which means the deployable artifact never has to be backed up — only written.
