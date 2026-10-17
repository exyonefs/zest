+++
title = "Welcome to Zest"
layout = "post"
date = 2026-08-02
description = "What this template is: a flat, minimal blog with no runtime dependencies, and how to publish your first post."
permalink = "/posts/2026-08-02-welcome-to-zest.html"
+++

This site is the Zest starter: a small blog with a flat structure, one stylesheet, and nothing that runs in your browser. No JavaScript, no webfonts, no analytics, no third-party requests — the page you are reading is the entire site.

## The shape of the site

Every page is one file, and every URL is that file's path:

```text
index.html              the home listing
about.html              the about page
archive.html            every post, grouped by year
404.html                the not-found page
assets/css/style.css    the one stylesheet
feed.xml                the RSS feed
atom.xml                the Atom feed
sitemap.xml             for crawlers
robots.txt              for crawlers
posts/                  one .html file per post
```

Posts live in `content/posts/` as Markdown. The file name and the front matter decide the URL and the metadata:

| Key | Meaning |
| --- | --- |
| `title` | the heading, and the `<title>` |
| `date` | orders the home listing, the archive and the feeds |
| `description` | the excerpt in listings and feeds |
| `permalink` | the exact URL, e.g. `/posts/hello.html` |
| `layout` | which file in `_layouts/` renders the page |

## Publishing a post

Create `content/posts/2026-10-17-hello.md`, give it a title, a date and a permalink, write it, and run `zest build`. The home listing, the archive, both feeds and the sitemap regenerate from the front matter — there is nothing else to update.

Deployment is a copy: `_site/` is plain static files, so any web server, object store or static host will serve it. Git is the backup; a push to a host that builds from source is the publish.

## What is deliberately missing

There is no comment system, no search box, no tag pages, no pagination and no admin screen. Comments can be an email link, search can be the browser's own find, and everything else can be added later if the site ever needs it. What ships here is the smallest thing that is genuinely a blog.
