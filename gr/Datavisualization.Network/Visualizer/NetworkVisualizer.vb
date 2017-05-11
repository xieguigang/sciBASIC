#Region "Microsoft.VisualBasic::67d848cf0dfdbbce09d09dff553b0fd0, ..\sciBASIC#\gr\Datavisualization.Network\VisualizationExtensions\NetworkVisualizer.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' Image drawing of a network model
''' </summary>
<PackageNamespace("Network.Visualizer", Publisher:="xie.guigang@gmail.com")>
Public Module NetworkVisualizer

    ''' <summary>
    ''' This background color was picked from https://github.com/whichlight/reddit-network-vis
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BackgroundColor As Color = Color.FromArgb(219, 243, 255)
    Public ReadOnly Property DefaultEdgeColor As Color = Color.FromArgb(131, 131, 131)

    <Extension>
    Public Function GetDisplayText(n As Node) As String
        If n.Data Is Nothing OrElse n.Data.origID.StringEmpty Then
            Return n.ID
        Else
            Return n.Data.origID
        End If
    End Function

    <Extension>
    Private Function __calOffsets(nodes As Dictionary(Of Node, Point), size As Size) As Point
        Return nodes.Values.CentralOffset(size)
    End Function

    <Extension>
    Private Function __scale(nodes As Node(), scale!) As Dictionary(Of Node, Point)
        Dim table As New Dictionary(Of Node, Point)

        For Each n As Node In nodes
            With n.Data.initialPostion.Point2D
                Call table.Add(n, New Point(.X * scale, .Y * scale))
            End With
        Next

        Return table
    End Function

    Const WhiteStroke$ = "stroke: white; stroke-width: 2px; stroke-dash: solid;"

    ''' <summary>
    ''' 假若属性是空值的话，在绘图之前可以调用<see cref="ApplyAnalysis"/>拓展方法进行一些分析
    ''' </summary>
    ''' <param name="net"></param>
    ''' <param name="frameSize"></param>
    ''' <param name="padding">上下左右的边距分别为多少？</param>
    ''' <param name="background">背景色或者背景图片的文件路径</param>
    ''' <param name="defaultColor"></param>
    ''' <returns></returns>
    <ExportAPI("Draw.Image")>
    <Extension>
    Public Function DrawImage(net As NetworkGraph,
                              frameSize As Size,
                              Optional padding$ = g.DefaultPadding,
                              Optional background$ = "white",
                              Optional defaultColor As Color = Nothing,
                              Optional displayId As Boolean = True,
                              Optional labelColorAsNodeColor As Boolean = False,
                              Optional nodeStroke$ = WhiteStroke,
                              Optional scale! = 1) As GraphicsData
        Dim br As Brush
        Dim rect As Rectangle
        Dim cl As Color
        Dim scalePos = net.nodes.ToArray.__scale(scale)
        Dim offset As Point = scalePos.__calOffsets(frameSize)
        Dim margin As Padding = padding
        Dim stroke As Pen = CSS.Stroke.TryParse(nodeStroke).GDIObject

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

                For Each edge As Edge In net.edges
                    Dim n As Node = edge.Source
                    Dim otherNode As Node = edge.Target

                    cl = DefaultEdgeColor

                    If edge.Data.weight < 0.5 Then
                        cl = Color.Gray
                    ElseIf edge.Data.weight < 0.75 Then
                        cl = Color.Blue
                    End If

                    Dim w As Integer = 5 * edge.Data.weight
                    w = If(w < 1.5, 1.5, w)
                    Dim lineColor As New Pen(cl, w)

                    ' 在这里绘制的是节点之间相连接的边
                    Dim a = scalePos(n), b = scalePos(otherNode)

                    Call g.DrawLine(
                        lineColor,
                        a.OffSet2D(offset),
                        b.OffSet2D(offset))
                Next

                defaultColor = If(defaultColor.IsEmpty, Color.Black, defaultColor)

                Dim pt As Point

                For Each n As Node In net.nodes  ' 在这里进行节点的绘制
                    Dim r As Single = n.Data.radius

                    If r = 0! Then
                        r = If(n.Data.Neighborhoods < 30, n.Data.Neighborhoods * 9, n.Data.Neighborhoods * 7)
                        r = If(r = 0, 9, r)
                    End If

                    br = If(n.Data.Color Is Nothing, New SolidBrush(defaultColor), n.Data.Color)
                    pt = scalePos(n)
                    With pt
                        pt = New Point(.X - r / 2, .Y - r / 2)
                    End With
                    pt = pt.OffSet2D(offset)
                    rect = New Rectangle(pt, New Size(r, r))

                    Call g.FillPie(br, rect, 0, 360)
                    Call g.DrawEllipse(stroke, rect)

                    If displayId Then
                        Dim Font As Font = New Font(FontFace.Ubuntu, 12 + n.Data.Neighborhoods)
                        Dim s As String = n.GetDisplayText
                        Dim size As SizeF = g.MeasureString(s, Font)
                        Dim sloci As New Point(pt.X - size.Width / 2, pt.Y + r / 2 + 2)

                        If sloci.X < margin.Left Then
                            sloci = New Point(margin.Left, sloci.Y)
                        End If
                        If sloci.Y + size.Height > frameSize.Height - margin.Bottom Then
                            sloci = New Point(sloci.X, frameSize.Height - margin.Bottom - size.Height)
                        End If
                        If sloci.X + size.Width > frameSize.Width - margin.Right Then
                            sloci = New Point(frameSize.Width - margin.Right - size.Width, sloci.Y)
                        End If

                        If Not labelColorAsNodeColor Then
                            br = Brushes.Black
                        End If

                        Call g.DrawString(s, Font, br, sloci)
                    End If
                Next
            End Sub

        Return GraphicsPlots(frameSize, margin, background, plotInternal)
    End Function
End Module
