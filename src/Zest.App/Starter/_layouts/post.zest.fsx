// @layout shell

// ---------------------------------------------------------------------------
// post.zest.fsx - a dated entry: title, date, body, and the way back to the
// archive.
//
// The ordinary page with the metadata only a dated entry has; `// @layout
// shell` supplies the document around it. The date is a real <time> element,
// so machines read it as well as people.
// ---------------------------------------------------------------------------

let articleTitle = if page.title = "" then site.title else page.title

let articleHead =
    let children =
        [ h1Class "article__title" [ text articleTitle ] ]
        @ (if page.date = "" then
               []
           else
               [ timeClass "article__meta" page.date [ text page.date ] ])

    headerClass "article__head" [ fragmentLines children ]

let articleFooter =
    let back =
        elem "p" [ attr "class" "article__back" ]
            [ elem "a" [ attr "href" "/archive.html" ] [ text "All posts" ] ]

    footerClass "article__footer" [ fragmentLines [ back ] ]

printfn
    "%s"
    (articleClass "article"
        [ fragmentLines
              [ articleHead
                divClass "article__body" [ raw content ]
                articleFooter ] ])
