// @layout default

// ---------------------------------------------------------------------------
// index.zest.fsx - the home listing: every post, newest first.
//
// Posts are the dated pages under /posts/; the about page, the archive, the
// 404 page and the feeds carry no date, so they never show up here. The
// listing is the only thing printed here; `// @layout default` wraps it in
// the page frame, which supplies the title, the navigation and the
// stylesheet.
// ---------------------------------------------------------------------------

open Zest.Markup

let posts =
    sitePages ()
    |> Array.filter (fun p -> p.date <> "" && p.url.StartsWith "/posts/")
    |> Array.sortByDescending (fun p -> p.date)

let items =
    posts
    |> Array.map (fun post ->
        let children =
            [ timeClass "post-list__date" post.date [ text post.date ]
              h2Class "post-list__title" [ aHref post.url post.title ] ]
            @ (if post.description = "" then
                   []
               else
                   [ pClass "post-list__excerpt" [ text post.description ] ])

        liClass "post-list__item" [ fragmentLines children ])
    |> String.concat "\n"

let body =
    [ pClass "list-intro" [ text (siteData "site.description") ]
      ulClass "post-list" [ items ] ]
    |> fragmentLines

printfn "%s" body
