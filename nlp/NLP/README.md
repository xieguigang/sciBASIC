# Natural Language Processing Toolkit: Tokenizers, Stemmer, BM25 and LDA

Natural language processing toolkit for sciBASIC#, covering Chinese segmentation, HuggingFace-compatible sub-word tokenizers, stemming, TF-IDF, BM25 search and LDA topic modeling.

## Overview
- Chinese word segmentation: maximum-match (forward / backward / bidirectional) over a `WordDictionary`, plus an HMM based tokenizer.
- HuggingFace-compatible pipeline (`tokenizer.json`): normalizer, pre-tokenizer, BPE / WordPiece / Unigram models, post-processor, decoder and byte-level alphabet.
- Ranked retrieval and topic modeling: BM25 with selectable IDF variants and per-term contribution details, plus Gibbs-sampled LDA with a corpus/vocabulary loader.
- Text utilities: Porter stemmer, stop words, TF-IDF weighting, sentence/paragraph/word models and token counters.

## Key Types
- `Microsoft.VisualBasic.Data.NLP.ChineseTokenizer.Tokenizer` — Chinese segmenter (`Segment`, `SegmentToString`, `TrainHmm`, `CreateDefault`).
- `Microsoft.VisualBasic.Data.NLP.ChineseTokenizer.HuggingFace.HuggingFaceTokenizer` — loads a pretrained tokenizer directory and encodes/decodes text or ids.
- `Microsoft.VisualBasic.Data.NLP.BM25.BM25Engine` — incremental BM25 index with `AddDocument`, `BuildIndex` and `Search`.
- `Microsoft.VisualBasic.Data.NLP.LDA.LdaGibbsSampler` — Gibbs sampler estimating the best topic assignment for words and documents.
- `Microsoft.VisualBasic.Data.NLP.LDA.Corpus` / `Vocabulary` — document collection and word-index mapping used by LDA.
- `Microsoft.VisualBasic.Data.NLP.Stemmer` — Porter stemmer reducing English words to their base form.
- `Microsoft.VisualBasic.Data.NLP.Model.Sentence` / `Paragraph` — ordered word/sentence models shared by the NLP stack.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.NLP.BM25
Imports Microsoft.VisualBasic.Data.NLP.ChineseTokenizer

Dim engine As New BM25Engine(k1:=1.2, b:=0.75)
Dim tokenizer As Tokenizer = Tokenizer.CreateDefault()

Call engine.AddDocument(1, tokenizer.Segment("蛋白质组学数据分析").ToArray())
Call engine.AddDocument(2, tokenizer.Segment("代谢组学数据分析").ToArray())
Call engine.BuildIndex()

For Each hit In engine.Search(tokenizer.Segment("数据分析").ToArray(), 10)
    Console.WriteLine($"{hit.DocId}  {hit.Score}")
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.NLP`
- TargetFramework: `net10.0`
- Tags: `scibasic;nlp;tokenizer;topic-model;bm25`

## License
GPL-3.0-or-later
