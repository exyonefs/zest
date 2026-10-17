+++
title = "When a blog outgrows its index"
layout = "post"
date = 2026-05-24
description = "Pagination, tags, categories and the archive: four ways to keep a growing site navigable without a database."
tags = ["zest", "writing"]
categories = ["engineering"]
+++

The first version of a blog is a list. The second version is a longer list. At some point the list stops being useful, and that is when a static site has to answer the same questions a dynamic one does — without the machinery.

## Pagination

The home page shows five posts at a time. The build renders the same template once per window: the first window at `/`, the second at `/page/2/`, and so on. Each window is a real HTML file with its own title, its own canonical link and its own pager, so a crawler sees ordinary pages and a reader sees a normal "next" link.

Five is a deliberately small number. A window should be a screenful or two, not a scroll through everything.

## Tags and categories

Two taxonomies, one mechanism. A post declares `tags = ["zest", "writing"]` and `categories = ["engineering"]` in its front matter; the build reads every post and generates `/tags/`, `/tags/zest/`, `/categories/` and `/categories/engineering/` from the same two templates.

The distinction is one of scale and intent. Tags are close to the post — a handful of words that describe what it is about. Categories are closer to the site — the shelf a post belongs on. Neither is a hierarchy; both are ways to enter the archive from the side.

## The archive

Finally, `/archive/` gathers every post by year. It is the page you want when you know roughly *when* you read something and not what it was tagged with.

None of this needs JavaScript, a search index, or a database. It is all computed at build time from the front matter of seven files, and it would keep working the same way at seven hundred.
