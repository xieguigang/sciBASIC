Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Impl
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Public Class Umap3D : Inherits UmapRender

    ReadOnly camera As Camera

    Public Sub New(umap As Umap, camera As Camera, labels$(), clusters As Dictionary(Of String, String), colorSet$, theme As Theme)
        MyBase.New(umap, labels, clusters, colorSet, theme)

        Me.camera = camera
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim embeddings As Point3D() = umap.GetPoint3D
        Dim clusterSerials As New Dictionary(Of String, List(Of NamedValue(Of Point3D)))
        Dim clusterName As String
        Dim colors = GetClusterColors()

        For i As Integer = 0 To embeddings.Length - 1
            clusterName = getClusterLabel(i)

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
                            .Points = cluster.Value.ToArray,
                            .Shape = LegendStyles.Triangle,
                            .Color = colors(cluster.Key).Color,
                            .PointSize = 5,
                            .Title = cluster.Key
                        }
                    End Function) _
            .ToArray
        Dim engine As New Scatter3D(
            serials:=serials,
            camera:=camera,
            arrowFactor:="1,1",
            showHull:=False,
            hullAlpha:=0.5,
            hullBspline:=2,
            theme:=theme
        )

        Call engine.Plot(g, canvas.PlotRegion)
    End Sub
End Class
