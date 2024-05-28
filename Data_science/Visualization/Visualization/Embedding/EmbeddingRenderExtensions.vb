#Region "Microsoft.VisualBasic::9e53ed8beaab66db07dc60aedd34ad29, Data_science\Visualization\Visualization\Embedding\EmbeddingRenderExtensions.vb"

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

    '   Total Lines: 147
    '    Code Lines: 102 (69.39%)
    ' Comment Lines: 36 (24.49%)
    '    - Xml Docs: 97.22%
    ' 
    '   Blank Lines: 9 (6.12%)
    '     File Size: 6.08 KB


    ' Module EmbeddingRenderExtensions
    ' 
    '     Function: DrawEmbedding2D, DrawEmbedding3D, GetPoint2D, GetPoint3D
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS

<HideModuleName>
Public Module EmbeddingRenderExtensions

    ''' <summary>
    ''' get embedding result as spatial location
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetPoint2D(umap As IDataEmbedding) As PointF()
        If umap.dimension <> 2 Then
            Throw New InvalidProgramException($"the given umap projection result(dimension={umap.dimension}) is not a 2D data!")
        Else
            Return umap.GetEmbedding() _
                .Select(Function(vec)
                            Return New PointF With {.X = vec(0), .Y = vec(1)}
                        End Function) _
                .ToArray()
        End If
    End Function

    ''' <summary>
    ''' get embedding result as spatial location
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetPoint3D(umap As IDataEmbedding) As Point3D()
        If umap.dimension <> 3 Then
            Throw New InvalidProgramException($"the given umap projection result(dimension={umap.dimension}) is not a 3D data!")
        Else
            Return umap.GetEmbedding _
                .Select(Function(vec)
                            Return New Point3D With {.X = vec(Scan0), .Y = vec(1), .Z = vec(2)}
                        End Function) _
                .ToArray
        End If
    End Function

    ''' <summary>
    ''' plot 2D projection result of umap
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <param name="labels"></param>
    ''' <param name="clusters">[label => clusterid]</param>
    ''' <returns></returns>
    <Extension>
    Public Function DrawEmbedding2D(umap As IDataEmbedding,
                                    Optional labels As IEnumerable(Of String) = Nothing,
                                    Optional clusters As Dictionary(Of String, String) = Nothing,
                                    Optional size$ = "2440,1920",
                                    Optional padding$ = g.DefaultUltraLargePadding,
                                    Optional colorSet$ = "Set1:c8",
                                    Optional showConvexHull As Boolean = True,
                                    Optional pointSize% = 10,
                                    Optional legendLabelCSS$ = CSSFont.PlotLabelNormal) As GraphicsData

        Dim theme As New Theme With {
            .padding = padding,
            .pointSize = pointSize,
            .legendLabelCSS = legendLabelCSS,
            .legendSplitSize = If(clusters Is Nothing OrElse clusters.Count <= 5, 0, 5),
            .legendBoxStroke = Nothing,
            .axisTickCSS = CSSFont.Win7LargerNormal
        }

        Return New Embedding2D(
            umap:=umap,
            labels:=labels.SafeQuery.ToArray,
            clusters:=clusters,
            colorSet:=colorSet,
            theme:=theme,
            showConvexHull:=showConvexHull
        ).Plot(size)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <param name="camera"></param>
    ''' <param name="labels"></param>
    ''' <param name="clusters">[label => clusterid]</param>
    ''' <param name="size$"></param>
    ''' <param name="padding$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="colorSet$"></param>
    ''' <param name="axisLabelCSS$"></param>
    ''' <param name="axisStroke$"></param>
    ''' <param name="labelCSS$"></param>
    ''' <param name="pointSize#"></param>
    ''' <param name="showLabels"></param>
    ''' <param name="labelColor$"></param>
    ''' <param name="bubbleAlpha"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DrawEmbedding3D(umap As IDataEmbedding, camera As Camera,
                                    Optional labels As IEnumerable(Of String) = Nothing,
                                    Optional clusters As Dictionary(Of String, String) = Nothing,
                                    Optional size$ = "2048,2048",
                                    Optional padding$ = g.DefaultPadding,
                                    Optional bg$ = "white",
                                    Optional colorSet$ = "Set1:c8",
                                    Optional axisLabelCSS$ = CSSFont.PlotLabelNormal,
                                    Optional axisStroke$ = Stroke.AxisStroke,
                                    Optional labelCSS$ = CSSFont.Win10Normal,
                                    Optional pointSize# = 10,
                                    Optional showLabels As Boolean = True,
                                    Optional labelColor$ = "black",
                                    Optional bubbleAlpha As Integer = 0) As GraphicsData

        Dim theme As New Theme With {
            .padding = padding,
            .axisLabelCSS = axisLabelCSS,
            .axisStroke = axisStroke,
            .tagCSS = labelCSS,
            .background = bg,
            .drawLabels = showLabels,
            .pointSize = pointSize,
            .tagColor = labelColor
        }

        Return New Embedding3D(
            umap:=umap,
            labels:=labels.SafeQuery.ToArray,
            clusters:=clusters,
            colorSet:=colorSet,
            theme:=theme,
            camera:=camera,
            bubbleAlpha:=bubbleAlpha
        ) With {
            .xlabel = "dim #1",
            .ylabel = "dim #2",
            .zlabel = "dim #3"
        }.Plot(size)
    End Function
End Module
