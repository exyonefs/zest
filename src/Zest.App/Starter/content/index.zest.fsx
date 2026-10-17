// @paginate 5
// @layout default

// ---------------------------------------------------------------------------
// index.zest.fsx - the home listing: the site's posts, five per window.
//
// `// @paginate 5` hands this URL to the pagination generator, which renders
// this file once per window with `pagination` in scope: currentPage,
// totalPages, totalItems, perPage, prevUrl, nextUrl and items. The listing
// and the pager are the only things printed here; `// @layout default` wraps
// each window in the page frame.
//
// This page is written in F#, so it composes its markup instead of including
// the Zestucks listing component in _includes/post-list.html. The class names
// are the same, so both render identically; the window size lives in
// [pagination] per_page in _config.toml.
// ---------------------------------------------------------------------------

open Zest.Markup

let items =
    pagination.items
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

let pagerStep (label: string) (url: string) =
    if url = "" then
        spanClass "pager__step pager__step--disabled" [ text label ]
    else
        aClass "pager__step" url [ text label ]

let pager =
    elem "nav" [ attr "class" "pager"; attr "aria-label" "Pagination" ]
        [ fragmentLines
              [ pagerStep "Newer" pagination.prevUrl
                spanClass "pager__status"
                    [ text (sprintf "Page %d of %d" pagination.currentPage pagination.totalPages) ]
                pagerStep "Older" pagination.nextUrl ] ]

let body =
    [ pClass "intro" [ text (siteData "site.description") ]
      olClass "post-list" [ items ]
      pager ]
    |> fragmentLines

printfn "%s" body
