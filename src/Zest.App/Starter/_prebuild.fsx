// _prebuild.fsx — Zest pre-build script.
//
// Runs once per build, after _config.toml is loaded and before the site is
// built. It must sit at the project root under exactly this name; it is never
// discovered in subdirectories, never routed, and never published.
//
// Its job is to shape what gets rendered: inject values templates read, or
// register helpers. Anything declarative belongs in _config.toml instead.
//
// Available helpers include addGlobal, addGlobalFunction, addFilter, loadJson,
// loadToml, loadEnv, consoleLog and exec.

// Expose the build year to templates as {{ site.buildYear }}, so the footer
// folio never shows a stale copyright year.
addGlobal "buildYear" (System.DateTime.UtcNow.Year)

// More examples — uncomment what you need.
//
// Load a JSON file into global data:
//
//   addGlobal "authors" (loadJson "_data/authors.json")
//
// Register a custom filter usable as {{ title | shout }}:
//
//   addFilter "shout" "upper"
//
// Inject the current commit as {{ site.commit }}:
//
//   match (exec "git" [ "rev-parse"; "--short"; "HEAD" ]).code with
//   | 0 -> addGlobal "commit" ((exec "git" [ "rev-parse"; "--short"; "HEAD" ]).stdout.Trim())
//   | _ -> consoleLog "not a git checkout"
