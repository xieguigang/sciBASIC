#Region "Microsoft.VisualBasic::bebf41fcde06b08b7faffd320dc0f560, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Extensions.vb"

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

    '   Total Lines: 290
    '    Code Lines: 190 (65.52%)
    ' Comment Lines: 69 (23.79%)
    '    - Xml Docs: 75.36%
    ' 
    '   Blank Lines: 31 (10.69%)
    '     File Size: 13.16 KB


    '     Module Extensions
    ' 
    '         Function: (+5 Overloads) Enlarge, (+4 Overloads) GetTextAnchor, Move, (+2 Overloads) MoveTo, (+2 Overloads) Rotate
    ' 
    '         Sub: ShapeGlow
    '         Enum MoveTypes
    ' 
    '             BoundsBoxTopLeft, PolygonCentre
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Extensions
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Drawing2D

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Sub ShapeGlow(ByRef g As IGraphics, path As GraphicsPath, glowColor As Color, Optional glowSize! = 10)
            For i As Integer = 1 To glowSize
                Using pen As New Pen(glowColor, i) With {
                    .LineJoin = LineJoin.Round
                }
                    g.DrawPath(pen, path)
                End Using
            Next
        End Sub

        Public ReadOnly BlackBrush As [Default](Of Brush) = Brushes.Black

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Move(rect As RectangleF, distance#, angle#) As RectangleF
            Return New RectangleF With {
                .Location = rect _
                    .Location _
                    .MovePoint(angle, distance),
                .Size = rect.Size
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetTextAnchor(label As Label, anchor As PointF) As Point
            Return label.rectangle.GetTextAnchor(anchor)
        End Function

        ''' <summary>
        ''' 分别计算出<paramref name="textLayout"/>的上下左右对<paramref name="anchor"/>的距离，取最小的距离的位置并返回
        ''' </summary>
        ''' <param name="textLayout">标签文本的大小和位置，生成一个<see cref="Rectangle"/>布局对象</param>
        ''' <param name="anchor">这个标签文本所属的对象的锚点</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTextAnchor(textLayout As Rectangle, anchor As PointF) As Point
            With textLayout
                Return GetTextAnchor(.Left, .Right, .Width, .Height, .Top, .Bottom, anchor)
            End With
        End Function

        Private Function GetTextAnchor(left!, right!, width!, height!, top!, bottom!, anchor As PointF) As Point
            Dim points As Point() = {
                New Point(left + width / 2, top),        ' top                'New Point(left, top),                    ' top_left                'New Point(left + width, top),            ' top_right
                New Point(left + width / 3, top),        ' top 1/3
                New Point(left + width / 3 * 2, top),    ' top 2/3
                New Point(left + width / 4, top),        ' top 1/4
                New Point(left + width / 4 * 3, top),    ' top 3/4
                New Point(left + width / 5, top),        ' top 1/5
                New Point(left + width / 5 * 2, top),    ' top 2/5
                New Point(left + width / 5 * 3, top),    ' top 3/5
                New Point(left + width / 5 * 4, top),    ' top 4/5
                                                         _
                New Point(left + width / 2, bottom),     ' bottom,                'New Point(left, bottom),                 ' bottom_left,                'New Point(left + width, bottom),         ' bottom_right,
                New Point(left + width / 3, bottom),     ' bottom 1/3,
                New Point(left + width / 3 * 2, bottom), ' bottom 2/3,
                New Point(left + width / 4, bottom),     ' bottom 1/4,
                New Point(left + width / 4 * 3, bottom), ' bottom 3/4,
                New Point(left + width / 5, bottom),     ' bottom 1/5,
                New Point(left + width / 5 * 2, bottom), ' bottom 2/5,
                New Point(left + width / 5 * 3, bottom), ' bottom 3/5,
                New Point(left + width / 5 * 4, bottom), ' bottom 4/5,
                                                         _
                New Point(left, top + height / 2),       ' left,
                New Point(right, top + height / 2)       ' right
            }
            Dim d#() = points.Distance(anchor.ToPoint)

            Return points(which.Min(d))
        End Function

        ''' <summary>
        ''' 分别计算出<paramref name="textLayout"/>的上下左右对<paramref name="anchor"/>的距离，取最小的距离的位置并返回
        ''' </summary>
        ''' <param name="textLayout">标签文本的大小和位置，生成一个<see cref="Rectangle"/>布局对象</param>
        ''' <param name="anchor">这个标签文本所属的对象的锚点</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTextAnchor(textLayout As RectangleF, anchor As PointF) As Point
            With textLayout
                Return GetTextAnchor(.Left, .Right, .Width, .Height, .Top, .Bottom, anchor)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Enlarge(size As SizeF, fold#) As SizeF
            With size
                Return New SizeF(CSng(.Width * fold), CSng(.Height * fold))
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Enlarge(shape As IEnumerable(Of Point), scale#) As Point()
            Return shape.PointF.Enlarge(scale).ToPoints
        End Function

        ''' <summary>
        ''' 将一个多边形放大指定的倍数<paramref name="scale"/>
        ''' </summary>
        ''' <param name="shape">矢量图形的点集合</param>
        ''' <param name="scale#"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Enlarge(shape As IEnumerable(Of PointF), scale#) As PointF()
            Return Enlarge(shape, (scale, scale))
        End Function

        ''' <summary>
        ''' 将一个多边形放大指定的倍数<paramref name="scale"/>
        ''' </summary>
        ''' <param name="shape">矢量图形的点集合</param>
        ''' <param name="scale#"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Enlarge(shape As IEnumerable(Of PointF), scale As SizeF) As PointF()
            Return Enlarge(shape, (scale.Width, scale.Height))
        End Function

        ''' <summary>
        ''' 将一个多边形放大指定的倍数<paramref name="scale"/>
        ''' </summary>
        ''' <param name="shape">矢量图形的点集合</param>
        ''' <param name="scale"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Enlarge(shape As IEnumerable(Of PointF), scale As (width#, height#)) As PointF()
            Dim shapeVector = shape.ToArray
            Dim center = shapeVector.Centre
            Dim x As New Vector(shapeVector.Select(Function(pt) pt.X))
            Dim y As New Vector(shapeVector.Select(Function(pt) pt.Y))
            Dim b = x - CDbl(center.X)
            Dim a = y - CDbl(center.Y)
            Dim c = Vector.Sqrt(b ^ 2 + a ^ 2)
            Dim dx = (b / c) * (c * scale.width - c)
            Dim dy = (a / c) * (c * scale.height - c)

            For i As Integer = 0 To c.Length - 1
                ' 2018-3-6 如果有个点是位于shape的中心，那么在scale之后c值为零
                ' 则计算出来的差异量为NaN，会导致出错
                ' 在这里将所有c值为零的点都设置为原来的值，即意味着在多边形放大之后其位置没有发生变化
                If c(i) = 0R Then
                    dx(i) = 0#
                    dy(i) = 0#
                End If
            Next

            x = x + dx
            y = y + dy

            ' 返回放大之后的矢量图形向量
            Return shapeVector _
                .Sequence _
                .Select(Function(i) New PointF(x(i), y(i))) _
                .ToArray
        End Function

        ''' <summary>
        ''' 请注意，这个是围绕坐标轴远点进行的旋转，如果想要围绕指定点进行旋转，还需要进行平移操作
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <param name="alpha">angle in radius</param>
        ''' <returns></returns>
        <Extension>
        Public Function Rotate(shape As IEnumerable(Of PointF), alpha#) As PointF()
            Dim vector = shape.ToArray
            Dim x0 As New Vector(vector.Select(Function(pt) pt.X))
            Dim y0 As New Vector(vector.Select(Function(pt) pt.Y))
            Dim x1 = x0 * std.Cos(alpha) + y0 * std.Sin(alpha)
            Dim y1 = -x0 * std.Sin(alpha) + y0 * std.Cos(alpha)
            Return (x1, y1).Point2D.ToArray
        End Function

        ' theta * stdNum.PI / 180

        ''' <summary>
        ''' The required alpha angle data should be in data unit of radians
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <param name="center"></param>
        ''' <param name="alpha">the angle in radius, could be translate from angle via function <see cref="ToRadians"/></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Rotate(shape As IEnumerable(Of PointF), center As PointF, alpha#) As PointF()
            Dim sin = std.Sin(alpha)
            Dim cos = std.Cos(alpha)
            Dim matrix As New NumericMatrix(
                {
                    {cos, -sin},
                    {sin, cos}
                }
            )

            'Dim vector = shape.ToArray
            'Dim x0 As New Vector(vector.Select(Function(pt) pt.X))
            'Dim y0 As New Vector(vector.Select(Function(pt) pt.Y))
            'Dim x1 As Vector = center.X + (x0 - center.X) * stdNum.Cos(alpha) - (y0 - center.Y) * stdNum.Sin(alpha)
            'Dim y1 As Vector = center.Y + (x0 - center.X) * stdNum.Sin(alpha) + (y0 - center.Y) * stdNum.Cos(alpha)
            Dim vector = shape.Select(Function(v) matrix.DotMultiply({v.X, v.Y})).ToArray
            Dim x0 As New Vector(From v In vector Select v(0))
            Dim y0 As New Vector(From v In vector Select v(1))
            Dim x1 As Vector = x0 - center.X
            Dim y1 As Vector = y0 - center.Y

            Return (x1, y1).Point2D.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <param name="location"></param>
        ''' <param name="type">By default, is move the shape its bounds box topleft to target place.</param>
        ''' <returns></returns>
        <Extension>
        Public Function MoveTo(shape As IEnumerable(Of PointF), location As PointF, Optional type As MoveTypes = MoveTypes.BoundsBoxTopLeft) As PointF()
            Dim polygon = shape.ToArray
            Dim offset As PointF

            Select Case type
                Case MoveTypes.BoundsBoxTopLeft
                    With polygon.GetBounds
                        offset = New PointF(.Left - location.X, .Top - location.Y)
                    End With
                Case Else
                    With polygon.Centre
                        offset = New PointF(.X - location.X, .Y - location.Y)
                    End With
            End Select

            Dim out As PointF() = polygon _
                .Select(Function(point)
                            Return New PointF With {
                                .X = point.X - offset.X,
                                .Y = point.Y - offset.Y
                            }
                        End Function) _
                .ToArray

            Return out
        End Function

        '<Extension>
        'Public Function MoveTopLeft(polygon As PointF(), topLeft As PointF) As PointF()

        'End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function MoveTo(shape As IEnumerable(Of Point), location As PointF, Optional type As MoveTypes = MoveTypes.BoundsBoxTopLeft) As Point()
            Return shape _
                .Select(Function(point) point.PointF) _
                .MoveTo(location, type) _
                .Select(Function(point) point.ToPoint) _
                .ToArray
        End Function

        Public Enum MoveTypes As Byte
            ''' <summary>
            ''' Move the shape its bounds box topleft to target place
            ''' </summary>
            BoundsBoxTopLeft
            PolygonCentre
        End Enum
    End Module
End Namespace
