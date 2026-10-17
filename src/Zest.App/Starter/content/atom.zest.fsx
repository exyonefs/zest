// @permalink /atom.xml
// @layout none
// @title Atom Feed

open Zest.Markup

let data = Context.get().SiteData
let opt k = if data.ContainsKey(k) then data.[k].ToString() else ""

let siteUrl = (let u = opt "site.base_url" in if u <> "" then u else "https://example.com").TrimEnd('/')
let siteTitle = if opt "site.title" <> "" then opt "site.title" else "Zest Site"
let siteDesc = opt "site.description"
let author = opt "site.author"

// Posts only — the same set the RSS feed carries; see feed.zest.fsx.
let articles =
    sitePages ()
    |> Array.filter (fun p -> p.date <> "" && p.url.StartsWith "/posts/")
    |> Array.sortByDescending (fun p -> p.date)
    |> Array.map (fun p ->
        { url = p.url; title = p.title; date = p.date; description = p.description } : Feeds.FeedItem)

printfn "%s" (Feeds.atomXml siteTitle siteUrl siteDesc author articles)
