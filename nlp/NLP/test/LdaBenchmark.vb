Imports System.Diagnostics
Imports Microsoft.VisualBasic.Data.NLP.LDA

''' <summary>
''' 并行 gibbs LDA 的基准测试与主题一致性校验
''' </summary>
''' <remarks>
''' 用法（在 test 项目目录下）：
''' 
''' ```
''' dotnet run -c Release -- lda
''' ```
''' 
''' 基准测试会生成一份结构已知的合成语料（不依赖任何磁盘数据文件），然后分别以
''' 串行模式（workers = 1）和若干种并行度训练同一个模型，最后输出：
''' 
''' + 每种并行度相对串行的加速比；
''' + 并行模型与串行模型主题词分布的 Jaccard 重合度，用于确认并行没有把模型训坏；
''' + 两种模式的困惑度（perplexity），用于确认模型质量相当。
''' </remarks>
Module LdaBenchmark

    Private Sub Noop(message As Object)
        ' keep the sampler quiet during the benchmark
    End Sub

    ''' <summary>
    ''' 生成一份结构已知的合成语料
    ''' </summary>
    ''' <param name="seed">随机种子</param>
    ''' <param name="nDocs">文档数量</param>
    ''' <param name="avgLen">文档的平均长度</param>
    ''' <param name="vSize">词表大小</param>
    ''' <param name="nTopics">真实的主题数量</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 主题 t 的特征词是词表之中满足 ``w Mod nTopics = t`` 的那一组词语，每篇文档
    ''' 有 75% 的词语来自其所属主题的特征词块，其余 25% 为全词表上的均匀噪声。
    ''' 这样生成的语料主题结构清晰，可以让串并行两次独立的采样链都收敛到同一组主题。
    ''' </remarks>
    Private Function MakeCorpus(seed As Integer, nDocs As Integer, avgLen As Integer, vSize As Integer, nTopics As Integer) As Corpus
        Dim rnd As New Random(seed)
        Dim corpus As New Corpus()
        Dim blockSize As Integer = CInt(System.Math.Ceiling(vSize / nTopics))
        Dim half As Integer = avgLen \ 2

        For i As Integer = 0 To nDocs - 1
            Dim topic As Integer = rnd.Next(nTopics)
            Dim len As Integer = avgLen + rnd.Next(avgLen) - half

            If len < 1 Then
                len = 1
            End If

            Dim words As String() = New String(len - 1) {}

            For j As Integer = 0 To len - 1
                Dim w As Integer

                If rnd.NextDouble() < 0.75 Then
                    w = topic + nTopics * rnd.Next(blockSize)

                    If w >= vSize Then
                        w = rnd.Next(vSize)
                    End If
                Else
                    w = rnd.Next(vSize)
                End If

                words(j) = "w" & w
            Next

            Call corpus.addDocument(words)
        Next

        Return corpus
    End Function

    ''' <summary>
    ''' 构造一份退化语料：文档数量远少于工作线程数量，并且含有空文档
    ''' </summary>
    Private Function MakeDegenerateCorpus(nDocs As Integer, vSize As Integer, emptyDocs As Integer) As Corpus
        Dim rnd As New Random(20240927)
        Dim corpus As New Corpus()

        For i As Integer = 0 To nDocs - 1
            Dim len As Integer = 3 + rnd.Next(8)
            Dim words As String() = New String(len - 1) {}

            For j As Integer = 0 To len - 1
                words(j) = "w" & rnd.Next(vSize)
            Next

            Call corpus.addDocument(words)
        Next

        For i As Integer = 0 To emptyDocs - 1
            Call corpus.addDocument(New String() {})
        Next

        Return corpus
    End Function

    ''' <summary>
    ''' 边界场景的冒烟测试：只要不抛异常、计数守恒即视为通过
    ''' </summary>
    Private Sub RunDegenerateCase(iterations As Integer, workerList As Integer())
        Console.WriteLine()
        Console.WriteLine("=== degenerate corpus (documents < threads, empty documents) ===")

        Dim corpus As Corpus = MakeDegenerateCorpus(3, 500, 2)
        Dim docs As Integer()() = corpus.Document
        Dim tokens As Long = 0

        For Each d As Integer() In docs
            tokens += d.Length
        Next

        Console.WriteLine($"    corpus ready: {corpus.VocabularySize} unique terms, {docs.Length} documents, {tokens} tokens")

        For Each workers As Integer In workerList
            Dim ms As Double
            Dim sampler As LdaGibbsSampler = Train(corpus, docs, 5, iterations, workers, 20240927, 1, 1, ms)
            Dim assigned As Long = sampler.TopicTokenTotal
            Dim ok As String = If(assigned = tokens, "OK", "FAILED")

            Console.WriteLine($"    {workers,2} threads : {ms,8:F1} ms | assigned={assigned}/{tokens} | {ok}")
        Next
    End Sub

    ''' <summary>
    ''' 训练一个模型并返回其耗时（毫秒）
    ''' </summary>
    ''' <param name="repeats">
    ''' 重复训练的次数。取多次运行之中的最优耗时，这样可以压掉 JIT 预热与
    ''' 操作系统调度抖动带来的噪声——单次测量的波动在本基准上可达 2 倍。
    ''' </param>
    Private Function Train(corpus As Corpus,
                           docs As Integer()(),
                           nTopics As Integer,
                           iterations As Integer,
                           workers As Integer,
                           seed As Integer,
                           repeats As Integer,
                           syncInterval As Integer,
                           ByRef elapsed As Double) As LdaGibbsSampler

        Dim result As LdaGibbsSampler = Nothing

        elapsed = Double.MaxValue

        For r As Integer = 1 To repeats
            Dim sampler As New LdaGibbsSampler(docs, corpus.VocabularySize, AddressOf Noop)
            Dim watch As Stopwatch = Stopwatch.StartNew()

            Call sampler.configure(
                iterations:=iterations,
                burnIn:=iterations \ 5,
                thinInterval:=System.Math.Max(iterations \ 10, 1),
                sampleLag:=5,
                workers:=workers,
                seed:=seed,
                syncInterval:=syncInterval
            )
            Call sampler.gibbs(nTopics, 2.0, 0.5)

            Dim ms As Double = watch.Elapsed.TotalMilliseconds

            If ms < elapsed Then
                elapsed = ms
                result = sampler
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' 计算语料的 in-sample 困惑度
    ''' </summary>
    Private Function Perplexity(sampler As LdaGibbsSampler, docs As Integer()()) As Double
        Dim theta As Double()() = sampler.Theta
        Dim phi As Double()() = sampler.Phi
        Dim nTopics As Integer = phi.Length
        Dim logLik As Double = 0.0
        Dim tokens As Long = 0

        For m As Integer = 0 To docs.Length - 1
            For Each w As Integer In docs(m)
                Dim p As Double = 0.0

                For k As Integer = 0 To nTopics - 1
                    p += theta(m)(k) * phi(k)(w)
                Next

                logLik += If(p > 0, System.Math.Log(p), System.Math.Log(1.0E-12))
                tokens += 1
            Next
        Next

        If tokens = 0 Then
            Return Double.NaN
        End If

        Return System.Math.Exp(-logLik / tokens)
    End Function

    ''' <summary>
    ''' 取出每个主题概率最高的前 N 个词语
    ''' </summary>
    Private Function TopWordsAll(phi As Double()(), topN As Integer) As Integer()()
        Dim nTopics As Integer = phi.Length
        Dim out As Integer()() = New Integer(nTopics - 1)() {}

        For k As Integer = 0 To nTopics - 1
            out(k) = Enumerable.Range(0, phi(k).Length) _
                .OrderByDescending(Function(w) phi(k)(w)) _
                .Take(topN) _
                .ToArray()
        Next

        Return out
    End Function

    ''' <summary>
    ''' 计算两个模型主题词分布的平均重合度
    ''' </summary>
    ''' <returns>
    ''' 对参考模型（<paramref name="a"/>）的每一个主题，在被比较模型
    ''' （<paramref name="b"/>）之中寻找 Jaccard 相似度最高的主题，返回这些最佳
    ''' 匹配相似度的平均值。主题在两次采样之间可能发生排列置换，所以这里取最佳
    ''' 匹配而不是按下标一一对应。
    ''' </returns>
    Private Function TopicOverlap(a As Double()(), b As Double()(), topN As Integer) As Double
        Dim nTopics As Integer = a.Length
        Dim topA As Integer()() = TopWordsAll(a, topN)
        Dim topB As Integer()() = TopWordsAll(b, topN)
        Dim sum As Double = 0.0

        For i As Integer = 0 To nTopics - 1
            Dim setA As New HashSet(Of Integer)(topA(i))
            Dim best As Double = 0.0

            For j As Integer = 0 To topB.Length - 1
                Dim inter As Integer = 0

                For Each w As Integer In topB(j)
                    If setA.Contains(w) Then
                        inter += 1
                    End If
                Next

                Dim union As Integer = setA.Count + topB(j).Length - inter

                If union > 0 Then
                    Dim jaccard As Double = inter / union

                    If jaccard > best Then
                        best = jaccard
                    End If
                End If
            Next

            sum += best
        Next

        Return sum / nTopics
    End Function

    ''' <summary>
    ''' 主题的有效数量：重合度极低的主题视为与别的主题发生了坍缩
    ''' </summary>
    Private Function DistinctTopics(phi As Double()(), topN As Integer, threshold As Double) As Integer
        Dim top As Integer()() = TopWordsAll(phi, topN)
        Dim distinct As Integer = 0

        For i As Integer = 0 To top.Length - 1
            Dim setA As New HashSet(Of Integer)(top(i))
            Dim isDistinct As Boolean = True

            For j As Integer = 0 To top.Length - 1
                If i = j Then
                    Continue For
                End If

                Dim inter As Integer = 0

                For Each w As Integer In top(j)
                    If setA.Contains(w) Then
                        inter += 1
                    End If
                Next

                Dim union As Integer = setA.Count + top(j).Length - inter

                If union > 0 AndAlso inter / union > threshold Then
                    isDistinct = False
                    Exit For
                End If
            Next

            If isDistinct Then
                distinct += 1
            End If
        Next

        Return distinct
    End Function

    Private Sub RunScenario(name As String, nDocs As Integer, avgLen As Integer, vSize As Integer,
                            nTopics As Integer, iterations As Integer, seed As Integer,
                            workerList As Integer(), repeats As Integer, syncInterval As Integer)

        Console.WriteLine()
        Console.WriteLine($"=== {name} ===")
        Console.WriteLine($"    documents={nDocs}, avgLen={avgLen}, vocabulary={vSize}, topics={nTopics}, iterations={iterations}")

        Dim corpus As Corpus = MakeCorpus(seed, nDocs, avgLen, vSize, nTopics)
        Dim docs As Integer()() = corpus.Document
        Dim tokens As Long = 0

        For Each d As Integer() In docs
            tokens += d.Length
        Next

        Console.WriteLine($"    corpus ready: {corpus.VocabularySize} unique terms, {tokens} tokens")

        ' ---------- 串行基线 ----------
        Dim baseMs As Double
        Dim baseSampler As LdaGibbsSampler = Train(corpus, docs, nTopics, iterations, 1, seed, repeats, 1, baseMs)
        Dim basePhi As Double()() = baseSampler.Phi
        Dim basePpx As Double = Perplexity(baseSampler, docs)
        Dim baseDistinct As Integer = DistinctTopics(basePhi, 10, 0.7R)

        Console.WriteLine()
        Console.WriteLine($"    [baseline] sequential : {baseMs,10:F1} ms | perplexity={basePpx,10:F2} | distinct={baseDistinct}/{nTopics} | assigned={baseSampler.TopicTokenTotal}/{tokens}")

        ' ---------- 并行 ----------
        For Each workers As Integer In workerList
            Dim ms As Double
            Dim sampler As LdaGibbsSampler = Train(corpus, docs, nTopics, iterations, workers, seed, repeats, syncInterval, ms)
            Dim phi As Double()() = sampler.Phi
            Dim ppx As Double = Perplexity(sampler, docs)
            Dim overlap As Double = TopicOverlap(basePhi, phi, 10)
            Dim distinct As Integer = DistinctTopics(phi, 10, 0.7R)
            Dim speedup As Double = If(ms > 0, baseMs / ms, 0.0)
            Dim assigned As Long = sampler.TopicTokenTotal

            Console.WriteLine($"    [parallel ] {workers,2} threads : {ms,10:F1} ms | speedup={speedup,5:F2}x | perplexity={ppx,10:F2} | overlap={overlap,5:F3} | distinct={distinct}/{nTopics} | assigned={assigned}/{tokens}")
        Next
    End Sub

    ''' <summary>
    ''' 运行全部基准测试
    ''' </summary>
    ''' <param name="iterations">每种配置的采样迭代次数</param>
    Public Sub RunLdaBenchmark(Optional iterations As Integer = 200,
                               Optional repeats As Integer = 3,
                               Optional syncInterval As Integer = 1)

        Dim cpuCount As Integer = Environment.ProcessorCount
        Dim workerList As Integer() = {2, 4, 8, cpuCount} _
            .Where(Function(w) w > 1) _
            .Distinct() _
            .OrderBy(Function(w) w) _
            .ToArray()

        Console.WriteLine("Gibbs LDA parallel benchmark")
        Console.WriteLine($"CPU threads available : {cpuCount}")
        Console.WriteLine($"worker configurations : 1 (baseline), {String.Join(", ", workerList)}")
        Console.WriteLine($"sampling iterations   : {iterations}")
        Console.WriteLine($"repeats (best of)     : {repeats}")
        Console.WriteLine($"sync interval         : {syncInterval} sweeps")

        Call RunScenario("many short documents", 4000, 30, 2000, 10, iterations, 20240927, workerList, repeats, syncInterval)
        Call RunScenario("few long documents", 300, 500, 3000, 10, iterations, 20240927, workerList, repeats, syncInterval)
        Call RunScenario("large vocabulary", 2000, 60, 20000, 20, iterations, 20240927, workerList, repeats, syncInterval)
        Call RunDegenerateCase(iterations, workerList)

        Console.WriteLine()
        Console.WriteLine("done.")
    End Sub
End Module
