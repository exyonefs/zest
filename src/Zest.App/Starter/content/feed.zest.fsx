// @permalink /feed.xml
// @layout none
// @title RSS Feed

open Zest.Markup

let data = Context.get().SiteData
let opt k = if data.ContainsKey(k) then data.[k].ToString() else ""

let siteUrl = (let u = opt "site.base_url" in if u <> "" then u else "https://example.com").TrimEnd('/')
let siteTitle = if opt "site.title" <> "" then opt "site.title" else "Zest Site"
let siteDesc = opt "site.description"
let language = opt "site.language"

// Posts only: every dated page is an entry, newest first. The home page, the
// archive, the about page and this feed itself carry no date and never enter
// the channel.
let articles =
    sitePages ()
    |> Array.filter (fun p -> p.date <> "")
    |> Array.sortByDescending (fun p -> p.date)
    |> Array.map (fun p ->
        { url = p.url; title = p.title; date = p.date; description = p.description } : Feeds.FeedItem)

// The first two arguments must agree with the @permalink directive above:
// the feed's own path (the self link) and the site language.
printfn "%s" (Feeds.rssXml "/feed.xml" language siteTitle siteUrl siteDesc articles)
