# Data/Trinity/Trinity.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.NLP
- AssemblyName  : Microsoft.VisualBasic.Data.TrinityNLP
- TargetFramework: net10.0
- Source files  : 7
- Existing Title: Trinity TextRank Keyword Extraction and Text Summarization
- Existing Desc : NLP module of sciBASIC#: TextRank keyword and key phrase extraction, PageRank based sentence scoring for automatic summarization, Brill part-of-speech tagging, and HTML article body extraction.
- Existing Tags : scibasic;nlp;textrank;keyword-extraction;summarization;pos-tagging

## Namespaces
- POSTagger  [files: 2]

## Public types
- Structure Article (Html\Article.vb) - 文章正文数据模型
- Class Html2Article (Html\Html2Article.vb) - 解析Html页面的文章正文内容,基于文本密度的HTML正文提取类 https://github.com/stanzhai/Html2Article http://www.cnblogs.com/jasondan/p/3497757.html
- Class UrlUtility (Html\UrlUtility.vb) - Url处理辅助类
- Module NLPExtensions (NLP.vb)
- Class BrillTransformationRules (POSTagger\BrillTransformationRules.vb) - https://github.com/Korthax/pos-net
- Class PartOfSpeech (POSTagger\PartOfSpeech.vb)
- Module TextRank (TextRank.vb) - This module implements TextRank, an unsupervised keyword significance scoring algorithm. TextRank builds a weighted graph representation Of a document Using words As nodes

## Notable public members
- Public Property title As String
- Public Property content As String
- Public Property contentWithTags As String
- Public Property publishDate As DateTime
- Public Overrides Function ToString() As String
- Public Shared Function ParseText(html As String,
- Public Property AppendMode() As Boolean = False
- Public Property Depth() As Integer = 6
- Public Property LimitCount() As Integer = 180
- Public Function GetArticle(html As String) As Article
- Public Shared Function FixUrl(baseUrl As String, html As String) As String
- Public Function KeyPhrases(originalText$, keywordsSet As IEnumerable(Of String), Optional minOccurNum% = 2) As IEnumerable(Of String)
- Public Function KeyWords(text As GraphMatrix) As Dictionary(Of String, Double)
- Public Function Abstract(text As WeightedPRGraph, Optional minWords% = 6, Optional minWeight# = 0.05) As Dictionary(Of String, Double)
- Public Function AbstractFilter(textRank As Dictionary(Of String, Double), Optional minWords% = 6, Optional minWeight# = 0.05) As Dictionary(Of String,…
- Public Function GetRule(index As Integer) As Action(Of List(Of PartOfSpeech), Integer)
- Public Sub SetRule(index As Integer, rule As Action(Of List(Of PartOfSpeech), Integer))
- Public Sub AppendRule(rule As Action(Of List(Of PartOfSpeech), Integer))
- Public Sub SetRules(newRules As IEnumerable(Of Action(Of List(Of PartOfSpeech), Integer)))
- Public Function GetRules() As IEnumerable(Of Action(Of List(Of PartOfSpeech), Integer))
- Public Property Word As String
- Public Property Tag As String
- Public Overrides Function Equals(obj As Object) As Boolean
- Public Overrides Function GetHashCode() As Integer
- Public Function TextRankGraph(sentences As IEnumerable(Of String), Optional win_size% = 2, Optional stopwords As StopWords = Nothing) As GraphMatrix
- Public Sub TextRankGraph(g As Graph, text As String(), Optional win_size% = 2, Optional directed As Boolean = True)
- Public Function TextGraph(text$, Optional similarityCut# = 0.05) As WeightedPRGraph

## Imports
- Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
- Microsoft.VisualBasic.ComponentModel.Algorithm.base
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.Data.GraphTheory
- Microsoft.VisualBasic.Data.GraphTheory.Analysis.PageRank
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math
- System.Net
- System.Runtime.CompilerServices
- System.Text
- System.Text.RegularExpressions

## File tree
- Html\Article.vb
- Html\Html2Article.vb
- Html\UrlUtility.vb
- NLP.vb
- POSTagger\BrillTransformationRules.vb
- POSTagger\PartOfSpeech.vb
- TextRank.vb

