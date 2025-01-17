#Region "Microsoft.VisualBasic::9ff9b486ca7f13d7adc1954fd86521bb, Data_science\DataMining\DataMining\AprioriRules\Algorithm\Implementation\StrongRules.vb"

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

    '   Total Lines: 140
    '    Code Lines: 108 (77.14%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 32 (22.86%)
    '     File Size: 5.83 KB


    '     Module StrongRules
    ' 
    '         Function: AddStrongRule, concatRules, GenerateRules, GenerateSubsets, GetConfidence
    '                   GetMaximalItemSets, GetStrongRules
    ' 
    '         Sub: GenerateSubsetsRecursive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace AprioriRules.Impl

    Module StrongRules

        <Extension>
        Public Function GenerateRules(allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As HashSet(Of Rule)
            Dim rulesList As New HashSet(Of Rule)()
            Dim concats = LinqAPI.Exec(Of Rule()) _
                                                  _
                () <= From item
                      In allFrequentItems.AsParallel
                      Where item.Key.Length > 1
                      Select item.concatRules()

            For Each rule As Rule In concats.IteratesALL
                If Not rulesList.Contains(rule) Then
                    Call rulesList.Add(rule)
                End If
            Next

            Call VBDebugger.EchoLine($"found {rulesList.Count} unique rules...")

            Return rulesList
        End Function

        <Extension>
        Private Function concatRules(token As KeyValuePair(Of ItemSet, TransactionTokensItem)) As Rule()
            Dim subsetsList As ItemSet() = GenerateSubsets(token.Key).ToArray
            Dim list As New List(Of Rule)

            For Each subset As ItemSet In subsetsList
                Dim remaining As ItemSet = token.Key.Remove(subset)
                Dim rule As New Rule(subset, remaining, 0, (0, 0))

                Call list.Add(rule)
            Next

            Return list.ToArray
        End Function

        Public Function GenerateSubsets(item As ItemSet) As IEnumerable(Of ItemSet)
            Dim allSubsets As IEnumerable(Of ItemSet) = New ItemSet() {}
            Dim subsetLength As Integer = item.Length / 2

            For i As Integer = 1 To subsetLength
                Dim subsets As New List(Of ItemSet)()
                GenerateSubsetsRecursive(item, i, New Item(item.Length - 1) {}, subsets)
                allSubsets = allSubsets.Concat(subsets)
            Next

            Return allSubsets
        End Function

        Public Sub GenerateSubsetsRecursive(item As ItemSet, subsetLength%, temp As Item(), subsets As IList(Of ItemSet), Optional q% = 0, Optional r% = 0)
            If q = subsetLength Then
                Dim sb As New List(Of Item)

                For i As Integer = 0 To subsetLength - 1
                    sb.Add(temp(i))
                Next

                subsets.Add(New ItemSet(sb))
            Else
                For i As Integer = r To item.Length - 1
                    temp(q) = item(i)
                    GenerateSubsetsRecursive(item, subsetLength, temp, subsets, q + 1, i + 1)
                Next
            End If
        End Sub

        Public Function GetStrongRules(minConfidence#, rules As HashSet(Of Rule), allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As IList(Of Rule)
            Dim strongRules As New List(Of Rule)()
            Dim populateStrongRules = From rule As Rule
                                      In rules.AsParallel
                                      Let xy As ItemSet = (rule.X & rule.Y).SorterSortTokens
                                      Select AddStrongRule(rule, xy, minConfidence, allFrequentItems)

            Call VBDebugger.EchoLine($"get strong rules via min_confidence threshold: {minConfidence}...")

            For Each rule As Rule In populateStrongRules.ToArray.IteratesALL
                Call strongRules.Add(rule)
            Next

            Call strongRules.Sort()
            Call VBDebugger.EchoLine($"found {strongRules.Count} strong rules!")

            Return strongRules
        End Function

        <Extension>
        Public Iterator Function AddStrongRule(rule As Rule, XY As ItemSet, minConfidence#, allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem)) As IEnumerable(Of Rule)
            Dim value = allFrequentItems.GetConfidence(rule.X, XY)

            If value.confidence >= minConfidence Then
                Dim newRule As New Rule(rule.X, rule.Y, value.confidence, value.support)
                Yield newRule
            End If

            value = allFrequentItems.GetConfidence(rule.Y, XY)

            If value.confidence >= minConfidence Then
                Dim newRule As New Rule(rule.Y, rule.X, value.confidence, value.support)
                Yield newRule
            End If
        End Function

        <Extension>
        Public Function GetConfidence(allFrequentItems As Dictionary(Of ItemSet, TransactionTokensItem), X As ItemSet, XY As ItemSet) As (support As (XY#, X#), confidence#)
            If Not (allFrequentItems.ContainsKey(X) AndAlso allFrequentItems.ContainsKey(XY)) Then
                Return ((0, 0), 0)
            End If

            Dim supportX As Double = allFrequentItems(X).Support
            Dim supportXY As Double = allFrequentItems(XY).Support
            Return ((supportXY, supportX), supportXY / supportX)
        End Function

        Public Function GetMaximalItemSets(closedItemSets As Dictionary(Of ItemSet, Dictionary(Of ItemSet, Double))) As IList(Of ItemSet)
            Dim maximalItemSets As New List(Of ItemSet)()

            Call VBDebugger.EchoLine("get maximal item sets...")

            For Each item In Tqdm.Wrap(closedItemSets, wrap_console:=App.EnableTqdm)
                Dim parents = item.Value

                If parents.Count = 0 Then
                    Call maximalItemSets.Add(item.Key)
                End If
            Next

            Return maximalItemSets
        End Function
    End Module
End Namespace
