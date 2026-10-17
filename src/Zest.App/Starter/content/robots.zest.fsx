// @permalink /robots.txt
// @layout none

// ---------------------------------------------------------------------------
// robots.zest.fsx - the crawler policy, written straight to /robots.txt.
//
// `// @layout none` opts out of the layout chain: this page is plain text,
// not an HTML document, so it is written exactly as printed here. The sitemap
// line tells crawlers where the index lives; the URL comes from [site] url.
// ---------------------------------------------------------------------------

open Zest.Markup

let siteUrl = (siteData "site.base_url").TrimEnd('/')

printfn "User-agent: *\nAllow: /\n\nSitemap: %s/sitemap.xml" siteUrl
