Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Impl
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Public Class Umap3D : Inherits UmapRender

    ReadOnly camera As Camera

    Public Sub New(umap As Umap, labels$(), clusters As Dictionary(Of String, String), colorSet$, theme As Theme)
        MyBase.New(umap, labels, clusters, colorSet, theme)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim embeddings As Point3D() = umap.GetPoint3D
        Dim clusterSerials As New Dictionary(Of String, List(Of NamedValue(Of Point3D)))
        Dim clusterName As String

        For i As Integer = 0 To embeddings.Length - 1
            clusterName = clusters(labels(i))

            If Not clusterSerials.ContainsKey(clusterName) Then
                clusterSerials.Add(clusterName, New List(Of NamedValue(Of Point3D)))
            End If

            Call New NamedValue(Of Point3D) With {
                .Name = labels(i),
                .Value = embeddings(i)
            }.DoCall(AddressOf clusterSerials(clusterName).Add)
        Next

        Dim serials As Serial3D() = clusterSerials _
            .Select(Function(cluster)
                        Return New Serial3D With {
                            .Points = cluster.Value.ToArray
                        }
                    End Function) _
            .ToArray
        Dim engine As New Scatter3D(serials, camera, "1,1", True, 0.5, 2, theme)

        Call engine.Plot(g, canvas.PlotRegion)
    End Sub
End Class
