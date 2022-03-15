#Region "Microsoft.VisualBasic::86010f91b7c9f5f11629e04d0212ff08, sciBASIC#\Data_science\MachineLearning\MachineLearning\RandomForests\RandomForests.vb"

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

    '   Total Lines: 66
    '    Code Lines: 58
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 2.81 KB


    '     Class RandomForests
    ' 
    '         Properties: trees
    ' 
    '         Function: CreateForests, Predicts
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.DecisionTree
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.Linq

Namespace RandomForests

    Public Class RandomForests

        Public Property trees As Tree()

        Public Function Predicts(test As IDictionary(Of String, String)) As ClassifyResult
            Dim votes = trees.AsParallel _
                .Select(Function(tree)
                            Return tree.CalculateResult(valuesForQuery:=test)
                        End Function) _
                .GroupBy(Function(result) result.result) _
                .Select(Function(vote)
                            Return New NamedCollection(Of ClassifyResult) With {
                                .Name = vote.Key,
                                .Value = vote.ToArray
                            }
                        End Function) _
                .OrderByDescending(Function(g) g.Length) _
                .ToArray
            Dim mostVoted As NamedCollection(Of ClassifyResult) = votes(Scan0)
            Dim explains As String() = mostVoted _
                .Select(Function(vote)
                            Return vote.explains.Take(vote.explains.Count - 1).Split(2)
                        End Function) _
                .IteratesALL _
                .GroupBy(Function(exp) exp(Scan0)) _
                .Select(Function(explain)
                            Dim mostExplains = explain _
                                .Select(Function(reason) reason(1)) _
                                .GroupBy(Function(r) r) _
                                .OrderByDescending(Function(g) g.Count) _
                                .First

                            Return {explain.Key, mostExplains.Key}
                        End Function) _
                .IteratesALL _
                .ToArray
            Dim classify As New ClassifyResult With {
                .explains = explains.AsList,
                .result = mostVoted.Name
            }

            Return classify
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateForests(trainingSet As DataTable, n%, size%) As RandomForests
            Return trainingSet.Sampling(n, size) _
                .AsParallel _
                .Select(Function(data) New Tree(data)) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(trees As Tree()) As RandomForests
            Return New RandomForests With {._trees = trees}
        End Operator
    End Class
End Namespace
