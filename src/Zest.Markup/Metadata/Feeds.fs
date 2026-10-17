namespace Zest.Markup

open System
open System.Text

// ============================================================
// Feeds — RSS 2.0, Atom 1.0, and Sitemap XML generation
// ============================================================

module Feeds =

    /// A single feed entry (RSS/Atom). Named record — unlike anonymous
    /// records, named types unify across assembly boundaries, so FSI page
    /// scripts can construct these and call rssXml / atomXml directly.
    type FeedItem = {
        url: string
        title: string
        date: string
        description: string
    }

    /// A single sitemap URL entry.
    type SitemapItem = {
        url: string
        date: string
        priority: float
    }

    /// XML-encode a string for safe insertion into XML content.
    let private xe (s: string) =
        if String.IsNullOrEmpty s then ""
        else
            s.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;")
             .Replace("\"", "&quot;")
             .Replace("'", "&apos;")

    /// Render a date as RFC 822 (RSS pubDate) using the invariant culture.
    let private rfc822 (d: DateTime) =
        d.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", Globalization.CultureInfo.InvariantCulture)

    /// Generate an RSS 2.0 feed. `feedPath` is the feed's own site-relative
    /// URL (e.g. "/feed.xml"), published as <atom:link rel="self">; `language`
    /// is the site language code (e.g. "en" or "zh-CN") for <language>.
    let rssXml (feedPath: string) (language: string) (siteTitle: string) (siteUrl: string) (siteDescription: string) (pages: FeedItem[]) =
        // Normalize once so joining with absolute paths cannot produce "//".
        let siteUrl = siteUrl.TrimEnd('/')
        let feedPath = if feedPath.StartsWith "/" then feedPath else "/" + feedPath
        // RSS 2.0 requires <language>; "en" is the fallback for a site that
        // declares none, because omitting the element makes the feed invalid.
        let language = if String.IsNullOrWhiteSpace language then "en" else language.Trim()
        let sb = StringBuilder()
        sb.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""") |> ignore
        sb.AppendLine("""<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">""") |> ignore
        sb.AppendLine("  <channel>") |> ignore
        sb.AppendFormat("    <title>{0}</title>\n", xe siteTitle) |> ignore
        sb.AppendFormat("    <link>{0}</link>\n", xe siteUrl) |> ignore
        sb.AppendFormat("    <description>{0}</description>\n", xe siteDescription) |> ignore
        sb.AppendFormat("    <atom:link href=\"{0}{1}\" rel=\"self\" type=\"application/rss+xml\" />\n", xe siteUrl, xe feedPath) |> ignore
        sb.AppendFormat("    <language>{0}</language>\n", xe language) |> ignore
        sb.AppendFormat("    <lastBuildDate>{0}</lastBuildDate>\n", rfc822 DateTime.UtcNow) |> ignore
        for page in pages do
            let pubDate =
                match DateTime.TryParse(page.date) with
                | true, d -> rfc822 d
                | _ -> rfc822 DateTime.UtcNow
            let fullUrl =
                if page.url.StartsWith("/") then siteUrl.TrimEnd('/') + page.url
                else page.url
            sb.AppendLine("    <item>") |> ignore
            sb.AppendFormat("      <title>{0}</title>\n", xe page.title) |> ignore
            sb.AppendFormat("      <link>{0}</link>\n", xe fullUrl) |> ignore
            sb.AppendFormat("      <guid>{0}</guid>\n", xe fullUrl) |> ignore
            sb.AppendFormat("      <pubDate>{0}</pubDate>\n", pubDate) |> ignore
            sb.AppendFormat("      <description>{0}</description>\n", xe page.description) |> ignore
            sb.AppendLine("    </item>") |> ignore
        done
        sb.AppendLine("  </channel>") |> ignore
        sb.AppendLine("</rss>") |> ignore
        sb.ToString()

    /// Generate an Atom 1.0 feed. `feedPath` and `language` play the same
    /// roles as in rssXml: the feed's own site-relative URL (the self link
    /// and the feed <id>) and the xml:lang of the feed's text.
    let atomXml (feedPath: string) (language: string) (siteTitle: string) (siteUrl: string) (siteDescription: string) (authorName: string) (pages: FeedItem[]) =
        // Normalize once so joining with absolute paths cannot produce "//".
        let siteUrl = siteUrl.TrimEnd('/')
        let feedPath = if feedPath.StartsWith "/" then feedPath else "/" + feedPath
        // xml:lang is optional in Atom, so it appears only when the site
        // declares a language.
        let langAttr =
            if String.IsNullOrWhiteSpace language then ""
            else sprintf " xml:lang=\"%s\"" (xe (language.Trim()))
        let sb = StringBuilder()
        sb.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""") |> ignore
        sb.AppendFormat("<feed xmlns=\"http://www.w3.org/2005/Atom\"{0}>\n", langAttr) |> ignore
        sb.AppendFormat("  <title>{0}</title>\n", xe siteTitle) |> ignore
        sb.AppendFormat("  <subtitle>{0}</subtitle>\n", xe siteDescription) |> ignore
        sb.AppendFormat("  <link href=\"{0}{1}\" rel=\"self\" />\n", xe siteUrl, xe feedPath) |> ignore
        sb.AppendFormat("  <link href=\"{0}\" rel=\"alternate\" />\n", xe siteUrl) |> ignore
        sb.AppendFormat("  <updated>{0}</updated>\n", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")) |> ignore
        sb.AppendFormat("  <id>{0}{1}</id>\n", xe siteUrl, xe feedPath) |> ignore
        if not (String.IsNullOrEmpty authorName) then
            sb.AppendFormat("  <author><name>{0}</name></author>\n", xe authorName) |> ignore
        for page in pages do
            let updated =
                match DateTime.TryParse(page.date) with
                | true, d -> d.ToString("yyyy-MM-ddTHH:mm:ssZ")
                | _ -> DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            let fullUrl =
                if page.url.StartsWith("/") then siteUrl.TrimEnd('/') + page.url
                else page.url
            sb.AppendLine("  <entry>") |> ignore
            sb.AppendFormat("    <title>{0}</title>\n", xe page.title) |> ignore
            sb.AppendFormat("    <link href=\"{0}\" />\n", xe fullUrl) |> ignore
            sb.AppendFormat("    <id>{0}</id>\n", xe fullUrl) |> ignore
            sb.AppendFormat("    <updated>{0}</updated>\n", updated) |> ignore
            sb.AppendFormat("    <summary>{0}</summary>\n", xe page.description) |> ignore
            sb.AppendLine("  </entry>") |> ignore
        done
        sb.AppendLine("</feed>") |> ignore
        sb.ToString()

    /// Generate a Sitemap XML string.
    let sitemapXml (baseUrl: string) (pages: SitemapItem[]) =
        // Normalize once so joining with absolute paths cannot produce "//".
        let baseUrl = baseUrl.TrimEnd('/')
        let sb = StringBuilder()
        sb.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""") |> ignore
        sb.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""") |> ignore
        for page in pages do
            let fullUrl =
                if page.url.StartsWith("/") then baseUrl.TrimEnd('/') + page.url
                else page.url
            let lastMod =
                match DateTime.TryParse(page.date) with
                | true, d -> d.ToString("yyyy-MM-dd")
                | _ -> DateTime.UtcNow.ToString("yyyy-MM-dd")
            sb.AppendLine("  <url>") |> ignore
            sb.AppendFormat("    <loc>{0}</loc>\n", xe fullUrl) |> ignore
            sb.AppendFormat("    <lastmod>{0}</lastmod>\n", lastMod) |> ignore
            sb.AppendFormat("    <priority>{0:F1}</priority>\n", page.priority) |> ignore
            sb.AppendLine("  </url>") |> ignore
        done
        sb.AppendLine("</urlset>") |> ignore
        sb.ToString()
