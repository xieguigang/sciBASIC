#Region "Microsoft.VisualBasic::3046761e5a58c1780c920b1c96bbbada, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\Algorithm\Implementation\Apriori.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports System.Linq
Imports System.ComponentModel.Composition
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.AprioriAlgorithm.Entities
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Threading

Namespace AprioriAlgorithm

    ''' <summary>
    ''' 
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

        Public Delegate Function AprioriPredictions(minSupport As Double, minConfidence As Double, items As IEnumerable(Of String), transactions As String()) As Output

#Region "IApriori"

        <ExportAPI("Apriori.Predictions")>
        Public Function InvokeAnalysis(<Parameter("Support.Min")> minSupport As Double,
                                       <Parameter("Confidence.Min")> minConfidence As Double,
                                       <Parameter("Items")> items As IEnumerable(Of String),
                                       <Parameter("Transactions")> Transactions As Generic.IEnumerable(Of String)) As Output

            Dim frequentItems As IList(Of TransactionTokensItem) = GetL1FrequentItems(minSupport, items, Transactions)
            Dim allFrequentItems As Dictionary(Of String, TransactionTokensItem) = frequentItems.ToDictionary(Function(obj) obj.Name)
            Dim candidates As IDictionary(Of String, Double) = New Dictionary(Of String, Double)()
            Dim transactionsCount As Double = Transactions.Count()

            Do
                candidates = GenerateCandidates(frequentItems, Transactions)
                frequentItems = GetFrequentItems(candidates, minSupport, transactionsCount)
                For Each obj In frequentItems
                    Call allFrequentItems.Add(obj.Name, obj)
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

            Dim out As Output = New Output() With {
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
        ''' ���ַ���֮�е��ַ������������
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

        Public Function GetL1FrequentItems(minSupport As Double,
                                           items As IEnumerable(Of String),
                                           transactions As IEnumerable(Of String)) As List(Of TransactionTokensItem)
            Dim transactionsCount As Double = transactions.Count()
            Dim frequentItemsL1 As List(Of TransactionTokensItem) = (From item As String In items.AsParallel
                                                                     Let support As Double = GetSupport(item, transactions)
                                                                     Where support / transactionsCount >= minSupport
                                                                     Select New TransactionTokensItem() With {
                                                                         .Name = item,
                                                                         .Support = support}).AsList
            Call frequentItemsL1.Sort()
            Return frequentItemsL1
        End Function

        <ExportAPI("Get.Support")>
        Public Function GetSupport(<Parameter("Generated.Candidate")> generatedCandidate As String,
                                   <Parameter("List.Transaction")> TransactionsList As IEnumerable(Of String)) As Double
            Dim support As Double = 0

            For Each transaction As String In TransactionsList
                If CheckIsSubset(generatedCandidate, transaction) Then
                    support += 1
                End If
            Next

            Return support
        End Function

        Public Function CheckIsSubset(child As String, parent As String) As Boolean
            For Each c As Char In child
                If Not parent.Contains(c) Then
                    Return False
                End If
            Next

            Return True
        End Function

        <ExportAPI("Candidates.Generate")>
        Public Function GenerateCandidates(frequentItems As IList(Of TransactionTokensItem), transactions As IEnumerable(Of String)) As Dictionary(Of String, Double)
            Dim LQuery = (From i As Integer In (frequentItems.Count).SeqIterator.AsParallel
                          Let firstItem As String = SorterSortTokens(frequentItems(i).Name)
                          Select GetCandidate(frequentItems, i, firstItem, transactions)).IteratesALL _
                                .ToDictionary(Function(obj) obj.Key,
                                              Function(obj) obj.Value)
            Return LQuery
        End Function

        Public Function GetCandidate(frequentItems As IList(Of TransactionTokensItem),
                                     i As Integer,
                                     firstItem As String,
                                     transactions As IEnumerable(Of String)) As KeyValuePair(Of String, Double)()
            Dim candidates As New Dictionary(Of String, Double)()

            For j As Integer = i + 1 To frequentItems.Count - 1
                Dim secondItem As String = SorterSortTokens(frequentItems(j).Name)
                Dim generatedCandidate As String = GenerateCandidate(firstItem, secondItem)

                If Not String.IsNullOrEmpty(generatedCandidate) Then
                    Dim support As Double = GetSupport(generatedCandidate, transactions)
                    Call candidates.Add(generatedCandidate, support)
                End If
            Next

            Return candidates.ToArray
        End Function

        Public Function GenerateCandidate(firstItem As String, secondItem As String) As String
            Dim length As Integer = firstItem.Length

            If length = 1 Then Return firstItem & secondItem

            Dim firstSubString As String = firstItem.Substring(0, length - 1)
            Dim secondSubString As String = secondItem.Substring(0, length - 1)

            If firstSubString = secondSubString Then
                Return firstItem & secondItem(length - 1)
            End If

            Return String.Empty
        End Function

        <ExportAPI("Get.FrequentItems")>
        Public Function GetFrequentItems(candidates As IDictionary(Of String, Double),
                                         <Parameter("Support.Min")> minSupport As Double,
                                         <Parameter("NumberOfTransactions")> transactionsCount As Double) As List(Of TransactionTokensItem)
            Dim frequentItems As List(Of TransactionTokensItem) = (From item As KeyValuePair(Of String, Double) In candidates.AsParallel
                                                                   Where item.Value / transactionsCount >= minSupport
                                                                   Select New TransactionTokensItem() With {
                                                                       .Name = item.Key,
                                                                       .Support = item.Value
                                                                       }).AsList
            Return frequentItems
        End Function

        Public Function GetClosedItemSets(allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Dictionary(Of String, Dictionary(Of String, Double))
            Dim closedItemSets = New Dictionary(Of String, Dictionary(Of String, Double))()
            Dim i As Integer = 0

            For Each item In allFrequentItems
                Dim parents As Dictionary(Of String, Double) =
                    GetItemParents(item.Key, Interlocked.Increment(i), allFrequentItems)

                If CheckIsClosed(item.Key, parents, allFrequentItems) Then
                    Call closedItemSets.Add(item.Key, parents)
                End If
            Next

            Return closedItemSets
        End Function

        Public Function GetItemParents(child As String, index As Integer, allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Dictionary(Of String, Double)
            Dim parents = New Dictionary(Of String, Double)()
            Dim ChunkBuffer = allFrequentItems.Values.ToArray

            For j As Integer = index To allFrequentItems.Count - 1
                Dim parent As String = ChunkBuffer(j).Name

                If parent.Length = child.Length + 1 Then
                    If CheckIsSubset(child, parent) Then
                        Call parents.Add(parent, allFrequentItems(parent).Support)
                    End If
                End If
            Next

            Return parents
        End Function

        Public Function CheckIsClosed(child As String, parents As Dictionary(Of String, Double), allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Boolean
            For Each parent As String In parents.Keys
                If allFrequentItems(child).Support = allFrequentItems(parent).Support Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Function GetMaximalItemSets(closedItemSets As Dictionary(Of String, Dictionary(Of String, Double))) As IList(Of String)
            Dim maximalItemSets = New List(Of String)()

            For Each item In closedItemSets
                Dim parents As Dictionary(Of String, Double) = item.Value

                If parents.Count = 0 Then
                    Call maximalItemSets.Add(item.Key)
                End If
            Next

            Return maximalItemSets
        End Function

        Public Function GenerateRules(allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As HashSet(Of Rule)
            Dim rulesList = New HashSet(Of Rule)()
            Dim LQuery = (From Token In allFrequentItems.AsParallel
                          Where Token.Key.Length > 1
                          Select ___generateRules(Token)).ToArray.Unlist

            For Each Rule In LQuery
                If Not rulesList.Contains(Rule) Then
                    Call rulesList.Add(Rule)
                End If
            Next

            Return rulesList
        End Function

        Private Function ___generateRules(Token As KeyValuePair(Of String, TransactionTokensItem)) As Rule()
            Dim subsetsList As IEnumerable(Of String) = GenerateSubsets(Token.Key)
            Dim List As New List(Of Rule)

            For Each subset In subsetsList
                Dim remaining As String = GetRemaining(subset, Token.Key)
                Dim rule As New Rule(subset, remaining, 0)
                Call List.Add(rule)
            Next
            Return List.ToArray
        End Function

        Public Function GenerateSubsets(item As String) As IEnumerable(Of String)
            Dim allSubsets As IEnumerable(Of String) = New String() {}
            Dim subsetLength As Integer = item.Length / 2

            For i As Integer = 1 To subsetLength
                Dim subsets As IList(Of String) = New List(Of String)()
                GenerateSubsetsRecursive(item, i, New Char(item.Length - 1) {}, subsets)
                allSubsets = allSubsets.Concat(subsets)
            Next

            Return allSubsets
        End Function

        Public Sub GenerateSubsetsRecursive(item As String, subsetLength As Integer, temp As Char(), subsets As IList(Of String), Optional q As Integer = 0, Optional r As Integer = 0)
            If q = subsetLength Then
                Dim sb As New StringBuilder()

                For i As Integer = 0 To subsetLength - 1
                    sb.Append(temp(i))
                Next

                subsets.Add(sb.ToString())
            Else

                For i As Integer = r To item.Length - 1
                    temp(q) = item(i)
                    GenerateSubsetsRecursive(item, subsetLength, temp, subsets, q + 1, i + 1)
                Next
            End If
        End Sub

        Public Function GetRemaining(child As String, parent As String) As String
            For i As Integer = 0 To child.Length - 1
                Dim index As Integer = parent.IndexOf(child(i))
                parent = parent.Remove(index, 1)
            Next

            Return parent
        End Function

        Public Function GetStrongRules(minConfidence As Double, rules As HashSet(Of Rule), allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As IList(Of Rule)
            Dim strongRules = New List(Of Rule)()

            For Each rule As Rule In rules
                Dim xy As String = Apriori.SorterSortTokens(rule.X & rule.Y)
                AddStrongRule(rule, xy, strongRules, minConfidence, allFrequentItems)
            Next

            strongRules.Sort()
            Return strongRules
        End Function

        Public Sub AddStrongRule(rule As Rule, XY As String, strongRules As List(Of Rule), minConfidence As Double, allFrequentItems As Dictionary(Of String, TransactionTokensItem))
            Dim confidence As Double = GetConfidence(rule.X, XY, allFrequentItems)

            If confidence >= minConfidence Then
                Dim newRule As New Rule(rule.X, rule.Y, confidence)
                strongRules.Add(newRule)
            End If

            confidence = GetConfidence(rule.Y, XY, allFrequentItems)

            If confidence >= minConfidence Then
                Dim newRule As New Rule(rule.Y, rule.X, confidence)
                strongRules.Add(newRule)
            End If
        End Sub

        <ExportAPI("Get.Confidence")>
        Public Function GetConfidence(X As String, XY As String, <Parameter("FrequentItems.ALL")> allFrequentItems As Dictionary(Of String, TransactionTokensItem)) As Double
            If Not (allFrequentItems.ContainsKey(X) AndAlso allFrequentItems.ContainsKey(XY)) Then
                Return 0
            End If

            Dim supportX As Double = allFrequentItems(X).Support
            Dim supportXY As Double = allFrequentItems(XY).Support
            Return supportXY / supportX
        End Function
#End Region
    End Module
End Namespace
