#Region "Microsoft.VisualBasic::da102b8b05276a0bf67841615b2d735d, nlp\NLP\BM25\BM25.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 530
    '    Code Lines: 291 (54.91%)
    ' Comment Lines: 159 (30.00%)
    '    - Xml Docs: 59.12%
    ' 
    '   Blank Lines: 80 (15.09%)
    '     File Size: 20.01 KB


    '     Class BM25Engine
    ' 
    '         Properties: AverageDocumentLength, B, DocumentCount, IdfMode, K1
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ComputeIdf, GetDocumentFrequency, GetDocumentLength, GetDocumentLengths, GetDocumentText
    '                   GetIdf, GetStatistics, GetTermFrequency, GetVocabulary, ScoreDocument
    '                   (+3 Overloads) Search, SimpleTokenizer
    ' 
    '         Sub: (+2 Overloads) AddDocument, (+2 Overloads) AddDocuments, BuildIndex, Clear, RefreshIdf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports std = System.Math

' ============================================================================
' BM25.vb - Best Matching 25 相关性排序算法
'
' 核心公式:
'   score(D, Q) = Σ IDF(qi) · f(qi,D)·(k1+1) / (f(qi,D) + k1·(1-b+b·|D|/avgdl))
'
' IDF (Lucene 变体，避免负值):
'   IDF(qi) = log(1 + (N - n(qi) + 0.5) / (n(qi) + 0.5))
'
' 参数:
'   k1 = 1.2  词频饱和参数（范围 0.8–2.0）
'   b  = 0.75 文档长度归一化参数（范围 0–1）
'
' 设计要点:
'   - 倒排索引: term → {docId: tf}，支持 O(1) 查找
'   - 支持 Lucene IDF 和原始 Okapi IDF 两种变体
'   - 支持自定义分词器
'   - 支持参数动态调优，无需重建索引
' ============================================================================

