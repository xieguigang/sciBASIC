# Microsoft Bing Web, Academic, Translation and DOI Services Client

Bing service clients for the sciBASIC# framework: scrape web search results with paging, query the academic search portal for article profiles, look up word translations, and decode DOI handle service responses.

## Overview
- Web search: builds Bing query URLs, downloads and parses result pages into `WebResult` items, with `NextPage`/`HaveNext` paging and an all-results iterator.
- Academic search: term search returning candidate article links, profile scraping into `ArticleProfile` metadata (authors, journal, DOI, citations, keywords) and a knowledge-base builder.
- Translation: word translation lookup returning pronunciation and a list of target words.
- DOI: strongly typed response model for the DOI handle service (handle, values, HS_ADMIN).

## Key Types
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.SearchEngineProvider` — Bing web search entry (`URLProvider`, `Search`, `DownloadResult`, `GetAllResults`).
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.SearchResult` — one result page: count, current items and next-page navigation.
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.WebResult` — one scraped result entry (title, URL, brief text, update date).
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Academic.AcademicSearch` — academic portal search plus article detail retrieval.
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Academic.ArticleProfile` — parsed article metadata for one publication.
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Academic.Extensions` — builds a literature knowledge base from a term and summarizes article sets.
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Translation.WordTranslation` — translation result for a single word.
- `Microsoft.VisualBasic.Data.KnowledgeBase.Web.DOI.Response` — decoded DOI handle service response.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing
Imports Microsoft.VisualBasic.Data.KnowledgeBase.Web.Bing.Academic

Dim page As SearchResult = SearchEngineProvider.Search("gcmodeller")

For Each item As WebResult In page.CurrentPage
    Console.WriteLine($"{item.Title} -> {item.URL}")
Next

For Each article In AcademicSearch.Search("gcmodeller")
    Console.WriteLine(article.Name)
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.Web.KnowledgeBase`
- TargetFramework: `net10.0`
- Tags: `scibasic;bing;search-engine;web-scraping;translation;doi`

## License
GPL-3.0-or-later
