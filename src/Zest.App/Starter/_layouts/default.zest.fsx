// @layout shell

// ---------------------------------------------------------------------------
// default.zest.fsx - an ordinary page: the title, an optional subtitle taken
// from the page description, and the body.
//
// The name is not free: `default` is the layout the engine falls back to for
// every page that declares none, so renaming this file needs a matching
// `default_layout` entry in _config.toml. `// @layout shell` hands the markup
// below to shell.zest.fsx, which wraps it in the <html> document.
// ---------------------------------------------------------------------------

// The home page carries the site title; every other page its own.
let articleTitle =
    if page.title = "" || page.url = "/" then site.title else page.title

let articleHead =
    let children =
        [ h1Class "article__title" [ text articleTitle ] ]
        @ (if page.description = "" then
               []
           else
               [ pClass "article__meta" [ text page.description ] ])

    headerClass "article__head" [ fragmentLines children ]

let articleBody = divClass "article__body" [ raw content ]

printfn
    "%s"
    (articleClass "article" [ fragmentLines [ articleHead; articleBody ] ])
