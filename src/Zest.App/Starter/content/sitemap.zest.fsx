// @permalink /sitemap.xml
// @layout none
// @title Sitemap

open Zest.Markup

let data = Context.get().SiteData
let opt k = if data.ContainsKey(k) then data.[k].ToString() else ""

let siteUrl = let u = opt "site.base_url" in if u <> "" then u else "https://example.com"

// The home page is rendered by the pagination generator, which does not add
// its windows to sitePages(); the first window is the most important URL on
// the site, so it is listed here by hand. The later windows (/page/N/) are
// listing views with no content of their own and stay out.
let home: Feeds.SitemapItem[] =
    [| { url = "/"; date = ""; priority = 1.0 } |]

// Everything else a crawler should index: the about page, the archive and the
// posts. The 404 page and the machine-readable files (this sitemap, the feeds,
// robots.txt) are deliberately left out.
let contentPages =
    sitePages ()
    |> Array.filter (fun p ->
        p.url <> "/404.html"
        && p.url <> "/feed.xml"
        && p.url <> "/atom.xml"
        && p.url <> "/sitemap.xml"
        && p.url <> "/robots.txt")
    |> Array.map (fun p ->
        let priority =
            if p.url = "/about/" || p.url = "/archive/" then 0.8
            else 0.6
        { url = p.url; date = p.date; priority = priority } : Feeds.SitemapItem)

printfn "%s" (Feeds.sitemapXml siteUrl (Array.append home contentPages))
