# Trinity TextRank Keyword Extraction and Text Summarization

NLP module of sciBASIC#: TextRank keyword and key-phrase extraction, PageRank based sentence scoring for automatic summarization, Brill part-of-speech tagging, and HTML article body extraction.

## Overview
- Builds a co-occurrence word graph from sentences and applies PageRank to score keyword significance (`TextRankGraph` + `KeyWords`).
- Scores sentences with a weighted PageRank graph for extractive summarization / abstracting (`TextGraph`, `Abstract`, `AbstractFilter`).
- Extracts multi-word key phrases from text given a keyword set, with a minimum occurrence filter.
- Extracts the main article body, title and publish date from an HTML page by text-density analysis, and provides a Brill transformation-rule based POS tagger.

## Key Types
- `Microsoft.VisualBasic.Data.NLP.TextRank` — builds the TextRank graphs (`TextRankGraph`, `TextGraph`) used for keyword and sentence scoring.
- `Microsoft.VisualBasic.Data.NLP.NLPExtensions` — `KeyWords`, `Abstract`, `AbstractFilter` and `KeyPhrases` extensions over the graphs and text.
- `Microsoft.VisualBasic.Data.NLP.Html2Article` — HTML article body extractor based on text density; returns an `Article`.
- `Microsoft.VisualBasic.Data.NLP.Article` — extracted article model: title, content, content with tags, publish date.
- `Microsoft.VisualBasic.Data.NLP.UrlUtility` — fixes relative URLs inside an HTML fragment against a base URL.
- `Microsoft.VisualBasic.Data.NLP.POSTagger.BrillTransformationRules` — rule list for Brill part-of-speech transformation.
- `Microsoft.VisualBasic.Data.NLP.POSTagger.PartOfSpeech` — a tagged word (word + tag pair).

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.NLP

Dim graph As GraphMatrix = TextRank.TextRankGraph(sentences, win_size:=2)
Dim keywords As Dictionary(Of String, Double) = graph.KeyWords()

Dim article As Article = New Html2Article().GetArticle(html)
Console.WriteLine(article.title)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.TrinityNLP`
- RootNamespace: `Microsoft.VisualBasic.Data.NLP`
- TargetFramework: `net10.0`
- Tags: `scibasic;nlp;textrank;keyword-extraction;summarization;pos-tagging`

## License
GPL-3.0-or-later
