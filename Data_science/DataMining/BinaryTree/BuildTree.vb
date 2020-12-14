Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
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
End Module


