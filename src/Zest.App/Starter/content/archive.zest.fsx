// @permalink /archive.html
// @layout default
// @title Archive
// @description Every post, grouped by year.

open Zest.Markup

// sitePages() returns every content page with its front matter already parsed.
// The feeds use the same call; dates are always "yyyy-MM-dd" strings.
let posts =
    sitePages ()
    |> Array.filter (fun p -> p.date <> "" && p.url <> "/archive.html")
    |> Array.sortByDescending (fun p -> p.date)

let body =
    posts
    |> Array.groupBy (fun p -> p.date.Substring(0, 4))
    |> Array.sortByDescending fst
    |> Array.map (fun (year, items) ->
        let list =
            items
            |> Array.map (fun p ->
                liClass "post-list__item"
                    [ timeClass "post-list__date" p.date [ text p.date ]
                      h3Class "post-list__title" [ aHref p.url p.title ] ])
            |> String.concat "\n"
        sectionClass "archive__year"
            [ h2 [ text year ]
              ulClass "post-list" [ list ] ])
    |> String.concat "\n"

printfn "%s" body
