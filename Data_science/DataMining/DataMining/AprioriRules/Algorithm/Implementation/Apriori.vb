#Region "Microsoft.VisualBasic::5472f38da971b280111b5f261075e366, Data_science\DataMining\DataMining\AprioriRules\Algorithm\Implementation\Apriori.vb"

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

    '   Total Lines: 230
    '    Code Lines: 177 (76.96%)
    ' Comment Lines: 7 (3.04%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 46 (20.00%)
    '     File Size: 10.85 KB


    '     Module Apriori
    ' 
    '         Function: CheckIsClosed, CheckIsSubset, GenerateCandidate, GenerateCandidates, GetAssociateRules
    '                   GetCandidate, GetClosedItemSets, GetFrequentItems, GetItemParents, GetL1FrequentItems
    '                   GetSupport
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Engine
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq.JoinExtensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace AprioriRules.Impl

    ''' <summary>
    ''' 关联分析程序（当某一种事务的样本较少的时候，将无法分析出关联性）
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <Package("Tools.DataMining.Apriori",
             Publisher:="Omar Gameel Salem",
             Url:="http://www.codeproject.com/Articles/70371/Apriori-Algorithm",
             Description:="In data mining, Apriori is a classic algorithm for learning association rules. 
                      Apriori is designed to operate on databases containing transactions 
                      (for example, collections of items bought by customers, or details of a website frequentation).
<p>Other algorithms are designed for finding association rules in data having no transactions (Winepi and Minepi), or having no timestamps (DNA sequencing).",
             Cites:="")>
    Public Module Apriori

        ''' <summary>
        ''' 关联规则挖掘入口
        ''' </summary>
        ''' <param name="minSupport">最小支持度阈值 ``(0,1]``</param>
        ''' <param name="minConfidence">最小置信度阈值 ``(0,1]``</param>
        ''' <param name="items">全部相异项的集合</param>
        ''' <param name="transactions">事务数据库</param>
        ''' <param name="maxDegreeOfParallelism">
        ''' 最大并行度；小于等于 0 时取 <see cref="Environment.ProcessorCount"/>。
        ''' </param>
        ''' <param name="bitIndexBudgetMB">
        ''' 位图索引的内存预算（MB）。频繁项位图超过该预算时自动降级为横向计数，
        ''' 保证超大数据集仍然可以跑完（只是慢一些）。
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 本函数现在只是转发给 <see cref="AprioriEngine"/>，
        ''' 下面保留的那些辅助函数是原始的分步实现，外部 API（含 R# 的 ``Get.Support``）仍然可用。
        ''' </remarks>
        Public Function GetAssociateRules(<Parameter("Support.Min")> minSupport#,
                                          <Parameter("Confidence.Min")> minConfidence#,
                                          <Parameter("Items")> items As IEnumerable(Of Item),
                                          <Parameter("Transactions")> transactions As ItemSet(),
                                          Optional maxDegreeOfParallelism As Integer = -1,
                                          Optional bitIndexBudgetMB As Integer = 512) As Output

            Return AprioriEngine _
                .Create(items:=items,
                        transactions:=transactions,
                        minSupport:=minSupport,
                        minConfidence:=minConfidence,
                        maxDegreeOfParallelism:=maxDegreeOfParallelism,
                        bitIndexBudgetMB:=bitIndexBudgetMB) _
                .Run()
        End Function

        ''' <summary>
        ''' 原始的逐步实现：保留下来作为回归对拍的基准实现
        ''' </summary>
        ''' <param name="minSupport">``(0,1]``</param>
        ''' <param name="minConfidence">``(0,1]``</param>
        ''' <param name="items"></param>
        ''' <param name="transactions"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这一份实现与 <see cref="GetAssociateRules(Double, Double, IEnumerable(Of Item), ItemSet(), Integer, Integer)"/>
        ''' 的结果完全一致，只是复杂度高得多（每个候选都要逐事务线性 Contains 计数）。
        ''' 生产路径请用 <see cref="GetAssociateRules(Double, Double, IEnumerable(Of Item), ItemSet(), Integer, Integer)"/>，
        ''' 这里保留它仅供自测工程做新旧结果等价性对拍与性能对比。
        ''' </remarks>
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Function GetAssociateRulesReference(<Parameter("Support.Min")> minSupport#,
                                                   <Parameter("Confidence.Min")> minConfidence#,
                                                   <Parameter("Items")> items As IEnumerable(Of Item),
                                                   <Parameter("Transactions")> transactions As ItemSet()) As Output

            Dim frequentItems As List(Of TransactionTokensItem) = transactions _
                .GetL1FrequentItems(minSupport, items.Select(Function(i) New ItemSet(i)).ToArray) _
                .AsList
            Dim allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem) = frequentItems.ToDictionary(Function(obj) obj.Name)
            Dim candidates As New Dictionary(Of ItemSet, Double)()
            Dim transactionsCount As Double = transactions.Length

            Do
                candidates = frequentItems.GenerateCandidates(transactions)
                frequentItems = candidates.GetFrequentItems(minSupport, transactionsCount)

                For Each item As TransactionTokensItem In frequentItems
                    allFrequentItems(item.Name) = item
                Next
            Loop While candidates.Count <> 0

            ' 注意：局部变量不能叫 strongRules——VB 大小写不敏感，会遮蔽 StrongRules 模块
            Dim candidates2 As HashSet(Of Rule) = StrongRules.GenerateRules(allFrequentItems)
            Dim strong As IList(Of Rule) = StrongRules.GetStrongRules(minConfidence, candidates2, allFrequentItems)
            Dim closedItemSets As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double)) = GetClosedItemSets(allFrequentItems)
            Dim maximalItemSets As IList(Of ItemSet) = StrongRules.GetMaximalItemSets(closedItemSets)

            Return New Output() With {
                .StrongRules = strong,
                .MaximalItemSets = maximalItemSets,
                .ClosedItemSets = closedItemSets,
                .FrequentItems = allFrequentItems,
                .TransactionSize = transactions.Length
            }
        End Function

        <Extension>
        Public Function GetL1FrequentItems(transactions As ItemSet(), minSupport#, items As ItemSet()) As IEnumerable(Of TransactionTokensItem)
            Dim transactionsCount As Double = transactions.Length
            Dim frequentItemsL1 = From item As ItemSet
                                  In items.AsParallel
                                  Let support As Double = GetSupport(item, transactions)
                                  Where support / transactionsCount >= minSupport
                                  Let t = New TransactionTokensItem() With {
                                      .Name = item,
                                      .Support = support
                                  }
                                  Select t
                                  Order By t

            Call VBDebugger.EchoLine("get L1 frequent items...")

            Return frequentItemsL1
        End Function

        <ExportAPI("Get.Support")>
        <Extension>
        Public Function GetSupport(generatedCandidate As ItemSet, transactionsList As IEnumerable(Of ItemSet)) As Double
            Dim support As Double = 0

            For Each transaction As ItemSet In transactionsList
                If True = CheckIsSubset(generatedCandidate, transaction) Then
                    support += 1
                End If
            Next

            Return support
        End Function

        Public Function CheckIsSubset(child As ItemSet, parent As ItemSet) As Boolean
            For Each c As Item In child.Items
                If Not parent.Contains(c) Then
                    Return False
                End If
            Next

            Return True
        End Function

        <Extension>
        Public Function GenerateCandidates(frequentItems As IList(Of TransactionTokensItem), transactions As IEnumerable(Of ItemSet)) As Dictionary(Of ItemSet, Double)
            Dim parallelBuild = From i As Integer
                                In Enumerable.Range(0, frequentItems.Count).AsParallel
                                Let firstItem As ItemSet = frequentItems(i).Name.SorterSortTokens
                                Let candidate = frequentItems.GetCandidate(i, firstItem, transactions)
                                Select candidate

            Call VBDebugger.EchoLine("parallel build of the candidates...")

            Dim candidates As New Dictionary(Of ItemSet, Double)

            ' 20240521
            ' ArgumentException: An item with the same key has already been added. Key: {10182_[M+CH3OH+H]+, 1024_[M-H]-}
            For Each item In parallelBuild.ToArray.IteratesALL
                candidates(item.Key) = item.Value
            Next

            Return candidates
        End Function

        <Extension>
        Public Function GetCandidate(frequentItems As IList(Of TransactionTokensItem),
                                     i%,
                                     firstItem As ItemSet,
                                     transactions As IEnumerable(Of ItemSet)) As Dictionary(Of ItemSet, Double)

            Dim candidates As New Dictionary(Of ItemSet, Double)()

            For j As Integer = i + 1 To frequentItems.Count - 1
                Dim secondItem As ItemSet = frequentItems(j).Name.SorterSortTokens
                Dim generatedCandidate As ItemSet = GenerateCandidate(firstItem, secondItem)

                If Not generatedCandidate.IsNullOrEmpty Then
                    Dim support = GetSupport(generatedCandidate, transactions)
                    Call candidates.Add(generatedCandidate, support)
                End If
            Next

            Return candidates
        End Function

        ''' <summary>
        ''' 由两个频繁的 (k-1) 项集拼接出 k 项候选
        ''' </summary>
        ''' <remarks>
        ''' 拼接结果必须重新排序：原始实现直接返回 ``firstItem &amp; secondItem.PopLast``，
        ''' 而 <see cref="GetFrequentItems"/> 的输出顺序并不保证字典序（内部是 AsParallel），
        ''' 于是拼接出来的候选可能是乱序的（例如 {B,E,C}）。
        ''' 乱序的项集在后续 <see cref="ItemSet.Remove(ItemSet)"/> 与字典查找里会与排序后的
        ''' 等价项集对不上，导致该类项集上的关联规则被静默丢弃。
        ''' </remarks>
        Public Function GenerateCandidate(firstItem As ItemSet, secondItem As ItemSet) As ItemSet
            Dim length As Integer = firstItem.Length

            If length = 1 Then
                Return (firstItem & secondItem).SorterSortTokens
            End If

            Dim firstSubString As ItemSet = firstItem.Slice(0, length - 1)
            Dim secondSubString As ItemSet = secondItem.Slice(0, length - 1)

            If firstSubString = secondSubString Then
                Return (firstItem & secondItem.PopLast).SorterSortTokens
            End If

            Return ItemSet.Empty
        End Function

        <Extension>
        Public Function GetFrequentItems(candidates As IDictionary(Of ItemSet, Double), minSupport#, transactionsCount#) As List(Of TransactionTokensItem)
            Call VBDebugger.EchoLine($"filter frequent items via min_supports threshold: {minSupport}...")
            Call VBDebugger.EchoLine($"total number of transactions: {transactionsCount}.")

            Return LinqAPI.MakeList(Of TransactionTokensItem) _
                                                              _
                () <= From candidate
                      In candidates.AsParallel
                      Where candidate.Value / transactionsCount >= minSupport
                      Select New TransactionTokensItem() With {
                          .Name = candidate.Key,
                          .Support = candidate.Value
                      }
        End Function

        Public Function GetClosedItemSets(allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))
            Dim closedItemSets As New Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))()
            Dim i As i32 = 0

            Call VBDebugger.EchoLine("get closed item sets...")

            For Each item In Tqdm.Wrap(allFrequentItems, wrap_console:=App.EnableTqdm)
                Dim parents = item.Key.GetItemParents(++i, allFrequentItems)

                If item.Key.CheckIsClosed(parents, allFrequentItems) Then
                    Call closedItemSets.Add(item.Key, parents)
                End If
            Next

            Return closedItemSets
        End Function

        <Extension>
        Public Function GetItemParents(child As ItemSet, index%, allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As Dictionary(Of ItemSet, Double)
            Dim parents = New Dictionary(Of ItemSet, Double)()
            Dim data = allFrequentItems.Values.ToArray

            For j As Integer = index To allFrequentItems.Count - 1
                Dim parent As ItemSet = data(j).Name

                If parent.Length = child.Length + 1 Then
                    If CheckIsSubset(child, parent) Then
                        Call parents.Add(parent, allFrequentItems(parent).Support)
                    End If
                End If
            Next

            Return parents
        End Function

        <Extension>
        Public Function CheckIsClosed(child As ItemSet, parents As Dictionary(Of ItemSet, Double), allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As Boolean
            For Each parent As ItemSet In parents.Keys
                If allFrequentItems(child).Support = allFrequentItems(parent).Support Then
                    Return False
                End If
            Next

            Return True
        End Function
    End Module
End Namespace
