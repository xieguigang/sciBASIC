' ============================================================================
' Program.vb - BM25 算法 Demo
'
' 展示内容：
'   Demo 1: 用户描述的分步计算示例（3 篇文档，查询"苹果"）
'   Demo 2: 多词查询 + Top-K 检索
'   Demo 3: 参数调优对比（k1 / b 不同取值的效果）
'   Demo 4: IDF 变体对比（Lucene vs Okapi）
'   Demo 5: 中英文混合语料检索
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.Data.NLP.BM25

Module bm25test

    Private Const BAR As String = "══════════════════════════════════════════════════════════════════════════"
    Private Const THIN As String = "──────────────────────────────────────────────────────────────────────────"

    Function Main(args As String()) As Integer
        Console.OutputEncoding = Encoding.UTF8

        Console.WriteLine(BAR)
        Console.WriteLine("  BM25 (Best Matching 25) 相关性排序算法 Demo")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' ================================================================
        ' Demo 1: 分步计算示例（与用户描述的手动计算对照）
        ' ================================================================
        Demo1_StepByStep()

        ' ================================================================
        ' Demo 2: 多词查询 + Top-K
        ' ================================================================
        Demo2_MultiTermSearch()

        ' ================================================================
        ' Demo 3: 参数调优对比
        ' ================================================================
        Demo3_ParameterTuning()

        ' ================================================================
        ' Demo 4: IDF 变体对比
        ' ================================================================
        Demo4_IdfVariants()

        ' ================================================================
        ' Demo 5: 中英文混合语料
        ' ================================================================
        Demo5_MixedCorpus()

        Console.WriteLine(BAR)
        Console.WriteLine("  全部 Demo 执行完毕")
        Console.WriteLine(BAR)

        Return 0
    End Function

    ' ========================================================================
    ' Demo 1: 分步计算示例
    ' ========================================================================
    Private Sub Demo1_StepByStep()
        Console.WriteLine(BAR)
        Console.WriteLine("  Demo 1: 分步计算示例（与用户描述的手动计算对照）")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' 语料（3 篇文档，已分词）
        Dim docs As String() = {
            "苹果 是 一种 美味 的 水果",   ' 长度 5 (6 tokens, 但分词后 "苹果 是 一种 美味 的 水果" = 6 tokens)
            "我 喜欢 吃 苹果 和 香蕉",      ' 长度 6
            "苹果 公司 发布 了 最新 的 智能 手机"  ' 长度 7
        }

        Console.WriteLine("  语料:")
        For i As Integer = 0 To docs.Length - 1
            Console.WriteLine($"    D{i + 1}: ""{docs(i)}""")
        Next
        Console.WriteLine()

        ' 构建引擎
        Dim engine As New BM25Engine(k1:=1.2, b:=0.75, idfMode:=IdfVariant.Lucene)

        ' 逐词添加（使用空格分词）
        For i As Integer = 0 To docs.Length - 1
            Dim tokens As String() = docs(i).Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            engine.AddDocument(i + 1, tokens, docs(i))
        Next
        engine.BuildIndex()

        ' 显示统计量
        Console.WriteLine("  统计量:")
        Console.WriteLine($"    N = {engine.DocumentCount}")
        Console.WriteLine($"    avgdl = {engine.AverageDocumentLength:F2}")
        Console.WriteLine()

        ' 查询: "苹果"
        Dim query As String() = {"苹果"}
        Console.WriteLine("  查询: ""苹果""")
        Console.WriteLine($"    k1 = 1.2, b = 0.75")
        Console.WriteLine()

        ' 手动计算 IDF
        Dim n_apple As Integer = engine.GetDocumentFrequency("苹果")
        Dim N As Integer = engine.DocumentCount
        Dim idf_apple As Double = engine.GetIdf("苹果")

        Console.WriteLine("  Step 1: 计算 IDF")
        Console.WriteLine($"    ""苹果"" 出现在 {n_apple} 篇文档中, N = {N}")
        Console.WriteLine($"    IDF = log(1 + (N - n + 0.5) / (n + 0.5))")
        Console.WriteLine($"        = log(1 + ({N} - {n_apple} + 0.5) / ({n_apple} + 0.5))")
        Console.WriteLine($"        = log(1 + {(N - n_apple + 0.5) / (n_apple + 0.5):F4})")
        Console.WriteLine($"        = {idf_apple:F6}")
        Console.WriteLine()

        ' 逐文档计算 TF 部分
        Console.WriteLine("  Step 2: 逐文档计算 TF 部分")
        Console.WriteLine($"    公式: f·(k1+1) / (f + k1·(1 - b + b·|D|/avgdl))")
        Console.WriteLine()

        Dim k1 As Double = 1.2
        Dim b As Double = 0.75
        Dim avgdl As Double = engine.AverageDocumentLength

        For docId As Integer = 1 To 3
            Dim tf As Integer = engine.GetTermFrequency("苹果", docId)
            Dim docLen As Integer = engine.GetDocumentLength(docId)
            Dim lengthFactor As Double = 1.0 - b + b * (docLen / avgdl)
            Dim tfSat As Double = (tf * (k1 + 1.0)) / (tf + k1 * lengthFactor)
            Dim score As Double = idf_apple * tfSat

            Console.WriteLine($"    D{docId} (|D|={docLen}, f={tf}):")
            Console.WriteLine($"      长度因子 = 1 - {b} + {b} × {docLen}/{avgdl:F2} = {lengthFactor:F4}")
            Console.WriteLine($"      TF 部分  = {tf}×{k1 + 1:F1} / ({tf} + {k1}×{lengthFactor:F4}) = {tfSat:F6}")
            Console.WriteLine($"      得分     = {idf_apple:F6} × {tfSat:F6} = {score:F6}")
            Console.WriteLine()
        Next

        ' 执行检索
        Dim results = engine.Search(query)

        Console.WriteLine("  Step 3: 最终排序结果")
        Console.WriteLine(THIN)
        Console.WriteLine($"  {"排名",-4} {"文档",-6} {"得分",12} {"文档内容"}")
        Console.WriteLine(THIN)
        For i As Integer = 0 To results.Count - 1
            Dim r = results(i)
            Console.WriteLine($"  {i + 1,4} D{r.DocId,-5} {r.Score,12:F6} {engine.GetDocumentText(r.DocId)}")
        Next
        Console.WriteLine(THIN)
        Console.WriteLine("  结论: 三篇文档 TF 相同，但最短的 D1 得分最高（长度归一化）")
        Console.WriteLine()
    End Sub

    ' ========================================================================
    ' Demo 2: 多词查询 + Top-K
    ' ========================================================================
    Private Sub Demo2_MultiTermSearch()
        Console.WriteLine(BAR)
        Console.WriteLine("  Demo 2: 多词查询 + Top-K + 贡献明细")
        Console.WriteLine(BAR)
        Console.WriteLine()

        Dim engine As New BM25Engine(k1:=1.2, b:=0.75)

        ' 构建一个小型科技新闻语料
        Dim corpus As String() = {
            "BM25 is a ranking function used by search engines to estimate the relevance of documents",
            "Elasticsearch uses BM25 as its default similarity algorithm for text search",
            "Vector search and BM25 sparse retrieval can be combined for hybrid search in RAG",
            "The TF-IDF algorithm has limitations that BM25 addresses with term frequency saturation",
            "Information retrieval systems use inverted indexes to quickly find documents containing query terms",
            "BM25 was developed by Stephen Robertson and Karen Sparck Jones in the Okapi system",
            "Deep learning models like BERT can capture semantic meaning that BM25 cannot",
            "Hybrid search combining dense vectors and BM25 achieves near perfect retrieval accuracy",
            "The k1 parameter controls term frequency saturation in BM25 scoring",
            "Document length normalization is a key improvement of BM25 over TF-IDF"
        }

        Console.WriteLine("  语料 (科技文档):")
        For i As Integer = 0 To corpus.Length - 1
            Console.WriteLine($"    D{i + 1,2}: {corpus(i).Substring(0, Math.Min(70, corpus(i).Length))}...")
        Next
        Console.WriteLine()

        ' 添加文档（使用内置英文分词器）
        For i As Integer = 0 To corpus.Length - 1
            engine.AddDocument(i + 1, corpus(i))
        Next
        engine.BuildIndex()

        Console.WriteLine(engine.GetStatistics())
        Console.WriteLine()

        ' 多词查询
        Dim query As String = "BM25 search retrieval"
        Console.WriteLine($"  查询: ""{query}""")
        Console.WriteLine($"  分词: [{String.Join(", ", BM25Engine.SimpleTokenizer(query))}]")
        Console.WriteLine()

        Dim results = engine.Search(query, topK:=5)

        Console.WriteLine($"  Top-5 检索结果:")
        Console.WriteLine(THIN)
        Console.WriteLine($"  {"排名",-4} {"文档",-6} {"得分",12}  文档摘要")
        Console.WriteLine(THIN)

        For i As Integer = 0 To results.Count - 1
            Dim r = results(i)
            Dim text As String = engine.GetDocumentText(r.DocId)
            Dim preview As String = If(text.Length > 50, text.Substring(0, 50) & "...", text)
            Console.WriteLine($"  {i + 1,4} D{r.DocId,-5} {r.Score,12:F6}  {preview}")
        Next
        Console.WriteLine(THIN)
        Console.WriteLine()

        ' 展示第一条结果的贡献明细
        If results.Count > 0 Then
            Dim top = results(0)
            Console.WriteLine($"  Top-1 结果 (D{top.DocId}) 贡献明细:")
            Console.WriteLine(THIN)
            Console.WriteLine($"  {"查询词",-12} {"IDF",10} {"TF",4} {"长度因子",10} {"TF饱和",10} {"贡献分",12}")
            Console.WriteLine(THIN)
            For Each tc In top.TermContributions
                Console.WriteLine($"  {tc.Term,-12} {tc.Idf,10:F6} {tc.Tf,4} {tc.LengthFactor,10:F4} {tc.TfSaturation,10:F6} {tc.Contribution,12:F6}")
            Next
            Console.WriteLine(THIN)
            Console.WriteLine($"  合计得分: {top.Score:F6}")
            Console.WriteLine()
        End If
    End Sub

    ' ========================================================================
    ' Demo 3: 参数调优对比
    ' ========================================================================
    Private Sub Demo3_ParameterTuning()
        Console.WriteLine(BAR)
        Console.WriteLine("  Demo 3: 参数调优对比 (k1 / b 不同取值)")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' 构建语料：文档长度差异明显
        Dim corpus As String() = {
            "machine learning",                                           ' 短
            "machine learning is a subset of artificial intelligence",   ' 中
            "machine learning is a subset of artificial intelligence that enables systems to learn and improve from experience without being explicitly programmed machine learning algorithms build models based on training data"
        }
        ' 长文档，"machine learning" 出现多次
        Dim longText As String = "machine machine machine machine machine machine learning learning learning learning " &
                                 "deep learning neural networks natural language processing"
        corpus = New String() {
            "machine learning",
            "machine learning is a subset of artificial intelligence",
            longText
        }

        Console.WriteLine("  语料（长度差异大）:")
        For i As Integer = 0 To corpus.Length - 1
            Console.WriteLine($"    D{i + 1} (len={BM25Engine.SimpleTokenizer(corpus(i)).Length}): {corpus(i).Substring(0, Math.Min(60, corpus(i).Length))}...")
        Next
        Console.WriteLine()

        Dim query As String() = {"machine", "learning"}
        Console.WriteLine($"  查询: ""machine learning""")
        Console.WriteLine()

        ' 测试不同参数组合
        Dim paramSets As (k1 As Double, b As Double, label As String)() = {
            (0.0, 0.75, "k1=0    (忽略TF)"),
            (1.2, 0.0, "b=0     (不归一化)"),
            (1.2, 0.75, "默认    (k1=1.2 b=0.75)"),
            (1.2, 1.0, "b=1     (完全归一化)"),
            (2.0, 0.75, "k1=2.0  (慢饱和)"),
            (0.5, 0.3, "k1=0.5  b=0.3  (短文本场景)")
        }

        Console.WriteLine($"  {"参数组合",-30} {"D1",12} {"D2",12} {"D3",12} {"排名"}")
        Console.WriteLine(THIN)

        For Each ps In paramSets
            Dim engine As New BM25Engine(k1:=ps.k1, b:=ps.b)
            For i As Integer = 0 To corpus.Length - 1
                engine.AddDocument(i + 1, corpus(i))
            Next
            engine.BuildIndex()

            Dim results = engine.Search(query)
            Dim s1 As Double = 0, s2 As Double = 0, s3 As Double = 0
            For Each r In results
                Select Case r.DocId
                    Case 1 : s1 = r.Score
                    Case 2 : s2 = r.Score
                    Case 3 : s3 = r.Score
                End Select
            Next

            ' 排序
            Dim ranking As New List(Of (Integer, Double)) From {(1, s1), (2, s2), (3, s3)}
            ranking.Sort(Function(a, b) b.Item2.CompareTo(a.Item2))
            Dim rankStr As String = String.Join(">", ranking.Select(Function(r) $"D{r.Item1}"))

            Console.WriteLine($"  {ps.label,-30} {s1,12:F6} {s2,12:F6} {s3,12:F6} {rankStr}")
        Next

        Console.WriteLine(THIN)
        Console.WriteLine("  分析:")
        Console.WriteLine("    · k1=0:   三篇文档得分相同（完全忽略 TF 差异）")
        Console.WriteLine("    · b=0:    长文档 D3 因高频得高分（无长度惩罚）")
        Console.WriteLine("    · b=1:    长文档 D3 被强力压缩（长度归一化最大化）")
        Console.WriteLine("    · k1=2.0: 饱和点延后，D3 的高频词获得更多加权")
        Console.WriteLine()
    End Sub

    ' ========================================================================
    ' Demo 4: IDF 变体对比
    ' ========================================================================
    Private Sub Demo4_IdfVariants()
        Console.WriteLine(BAR)
        Console.WriteLine("  Demo 4: IDF 变体对比 (Lucene vs Okapi)")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' 构建语料：一个词出现在超过半数文档中（测试负 IDF）
        Dim corpus As String() = {
            "common rare wordA",
            "common rare wordB",
            "common wordA wordB",
            "common only here"
        }

        Console.WriteLine("  语料:")
        For i As Integer = 0 To corpus.Length - 1
            Console.WriteLine($"    D{i + 1}: ""{corpus(i)}""")
        Next
        Console.WriteLine()

        Dim query As String() = {"common", "rare", "worda", "wordb"}

        ' Lucene 变体
        Console.WriteLine("  IDF 对比:")
        Console.WriteLine(THIN)
        Console.WriteLine($"  {"词项",-10} {"df",4} {"N",4} {"Lucene IDF",14} {"Okapi IDF",14} {"差异"}")
        Console.WriteLine(THIN)

        For Each term In query
            Dim luceneEngine As New BM25Engine(k1:=1.2, b:=0.75, idfMode:=IdfVariant.Lucene)
            For i As Integer = 0 To corpus.Length - 1
                luceneEngine.AddDocument(i + 1, corpus(i))
            Next
            luceneEngine.BuildIndex()

            Dim okapiEngine As New BM25Engine(k1:=1.2, b:=0.75, idfMode:=IdfVariant.Okapi)
            For i As Integer = 0 To corpus.Length - 1
                okapiEngine.AddDocument(i + 1, corpus(i))
            Next
            okapiEngine.BuildIndex()

            Dim df As Integer = luceneEngine.GetDocumentFrequency(term)
            Dim luceneIdf As Double = luceneEngine.GetIdf(term)
            Dim okapiIdf As Double = okapiEngine.GetIdf(term)
            Dim diff As String = ""
            If okapiIdf < 0 Then diff = "← Okapi 产生负值!"
            If okapiIdf = 0 AndAlso luceneIdf > 0 Then diff = "← Okapi 被截断为0"

            Console.WriteLine($"  {term,-10} {df,4} {luceneEngine.DocumentCount,4} {luceneIdf,14:F6} {okapiIdf,14:F6} {diff}")
        Next
        Console.WriteLine(THIN)
        Console.WriteLine()
        Console.WriteLine("  分析:")
        Console.WriteLine("    · 'common' 出现在 4/4 文档中（超过半数）")
        Console.WriteLine("    · Lucene 变体: log(1+ratio) 永远非负，'common' 的 IDF 接近 0 但不为负")
        Console.WriteLine("    · Okapi 变体: log(ratio) 会产生负 IDF，代码中截断为 0 避免负分")
        Console.WriteLine("    · 工程实践中推荐 Lucene 变体（Elasticsearch/Lucene 默认）")
        Console.WriteLine()
    End Sub

    ' ========================================================================
    ' Demo 5: 中英文混合语料
    ' ========================================================================
    Private Sub Demo5_MixedCorpus()
        Console.WriteLine(BAR)
        Console.WriteLine("  Demo 5: 中英文混合语料检索（已预分词）")
        Console.WriteLine(BAR)
        Console.WriteLine()

        ' 预分词的中文语料（空格分隔）
        Dim corpus As String() = {
            "苹果 是 一种 美味 的 水果 富含 维生素",
            "苹果 公司 发布 了 最新 的 iPhone 智能 手机",
            "我 喜欢 吃 苹果 香蕉 橙子 等 水果",
            "iPhone 是 苹果 公司 的 旗舰 产品",
            "水果 中 的 维生素C 对 健康 很 重要",
            "苹果 股价 今年 上涨 了 百分之二十",
            "香蕉 含有 丰富 的 钾 元素 和 维生素",
            "智能 手机 市场 竞争 激烈 苹果 三星 华为"
        }

        Console.WriteLine("  语料:")
        For i As Integer = 0 To corpus.Length - 1
            Console.WriteLine($"    D{i + 1}: {corpus(i)}")
        Next
        Console.WriteLine()

        ' 构建引擎（使用空格分词）
        Dim engine As New BM25Engine(k1:=1.2, b:=0.75)
        For i As Integer = 0 To corpus.Length - 1
            Dim tokens As String() = corpus(i).Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            engine.AddDocument(i + 1, tokens, corpus(i))
        Next
        engine.BuildIndex()

        Console.WriteLine(engine.GetStatistics())
        Console.WriteLine()

        ' 多组查询
        Dim queries As String() = {"苹果", "苹果 水果", "苹果 手机", "维生素 健康"}

        For Each q As String In queries
            Dim tokens As String() = q.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
            Console.WriteLine($"  查询: ""{q}""  [词: {String.Join(", ", tokens)}]")
            Console.WriteLine(THIN)

            Dim results = engine.Search(tokens, topK:=4)
            If results.Count = 0 Then
                Console.WriteLine("    无匹配结果")
            Else
                For i As Integer = 0 To results.Count - 1
                    Dim r = results(i)
                    Console.WriteLine($"    #{i + 1} D{r.DocId}  Score={r.Score:F6}  | {engine.GetDocumentText(r.DocId)}")
                Next
            End If
            Console.WriteLine()
        Next

        ' 分析词项 IDF 分布
        Console.WriteLine("  词汇表 IDF 分布:")
        Console.WriteLine(THIN)
        Console.WriteLine($"  {"词项",-10} {"df",4} {"IDF",12}")
        Console.WriteLine(THIN)

        Dim vocab As List(Of String) = engine.GetVocabulary()
        ' 按 IDF 降序排列
        Dim idfList As New List(Of (String, Integer, Double))
        For Each term In vocab
            idfList.Add((term, engine.GetDocumentFrequency(term), engine.GetIdf(term)))
        Next
        idfList.Sort(Function(a, b) b.Item3.CompareTo(a.Item3))

        For Each item In idfList
            Console.WriteLine($"  {item.Item1,-10} {item.Item2,4} {item.Item3,12:F6}")
        Next
        Console.WriteLine(THIN)
        Console.WriteLine("  分析: '苹果' 出现在 6/8 文档中，IDF 很低（区分度低）")
        Console.WriteLine("         '旗舰' 仅在 1/8 文档中，IDF 最高（区分度最高）")
        Console.WriteLine()
    End Sub

End Module
