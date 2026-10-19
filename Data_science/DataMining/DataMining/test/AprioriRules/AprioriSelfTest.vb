Imports System
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Impl

''' <summary>
''' Apriori 优化前后的等价性对拍。
''' 
''' 每个用例都会同时跑一遍 <see cref="Apriori.GetAssociateRulesReference"/>（原始的分步实现）
''' 与 <see cref="Apriori.GetAssociateRules"/>（新引擎），然后逐项比对四组结果：
''' 频繁项集（键 + 支持度）、闭项集（键 + 父集）、极大项集、强规则（X/Y + 置信度 + 支持度）。
''' </summary>
Module AprioriSelfTest

    Private Structure CaseResult
        Public Name As String
        Public Frequent As Integer
        Public Closed As Integer
        Public Maximal As Integer
        Public Rules As Integer
        Public Ok As Boolean
    End Structure

    ''' <summary>
    ''' 执行全部对拍用例
    ''' </summary>
    ''' <param name="failures">失败信息汇总（沿用 NumericTableTests 的风格）</param>
    Friend Sub Run(failures As List(Of String))
        Dim results As New List(Of CaseResult)()

        ' 固定用例
        Call RunCase(failures, results, "readme-5x5", ReadmeTransactions(), 0.4, 0.8)
        Call RunCase(failures, results, "test2-7x5", Test2Transactions(), 3 / 7, 5 / 7)
        Call RunCase(failures, results, "quick-4x5", QuickTransactions(), 0, 60)
        ' 边界：含空事务与重复项的事务
        Call RunCase(failures, results, "edge-empty-and-dup", EdgeTransactions(), 0.25, 0.4)
        ' 边界：事务之间完全没有交集
        Call RunCase(failures, results, "edge-disjoint", DisjointTransactions(), 0.5, 0.5)
        ' 边界：支持度阈值大到没有任何频繁项
        Call RunCase(failures, results, "edge-no-frequent", ReadmeTransactions(), 2, 2)
        ' 对外 API（AnalysisTransactions）端到端冒烟
        Call CheckPublicApi(failures, results, "api-readme", ReadmeTransactions(), 0.4, 0.8)

        ' 随机用例：项的流行度服从 Zipf 分布，否则所有项的支持度都差不多，挖不出结构
        For Each seed As Integer In {1001, 1002, 1003}
            Call RunRandom(failures, results, seed, 40, 10, 4, 0.3, 0.5)
            Call RunRandom(failures, results, seed, 60, 12, 5, 0.25, 0.4)
            Call RunRandom(failures, results, seed, 60, 20, 6, 0.15, 0.2)
            Call RunRandom(failures, results, seed, 150, 30, 8, 0.2, 0)
            Call RunRandom(failures, results, seed, 150, 30, 8, 0.08, 0.6)
            Call RunRandom(failures, results, seed, 250, 60, 10, 0.25, 0.4)
        Next

        Dim passed As Integer = 0

        For Each r As CaseResult In results
            If r.Ok Then
                passed += 1
            End If

            Dim flag As String = If(r.Ok, "ok  ", "FAIL")

            Console.WriteLine($"  [{flag}] {r.Name,-22} frequent={r.Frequent,5}  closed={r.Closed,5}  maximal={r.Maximal,4}  rules={r.Rules,6}")
        Next

        Console.WriteLine($"apriori equivalence: {passed}/{results.Count} cases passed.")
    End Sub

    Private Sub RunCase(failures As List(Of String), results As List(Of CaseResult),
                        name As String, trans As Transaction(),
                        minSupport As Double, minConfidence As Double)

        Dim before As Integer = failures.Count
        Dim expected As Output = RunReference(trans, minSupport, minConfidence)
        Dim actual As Output = RunEngine(trans, minSupport, minConfidence)
        Dim fallback As Output = RunEngine(trans, minSupport, minConfidence, bitIndexBudgetMB:=0)

        CompareOutputs(failures, name, expected, actual)
        ' 内存预算置 0 会强制走横向归并计数的降级路径，结果必须一致
        CompareOutputs(failures, name & "/fallback", expected, fallback)

        results.Add(New CaseResult With {
            .Name = name,
            .Frequent = actual.FrequentItems.Count,
            .Closed = actual.ClosedItemSets.Count,
            .Maximal = actual.MaximalItemSets.Count,
            .Rules = actual.StrongRules.Count,
            .Ok = failures.Count = before
        })
    End Sub

    ''' <summary>
    ''' 对外 API 冒烟：<see cref="AprioriExport.AnalysisTransactions(IEnumerable(Of Transaction), Double, Double, Integer, AprioriPredictions)"/>
    ''' 会额外做 minlen 过滤与按置信度排序，这里校验它跑出来的结果与引擎一致
    ''' </summary>
    Private Sub CheckPublicApi(failures As List(Of String), results As List(Of CaseResult),
                               name As String, trans As Transaction(),
                               minSupport As Double, minConfidence As Double)

        Dim before As Integer = failures.Count
        Dim api As Output = Nothing

        Call Quietly(Sub()
                         api = trans.AnalysisTransactions(minSupport, minConfidence, minlen:=2)
                     End Sub)

        If api.TransactionSize <> trans.Length Then
            Call failures.Add($"{name}: api transaction size {api.TransactionSize} <> {trans.Length}")
        End If

        Dim engine As Output = RunEngine(trans, minSupport, minConfidence)
        Dim expect As New Dictionary(Of String, String)()

        For Each r As Rule In engine.StrongRules
            If r.length >= 2 Then
                expect(RuleKeyOf(r)) = r.Confidence.ToString("R")
            End If
        Next

        Dim got As New Dictionary(Of String, String)()

        For Each r As Rule In api.StrongRules
            got(RuleKeyOf(r)) = r.Confidence.ToString("R")
        Next

        If expect.Count <> got.Count Then
            Call failures.Add($"{name}: api rule count {got.Count} <> {expect.Count}")
        End If

        For Each kv As KeyValuePair(Of String, String) In expect
            If Not got.ContainsKey(kv.Key) Then
                Call failures.Add($"{name}: api missing rule [{kv.Key}]")
            End If
        Next

        ' API 出口按置信度降序排列
        For i As Integer = 1 To api.StrongRules.Count - 1
            If api.StrongRules(i - 1).Confidence < api.StrongRules(i).Confidence Then
                Call failures.Add($"{name}: api rules are not sorted by confidence descending")
                Exit For
            End If
        Next

        results.Add(New CaseResult With {
            .Name = name,
            .Frequent = api.FrequentItems.Count,
            .Closed = api.ClosedItemSets.Count,
            .Maximal = api.MaximalItemSets.Count,
            .Rules = api.StrongRules.Count,
            .Ok = failures.Count = before
        })
    End Sub

    Private Sub RunRandom(failures As List(Of String), results As List(Of CaseResult),
                          seed As Integer, n As Integer, d As Integer, avgLen As Integer,
                          minSupport As Double, minConfidence As Double)

        Dim trans As Transaction() = RandomTransactions(seed, n, d, avgLen)

        Call RunCase(failures, results, $"rnd-{n}x{d}L{avgLen}s{minSupport}c{minConfidence}#{seed}",
                     trans, minSupport, minConfidence)
    End Sub

    Private Function RunEngine(trans As Transaction(), minSupport As Double, minConfidence As Double, Optional bitIndexBudgetMB As Integer = 512) As Output
        Dim encoding As New Encoding(trans.AllItems)
        Dim itemSets As ItemSet() = encoding.TransactionEncoding(trans).ToArray()
        Dim out As Output = Nothing

        ' 两侧都会往控制台打进度与 tqdm 进度条，对拍时全部静音，只保留结果汇总
        Call Quietly(Sub()
                         out = Apriori.GetAssociateRules(minSupport, minConfidence, encoding.AllItems, itemSets, bitIndexBudgetMB:=bitIndexBudgetMB)
                     End Sub)

        Return out
    End Function

    Private Function RunReference(trans As Transaction(), minSupport As Double, minConfidence As Double) As Output
        Dim encoding As New Encoding(trans.AllItems)
        Dim itemSets As ItemSet() = encoding.TransactionEncoding(trans).ToArray()
        Dim out As Output = Nothing

        Call Quietly(Sub()
                         out = Apriori.GetAssociateRulesReference(minSupport, minConfidence, encoding.AllItems, itemSets)
                     End Sub)

        Return out
    End Function

    ''' <summary>
    ''' 静音执行：屏蔽被调用者往控制台输出的进度信息
    ''' </summary>
    Friend Sub Quietly(work As Action)
        Dim savedOut As IO.TextWriter = Console.Out
        Dim savedErr As IO.TextWriter = Console.Error

        Try
            Console.SetOut(NullOutput.Instance)
            Console.SetError(NullOutput.Instance)
            Call work()
        Finally
            Console.SetOut(savedOut)
            Console.SetError(savedErr)
        End Try
    End Sub

    Private Class NullOutput : Inherits IO.TextWriter

        Public Shared ReadOnly Instance As New NullOutput()

        Public Overrides ReadOnly Property Encoding As System.Text.Encoding
            Get
                Return System.Text.Encoding.UTF8
            End Get
        End Property

        Public Overrides Sub Write(value As Char)
            ' 丢弃
        End Sub
    End Class

    ''' <summary>
    ''' README 中的经典例子：5 个事务，min sup = 40%，min conf = 80%
    ''' </summary>
    ''' <remarks>
    ''' 期望的频繁项集恰好是 README 里列出的
    ''' {A} {B} {C} {E} {A C} {B C} {B E} {C E} {B C E}
    ''' </remarks>
    Friend Function ReadmeTransactions() As Transaction()
        Return {
            New Transaction("T1", {"A", "C", "D"}),
            New Transaction("T2", {"B", "C", "E"}),
            New Transaction("T3", {"A", "B", "C", "E"}),
            New Transaction("T4", {"B", "E"}),
            New Transaction("T5", {"B", "E"})
        }
    End Function

    ''' <summary>
    ''' 原有 test2 用例里的 7 事务数据集
    ''' </summary>
    Private Function Test2Transactions() As Transaction()
        Return {
            New Transaction("ABC"),
            New Transaction("ABD"),
            New Transaction("ACD"),
            New Transaction("ABCE"),
            New Transaction("ACE"),
            New Transaction("BDE"),
            New Transaction("ABCD")
        }
    End Function

    ''' <summary>
    ''' 原有 BasicQuickTest 用例：min support = 0（所有项集都算频繁），min confidence = 60（不可能达标）
    ''' </summary>
    Private Function QuickTransactions() As Transaction()
        Return {
            New Transaction("ACD", {"A", "C", "D"}),
            New Transaction("BCE", {"B", "C", "E"}),
            New Transaction("ABCE", {"A", "B", "C", "E"}),
            New Transaction("BE", {"B", "E"})
        }
    End Function

    ''' <summary>
    ''' 空事务与事务内重复项
    ''' </summary>
    Friend Function EdgeTransactions() As Transaction()
        Return {
            New Transaction("T1", {"A", "A", "B", "C"}),
            New Transaction("T2", {"B", "B", "C", "D"}),
            New Transaction("T3", {"A", "C", "D", "E"}),
            New Transaction("T4", New String() {}),
            New Transaction("T5", {"B", "C", "E", "E"}),
            New Transaction("T6", {"A", "B", "C", "E"}),
            New Transaction("T7", {"C", "D", "E", "E"})
        }
    End Function

    ''' <summary>
    ''' 事务之间没有任何交集：除了空集以外什么也挖不出来
    ''' </summary>
    Private Function DisjointTransactions() As Transaction()
        Return {
            New Transaction("T1", {"A", "B"}),
            New Transaction("T2", {"C", "D"}),
            New Transaction("T3", {"E", "F"}),
            New Transaction("T4", {"G", "H"})
        }
    End Function

    ''' <summary>
    ''' 生成随机事务数据集（固定种子，结果可复现）
    ''' </summary>
    ''' <param name="clusters">
    ''' 大于 0 时按「簇相关」方式生成：项被均分到若干簇，每个事务只从少数几个簇里取项。
    ''' 这样才会产生大量真正频繁的项集；纯随机数据几乎挖不出任何结构。
    ''' </param>
    Friend Function RandomTransactions(seed As Integer, n As Integer, d As Integer, avgLen As Integer, Optional clusters As Integer = 0) As Transaction()
        Dim rand As New Random(seed)
        ' 项的流行度服从 Zipf 分布：热门项出现在大量事务里，冷门项只有零星几个
        Dim cum As Double() = ZipfCumulative(d)
        Dim clusterCount As Integer = clusters
        Dim clusterSize As Integer = If(clusterCount > 0, System.Math.Max(1, d \ clusterCount), 0)
        Dim cumInCluster As Double() = If(clusterCount > 0, ZipfCumulative(clusterSize), Nothing)
        Dim trans As Transaction() = New Transaction(n - 1) {}

        For t As Integer = 0 To n - 1
            Dim len As Integer = 1 + rand.Next(avgLen * 2)
            Dim items As New List(Of String)()
            Dim picked As Integer() = Nothing

            If clusterCount > 0 Then
                Dim k As Integer = 1 + rand.Next(3)
                Dim chosen As New List(Of Integer)()

                For i As Integer = 0 To k - 1
                    Dim g As Integer = rand.Next(clusterCount)

                    If Not chosen.Contains(g) Then
                        Call chosen.Add(g)
                    End If
                Next

                picked = chosen.ToArray()
            End If

            For s As Integer = 0 To len - 1
                Dim index As Integer

                If clusterCount > 0 Then
                    Dim g As Integer = picked(rand.Next(picked.Length))

                    index = g * clusterSize + PickItem(rand, cumInCluster)

                    If index >= d Then
                        index = d - 1
                    End If
                Else
                    index = PickItem(rand, cum)
                End If

                Call items.Add("i" & index.ToString("0000"))
            Next

            trans(t) = New Transaction("T" & t.ToString(), items)
        Next

        Return trans
    End Function

    ''' <summary>
    ''' Zipf(s=0.8) 的累积分布
    ''' </summary>
    Private Function ZipfCumulative(size As Integer) As Double()
        Dim cum As Double() = New Double(size - 1) {}
        Dim total As Double = 0

        For i As Integer = 0 To size - 1
            total += 1.0 / System.Math.Pow(i + 1, 0.8)
        Next

        Dim acc As Double = 0

        For i As Integer = 0 To size - 1
            acc += 1.0 / System.Math.Pow(i + 1, 0.8)
            cum(i) = acc / total
        Next

        Return cum
    End Function

    ''' <summary>
    ''' 按 Zipf 累积分布抽取一个项（二分查找）
    ''' </summary>
    Private Function PickItem(rand As Random, cum As Double()) As Integer
        Dim u As Double = rand.NextDouble()
        Dim lo As Integer = 0
        Dim hi As Integer = cum.Length - 1

        While lo < hi
            Dim mid As Integer = (lo + hi) \ 2

            If cum(mid) < u Then
                lo = mid + 1
            Else
                hi = mid
            End If
        End While

        Return lo
    End Function

    Private Sub CompareOutputs(failures As List(Of String), name As String, expected As Output, actual As Output)
        If expected.TransactionSize <> actual.TransactionSize Then
            Call failures.Add($"{name}: transaction size {actual.TransactionSize} <> {expected.TransactionSize}")
        End If

        ' 1. 频繁项集：键 + 支持度
        If expected.FrequentItems.Count <> actual.FrequentItems.Count Then
            Call failures.Add($"{name}: frequent item count {actual.FrequentItems.Count} <> {expected.FrequentItems.Count}")
        End If

        For Each kv As KeyValuePair(Of ItemSet, TransactionTokensItem) In expected.FrequentItems
            Dim token As TransactionTokensItem = Nothing

            If Not actual.FrequentItems.TryGetValue(kv.Key, token) Then
                Call failures.Add($"{name}: missing frequent item {KeyOf(kv.Key)} (support={kv.Value.Support})")
            ElseIf token.Support <> kv.Value.Support Then
                Call failures.Add($"{name}: support of {KeyOf(kv.Key)} is {token.Support} <> {kv.Value.Support}")
            End If
        Next

        For Each kv As KeyValuePair(Of ItemSet, TransactionTokensItem) In actual.FrequentItems
            If Not expected.FrequentItems.ContainsKey(kv.Key) Then
                Call failures.Add($"{name}: unexpected frequent item {KeyOf(kv.Key)} (support={kv.Value.Support})")
            End If
        Next

        ' 2. 闭项集：键 + 父集（键 + 父集支持度）
        If expected.ClosedItemSets.Count <> actual.ClosedItemSets.Count Then
            Call failures.Add($"{name}: closed item set count {actual.ClosedItemSets.Count} <> {expected.ClosedItemSets.Count}")
        End If

        For Each kv As KeyValuePair(Of ItemSet, Dictionary(Of ItemSet, Double)) In expected.ClosedItemSets
            Dim parents As Dictionary(Of ItemSet, Double) = Nothing

            If Not actual.ClosedItemSets.TryGetValue(kv.Key, parents) Then
                Call failures.Add($"{name}: missing closed item {KeyOf(kv.Key)}")
                Continue For
            End If

            If kv.Value.Count <> parents.Count Then
                Call failures.Add($"{name}: parent count of {KeyOf(kv.Key)} is {parents.Count} <> {kv.Value.Count}")
            End If

            For Each p As KeyValuePair(Of ItemSet, Double) In kv.Value
                Dim sup As Double = 0

                If Not parents.TryGetValue(p.Key, sup) Then
                    Call failures.Add($"{name}: missing parent {KeyOf(p.Key)} of {KeyOf(kv.Key)}")
                ElseIf sup <> p.Value Then
                    Call failures.Add($"{name}: support of parent {KeyOf(p.Key)} is {sup} <> {p.Value}")
                End If
            Next

            For Each p As KeyValuePair(Of ItemSet, Double) In parents
                If Not kv.Value.ContainsKey(p.Key) Then
                    Call failures.Add($"{name}: unexpected parent {KeyOf(p.Key)} of {KeyOf(kv.Key)}")
                End If
            Next
        Next

        For Each kv As KeyValuePair(Of ItemSet, Dictionary(Of ItemSet, Double)) In actual.ClosedItemSets
            If Not expected.ClosedItemSets.ContainsKey(kv.Key) Then
                Call failures.Add($"{name}: unexpected closed item {KeyOf(kv.Key)}")
            End If
        Next

        ' 3. 极大项集：键集合
        Dim expectedMaximal As HashSet(Of String) = KeySet(expected.MaximalItemSets)
        Dim actualMaximal As HashSet(Of String) = KeySet(actual.MaximalItemSets)

        For Each k As String In expectedMaximal
            If Not actualMaximal.Contains(k) Then
                Call failures.Add($"{name}: missing maximal item {k}")
            End If
        Next

        For Each k As String In actualMaximal
            If Not expectedMaximal.Contains(k) Then
                Call failures.Add($"{name}: unexpected maximal item {k}")
            End If
        Next

        ' 4. 强规则：X/Y + 置信度 + 支持度
        Dim expectedRules As Dictionary(Of String, String) = RuleMap(expected)
        Dim actualRules As Dictionary(Of String, String) = RuleMap(actual)

        If expectedRules.Count <> actualRules.Count Then
            Call failures.Add($"{name}: strong rule count {actualRules.Count} <> {expectedRules.Count}")
        End If

        For Each kv As KeyValuePair(Of String, String) In expectedRules
            Dim value As String = Nothing

            If Not actualRules.TryGetValue(kv.Key, value) Then
                Call failures.Add($"{name}: missing strong rule [{kv.Key}] {kv.Value}")
            ElseIf value <> kv.Value Then
                Call failures.Add($"{name}: strong rule [{kv.Key}] is {value} <> {kv.Value}")
            End If
        Next

        For Each kv As KeyValuePair(Of String, String) In actualRules
            If Not expectedRules.ContainsKey(kv.Key) Then
                Call failures.Add($"{name}: unexpected strong rule [{kv.Key}] {kv.Value}")
            End If
        Next
    End Sub

    Private Function RuleMap(out As Output) As Dictionary(Of String, String)
        Dim map As New Dictionary(Of String, String)()

        For Each r As Rule In out.StrongRules
            Dim key As String = $"{KeyOf(r.X)} => {KeyOf(r.Y)}"
            Dim value As String = $"conf={r.Confidence.ToString("R")}, sXY={r.SupportXY.ToString("R")}, sX={r.SupportX.ToString("R")}"

            map(key) = value
        Next

        Return map
    End Function

    ''' <summary>
    ''' 规则的规范字符串表示（X 与 Y 内部都按项名字排序，与内部顺序无关）
    ''' </summary>
    Friend Function RuleKeyOf(r As Rule) As String
        Return $"{KeyOf(r.X)} => {KeyOf(r.Y)}"
    End Function

    Private Function KeySet(list As IEnumerable(Of ItemSet)) As HashSet(Of String)
        Dim keys As New HashSet(Of String)()

        For Each itemSet As ItemSet In list
            Call keys.Add(KeyOf(itemSet))
        Next

        Return keys
    End Function

    ''' <summary>
    ''' 项集的规范字符串表示（按项名字排序，与内部顺序无关）
    ''' </summary>
    Private Function KeyOf(itemSet As ItemSet) As String
        Dim parts As New List(Of String)()

        For Each i As Item In itemSet.Items
            Call parts.Add(i.Item)
        Next

        parts.Sort()

        Return "{" & String.Join(",", parts) & "}"
    End Function
End Module
