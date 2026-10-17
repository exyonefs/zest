+++
title = "The measure of a page"
layout = "post"
date = 2026-04-19
description = "A short note on line length, system fonts, and the discipline of a single stylesheet."
tags = ["typography", "css"]
categories = ["design"]
+++

Typography on the web is mostly a question of restraint. The page you are reading uses one font stack, one measure, and two colours. Almost every decision was made by removing something.

## Sixty-five characters is enough

The measure — the width of the text column — is capped at about forty rems, which works out to roughly sixty-five characters per line at the default size. That is the range reading research has pointed at for a century, and it is why the column does not stretch on a wide screen.

Narrow screens get the same measure, minus the margins. The text never needs a horizontal scrollbar, and the line length never collapses into a newspaper column.

## System fonts are a feature

There is no webfont here. The stack asks the operating system for its own interface face — `system-ui`, then the platform's fallbacks — and the page renders in whatever the reader already trusts. That means zero font requests, no flash of unstyled text, and a page that looks native on every device without shipping a single byte of type.

The trade is that the page does not look exactly the same everywhere. It looks appropriate everywhere, which is the better goal.

## One stylesheet

Everything the site needs lives in `assets/css/style.zcss`: the tokens, the reset, the chrome, the article block, the listings, the taxonomy pages and the print rules. One file in, one file out. A single stylesheet is easy to audit, impossible to leave half-loaded, and small enough that the browser fetches it once and forgets about it.
