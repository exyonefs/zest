// @permalink /feed.xml
// @layout none
// @title RSS Feed

open Zest.Markup

let data = Context.get().SiteData
let opt k = if data.ContainsKey(k) then data.[k].ToString() else ""

let siteUrl = (let u = opt "site.base_url" in if u <> "" then u else "https://example.com").TrimEnd('/')
let siteTitle = if opt "site.title" <> "" then opt "site.title" else "Zest Site"
let siteDesc = opt "site.description"

// Posts only: the dated pages under /posts/. The home page, the archive, the
// about page and this feed itself have no date and never enter the channel.
// rssXmlAt points the feed's <atom:link rel="self"> at its real path.
let articles =
    sitePages ()
    |> Array.filter (fun p -> p.date <> "" && p.url.StartsWith "/posts/")
    |> Array.sortByDescending (fun p -> p.date)
    |> Array.map (fun p ->
        { url = p.url; title = p.title; date = p.date; description = p.description } : Feeds.FeedItem)

printfn "%s" (Feeds.rssXmlAt "/feed.xml" siteTitle siteUrl siteDesc articles)
