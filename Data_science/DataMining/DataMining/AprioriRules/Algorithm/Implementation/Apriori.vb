#Region "Microsoft.VisualBasic::81f798b5bc6f6fb869f1930061df2aff, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/DataMining//AprioriRules/Algorithm/Implementation/Apriori.vb"

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

    Public Structure ItemSet : Implements IComparable(Of ItemSet)

        Public Items As Integer()

        Public ReadOnly Property Length As Integer
            Get
                Return Items.Length
            End Get
        End Property

        Default Public ReadOnly Property Item(offset As Integer) As Integer
            Get
                Return Items(offset)
            End Get
        End Property

        Sub New(scalar As Integer)
            Items = {scalar}
        End Sub

        Sub New(items As IEnumerable(Of Integer))
            Me.Items = items.ToArray
        End Sub

        Public Overrides Function GetHashCode() As Integer
            If IsNullOrEmpty() Then
                Return 0
            End If

            Dim hash As Integer = 1
            Dim el_hash As Integer

            For Each element As Integer In Items
                el_hash = element Xor (element >> 32)
                hash = 31 * hash + el_hash
            Next

            Return hash
        End Function

        Public Function IsNullOrEmpty() As Boolean
            Return Items Is Nothing OrElse Items.Length = 0
        End Function

        Public Function SorterSortTokens() As ItemSet
            Return New ItemSet With {.Items = Items.OrderBy(Function(a) a).ToArray}
        End Function

        Public Overrides Function ToString() As String
            Return "{" & Items.JoinBy(", ") & "}"
        End Function

        Public Function Contains(i As Integer) As Boolean
            For Each i32 As Integer In Items
                If i32 = i Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Function PopLast() As ItemSet
            Return New ItemSet(scalar:=Items.Last)
        End Function

        Public Shared Function Empty() As ItemSet
            Return New ItemSet With {.Items = {}}
        End Function

        ''' <summary>
        ''' GetRemaining: removes all child elements from parent
        ''' </summary>
        ''' <param name="child"></param>
        ''' <returns></returns>
        Public Function Remove(child As ItemSet) As ItemSet
            Dim remaining As New List(Of Integer)

            For Each i As Integer In Items
                If Not child.Contains(i) Then
                    Call remaining.Add(i)
                End If
            Next

            Return New ItemSet(remaining)
        End Function

        Public Function Slice(start As Integer, count As Integer) As ItemSet
            Return New ItemSet With {.Items = Items.Skip(start).Take(count).ToArray}
        End Function

        Public Overloads Shared Operator &(a As ItemSet, b As ItemSet) As ItemSet
            Return New ItemSet With {.Items = a.Items.JoinIterates(b.Items).ToArray}
        End Operator

        Public Overloads Shared Operator =(a As ItemSet, b As ItemSet) As Boolean
            Return a.Items.SequenceEqual(b.Items)
        End Operator

        Public Overloads Shared Operator <>(a As ItemSet, b As ItemSet) As Boolean
            Return Not a = b
        End Operator

        Private Function GetCompareValue() As ULong
            Dim p As ULong = 0

            For i As Integer = 0 To Items.Length - 1
                p += Items(i) * (10 ^ i)
            Next

            Return p
        End Function

        Public Function CompareTo(other As ItemSet) As Integer Implements IComparable(Of ItemSet).CompareTo
            Dim val1 As ULong = GetCompareValue()
            Dim val2 As ULong = other.GetCompareValue

            Return val1.CompareTo(val2)
        End Function
    End Structure

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
                                          <Parameter("Items")> items As IEnumerable(Of Integer),
                                          <Parameter("Transactions")> transactions As ItemSet()) As Output

            Dim frequentItems As IList(Of TransactionTokensItem) = transactions.GetL1FrequentItems(minSupport, items.Select(Function(i) New ItemSet(i)).ToArray)
            Dim allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem) = frequentItems.ToDictionary(Function(obj) obj.Name)
            Dim candidates As New Dictionary(Of ItemSet, Double)()
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
            Dim closedItemSets As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double)) = GetClosedItemSets(allFrequentItems)
            Call Console.WriteLine("maximal item rules...")
            Dim maximalItemSets As IList(Of ItemSet) = GetMaximalItemSets(closedItemSets)

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

        <Extension>
        Public Function GetL1FrequentItems(transactions As ItemSet(), minSupport#, items As ItemSet()) As List(Of TransactionTokensItem)
            Dim transactionsCount As Double = transactions.Length
            Dim frequentItemsL1 = LinqAPI.MakeList(Of TransactionTokensItem) _
                                                                             _
                () <= From item As ItemSet
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
            For Each c As Integer In child.Items
                If Not parent.Contains(c) Then
                    Return False
                End If
            Next

            Return True
        End Function

        <ExportAPI("Candidates.Generate")>
        <Extension>
        Public Function GenerateCandidates(frequentItems As IList(Of TransactionTokensItem), transactions As IEnumerable(Of ItemSet)) As Dictionary(Of ItemSet, Double)
            Dim parallelBuild = From i As Integer
                                In (frequentItems.Count) _
                                    .SeqIterator _
                                    .AsParallel
                                Let firstItem As ItemSet = frequentItems(i).Name.SorterSortTokens
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

        <ExportAPI("Get.FrequentItems")>
        <Extension>
        Public Function GetFrequentItems(candidates As IDictionary(Of ItemSet, Double), minSupport#, transactionsCount#) As List(Of TransactionTokensItem)
            Return LinqAPI.MakeList(Of TransactionTokensItem) _
                                                              _
                () <= From candidate
                      In candidates
                      Where candidate.Value / transactionsCount >= minSupport
                      Select New TransactionTokensItem() With {
                          .Name = candidate.Key,
                          .Support = candidate.Value
                      }
        End Function

        Public Function GetClosedItemSets(allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))
            Dim closedItemSets As New Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))()
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
#End Region
    End Module
End Namespace
