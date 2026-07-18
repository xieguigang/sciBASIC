#Region "Microsoft.VisualBasic::30782ac99839677d404f58678506cfb7, gr\network-visualization\Visualizer\Render\HullPolygonRendering.vb"

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

    '   Total Lines: 133
    '    Code Lines: 106 (79.70%)
    ' Comment Lines: 12 (9.02%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 15 (11.28%)
    '     File Size: 5.68 KB


    ' Class HullPolygonRendering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: RenderHull
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

''' <summary>
''' 将网络节点按照分组绘制出凸包多边形（convex hull polygon）以及其图例。
''' 
''' 该类由原 <see cref="NetworkVisualizer"/> 模块内的私有扩展方法
''' <c>drawhullPolygon</c> 迁移而来，改为从共享的 <see cref="NetworkRenderConfig"/>
''' 配置对象读取参数，消除原先一长串的函数参数传递。
''' </summary>
Friend Class HullPolygonRendering

    ReadOnly config As NetworkRenderConfig
    ReadOnly scalePos As Dictionary(Of String, PointF)

    Sub New(config As NetworkRenderConfig, scalePos As Dictionary(Of String, PointF))
        Me.config = config
        Me.scalePos = scalePos
    End Sub

    ''' <summary>
    ''' 渲染分组凸包多边形层
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="drawPoints">将会被绘制到画布上面的节点集合</param>
    Public Sub RenderHull(g As IGraphics, drawPoints As Node())
        Dim hullPolygonGroups = config.HullPolygonGroups
        Dim hullPolygon As Index(Of String)
        Dim groups = drawPoints _
            .Where(Function(n) Not n.data(hullPolygonGroups.Name).StringEmpty) _
            .GroupBy(Function(n)
                         Return n.data(hullPolygonGroups.Name)
                     End Function) _
            .ToArray
        Dim colors As LoopArray(Of Color) = Designer.GetColors(hullPolygonGroups.Description Or "set1".AsDefault)
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim convexHullLabelFont As Font = css.GetFont(CSSFont.TryParse(config.ConvexHullLabelFontCSS))
        Dim singleGroupKey As String = Nothing

        If hullPolygonGroups.Value.StringEmpty Then
            Return
        End If

        If hullPolygonGroups.Value.TextEquals("max") Then
            singleGroupKey = $"max({hullPolygonGroups.Name})"
            hullPolygon = {
                groups.OrderByDescending(Function(node) node.Count) _
                      .First _
                      .Key
            }
        ElseIf hullPolygonGroups.Value.TextEquals("min") Then
            singleGroupKey = $"min({hullPolygonGroups.Name})"
            hullPolygon = {
                groups.Where(Function(group) group.Count > 2) _
                      .OrderBy(Function(node) node.Count) _
                      .First _
                      .Key
            }
        ElseIf hullPolygonGroups.Value.IsPattern("top\s*\d+") Then
            hullPolygon = groups _
                .Where(Function(group) group.Count > 2) _
                .OrderByDescending(Function(n) n.Count) _
                .Take(hullPolygonGroups.Value.Match("\d+").DoCall(AddressOf Integer.Parse)) _
                .Select(Function(group) group.Key) _
                .ToArray
        Else
            hullPolygon = hullPolygonGroups.Value.Split(","c)
        End If

        Dim labels As New List(Of (String, Color))

        For Each group In groups
            If group.Count > 2 AndAlso group.Key Like hullPolygon Then

                Call $"[ConvexHull] render for {group.Key}".debug

                Dim positions = group _
                    .Select(Function(p) scalePos(p.label)) _
                    .ConcaveHull _ ' .JarvisMatch _
                    .Enlarge(config.ConvexHullScale)
                Dim color As Color = colors.Next

                Call g.DrawHullPolygon(
                    polygon:=positions,
                    color:=color,
                    alpha:=50,
                    convexHullCurveDegree:=config.ConvexHullCurveDegree,
                    fillPolygon:=config.FillConvexHullPolygon
                )
                Call labels.Add((group.Key, color))
            End If
        Next

        If Not singleGroupKey.StringEmpty Then
            labels = New List(Of (String, Color)) From {(singleGroupKey, labels.Last.Item2)}
        End If

        If config.ShowConvexHullLegend Then
            Dim maxLabel = labels.Select(Function(lb) lb.Item1).MaxLengthString
            Dim maxSize As SizeF = g.MeasureString(maxLabel, convexHullLabelFont)
            Dim legendShapeSize As New SizeF With {
                .Width = maxSize.Height * 1.5,
                .Height = maxSize.Height
            }
            Dim topLeft As New PointF With {
                .X = g.Size.Width - maxSize.Width - maxSize.Height * 2.5,
                .Y = legendShapeSize.Width
            }

            For Each label In labels
                Call g.FillRectangle(New SolidBrush(label.Item2), New RectangleF(topLeft, legendShapeSize))
                Call g.DrawString(label.Item1, convexHullLabelFont, Brushes.Black, New PointF(topLeft.X + legendShapeSize.Width + 20, topLeft.Y))

                topLeft = New PointF With {
                    .X = topLeft.X,
                    .Y = topLeft.Y + maxSize.Height * 1.25
                }
            Next
        End If
    End Sub
End Class

