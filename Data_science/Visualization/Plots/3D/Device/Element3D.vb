#Region "Microsoft.VisualBasic::2eeb0d6e28245a132ca775da8105c60b, Data_science\Visualization\Plots\3D\Device\Element3D.vb"

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

    '     Class Element3D
    ' 
    '         Properties: Location
    ' 
    '         Function: GetPosition, ToString
    ' 
    '         Sub: Transform
    ' 
    '     Class Polygon
    ' 
    '         Properties: Brush, Path
    ' 
    '         Function: EnumeratePath
    ' 
    '         Sub: Draw, Transform
    ' 
    '     Class ConvexHullPolygon
    ' 
    '         Properties: bspline
    ' 
    '         Sub: Draw
    ' 
    '     Class Label
    ' 
    '         Properties: Color, Font, Text
    ' 
    '         Function: EnumeratePath
    ' 
    '         Sub: Draw
    ' 
    '     Class Line
    ' 
    '         Properties: A, B, Stroke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: EnumeratePath
    ' 
    '         Sub: __init, Draw, Transform
    ' 
    '     Class ShapePoint
    ' 
    '         Properties: Fill, Label, Point2D, Size, Style
    ' 
    '         Function: EnumeratePath
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Plot3D.Device

    ''' <summary>
    ''' 因为先绘制坐标轴再绘制系列点，会没有太多层次感，所以在这里首先需要将这些需要绘制的原件转换为这个元素对象，然后做一次Z排序生成绘图顺序
    ''' 最后再调用<see cref="Draw"/>方法进行3D图表的绘制
    ''' </summary>
    Public MustInherit Class Element3D

        Public Property Location As Point3D

        Public MustOverride Sub Draw(g As IGraphics, rect As GraphicsRegion, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
        Public MustOverride Function EnumeratePath() As IEnumerable(Of Point3D)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Transform(camera As Camera)
            Location = camera.Project(camera.Rotate(Location))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPosition(frameSize As Size) As PointF
            Return Location.PointXY(frameSize)
        End Function

        Public Overrides Function ToString() As String
            Return Location.ToString
        End Function
    End Class

    ''' <summary>
    ''' 一个三维空间之中的面
    ''' </summary>
    Public Class Polygon : Inherits Element3D

        Public Property Path As Point3D()
        Public Property Brush As Brush

        Public Overrides Sub Transform(camera As Camera)
            Path = Path.Select(Function(p) camera.Project(camera.Rotate(p))).ToArray
            Location = Path.Center
        End Sub

        Public Overrides Function EnumeratePath() As IEnumerable(Of Point3D)
            Return Path.AsEnumerable
        End Function

        Public Overrides Sub Draw(g As IGraphics, rect As GraphicsRegion, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim screen As Size = rect.Size
            Dim shape As PointF() = Path _
                .Select(Function(p)
                            Return p.PointXY(screen)
                        End Function) _
                .Select(Function(p)
                            Return New PointF(scaleX(p.X), scaleY(p.Y))
                        End Function) _
                .ToArray

            Call g.FillPolygon(Brush, shape)
        End Sub
    End Class

    Public Class ConvexHullPolygon : Inherits Polygon

        Public Property bspline As Single = 2

        Public Overrides Sub Draw(g As IGraphics, rect As GraphicsRegion, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim screen As Size = rect.Size
            Dim shape As PointF() = Path _
                .Select(Function(p)
                            Return p.PointXY(screen)
                        End Function) _
                .Select(Function(p)
                            Return New PointF(scaleX(p.X), scaleY(p.Y))
                        End Function) _
                .ToArray

            If shape.Length > 2 Then
                shape = shape.JarvisMatch

                If bspline > 1 Then
                    shape = shape.BSpline(degree:=bspline).ToArray
                End If

                If shape.Length > 0 Then
                    Call g.FillPolygon(Brush, shape)
                End If
            End If
        End Sub
    End Class

    Public Class Label : Inherits Element3D

        Public Property Text As String
        Public Property Font As Font
        Public Property Color As Brush

        Public Overrides Function EnumeratePath() As IEnumerable(Of Point3D)
            Return {Location}
        End Function

        <DebuggerStepThrough>
        Public Overrides Sub Draw(g As IGraphics, rect As GraphicsRegion, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim praw As PointF = GetPosition(rect.Size)
            Dim pscale As New PointF(scaleX(praw.X), scaleY(praw.Y))

            Call g.DrawString(Text, Font, Color, pscale)
        End Sub
    End Class

    Public Class Line : Inherits Element3D

        Public ReadOnly Property A As Point3D
        Public ReadOnly Property B As Point3D
        Public Property Stroke As Pen

        ''' <summary>
        ''' 线段的<see cref="Location"/>位置数据会自动从<paramref name="a"/>和
        ''' <paramref name="b"/>计算出来
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        Sub New(a As Point3D, b As Point3D)
            Me.A = a
            Me.B = b

            Call Me.__init()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub __init()
            Me.Location = New Point3D With {
                .X = (A.X + B.X) / 2,
                .Y = (A.Y + B.Y) / 2,
                .Z = (A.Z + B.Z) / 2
            }
        End Sub

        Public Overrides Function EnumeratePath() As IEnumerable(Of Point3D)
            Return {A, B}
        End Function

        Public Overrides Sub Draw(g As IGraphics, rect As GraphicsRegion, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim size As Size = rect.Size
            Dim p1 As PointF = A.PointXY(size)
            Dim p2 As PointF = B.PointXY(size)

            p1 = New PointF(scaleX(p1.X), scaleY(p1.Y))
            p2 = New PointF(scaleX(p2.X), scaleY(p2.Y))

            Call g.DrawLine(Stroke, p1, p2)
        End Sub

        Public Overrides Sub Transform(camera As Camera)
            Dim list = camera.Project(camera.Rotate({A, B})).ToArray

            _A = list(0)
            _B = list(1)

            Call Me.__init()
        End Sub
    End Class

    Public Class ShapePoint : Inherits Element3D

        Public Property Size As Size
        Public Property Fill As Brush
        Public Property Style As LegendStyles
        Public Property Label As String

        ''' <summary>
        ''' Project the 3D point to the location on 2D plot canvas
        ''' </summary>
        ''' <param name="frameSize">The size of 2D plot canvas</param>
        ''' <returns></returns>
        Public ReadOnly Property Point2D(frameSize As Size) As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetPosition(frameSize)
            End Get
        End Property

        Public Overrides Function EnumeratePath() As IEnumerable(Of Point3D)
            Return {Location}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Draw(g As IGraphics, rect As GraphicsRegion, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim praw As PointF = GetPosition(rect.Size)
            Dim pscale As New PointF(scaleX(praw.X), scaleY(praw.Y))

            Call g.DrawLegendShape(pscale, Size, Style, Fill)
        End Sub
    End Class
End Namespace
