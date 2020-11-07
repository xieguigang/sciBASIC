#Region "Microsoft.VisualBasic::553006dfd9f4917df20589e6777bc2c0, Data_science\Visualization\Plots\3D\Device\Element3D.vb"

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
    '         Properties: brush, Path
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
    '         Sub: Draw
    ' 
    '     Class Line
    ' 
    '         Properties: A, B, Stroke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: __init, Draw, Transform
    ' 
    '     Class ShapePoint
    ' 
    '         Properties: Fill, Label, Point2D, Size, Style
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

        Public MustOverride Sub Draw(g As IGraphics, offset As PointF)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Transform(camera As Camera)
            Location = camera.Project(camera.Rotate(Location))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPosition(g As IGraphics) As Point
            Return Location.PointXY(g.Size)
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
        Public Property brush As Brush

        Public Overrides Sub Transform(camera As Camera)
            Path = Path.Select(Function(p) camera.Project(camera.Rotate(p))).ToArray
            Location = Path.Center
        End Sub

        Public Overrides Sub Draw(g As IGraphics, offset As PointF)
            Dim screen As Size = g.Size
            Dim shape As Point() = Path _
                .Select(Function(p)
                            Return p.PointXY(screen).OffSet2D(offset)
                        End Function) _
                .ToArray

            Call g.FillPolygon(brush, shape)
        End Sub
    End Class

    Public Class ConvexHullPolygon : Inherits Polygon

        Public Property bspline As Single = 2

        Public Overrides Sub Draw(g As IGraphics, offset As PointF)
            Dim screen As Size = g.Size
            Dim shape As PointF() = Path _
                .Select(Function(p)
                            Return p.PointXY(screen).OffSet2D(offset).PointF
                        End Function) _
                .ToArray

            If shape.Length > 2 Then
                shape = shape.JarvisMatch

                If bspline > 1 Then
                    shape = shape.BSpline(degree:=bspline).ToArray
                End If

                Call g.FillPolygon(brush, shape)
            End If
        End Sub
    End Class

    Public Class Label : Inherits Element3D

        Public Property Text As String
        Public Property Font As Font
        Public Property Color As Brush

        Public Overrides Sub Draw(g As IGraphics, offset As PointF)
            Call g.DrawString(Text, Font, Color, GetPosition(g).OffSet2D(offset))
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

        Public Overrides Sub Draw(g As IGraphics, offset As PointF)
            Dim p1 As Point = A.PointXY(g.Size).OffSet2D(offset)
            Dim p2 As Point = B.PointXY(g.Size).OffSet2D(offset)

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
        ''' <param name="g">The 2D plot canvas</param>
        ''' <returns></returns>
        Public ReadOnly Property Point2D(g As IGraphics) As Point
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetPosition(g)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Draw(g As IGraphics, offset As PointF)
            Call g.DrawLegendShape(Point2D(g).OffSet2D(offset), Size, Style, Fill)
        End Sub
    End Class
End Namespace
