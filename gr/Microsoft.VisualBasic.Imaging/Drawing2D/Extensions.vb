#Region "Microsoft.VisualBasic::d413443711d19e0ae6df0214a17b7395, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Extensions.vb"

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
Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension
Imports Vec = Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector

Namespace Drawing2D

    Public Module Extensions

        <Extension>
        Public Function Enlarge(size As SizeF, fold#) As SizeF
            With size
                Return New SizeF(.Width * fold, .Height * fold)
            End With
        End Function

        ''' <summary>
        ''' 将一个多边形放大指定的倍数<paramref name="scale"/>
        ''' </summary>
        ''' <param name="shape">矢量图形的点集合</param>
        ''' <param name="scale#"></param>
        ''' <returns></returns>
        <Extension> Public Function Enlarge(shape As IEnumerable(Of Point), scale#) As Point()
            Dim vector = shape.ToArray
            Dim center = vector.Centre
            Dim x As New Vec(vector.Select(Function(pt) pt.X))
            Dim y As New Vec(vector.Select(Function(pt) pt.Y))
            Dim b = x - CDbl(center.X)
            Dim a = y - CDbl(center.Y)
            Dim c = VectorMath.Sqrt(b ^ 2 + a ^ 2)
            Dim cs = c * scale
            Dim dc = cs - c
            Dim dx = (b / c) * dc
            Dim dy = (a / c) * dc

            x = x + dx
            y = y + dy

            ' 返回放大之后的矢量图形向量
            Return vector _
                .Sequence _
                .Select(Function(i) New Point(x(i), y(i))) _
                .ToArray
        End Function

        ''' <summary>
        ''' Move the shape its bounds box topleft to target place
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <param name="topLeft"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MoveTo(shape As IEnumerable(Of PointF), topLeft As PointF) As PointF()
            Dim polygon = shape.ToArray
            Dim rect As RectangleF = polygon.GetBounds
            Dim offset As New PointF(rect.Left - topLeft.X, rect.Top - topLeft.Y)
            Dim out = polygon _
                .Select(Function(point)
                            Return New PointF(point.X - offset.X,
                                              point.Y - offset.Y)
                        End Function) _
                .ToArray
            Return out
        End Function

        <Extension>
        Public Function MoveTo(shape As IEnumerable(Of Point), topLeft As PointF) As Point()
            Return shape _
                .Select(Function(point) point.PointF) _
                .MoveTo(topLeft) _
                .Select(Function(point) point.ToPoint) _
                .ToArray
        End Function
    End Module
End Namespace
