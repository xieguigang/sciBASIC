Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining.UMAP

Public Module UmapGraph

    <Extension>
    Public Function CreateGraph(umap As Umap, labels As String()) As NetworkGraph
        Dim matrix = umap.GetGraph.ToArray
        Dim g As New NetworkGraph

        For Each label As String In labels
            Call g.CreateNode(label)
        Next

        For i As Integer = 0 To matrix.Length - 1
            For j As Integer = 0 To matrix(i).Length - 1
                If i <> j AndAlso matrix(i)(j) <> 0 Then
                    g.CreateEdge(labels(i), labels(j), weight:=matrix(i)(j))
                End If
            Next
        Next

        Return g
    End Function
End Module
