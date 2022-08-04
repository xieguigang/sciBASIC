#Region "Microsoft.VisualBasic::47c2d8ea59928d4307e38d10c2f05d73, sciBASIC#\Data_science\DataMining\BinaryTree\BuildTree.vb"

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

    '   Total Lines: 53
    '    Code Lines: 43
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 2.33 KB


    ' Module BuildTree
    ' 
    '     Function: (+3 Overloads) BTreeCluster
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.DataFrame

Public Module BuildTree

    <Extension>
    Public Function BTreeCluster(d As DistanceMatrix, Optional equals As Double = 0.9, Optional gt As Double = 0.7) As BTreeCluster
        Dim btree As New AVLTree(Of String, String)(New Comparison(d, equals, gt).GetComparer, Function(str) str)

        For Each id As String In d.keys
            Call btree.Add(id, id, valueReplace:=False)
        Next

        Return BTreeCluster.GetClusters(btree)
    End Function

    <Extension>
    Public Function BTreeCluster(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet),
                                                                                              Optional equals As Double = 0.9,
                                                                                              Optional gt As Double = 0.7) As BTreeCluster
        Dim list = data _
            .Select(Function(d)
                        Return New NamedValue(Of Dictionary(Of String, Double)) With {
                            .Name = d.Key,
                            .Value = d.Properties
                        }
                    End Function) _
            .ToArray
        Dim compares As New AlignmentComparison(list, equals, gt)
        Dim btree As New AVLTree(Of String, String)(compares.GetComparer, Function(str) str)

        For Each id As String In list.Keys
            Call btree.Add(id, id, valueReplace:=False)
        Next

        Return BTreeCluster.GetClusters(btree)
    End Function

    <Extension>
    Public Function BTreeCluster(uniqueId As IEnumerable(Of String), alignment As ComparisonProvider) As BTreeCluster
        Dim btree As New AVLTree(Of String, String)(alignment.GetComparer, Function(str) str)

        For Each id As String In uniqueId
            Call btree.Add(id, id, valueReplace:=False)
        Next

        Return BTreeCluster.GetClusters(btree)
    End Function
End Module
