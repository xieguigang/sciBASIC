#Region "Microsoft.VisualBasic::248fb84b37195b77d27e94ee68128e41, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\Device\Element3D.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
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

    Public Class Polygon : Inherits Element3D

        Public Property Path As PointF()

        Public Overrides Sub Draw(g As IGraphics, offset As PointF)

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

        Public Overrides Sub Draw(g As IGraphics, offset As PointF)
            Dim position As Point = GetPosition(g).OffSet2D(offset)
            Call g.DrawLegendShape(position, Size, Style, Fill)
        End Sub
    End Class
End Namespace
