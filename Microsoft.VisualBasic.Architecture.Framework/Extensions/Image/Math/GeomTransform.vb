#Region "Microsoft.VisualBasic::fa2ca1194b5f77e8fb8a1a1281243d65, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\GDI+\GeomTransform.vb"

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
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports sys = System.Math

Namespace Imaging.Math2D

    <Package("GDI.Transform")> Public Module GeomTransform

        ''' <summary>
        ''' 这个方形区域的面积
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Area(rect As Rectangle) As Double
            Return rect.Width * rect.Height
        End Function

        ''' <summary>
        ''' Is target point in the target region?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function InRegion(x As Point, rect As Rectangle) As Boolean
            Return New PointF(x.X, x.Y).InRegion(rect)
        End Function

        ''' <summary>
        ''' Is target point in the target region?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <Extension>
        Public Function InRegion(x As PointF, rect As Rectangle) As Boolean
            If x.X < rect.Left OrElse x.X > rect.Right Then
                Return False
            End If
            If x.Y < rect.Top OrElse x.Y > rect.Bottom Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Calculate the center location of the target sized region
        ''' </summary>
        ''' <param name="size"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function GetCenter(size As Size) As Point
            Return New Point(size.Width / 2, size.Height / 2)
        End Function

        ''' <summary>
        ''' 返回位移的新的点位置值
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        <ExportAPI("Offset")>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function OffSet2D(p As Point, x As Integer, y As Integer) As Point
            Return New Point(x + p.X, y + p.Y)
        End Function

        ''' <summary>
        ''' 返回位置的新的点位置值
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <ExportAPI("Offset")>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function OffSet2D(p As Point, offset As Point) As Point
            Return p.OffSet2D(offset.PointF)
        End Function

        ''' <summary>
        ''' 返回位置的新的点位置值
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <ExportAPI("Offset")>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function OffSet2D(p As Point, offset As PointF) As Point
            Return New Point(offset.X + p.X, offset.Y + p.Y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function OffSet2D(pt As PointF, offset As PointF) As PointF
            With pt
                Return New PointF(offset.X + .X, offset.Y + .Y)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function OffSet2D(pt As PointF, x!, y!) As PointF
            With pt
                Return New PointF(x + .X, y + .Y)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MirrorX(pt As PointF, rect As RectangleF) As PointF
            Return New PointF With {
                .X = rect.Right - (pt.X - rect.Left),
                .Y = pt.Y
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pt">假设<paramref name="pt"/>是位于<paramref name="rect"/>内部的</param>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MirrorY(pt As PointF, rect As RectangleF) As PointF
            Return New PointF With {
                .X = pt.X,
                .Y = rect.Bottom - (pt.Y - rect.Top)
            }
        End Function

        ''' <summary>
        ''' <see cref="Graphics.DrawEllipse(Pen, RectangleF)"/>
        ''' </summary>
        ''' <param name="center"></param>
        ''' <param name="r!"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CircleRectangle(center As PointF, r!) As RectangleF
            Dim d = r * 2
            Return New RectangleF(center.OffSet2D(-r, -r), New SizeF(d, d))
        End Function

        <Extension>
        Public Function CenterAlign(rect As RectangleF, size As SizeF) As PointF
            Dim x! = (rect.Width - size.Width) / 2 + rect.Left
            Dim y! = (rect.Height - size.Height) / 2 + rect.Top
            Return New PointF(x, y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Distance(x1#, y1#, x2#, y2#) As Double
            Return sys.Sqrt((x1 - x2) ^ 2 + (y1 - y2) ^ 2)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Distance(points As IEnumerable(Of Point), anchor As Point) As Double()
            Return points _
                .Select(Function(pt) Distance(pt.X, pt.Y, anchor.X, anchor.Y)) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function CalculateAngle(p1 As Point, p2 As Point) As Double
            Dim xDiff As Single = p2.X - p1.X
            Dim yDiff As Single = p2.Y - p1.Y
            Return sys.Atan2(yDiff, xDiff) * 180.0 / PI
        End Function

        ''' <summary>
        ''' 获取目标多边形对象的边界结果，包括左上角的位置以及所占的矩形区域的大小
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetBounds(points As IEnumerable(Of Point)) As RectangleF
            Return points.Select(Function(pt) pt.PointF).GetBounds
        End Function

        ''' <summary>
        ''' 获取目标多边形对象的边界结果，包括左上角的位置以及所占的矩形区域的大小
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetBounds(points As IEnumerable(Of PointF)) As RectangleF
            Dim array = points.ToArray
            Dim xmin = array.Min(Function(pt) pt.X)
            Dim xmax = array.Max(Function(pt) pt.X)
            Dim ymin = array.Min(Function(pt) pt.Y)
            Dim ymax = array.Max(Function(pt) pt.Y)
            Dim topLeft As New PointF(xmin, ymin)
            Dim size As New SizeF(xmax - xmin, ymax - ymin)
            Return New RectangleF(topLeft, size)
        End Function

        ''' <summary>
        ''' Gets the center location of the region rectangle.
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <ExportAPI("Center")>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Centre(rect As Rectangle) As Point
            Return New Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2)
        End Function

        ''' <summary>
        ''' Resize the rectangle
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <param name="factor"></param>
        ''' <returns></returns>
        <Extension> Public Function Scale(rect As RectangleF, factor As SizeF) As RectangleF
            Dim size = New SizeF(rect.Width * factor.Width, rect.Height * factor.Height)
            Dim delta = size - rect.Size
            Dim location As New PointF(rect.Left - delta.Width / 2, rect.Top - delta.Height / 2)
            Return New RectangleF(location, size)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Scale(rect As Rectangle, factor As SizeF) As Rectangle
            With rect
                With New RectangleF(.Location.PointF, .Size.SizeF).Scale(factor)
                    Return New Rectangle(.Location.ToPoint, .Size.ToSize)
                End With
            End With
        End Function

        ''' <summary>
        ''' 获取目标多边形对象的中心点的坐标位置
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Centre(shape As IEnumerable(Of PointF)) As PointF
            Dim x As New List(Of Single)
            Dim y As New List(Of Single)

            Call shape.DoEach(Sub(pt)
                                  x += pt.X
                                  y += pt.Y
                              End Sub)

            Return New PointF(x.Average, y.Average)
        End Function

        <Extension>
        Public Function Centre(shape As IEnumerable(Of Point)) As PointF
            Return shape.PointF.Centre
        End Function

        ''' <summary>
        ''' Gets the center location of the region rectangle.
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        <ExportAPI("Center")>
        <Extension> Public Function Centre(rect As RectangleF) As PointF
            Return New PointF(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2)
        End Function

        ''' <summary>
        ''' 获取将目标多边形置于区域的中央位置的位置偏移量
        ''' </summary>
        ''' <param name="pts"></param>
        ''' <param name="frameSize"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CentralOffset(pts As IEnumerable(Of Point), frameSize As Size) As PointF
            Return pts _
                .Select(Function(pt) pt.PointF) _
                .CentralOffset(frameSize.SizeF)
        End Function

        ''' <summary>
        ''' 获取将目标多边形置于区域的中央位置的位置偏移量
        ''' </summary>
        ''' <param name="pts"></param>
        ''' <param name="frameSize"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CentralOffset(pts As IEnumerable(Of PointF), frameSize As SizeF) As PointF
            Dim xOffset!() = pts.ToArray(Function(x) x.X)
            Dim yOffset!() = pts.ToArray(Function(x) x.Y)
            Dim xo, yo As Single

            If xOffset.Length > 0 Then
                xo = xOffset.Min
            End If
            If yOffset.Length > 0 Then
                yo = yOffset.Min
            End If

            Dim size As New SizeF(xOffset.Max - xOffset.Min, yOffset.Max - yOffset.Min)
            Dim left! = (frameSize.Width - size.Width) / 2
            Dim top! = (frameSize.Height - size.Height) / 2

            Return New PointF(left - xo, top - yo)
        End Function
    End Module
End Namespace
