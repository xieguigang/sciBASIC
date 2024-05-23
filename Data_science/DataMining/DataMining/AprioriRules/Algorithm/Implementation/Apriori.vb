#Region "Microsoft.VisualBasic::a4da79841cd189620265cbca637715ed, Data_science\DataMining\DataMining\AprioriRules\Algorithm\Implementation\Apriori.vb"

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
    '     File Size: 10.83 KB


    '     Module Apriori
    ' 
    '         Function: CheckIsClosed, CheckIsSubset, GenerateCandidate, GenerateCandidates, GetAssociateRules
    '                   GetCandidate, GetClosedItemSets, GetFrequentItems, GetItemParents, GetL1FrequentItems
    '                   GetSupport
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
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

        Public Function GetAssociateRules(<Parameter("Support.Min")> minSupport#,
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

                Call VBDebugger.EchoLine($" ..... {frequentItems.Count} / {allFrequentItems.Count}")
            Loop While candidates.Count <> 0

            Call VBDebugger.EchoLine($"generate rules based on all {allFrequentItems.Count} frequent items!")

            Dim rules As HashSet(Of Rule) = GenerateRules(allFrequentItems)
            Dim strongRules As IList(Of Rule) = GetStrongRules(minConfidence, rules, allFrequentItems)
            Dim closedItemSets As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double)) = GetClosedItemSets(allFrequentItems)
            Dim maximalItemSets As IList(Of ItemSet) = GetMaximalItemSets(closedItemSets)

            Return New Output() With {
                .StrongRules = strongRules,
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

        Public Function GenerateCandidate(firstItem As ItemSet, secondItem As ItemSet) As ItemSet
            Dim length As Integer = firstItem.Length

            If length = 1 Then
                Return firstItem & secondItem
            End If

            Dim firstSubString As ItemSet = firstItem.Slice(0, length - 1)
            Dim secondSubString As ItemSet = secondItem.Slice(0, length - 1)

            If firstSubString = secondSubString Then
                Return firstItem & secondItem.PopLast
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

            For Each item In Tqdm.Wrap(allFrequentItems)
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
