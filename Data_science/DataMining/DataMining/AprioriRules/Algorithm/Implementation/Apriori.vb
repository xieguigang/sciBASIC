#Region "Microsoft.VisualBasic::81f798b5bc6f6fb869f1930061df2aff, sciBASIC#\Data_science\DataMining\DataMining\AprioriRules\Algorithm\Implementation\Apriori.vb"

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

    '   Total Lines: 244
    '    Code Lines: 189
    ' Comment Lines: 12
    '   Blank Lines: 43
    '     File Size: 10.75 KB


    '     Module Apriori
    ' 
    '         Function: CheckIsClosed, CheckIsSubset, GenerateCandidate, GenerateCandidates, GetAssociateRules
    '                   GetCandidate, GetClosedItemSets, GetFrequentItems, GetItemParents, GetL1FrequentItems
    '                   GetSupport, SorterSortTokens
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

#Region "IApriori"

        <ExportAPI("Apriori.Predictions")>
        Public Function GetAssociateRules(<Parameter("Support.Min")> minSupport#,
                                          <Parameter("Confidence.Min")> minConfidence#,
                                          <Parameter("Items")> items As IEnumerable(Of String),
                                          <Parameter("Transactions")> transactions$()) As Output

            Dim frequentItems As IList(Of TransactionTokensItem) = transactions.GetL1FrequentItems(minSupport, items)
            Dim allFrequentItems As Dictionary(Of String, TransactionTokensItem) = frequentItems.ToDictionary(Function(obj) obj.Name)
            Dim candidates As IDictionary(Of String, Double) = New Dictionary(Of String, Double)()
            Dim transactionsCount As Double = transactions.Length

            Do
                candidates = frequentItems.GenerateCandidates(transactions)
                frequentItems = candidates.GetFrequentItems(minSupport, transactionsCount)

                For Each item In frequentItems
                    Call allFrequentItems.Add(item.Name, item)
                Next

                Call Console.Write(".")
            Loop While candidates.Count <> 0

            Call Console.WriteLine("Start to export association rules data....")

            Call Console.WriteLine("rules...")
            Dim rules As HashSet(Of Rule) = GenerateRules(allFrequentItems)
            Call Console.WriteLine("strong rules...")
            Dim strongRules As IList(Of Rule) = GetStrongRules(minConfidence, rules, allFrequentItems)
            Call Console.WriteLine("closed item rules...")
            Dim closedItemSets As Dictionary(Of String, Dictionary(Of String, Double)) = GetClosedItemSets(allFrequentItems)
            Call Console.WriteLine("maximal item rules...")
            Dim maximalItemSets As IList(Of String) = GetMaximalItemSets(closedItemSets)

            Dim out As New Output() With {
                .StrongRules = strongRules,
                .MaximalItemSets = maximalItemSets,
                .ClosedItemSets = closedItemSets,
                .FrequentItems = allFrequentItems
            }
            Return out
        End Function

#End Region

