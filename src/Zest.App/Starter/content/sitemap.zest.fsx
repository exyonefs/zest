// @permalink /sitemap.xml
// @layout none
// @title Sitemap

open Zest.Markup

let data = Context.get().SiteData
let opt k = if data.ContainsKey(k) then data.[k].ToString() else ""

let siteUrl = let u = opt "site.base_url" in if u <> "" then u else "https://example.com"

// Everything a crawler should index: the home page, the about page, the
// archive and the posts. The 404 page and the machine-readable files (this
// sitemap, the feeds, robots.txt) are deliberately left out.
let pages =
    sitePages ()
    |> Array.filter (fun p ->
        p.url <> "/404.html"
        && p.url <> "/feed.xml"
        && p.url <> "/atom.xml"
        && p.url <> "/sitemap.xml"
        && p.url <> "/robots.txt")
    |> Array.map (fun p ->
        let priority =
            if p.url = "/" then 1.0
            elif p.url = "/about.html" || p.url = "/archive.html" then 0.8
            else 0.6
        { url = p.url; date = p.date; priority = priority } : Feeds.SitemapItem)

printfn "%s" (Feeds.sitemapXml siteUrl pages)
