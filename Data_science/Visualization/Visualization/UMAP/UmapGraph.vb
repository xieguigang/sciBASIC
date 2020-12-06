Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Language

Public Module UmapGraph

    <Extension>
    Public Function CreateGraph(umap As Umap, labels As String()) As NetworkGraph
        Dim matrix = umap.GetGraph.ToArray
        Dim g As New NetworkGraph
        Dim points As PointF() = Nothing
        Dim data As NodeData = Nothing
        Dim index As i32 = Scan0

        If umap.dimension = 2 Then
            points = umap.GetPoint2D
        End If

        For Each label As String In labels
            If Not points Is Nothing Then
                data = New NodeData With {
                    .label = label,
                    .origID = label,
                    .initialPostion = New FDGVector2(points(++index))
                }
            Else
                data = Nothing
            End If

            g.CreateNode(label, data)
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