#Region "Private Internal Methods"

        ''' <summary>
        ''' 将字符串之中的字符进行排序操作
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Tokens.Sort")>
        Public Function SorterSortTokens(token As String) As String
            Dim tokenArray As Char() = token.ToCharArray()
            Call Array.Sort(tokenArray)
            Return New String(tokenArray)
        End Function

        <Extension>
        Public Function GetL1FrequentItems(transactions$(), minSupport#, items$()) As List(Of TransactionTokensItem)
            Dim transactionsCount As Double = transactions.Length
            Dim frequentItemsL1 = LinqAPI.MakeList(Of TransactionTokensItem) _
 _
                () <= From item As String
                      In items.AsParallel
                      Let support As Double = GetSupport(item, transactions)
                      Where support / transactionsCount >= minSupport
                      Select New TransactionTokensItem() With {
                          .Name = item,
                          .Support = support
                      }

            Call frequentItemsL1.Sort()
            Return frequentItemsL1
        End Function

        <ExportAPI("Get.Support")>
        <Extension>
        Public Function GetSupport(generatedCandidate$, transactionsList As IEnumerable(Of String)) As Double
            Dim support As Double = 0

            For Each transaction As String In transactionsList
                If True = CheckIsSubset(generatedCandidate, transaction) Then
                    support += 1
                End If
            Next

            Return support
        End Function

        Public Function CheckIsSubset(child$, parent$) As Boolean
            For Each c As Char In child
                If Not parent.Contains(c) Then
                    Return False
                End If
            Next

            Return True
        End Function

        <ExportAPI("Candidates.Generate")>
        <Extension>
        Public Function GenerateCandidates(frequentItems As IList(Of TransactionTokensItem), transactions As IEnumerable(Of String)) As Dictionary(Of String, Double)
            Dim parallelBuild = From i As Integer
                                In (frequentItems.Count) _
                                    .SeqIterator _
                                    .AsParallel
                                Let firstItem As String = SorterSortTokens(frequentItems(i).Name)
                                Let candidate = frequentItems.GetCandidate(i, firstItem, transactions)
                                Select candidate
            Dim candidates = parallelBuild _
                .IteratesALL _
                .ToDictionary(Function(item) item.Key,
                              Function(item) item.Value)
            Return candidates
        End Function

        <Extension>
        Public Function GetCandidate(frequentItems As IList(Of TransactionTokensItem),
                                     i%,
                                     firstItem$,
                                     transactions As IEnumerable(Of String)) As Dictionary(Of String, Double)

            Dim candidates As New Dictionary(Of String, Double)()

            For j As Integer = i + 1 To frequentItems.Count - 1
                Dim secondItem As String = SorterSortTokens(frequentItems(j).Name)
                Dim generatedCandidate As String = GenerateCandidate(firstItem, secondItem)

                If Not String.IsNullOrEmpty(generatedCandidate) Then
                    Dim support = GetSupport(generatedCandidate, transactions)
                    Call candidates.Add(generatedCandidate, support)
                End If
            Next

            Return candidates
        End Function

        Public Function GenerateCandidate(firstItem As String, secondItem As String) As String
            Dim length As Integer = firstItem.Length

            If length = 1 Then
                Return firstItem & secondItem
            End If

            Dim firstSubString As String = firstItem.Substring(0, length - 1)
            Dim secondSubString As String = secondItem.Substring(0, length - 1)

            If firstSubString = secondSubString Then
                Return firstItem & secondItem(length - 1)
            End If

            Return String.Empty
        End Function

        <ExportAPI("Get.FrequentItems")>
        <Extension>
        Public Function GetFrequentItems(candidates As IDictionary(Of String, Double), minSupport#, transactionsCount#) As List(Of TransactionTokensItem)
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

        Public Function GetClosedItemSets(allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Dictionary(Of String, Dictionary(Of String, Double))
            Dim closedItemSets As New Dictionary(Of String, Dictionary(Of String, Double))()
            Dim i As i32 = 0

            For Each item In allFrequentItems
                Dim parents = item.Key.GetItemParents(++i, allFrequentItems)

                If item.Key.CheckIsClosed(parents, allFrequentItems) Then
                    Call closedItemSets.Add(item.Key, parents)
                End If
            Next

            Return closedItemSets
        End Function

        <Extension>
        Public Function GetItemParents(child$, index%, allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Dictionary(Of String, Double)
            Dim parents = New Dictionary(Of String, Double)()
            Dim data = allFrequentItems.Values.ToArray

            For j As Integer = index To allFrequentItems.Count - 1
                Dim parent As String = data(j).Name

                If parent.Length = child.Length + 1 Then
                    If CheckIsSubset(child, parent) Then
                        Call parents.Add(parent, allFrequentItems(parent).Support)
                    End If
                End If
            Next

            Return parents
        End Function

        <Extension>
        Public Function CheckIsClosed(child$, parents As Dictionary(Of String, Double), allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Boolean
            For Each parent As String In parents.Keys
                If allFrequentItems(child).Support = allFrequentItems(parent).Support Then
                    Return False
                End If
            Next

            Return True
        End Function
#End Region
    End Module
End Namespace