Namespace BM25

    ''' <summary>
    ''' BM25 引擎。
    '''
    ''' 使用方式:
    '''   Dim engine As New BM25Engine(k1:=1.2, b:=0.75)
    '''   engine.AddDocument(0, {"苹果", "是", "一种", "水果"})
    '''   engine.AddDocument(1, {"苹果", "公司", "发布", "手机"})
    '''   engine.BuildIndex()
    '''   Dim results = engine.Search({"苹果"})
    ''' </summary>
    Public Class BM25Engine

        ' === 参数 ===

        ''' <summary>词频饱和参数，默认 1.2。</summary>
        Public Property K1 As Double = 1.2

        ''' <summary>文档长度归一化参数，默认 0.75。</summary>
        Public Property B As Double = 0.75

        ''' <summary>IDF 计算变体，默认 Lucene。</summary>
        Public Property IdfMode As IdfVariant = IdfVariant.Lucene

        ' === 索引数据结构 ===

        ' 文档列表
        Private ReadOnly _documents As New List(Of Document)

        ' 倒排索引: term → (docId → tf)
        Private ReadOnly _invertedIndex As New Dictionary(Of String, Dictionary(Of Integer, Integer))

        ' 文档频率: term → 包含该词的文档数
        Private ReadOnly _docFrequency As New Dictionary(Of String, Integer)

        ' IDF 缓存: term → idf 值
        Private ReadOnly _idfCache As New Dictionary(Of String, Double)

        ' 语料库统计量
        Private _totalDocs As Integer = 0
        Private _totalLength As Integer = 0
        Private _avgDocLength As Double = 0.0

        ' 是否需要重建 IDF 缓存
        Private _idfDirty As Boolean = True

        ''' <summary>获取语料库文档总数。</summary>
        Public ReadOnly Property DocumentCount As Integer
            Get
                Return _totalDocs
            End Get
        End Property

        ''' <summary>获取语料库平均文档长度。</summary>
        Public ReadOnly Property AverageDocumentLength As Double
            Get
                Return _avgDocLength
            End Get
        End Property

        ''' <summary>
        ''' 构造函数。
        ''' </summary>
        ''' <param name="k1">词频饱和参数，默认 1.2。</param>
        ''' <param name="b">长度归一化参数，默认 0.75。</param>
        ''' <param name="idfMode">IDF 计算变体，默认 Lucene。</param>
        Public Sub New(Optional k1 As Double = 1.2,
                       Optional b As Double = 0.75,
                       Optional idfMode As IdfVariant = IdfVariant.Lucene)
            Me.K1 = k1
            Me.B = b
            Me.IdfMode = idfMode
        End Sub

        ' ========================================================================
        ' 索引构建
        ' ========================================================================

        ''' <summary>
        ''' 添加文档到语料库。必须在 BuildIndex 之前调用。
        ''' </summary>
        ''' <param name="docId">文档唯一标识。</param>
        ''' <param name="tokens">分词后的词元数组。</param>
        ''' <param name="rawText">原始文本（可选）。</param>
        Public Sub AddDocument(docId As Integer, tokens As String(), Optional rawText As String = "")
            If tokens Is Nothing Then tokens = New String() {}
            Dim doc As New Document(docId, tokens, rawText)
            _documents.Add(doc)
            _totalDocs += 1
            _totalLength += tokens.Length

            ' 构建词频统计
            Dim termFreqs As New Dictionary(Of String, Integer)
            For Each term As String In tokens
                If term Is Nothing Then Continue For
                Dim t As String = term.ToLowerInvariant()
                If termFreqs.ContainsKey(t) Then
                    termFreqs(t) += 1
                Else
                    termFreqs(t) = 1
                End If
            Next

            ' 写入倒排索引
            For Each kvp As KeyValuePair(Of String, Integer) In termFreqs
                Dim term As String = kvp.Key
                Dim tf As Integer = kvp.Value

                If Not _invertedIndex.ContainsKey(term) Then
                    _invertedIndex(term) = New Dictionary(Of Integer, Integer)
                    _docFrequency(term) = 0
                End If

                _invertedIndex(term)(docId) = tf
                _docFrequency(term) += 1
            Next

            _idfDirty = True
        End Sub

        ''' <summary>
        ''' 添加文档（直接传原始文本，使用内置简单分词器）。
        ''' </summary>
        Public Sub AddDocument(docId As Integer, text As String)
            Dim tokens As String() = SimpleTokenizer(text)
            AddDocument(docId, tokens, text)
        End Sub

        ''' <summary>
        ''' 批量添加文档。
        ''' </summary>
        Public Sub AddDocuments(documents As IEnumerable(Of (Integer, String())))
            For Each doc In documents
                AddDocument(doc.Item1, doc.Item2)
            Next
        End Sub

        ''' <summary>
        ''' 批量添加文档（使用内置分词器）。
        ''' </summary>
        Public Sub AddDocuments(documents As IEnumerable(Of (Integer, String)))
            For Each doc In documents
                Dim tokens As String() = SimpleTokenizer(doc.Item2)
                AddDocument(doc.Item1, tokens, doc.Item2)
            Next
        End Sub

        ''' <summary>
        ''' 构建索引，计算 avgdl 和 IDF 缓存。
        ''' 在添加完所有文档后调用一次。
        ''' </summary>
        Public Sub BuildIndex()
            ' 计算平均文档长度
            If _totalDocs > 0 Then
                _avgDocLength = CDbl(_totalLength) / _totalDocs
            Else
                _avgDocLength = 0.0
            End If

            ' 预计算 IDF 缓存
            _idfCache.Clear()
            For Each term As String In _invertedIndex.Keys
                _idfCache(term) = ComputeIdf(term)
            Next

            _idfDirty = False
        End Sub

        ''' <summary>
        ''' 动态修改参数后重新计算 IDF 缓存（k1/b 变化不影响 IDF，仅 IdfMode 变化时需要）。
        ''' </summary>
        Public Sub RefreshIdf()
            _idfCache.Clear()
            For Each term As String In _invertedIndex.Keys
                _idfCache(term) = ComputeIdf(term)
            Next
            _idfDirty = False
        End Sub

        ' ========================================================================
        ' 查询与评分
        ' ========================================================================

        ''' <summary>
        ''' 对查询进行检索，返回按得分降序排列的所有结果。
        ''' </summary>
        ''' <param name="queryTokens">查询分词后的词元数组。</param>
        Public Function Search(queryTokens As String()) As List(Of SearchResult)
            Return Search(queryTokens, topK:=Integer.MaxValue)
        End Function

        ''' <summary>
        ''' 对查询进行检索，返回 Top-K 结果。
        ''' </summary>
        ''' <param name="queryTokens">查询分词后的词元数组。</param>
        ''' <param name="topK">返回前 K 条结果，0 或负数表示返回全部。</param>
        Public Function Search(queryTokens As String(), topK As Integer) As List(Of SearchResult)
            If _idfDirty Then
                RefreshIdf()
            End If

            ' 查询词去重（同一词只计算一次）
            Dim queryTerms As New HashSet(Of String)
            For Each t As String In queryTokens
                If t IsNot Nothing Then queryTerms.Add(t.ToLowerInvariant())
            Next

            If queryTerms.Count = 0 OrElse _totalDocs = 0 Then
                Return New List(Of SearchResult)
            End If

            ' 收集所有包含查询词的候选文档
            Dim candidateDocs As New HashSet(Of Integer)
            For Each term As String In queryTerms
                If _invertedIndex.ContainsKey(term) Then
                    For Each docId As Integer In _invertedIndex(term).Keys
                        candidateDocs.Add(docId)
                    Next
                End If
            Next

            ' 逐文档计算 BM25 得分
            Dim results As New List(Of SearchResult)
            For Each docId As Integer In candidateDocs
                Dim result As SearchResult = ScoreDocument(docId, queryTerms)
                If result.Score > 0 Then
                    results.Add(result)
                End If
            Next

            ' 按得分降序排序
            results.Sort(Function(a, b) b.Score.CompareTo(a.Score))

            ' 截取 Top-K
            If topK > 0 AndAlso results.Count > topK Then
                results = results.GetRange(0, topK)
            End If

            Return results
        End Function

        ''' <summary>
        ''' 对查询字符串进行检索（使用内置分词器）。
        ''' </summary>
        Public Function Search(query As String, Optional topK As Integer = 10) As List(Of SearchResult)
            Return Search(SimpleTokenizer(query), topK)
        End Function

        ''' <summary>
        ''' 计算单个文档对查询的 BM25 得分（含贡献明细）。
        ''' </summary>
        Private Function ScoreDocument(docId As Integer, queryTerms As HashSet(Of String)) As SearchResult

            ' 查找文档长度
            Dim docLength As Integer = 0
            Dim docIdx As Integer = -1
            For i As Integer = 0 To _documents.Count - 1
                If _documents(i).Id = docId Then
                    docLength = _documents(i).Length
                    docIdx = i
                    Exit For
                End If
            Next

            Dim result As New SearchResult With {
                .DocId = docId,
                .Score = 0.0,
                .TermContributions = New List(Of TermContribution)
            }

            If _avgDocLength <= 0 Then Return result

            ' 逐词累加
            For Each term As String In queryTerms
                ' 该词在此文档中的词频
                Dim tf As Integer = 0
                If _invertedIndex.ContainsKey(term) AndAlso
                   _invertedIndex(term).ContainsKey(docId) Then
                    tf = _invertedIndex(term)(docId)
                End If

                ' 词未在文档中出现，贡献为 0
                If tf = 0 Then Continue For

                ' IDF
                Dim idf As Double = 0.0
                If _idfCache.ContainsKey(term) Then
                    idf = _idfCache(term)
                End If

                ' 文档长度因子: 1 - b + b * |D| / avgdl
                Dim lengthFactor As Double = 1.0 - B + B * (CDbl(docLength) / _avgDocLength)

                ' TF 饱和部分: f * (k1+1) / (f + k1 * lengthFactor)
                Dim tfSaturation As Double = (CDbl(tf) * (K1 + 1.0)) /
                                              (CDbl(tf) + K1 * lengthFactor)

                ' 该词贡献 = IDF * TF饱和
                Dim contribution As Double = idf * tfSaturation

                result.Score += contribution

                result.TermContributions.Add(New TermContribution With {
                    .Term = term,
                    .Idf = idf,
                    .Tf = tf,
                    .LengthFactor = lengthFactor,
                    .TfSaturation = tfSaturation,
                    .Contribution = contribution
                })
            Next

            Return result
        End Function

        ' ========================================================================
        ' IDF 计算
        ' ========================================================================

        ''' <summary>
        ''' 计算词项的 IDF。
        ''' Lucene 变体: log(1 + (N - n + 0.5) / (n + 0.5))
        ''' Okapi 变体:  log((N - n + 0.5) / (n + 0.5))
        ''' </summary>
        Private Function ComputeIdf(term As String) As Double
            Dim n As Integer = 0
            If _docFrequency.ContainsKey(term) Then
                n = _docFrequency(term)
            End If

            ' 词项不在任何文档中，IDF = 0
            If n = 0 Then Return 0.0

            Dim numerator As Double = CDbl(_totalDocs - n + 0.5)
            Dim denominator As Double = CDbl(n + 0.5)

            Select Case IdfMode
                Case IdfVariant.Lucene
                    ' log(1 + (N-n+0.5)/(n+0.5))，永远非负
                    Return std.Log(1.0 + numerator / denominator)
                Case IdfVariant.Okapi
                    ' log((N-n+0.5)/(n+0.5))，可能为负
                    Dim ratio As Double = numerator / denominator
                    If ratio <= 0 Then Return 0.0
                    Return std.Log(ratio)
                Case Else
                    Return std.Log(1.0 + numerator / denominator)
            End Select
        End Function

        ''' <summary>
        ''' 获取词项的 IDF（外部查询用）。
        ''' </summary>
        Public Function GetIdf(term As String) As Double
            term = term.ToLowerInvariant()
            If _idfDirty Then RefreshIdf()
            If _idfCache.ContainsKey(term) Then
                Return _idfCache(term)
            End If
            Return 0.0
        End Function

        ''' <summary>
        ''' 获取词项的文档频率 df。
        ''' </summary>
        Public Function GetDocumentFrequency(term As String) As Integer
            term = term.ToLowerInvariant()
            If _docFrequency.ContainsKey(term) Then
                Return _docFrequency(term)
            End If
            Return 0
        End Function

        ''' <summary>
        ''' 获取词项在指定文档中的词频 tf。
        ''' </summary>
        Public Function GetTermFrequency(term As String, docId As Integer) As Integer
            term = term.ToLowerInvariant()
            If _invertedIndex.ContainsKey(term) AndAlso
               _invertedIndex(term).ContainsKey(docId) Then
                Return _invertedIndex(term)(docId)
            End If
            Return 0
        End Function

        ''' <summary>
        ''' 获取指定文档的长度。
        ''' </summary>
        Public Function GetDocumentLength(docId As Integer) As Integer
            For i As Integer = 0 To _documents.Count - 1
                If _documents(i).Id = docId Then
                    Return _documents(i).Length
                End If
            Next
            Return 0
        End Function

        ''' <summary>
        ''' 获取文档原始文本。
        ''' </summary>
        Public Function GetDocumentText(docId As Integer) As String
            For i As Integer = 0 To _documents.Count - 1
                If _documents(i).Id = docId Then
                    Return _documents(i).RawText
                End If
            Next
            Return ""
        End Function

        ' ========================================================================
        ' 内置分词器
        ' ========================================================================

        ''' <summary>
        ''' 简单分词器：按空格和标点切分，转小写，去除空词条。
        ''' 适用于英文和已预分词的中文文本。
        ''' 实际使用时应替换为专业分词工具（如 jieba）。
        ''' </summary>
        Public Shared Function SimpleTokenizer(text As String) As String()
            If String.IsNullOrEmpty(text) Then Return New String() {}

            ' 按非字母数字字符切分
            Dim tokens As New List(Of String)
            Dim sb As New StringBuilder()

            For Each ch As Char In text
                If Char.IsLetterOrDigit(ch) Then
                    sb.Append(ch)
                Else
                    If sb.Length > 0 Then
                        tokens.Add(sb.ToString().ToLowerInvariant())
                        sb.Clear()
                    End If
                End If
            Next

            If sb.Length > 0 Then
                tokens.Add(sb.ToString().ToLowerInvariant())
            End If

            Return tokens.ToArray()
        End Function

        ' ========================================================================
        ' 统计信息
        ' ========================================================================

        ''' <summary>
        ''' 获取语料库的统计摘要。
        ''' </summary>
        Public Function GetStatistics() As String
            Dim sb As New StringBuilder()
            sb.AppendLine($"=== BM25 语料库统计 ===")
            sb.AppendLine($"  文档总数 (N):       {_totalDocs}")
            sb.AppendLine($"  平均文档长度 (avgdl): {_avgDocLength:F2}")
            sb.AppendLine($"  词汇表大小:         {_invertedIndex.Count}")
            sb.AppendLine($"  参数 k1:            {K1}")
            sb.AppendLine($"  参数 b:             {B}")
            sb.AppendLine($"  IDF 变体:           {IdfMode.ToString()}")

            ' 文档长度分布
            If _documents.Count > 0 Then
                Dim lengths As Integer() = _documents.Select(Function(d) d.Length).ToArray()
                Dim minLen As Integer = lengths.Min()
                Dim maxLen As Integer = lengths.Max()
                sb.AppendLine($"  文档长度范围:       [{minLen}, {maxLen}]")
            End If

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 获取所有文档的长度列表。
        ''' </summary>
        Public Function GetDocumentLengths() As List(Of (Integer, Integer))
            Dim result As New List(Of (Integer, Integer))
            For Each doc In _documents
                result.Add((doc.Id, doc.Length))
            Next
            Return result
        End Function

        ''' <summary>
        ''' 获取词汇表（所有已索引的词项）。
        ''' </summary>
        Public Function GetVocabulary() As List(Of String)
            Return New List(Of String)(_invertedIndex.Keys)
        End Function

        ''' <summary>
        ''' 清空索引，释放内存。
        ''' </summary>
        Public Sub Clear()
            _documents.Clear()
            _invertedIndex.Clear()
            _docFrequency.Clear()
            _idfCache.Clear()
            _totalDocs = 0
            _totalLength = 0
            _avgDocLength = 0.0
            _idfDirty = True
        End Sub

    End Class

End Namespace

