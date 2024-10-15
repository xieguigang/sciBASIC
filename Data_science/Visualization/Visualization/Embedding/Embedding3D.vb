#Region "Microsoft.VisualBasic::7ae53c9a7cac78ab6fdc8e8d2b7b13ae, Data_science\Visualization\Visualization\Embedding\Embedding3D.vb"

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

    '   Total Lines: 67
    '    Code Lines: 57 (85.07%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (14.93%)
    '     File Size: 2.67 KB


    ' Class Embedding3D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Impl
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Public Class Embedding3D : Inherits EmbeddingRender

    ReadOnly camera As Camera
    ReadOnly bubbleAlpha%

    Public Sub New(umap As IDataEmbedding, camera As Camera, labels$(), clusters As Dictionary(Of String, String), colorSet$, bubbleAlpha%, theme As Theme)
        MyBase.New(umap, labels, clusters, colorSet, theme)

        Me.camera = camera
        Me.bubbleAlpha = bubbleAlpha
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
                            .PointSize = theme.pointSize,
                            .Title = cluster.Key
                        }
                    End Function) _
            .ToArray
        Dim engine As New Scatter3D(
            serials:=serials,
            camera:=camera,
            arrowFactor:="1,1",
            showHull:=bubbleAlpha > 0,
            hullAlpha:=bubbleAlpha,
            hullBspline:=2,
            theme:=theme
        )
        Dim css As CSSEnvirnment = g.LoadEnvironment

        Call engine.Plot(g, canvas.PlotRegion(css))
    End Sub
End Class
