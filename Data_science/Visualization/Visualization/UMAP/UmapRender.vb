Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq

Public MustInherit Class UmapRender : Inherits Plot

    Protected ReadOnly labels As String()
    Protected ReadOnly clusters As Dictionary(Of String, String)
    Protected ReadOnly umap As Umap

    ReadOnly colorSet As String

    Protected Sub New(umap As Umap, labels$(), clusters As Dictionary(Of String, String), colorSet$, theme As Theme)
        MyBase.New(theme)

        Me.clusters = clusters
        Me.colorSet = colorSet
        Me.umap = umap

        If Not labels Is Nothing Then
            Me.labels = labels.ToArray
        Else
            Me.labels = umap.GetGraph.Dims.rows _
                .SeqIterator _
                .Select(Function(i) $"x_{i + 1}") _
                .ToArray
        End If
    End Sub

    Protected Function GetClusterColors() As Dictionary(Of String, SolidBrush)
        Dim clusterLabels As String() = clusters.Values.Distinct.ToArray
        Dim colors As Color() = Designer.GetColors(colorSet, clusterLabels.Length)
        Dim map As New Dictionary(Of String, SolidBrush)

        For i As Integer = 0 To clusterLabels.Length - 1
            map(clusterLabels(i)) = New SolidBrush(colors(i))
        Next

        Return map
    End Function
End Class
