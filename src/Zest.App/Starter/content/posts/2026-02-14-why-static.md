+++
title = "Why static"
layout = "post"
date = 2026-02-14
description = "What a page costs when nothing runs on the server, and why that is still the fastest, longest-lived way to publish."
tags = ["static", "writing"]
categories = ["writing"]
+++

A static page is finished before anyone reads it. The server does not render it, the database is not consulted, and no script rearranges it after it arrives. That sounds like a limitation until you notice what it removes: every failure mode that needs a process to stay alive.

## The page is the artifact

When a page is built ahead of time, the thing you test is the thing you ship. There is no "works on my machine" gap between development and production, because both serve the same bytes from disk. A rebuild is cheap enough to run on every save, and a rollback is a file copy.

The same property makes the site durable. A static site does not care whether it is served by a large CDN, a small VPS, or a folder on a laptop. When the host changes, the site does not.

## What the reader pays for

One HTML document, one stylesheet, and whatever images the page actually shows. No framework bootstraps, no webfont requests, no analytics beacon waiting to time out. On a slow connection the text is readable almost immediately, and it stays readable when the network is gone.

> Speed is a feature, but the quieter benefit is predictability. A page that needs nothing cannot fail to need it.

## What it costs

The trade is honest: a static site cannot comment, search, or personalize by itself. Those are real losses, and pretending otherwise would be dishonest. But most personal blogs do not need them, and the ones that do can bolt them on later — a hosted comment service, a client-side search index, a small JSON feed.

The point is to start from the simplest thing that is genuinely good. A blog is a reading experience first; everything else is optional.
