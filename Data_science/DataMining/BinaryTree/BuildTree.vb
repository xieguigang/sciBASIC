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
End Module


