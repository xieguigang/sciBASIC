Imports System
Imports System.Diagnostics
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Impl

''' <summary>
''' Apriori 的多档规模基准。
''' 
''' 事务数据按「簇相关」方式生成：项被划分到若干簇，每个事务只从少数几个簇里取项，
''' 这样才会产生大量真正频繁的项集，否则纯随机数据几乎挖不出结构，基准没有意义。
''' 
''' ```bash
''' dotnet run --project test/test.vbproj -- bench
''' dotnet run --project test/test.vbproj -- bench --legacy
''' ```
''' </summary>
Module AprioriBench

    Private Structure BenchCase
        Public Name As String
        Public Transactions As Integer
        Public Items As Integer
        Public AvgLen As Integer
        Public Clusters As Integer
        Public MinSupport As Double
        Public MinConfidence As Double
        ''' <summary>
        ''' 原始实现在该档位上是否还跑得动
        ''' </summary>
        Public LegacyFeasible As Boolean
    End Structure

    ''' <summary>
    ''' 执行基准
    ''' </summary>
    ''' <param name="withLegacy">是否额外跑一遍原始实现</param>
    ''' <param name="legacyAll">
    ''' 是否连大档位也跑原始实现（大档位上原始实现会非常慢，默认跳过）
    ''' </param>
    Friend Sub Run(withLegacy As Boolean, legacyAll As Boolean)
        ' 数据按簇生成：簇内约 10 个项，事务长度与簇大小相当，
        ' 这样簇内的热门项与热门项对才会真正跨过支持度阈值，挖掘才有实际工作量
        Dim cases As BenchCase() = {
            New BenchCase With {.Name = "tiny    300x60", .Transactions = 300, .Items = 60, .AvgLen = 10, .Clusters = 5, .MinSupport = 0.05, .MinConfidence = 0.5, .LegacyFeasible = True},
            New BenchCase With {.Name = "small  1500x200", .Transactions = 1500, .Items = 200, .AvgLen = 12, .Clusters = 5, .MinSupport = 0.05, .MinConfidence = 0.5, .LegacyFeasible = True},
            New BenchCase With {.Name = "mid   10000x500", .Transactions = 10000, .Items = 500, .AvgLen = 14, .Clusters = 5, .MinSupport = 0.05, .MinConfidence = 0.5, .LegacyFeasible = False},
            New BenchCase With {.Name = "large 50000x1000", .Transactions = 50000, .Items = 1000, .AvgLen = 16, .Clusters = 5, .MinSupport = 0.05, .MinConfidence = 0.5, .LegacyFeasible = False},
            New BenchCase With {.Name = "wide 200000x300", .Transactions = 200000, .Items = 300, .AvgLen = 14, .Clusters = 5, .MinSupport = 0.05, .MinConfidence = 0.5, .LegacyFeasible = False}
        }

        Console.WriteLine("case                  txn   items  len   frequent   rules   total(ms)  index(ms)   mine(ms)  fallback(ms)   legacy(ms)")
        Console.WriteLine(New String("-"c, 118))

        For Each c As BenchCase In cases
            Dim trans As Transaction() = AprioriSelfTest.RandomTransactions(20241019, c.Transactions, c.Items, c.AvgLen, c.Clusters)
            Dim encoding As New Encoding(trans.AllItems)
            Dim itemSets As ItemSet() = encoding.TransactionEncoding(trans).ToArray()
            Dim out As Output = Nothing
            Dim outFallback As Output = Nothing
            Dim outEmpty As Output = Nothing

            Dim msTotal As Long = Measure(Sub()
                                              out = Apriori.GetAssociateRules(c.MinSupport, c.MinConfidence, encoding.AllItems, itemSets)
                                          End Sub)
            ' 支持度阈值取 2 时挖不出任何频繁项，这一次只体现事务索引的构建成本
            Dim msIndex As Long = Measure(Sub()
                                              outEmpty = Apriori.GetAssociateRules(2, c.MinConfidence, encoding.AllItems, itemSets)
                                          End Sub)
            ' 内存预算置 0 会强制走横向归并计数的降级路径
            Dim msFallback As Long = Measure(Sub()
                                                 outFallback = Apriori.GetAssociateRules(c.MinSupport, c.MinConfidence, encoding.AllItems, itemSets, bitIndexBudgetMB:=0)
                                             End Sub)

            If out.FrequentItems.Count <> outFallback.FrequentItems.Count OrElse out.StrongRules.Count <> outFallback.StrongRules.Count Then
                Console.WriteLine($"  !! fallback mismatch: {out.FrequentItems.Count} vs {outFallback.FrequentItems.Count} frequent")
            End If

            Dim legacy As String = "-"

            If withLegacy AndAlso (c.LegacyFeasible OrElse legacyAll) Then
                Dim outRef As Output = Nothing
                Dim msLegacy As Long = Measure(Sub()
                                                   outRef = Apriori.GetAssociateRulesReference(c.MinSupport, c.MinConfidence, encoding.AllItems, itemSets)
                                               End Sub)

                legacy = msLegacy.ToString()

                If out.FrequentItems.Count <> outRef.FrequentItems.Count OrElse out.StrongRules.Count <> outRef.StrongRules.Count Then
                    legacy &= "(!MISMATCH!)"
                End If
            End If

            Console.WriteLine($"{c.Name,-22} {c.Transactions,6}  {c.Items,5}  {c.AvgLen,3}  {out.FrequentItems.Count,8}  {out.StrongRules.Count,6}  {msTotal,9}  {msIndex,8}  {msTotal - msIndex,8}  {msFallback,12}  {legacy,10}")
        Next
    End Sub

    ''' <summary>
    ''' 静音测量一段工作的耗时（毫秒）
    ''' </summary>
    Private Function Measure(work As Action) As Long
        Dim watch As Stopwatch = Stopwatch.StartNew()

        Call AprioriSelfTest.Quietly(work)

        Return watch.ElapsedMilliseconds
    End Function
End Module
