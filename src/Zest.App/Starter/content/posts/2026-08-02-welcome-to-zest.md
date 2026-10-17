+++
title = "Welcome to Zest"
layout = "post"
date = 2026-08-02
description = "What this template is: a full-featured static blog with pagination, tags, categories and no runtime dependencies."
tags = ["zest", "writing"]
categories = ["writing"]
+++

This site is the Zest starter: a blog with a flat structure, one stylesheet, and nothing that runs in your browser. No JavaScript, no webfonts, no analytics, no third-party requests — the page you are reading is the entire site.

## The shape of the site

Every page is one file, and every URL is that file's path:

```text
index.html                  the home listing, five posts per page
page/2/index.html           the second window of the listing
about/index.html            the about page
archive/index.html          every post, grouped by year
tags/index.html             the tag index, one page per tag
categories/index.html       the category index, one page per category
404.html                    the not-found page
assets/css/style.css        the one stylesheet
feed.xml                    the RSS feed
atom.xml                    the Atom feed
sitemap.xml                 for crawlers
robots.txt                  for crawlers
posts/                      one directory per post
```

Posts live in `content/posts/` as Markdown. The file name decides the URL, and the front matter decides the metadata:

| Key | Meaning |
| --- | --- |
| `title` | the heading, and the `<title>` |
| `date` | orders the home listing, the archive and the feeds |
| `description` | the excerpt in listings and feeds |
| `tags` | one page per label under `/tags/` |
| `categories` | one page per label under `/categories/` |
| `layout` | which file in `_layouts/` renders the page |

A `permalink` is not needed: only the pages whose URLs are fixed by the format itself declare one — `/404.html` for the not-found page, and the feed, sitemap and robots files.

## Publishing a post

Create `content/posts/2026-10-17-hello.md`, give it a title, a date and some tags, write it, and run `zest build`. The home listing, the pager, the archive, the tag and category pages, both feeds and the sitemap regenerate from the front matter — there is nothing else to update.

Deployment is a copy: `_site/` is plain static files, so any web server, object store or static host will serve it. Git is the backup; a push to a host that builds from source is the publish.

## What is deliberately missing

There is no comment system, no search box and no admin screen. Comments can be an email link, search can be the browser's own find, and everything else can be added later if the site ever needs it. What ships here is a complete blog and nothing more.
