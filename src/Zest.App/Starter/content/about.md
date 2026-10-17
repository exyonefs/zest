+++
title = "About"
layout = "default"
description = "What this site is, who writes it, and how to reach me."
permalink = "/about.html"
+++

This is a minimal static blog built with [Zest](https://github.com/zest-ssg/zest): one HTML file per page, one stylesheet, and no JavaScript, tracking or external requests of any kind. What you read is the whole site.

## Who writes here

Replace this paragraph with a short biography. The name in the footer and the title in the header both come from `[site]` in `_config.toml`, so you edit them once.

## How to reach me

Email is the only channel, and it is enough: [hello@example.com](mailto:hello@example.com). There is no comment section, no account and no tracking pixel — if something here helped, or is wrong, write to me and I will answer.

## How this site is built

Posts are Markdown files under `content/posts/`. The build reads them and writes plain HTML into `_site/`:

```text
content/posts/2026-08-02-welcome-to-zest.md  →  _site/posts/2026-08-02-welcome-to-zest.html
```

The home listing, the archive, the feeds and the sitemap are generated from the front matter, so a new post needs nothing but its own file. Publish by copying `_site/` to any static host, or by committing and pushing to one that builds from Git.
