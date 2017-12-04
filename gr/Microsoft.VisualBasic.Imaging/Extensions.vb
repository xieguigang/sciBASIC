#Region "Microsoft.VisualBasic::a35e5eb902bb37a64314d877fd2c5d57, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Extensions.vb"

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
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    ''' <summary>
    ''' 假若目标图像是svg类型，则会被合成为gdi图像，如果是gdi图像，则会被直接转换
    ''' </summary>
    ''' <param name="img"></param>
    ''' <returns></returns>
    <Extension> Public Function AsGDIImage(img As GraphicsData) As Image
        If img.Driver = Drivers.GDI Then
            Return DirectCast(img, ImageData).Image
        Else
            ' 将svg矢量图合成为gdi图像
            Return DirectCast(img, SVGData).Render
        End If
    End Function

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, brush As Brush, points As Point(), radius#)
        Dim size As New Size(radius * 2, radius * 2)
        Dim offset = -radius
        For Each point As Point In points
            Dim rect As New Rectangle(point.OffSet2D(offset, offset), size)
            Call g.FillEllipse(brush, rect)
        Next
    End Sub

    <Extension>
    Public Sub FillCircles(ByRef g As IGraphics, points As Point(), fill As Func(Of Integer, Point, Brush), radius#)
        Dim size As New Size(radius * 2, radius * 2)
        Dim offset = -radius

        For Each point As SeqValue(Of Point) In points.SeqIterator
            Dim rect As New Rectangle((+point).OffSet2D(offset, offset), size)
            Call g.FillEllipse(fill(point, point), rect)
        Next
    End Sub
End Module
