+++
title = "The shape of a URL"
layout = "post"
date = 2026-03-08
description = "Why every page here is a directory with an index, and what that buys in stability and simplicity."
tags = ["urls", "writing"]
categories = ["design"]
+++

A URL is a promise: if someone links to it, saves it, or prints it, it should still work years later. Everything about how this site lays out its files follows from that one sentence.

## One file per page

The content directory mirrors the site. `content/posts/2026-08-02-welcome-to-zest.md` becomes `/posts/2026-08-02-welcome-to-zest/`, `content/about.md` becomes `/about/`, and the archive lives at `/archive/`. There is no router table to keep in sync with the filesystem; the path on disk is the path in the browser.

The trailing slash means the URL names a directory, and the server hands back its `index.html`. This is the oldest trick in static hosting, and it is still the most robust: no extension to guess, no rewrite rule to debug, and moving the whole site to a different host changes nothing.

## Stable, readable, collapsible

Detail pages keep their whole path even when the design changes. A post written in February keeps its URL when the stylesheet is replaced in June. The only thing that would break a link is renaming the file, and that is a deliberate act with a visible consequence.

URLs also collapse well. `/posts/` is the group, and `/posts/2026-08-02-welcome-to-zest/` is the entry inside it. Feeds and archives can be expressed as filters over the same tree instead of as a second, parallel naming scheme.

## When to break the pattern

Two pages genuinely need a non-directory path: the 404 page, which static hosts require at `/404.html`, and the machine-readable files, which must be exactly `/feed.xml`, `/atom.xml`, `/sitemap.xml` and `/robots.txt`. Those are the exceptions the format itself dictates, so each declares its `permalink` explicitly and nothing else does.
