# Trinity：TextRank 关键词抽取与文本摘要

## 引言

给定一篇长文，我们往往只想知道三件事：

1. **它讲什么**（关键词 / 关键短语）；
2. **哪几句最重要**（抽取式摘要）；
3. **正文从哪里开始到哪里结束**（网页正文抽取）。

这三件事都可以归结为同一个数学工具：**PageRank 在图上的传播**。

- 把「词」作为节点、共现关系作为边 → 词的重要性（TextRank）；
- 把「句」作为节点、句子相似度作为边 → 句的重要性（摘要）。

本包（Trinity）为 `sciBASIC#` 提供了这套实现，并附带 Brill 词性标注与网页正文抽取。

## 设计目标

- **无监督**：不需要标注语料或训练模型，给定文本即可产出结果；
- **可解释**：每个关键词 / 句子都带有得分，便于人工核查与调参；
- **面向真实网页**：内置按文本密度抽取正文的能力，跳过导航与页脚。

## 核心特性

- 由句子构建**共现词图**并应用 PageRank 为关键词打分（`TextRankGraph` + `KeyWords`）；
- 用**加权 PageRank 图**为句子打分，实现抽取式摘要 / 文摘（`TextGraph`、`Abstract`、`AbstractFilter`）；
- 给定关键词集合，从文本中抽取**多词关键短语**（带最小出现次数过滤）；
- 通过**文本密度分析**从 HTML 页面抽取正文、标题与发布日期，并提供基于 Brill 变换规则的词性标注器。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.NLP.TextRank` —— 构建用于关键词与句子打分的 TextRank 图（`TextRankGraph`、`TextGraph`）；
- `...Data.NLP.NLPExtensions` —— 图与文本之上的 `KeyWords`、`Abstract`、`AbstractFilter`、`KeyPhrases` 扩展；
- `...Data.NLP.Html2Article` —— 基于文本密度的 HTML 正文抽取器，返回 `Article`；
- `...Data.NLP.Article` —— 抽取出的文章模型：标题、正文、带标签正文与发布日期；
- `...Data.NLP.UrlUtility` —— 依据基准 URL 修正 HTML 片段中的相对 URL；
- `...Data.NLP.POSTagger.BrillTransformationRules` —— Brill 词性变换规则表；
- `...Data.NLP.POSTagger.PartOfSpeech` —— 一个标注词（词 + 词性对）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.NLP

Dim graph As GraphMatrix = TextRank.TextRankGraph(sentences, win_size:=2)
Dim keywords As Dictionary(Of String, Double) = graph.KeyWords()

Dim article As Article = New Html2Article().GetArticle(html)
Console.WriteLine(article.title)
```

## 实现要点

- **窗口大小 `win_size` 的作用**：它决定「哪些词算共现」，从而控制关键词偏向词组还是分散词；窗口过大会把无关词连在一起。
- **句子图的权重**：句子相似度不仅取决于共词数量，还与词的重要性有关；实现中用加权 PageRank 让「重要词构成的句子」得分更高。
- **文本密度抽取正文**：网页正文的特点是「文字密集、标签稀疏」；通过统计标签与文本的比例即可把导航条与页脚排除在外，无需针对每个站点写规则。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.TrinityNLP`
- RootNamespace：`Microsoft.VisualBasic.Data.NLP`
- TargetFramework：`net10.0`
- Tags：`scibasic;nlp;textrank;pagerank;keyword-extraction;summarization;pos-tagging;html-extraction`
- 许可：GPL-3.0-or-later
