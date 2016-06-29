#Region "Microsoft.VisualBasic::aa995db37fbcf8615914e0bef32934a5, ..\Datavisualization.Network\Datavisualization.Network\Visualize\NetworkVisualizer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq

Namespace Visualize

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
            If n.Data Is Nothing OrElse n.Data.origID.IsBlank Then
                Return n.ID
            Else
                Return n.Data.origID
            End If
        End Function

        <Extension>
        Private Function __calOffsets(net As NetworkGraph, size As Size) As Point
            Dim nodes As Point() =
            net.nodes.ToArray(Function(n) n.Data.initialPostion.Point2D)
            Return nodes.CentralOffset(size)
        End Function

        ''' <summary>
        ''' 假若属性是空值的话，在绘图之前可以调用<see cref="ApplyAnalysis"/>拓展方法进行一些分析
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="frameSize"></param>
        ''' <param name="margin"></param>
        ''' <param name="backgroundImage"></param>
        ''' <param name="defaultColor"></param>
        ''' <returns></returns>
        <ExportAPI("Draw.Image")>
        <Extension>
        Public Function DrawImage(net As NetworkGraph,
                              frameSize As Size,
                              Optional margin As Point = Nothing,
                              Optional backgroundImage As Image = Nothing,
                              Optional defaultColor As Color = Nothing,
                              Optional displayId As Boolean = True) As Bitmap

            Dim br As Brush
            Dim rect As Rectangle
            Dim cl As Color
            Dim offset As Point = net.__calOffsets(frameSize)

            Using Graphic As GDIPlusDeviceHandle = frameSize.CreateGDIDevice

                If backgroundImage Is Nothing Then
                    br = New SolidBrush(BackgroundColor)
                    rect = New Rectangle(
                    New Point,
                    New Size(frameSize.Width, frameSize.Height))
                    Call Graphic.FillRectangle(br, rect) '绘制背景纹理
                Else
                    Call Graphic.DrawImage(backgroundImage, 0, 0, frameSize.Width, frameSize.Height)
                End If

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
                    Dim LineColor As New Pen(cl, w)

                    Call Graphic.DrawLine(   ' 在这里绘制的是节点之间相连接的边
                    LineColor,
                    n.Data.initialPostion.Point2D.OffSet2D(offset),
                    otherNode.Data.initialPostion.Point2D.OffSet2D(offset))
                Next

                margin = If(margin.IsEmpty, New Point(3, 3), margin)
                defaultColor = If(defaultColor.IsEmpty, Color.Black, defaultColor)

                Dim pt As Point

                For Each n As Node In net.nodes  ' 在这里进行节点的绘制
                    Dim r As Single = n.Data.radius

                    If r = 0! Then
                        r = If(n.Data.Neighborhoods < 30, n.Data.Neighborhoods * 9, n.Data.Neighborhoods * 7)
                        r = If(r = 0, 9, r)
                    End If

                    br = If(n.Data.Color Is Nothing, New SolidBrush(defaultColor), n.Data.Color)
                    pt = New Point(n.Data.initialPostion.x - r / 2, n.Data.initialPostion.y - r / 2)
                    pt = pt.OffSet2D(offset)
                    rect = New Rectangle(pt, New Size(r, r))

                    Call Graphic.FillPie(br, rect, 0, 360)

                    If displayId Then
                        Dim Font As Font = New Font(FontFace.Ubuntu, 12 + n.Data.Neighborhoods)
                        Dim s As String = n.GetDisplayText
                        Dim size As SizeF = Graphic.MeasureString(s, Font)
                        Dim sloci As New Point(pt.X - size.Width / 2, pt.Y + r / 2 + 2)

                        If sloci.X < margin.X Then
                            sloci = New Point(margin.X, sloci.Y)
                        End If
                        If sloci.Y + size.Height > frameSize.Height - margin.Y Then
                            sloci = New Point(sloci.X, frameSize.Height - margin.Y - size.Height)
                        End If
                        If sloci.X + size.Width > frameSize.Width - margin.X Then
                            sloci = New Point(frameSize.Width - margin.X - size.Width, sloci.Y)
                        End If

                        Call Graphic.DrawString(s, Font, Brushes.Black, sloci)
                    End If
                Next

                Return Graphic.ImageResource
            End Using
        End Function
    End Module
End Namespace
