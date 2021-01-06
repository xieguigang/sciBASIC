#Region "Microsoft.VisualBasic::10cbff6209c78ead446c7e594e7e0331, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\Layouts\Rectangle2D.vb"

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

    '     Class Rectangle2D
    ' 
    '         Properties: Bottom, CenterX, CenterY, Height, Rectangle
    '                     Right, Width
    ' 
    '         Constructor: (+7 Overloads) Sub New
    ' 
    '         Function: Clone, contains, Equals, inflate, intersection
    '                   intersectLine, OverlapLambda, OverlapX, OverlapY, ToString
    '                   union, Union, Vertices
    ' 
    '         Sub: add, grow, setRect
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

' $Id: mxRectangle.java,v 1.1 2012/11/15 13:26:39 gaudenz Exp $
' Copyright (c) 2007-2010, Gaudenz Alder, David Benson

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Implements a 2-dimensional rectangle with double precision coordinates.
    ''' </summary>
    ''' <remarks>
    ''' 除了左上角+宽和高这种方式进行矩形区域的定义，还可以用左上角+右下角的方式来定义一个矩形
    ''' </remarks>
    Public Class Rectangle2D : Inherits Point2D

        ''' <summary>
        ''' Returns the width of the rectangle.
        ''' </summary>
        ''' <returns> Returns the width. </returns>
        Public Overridable Property Width As Double

        ''' <summary>
        ''' Returns the height of the rectangle.
        ''' </summary>
        ''' <returns> Returns the height. </returns>
        Public Overridable Property Height As Double

        ''' <summary>
        ''' Returns the x-coordinate of the center.
        ''' </summary>
        ''' <returns> Returns the x-coordinate of the center. </returns>
        Public Overridable ReadOnly Property CenterX As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return X + Width / 2
            End Get
        End Property

        ''' <summary>
        ''' Returns the y-coordinate of the center.
        ''' </summary>
        ''' <returns> Returns the y-coordinate of the center. </returns>
        Public Overridable ReadOnly Property CenterY As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Y + Height / 2
            End Get
        End Property

        Public Property Right As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return X + Width
            End Get
            Set(value As Double)
                ' 保持当前的位置不变，但是宽度变了
                Width = value - X
            End Set
        End Property

        ''' <summary>
        ''' 底部的``Y``坐标值
        ''' </summary>
        ''' <returns></returns>
        Public Property Bottom As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Y + Height
            End Get
            Set(value As Double)
                ' 保持当前的位置不变，但是高度变了
                Height = value - Y
            End Set
        End Property

        ''' <summary>
        ''' Returns the bounds as a new rectangle.
        ''' </summary>
        ''' <returns> Returns a new rectangle for the bounds. </returns>
        Public Overridable ReadOnly Property Rectangle As RectangleF
            Get
                Dim ix As Integer = CInt(Fix(stdNum.Round(X)))
                Dim iy As Integer = CInt(Fix(stdNum.Round(Y)))
                Dim iw As Integer = CInt(Fix(stdNum.Round(Width - ix + X)))
                Dim ih As Integer = CInt(Fix(stdNum.Round(Height - iy + Y)))

                Return New RectangleF(ix, iy, iw, ih)
            End Get
        End Property

        Default Public Overrides Property Axis(a As String) As Double
            Get
                Select Case Strings.LCase(a)
                    Case "cx" : Return CenterX
                    Case "cy" : Return CenterY
                    Case Else
                        Return MyBase.Axis(a)
                End Select
            End Get
            Set(value As Double)
                MyBase.Axis(a) = value
            End Set
        End Property

        ''' <summary>
        ''' Constructs a new rectangle at (0, 0) with the width and height set to 0.
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New()
            Me.New(0, 0, 0, 0)
        End Sub

        ''' <summary>
        ''' Constructs a copy of the given rectangle.
        ''' </summary>
        ''' <param name="rect"> Rectangle to construct a copy of. </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(rect As Rectangle)
            Me.New(rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        ''' <summary>
        ''' Constructs a copy of the given rectangle.
        ''' </summary>
        ''' <param name="rect"> Rectangle to construct a copy of. </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(rect As RectangleF)
            Me.New(rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        ''' <summary>
        ''' Constructs a copy of the given rectangle.
        ''' </summary>
        ''' <param name="rect"> Rectangle to construct a copy of. </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(rect As Rectangle2D)
            Me.New(rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        ''' <summary>
        ''' Constructs a rectangle using the given parameters.
        ''' </summary>
        ''' <param name="x"> X-coordinate of the new rectangle. </param>
        ''' <param name="y"> Y-coordinate of the new rectangle. </param>
        ''' <param name="width"> Width of the new rectangle. </param>
        ''' <param name="height"> Height of the new rectangle. </param>
        Public Sub New(x As Double, y As Double, width As Double, height As Double)
            MyBase.New(x, y)

            Me.Width = width
            Me.Height = height
        End Sub

#If NET_48 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(layout As (x#, y#, w#, h#))
            Call Me.New(layout.x, layout.y, layout.w, layout.h)
        End Sub

#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(width%, height%)
            Call Me.New(0, 0, width, height)
        End Sub

        Public Function OverlapLambda(axis As String) As Func(Of Rectangle2D, Double)
            Select Case Strings.LCase(axis)
                Case "x"
                    Return AddressOf OverlapX
                Case "y"
                    Return AddressOf OverlapY
                Case Else
                    Throw New InvalidExpressionException(axis)
            End Select
        End Function

        Public Function OverlapX(r As Rectangle2D) As Double
            Dim ux = CenterX, vx = r.CenterX

            If (ux <= vx AndAlso r.X < Right) Then
                Return Right - r.X
            End If
            If (vx <= ux AndAlso X < r.Right) Then
                Return r.Right - X
            End If

            Return 0
        End Function

        Public Function OverlapY(r As Rectangle2D) As Double
            Dim uy = CenterY, vy = r.CenterY

            If (uy <= vy AndAlso r.Y < Bottom) Then
                Return Bottom - r.Y
            End If
            If (vy <= uy AndAlso Y < r.Bottom) Then
                Return r.Bottom - Y
            End If

            Return 0
        End Function

        ''' <summary>
        ''' Sets this rectangle to the specified values
        ''' </summary>
        ''' <param name="x"> the new x-axis position </param>
        ''' <param name="y"> the new y-axis position </param>
        ''' <param name="w"> the new width of the rectangle </param>
        ''' <param name="h"> the new height of the rectangle </param>
        Public Overridable Sub setRect(x As Double, y As Double, w As Double, h As Double)
            Me.X = x
            Me.Y = y
            Me.Width = w
            Me.Height = h
        End Sub

#If NET_48 Or netcore5 = 1 Then

        ''' <summary>
        ''' Adds the given rectangle to this rectangle. Union two rectangle.
        ''' (取两个区域的交集部分，并且这个函数会改变当前的这个矩形对象的值)
        ''' </summary>
        Public Overridable Sub add(rect As Rectangle2D)
            If rect IsNot Nothing Then
                With union(X, Y, Right, Bottom, rect)
                    X = .x
                    Y = .y
                    Width = .w
                    Height = .h
                End With
            End If
        End Sub
        Private Shared Function union(x#, y#, right#, bottom#, r As Rectangle2D) As (x#, y#, w#, h#)
            Dim minX As Double = stdNum.Min(x, r.X)
            Dim minY As Double = stdNum.Min(y, r.Y)
            Dim maxX As Double = stdNum.Max(right, r.Right)
            Dim maxY As Double = stdNum.Max(bottom, r.Bottom)

            x = minX
            y = minY

            Return (x, y, w:=maxX - minX, h:=maxY - minY)
        End Function

        ''' <summary>
        ''' 这个函数的功能和<see cref="add"/>函数几乎一致，只不过这个函数返回新的<see cref="Rectangle2D"/>
        ''' 而非修改自身的值
        ''' </summary>
        ''' <param name="rect"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Union(rect As Rectangle2D) As Rectangle2D
            Return New Rectangle2D(union(X, Y, Right, Bottom, rect))
        End Function

#End If

        ''' <summary>
        ''' Grows the rectangle by the given amount, that is, this method subtracts
        ''' the given amount from the x- and y-coordinates and adds twice the amount
        ''' to the width and height.
        ''' 
        ''' ``inflate(pad: number): Rectangle``
        ''' </summary>
        ''' <param name="amount"> Amount by which the rectangle should be grown. </param>
        Public Overridable Sub grow(amount As Double)
            X -= amount
            Y -= amount
            Width += 2 * amount
            Height += 2 * amount
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function inflate(pad As Double) As Rectangle2D
            Return New Rectangle2D(X - pad, Y - pad, Width + 2 * pad, Height + 2 * pad)
        End Function

        ''' <summary>
        ''' Returns true if the given point is contained in the rectangle.
        ''' </summary>
        ''' <param name="x"> X-coordinate of the point. </param>
        ''' <param name="y"> Y-coordinate of the point. </param>
        ''' <returns> Returns true if the point is contained in the rectangle.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function contains(x As Double, y As Double) As Boolean
            Return (Me.X <= x AndAlso Me.X + Width >= x AndAlso Me.Y <= y AndAlso Me.Y + Height >= y)
        End Function

        ''' <summary>
        ''' Returns the point at which the specified point intersects the perimeter 
        ''' of this rectangle or null if there is no intersection.
        ''' </summary>
        ''' <param name="x0"> the x co-ordinate of the first point of the line </param>
        ''' <param name="y0"> the y co-ordinate of the first point of the line </param>
        ''' <param name="x1"> the x co-ordinate of the second point of the line </param>
        ''' <param name="y1"> the y co-ordinate of the second point of the line </param>
        ''' <returns>
        ''' The point at which the line intersects this rectangle, or null if there is no intersection 
        ''' </returns>
        Public Overridable Function intersectLine(x0 As Double, y0 As Double, x1 As Double, y1 As Double) As Point2D
            Dim result As Point2D = intersection(X, Y, X + Width, Y, x0, y0, x1, y1)

            If result Is Nothing Then result = intersection(X + Width, Y, X + Width, Y + Height, x0, y0, x1, y1)
            If result Is Nothing Then result = intersection(X + Width, Y + Height, X, Y + Height, x0, y0, x1, y1)
            If result Is Nothing Then result = intersection(X, Y, X, Y + Height, x0, y0, x1, y1)

            Return result
        End Function

        ''' <summary>
        ''' Returns the intersection of two lines as an mxPoint.
        ''' </summary>
        ''' <param name="x0">X-coordinate of the first line's startpoint.</param>
        ''' <param name="y0">Y-coordinate of the first line's startpoint.</param>
        ''' <param name="x1">X-coordinate of the first line's endpoint.</param>
        ''' <param name="y1">Y-coordinate of the first line's endpoint.</param>
        ''' <param name="x2">X-coordinate of the second line's startpoint.</param>
        ''' <param name="y2">Y-coordinate of the second line's startpoint.</param>
        ''' <param name="x3">X-coordinate of the second line's endpoint.</param>
        ''' <param name="y3">Y-coordinate of the second line's endpoint.</param>
        ''' <returns> Returns the intersection between the two lines.</returns>
        Public Shared Function intersection(x0 As Double, y0 As Double,
                                            x1 As Double, y1 As Double,
                                            x2 As Double, y2 As Double,
                                            x3 As Double, y3 As Double) As Point2D

            Dim denom As Double = ((y3 - y2) * (x1 - x0)) - ((x3 - x2) * (y1 - y0))
            Dim nume_a As Double = ((x3 - x2) * (y0 - y2)) - ((y3 - y2) * (x0 - x2))
            Dim nume_b As Double = ((x1 - x0) * (y0 - y2)) - ((y1 - y0) * (x0 - x2))
            Dim ua As Double = nume_a / denom
            Dim ub As Double = nume_b / denom

            If ua >= 0.0 AndAlso ua <= 1.0 AndAlso ub >= 0.0 AndAlso ub <= 1.0 Then
                ' Get the intersection point
                Dim iX As Double = x0 + ua * (x1 - x0)
                Dim iY As Double = y0 + ua * (y1 - y0)

                Return New Point2D(iX, iY)
            Else
                ' No intersection
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Returns true if the given object value equals this rectangle.
        ''' </summary>
        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is Rectangle2D Then
                With TryCast(obj, Rectangle2D)
                    Return .X = X AndAlso
                        .Y = Y AndAlso
                        .Width = Width AndAlso
                        .Height = Height
                End With
            End If

            Return False
        End Function

        ''' <summary>
        ''' Returns a new instance of the same rectangle.
        ''' </summary>
        Public Overrides Function Clone() As Object
            With CType(MyBase.Clone(), Rectangle2D)
                .Width = Width
                .Height = Height

                Return .ByRef
            End With
        End Function

        ''' <summary>
        ''' Returns the <code>String</code> representation of this
        ''' <code>mxRectangle</code>. </summary>
        ''' <returns> a <code>String</code> representing this
        ''' <code>mxRectangle</code>. </returns>
        Public Overrides Function ToString() As String
            Return $"{Me.GetType.Name} [x={X}, y={Y}, w={Width}, h={Height}]"
        End Function

        ''' <summary>
        ''' 枚举出当前的这个矩形对象之中的4个顶点坐标
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function Vertices() As IEnumerable(Of Point2D)
            Yield New Point2D(X, Y)
            Yield New Point2D(Right, Y)
            Yield New Point2D(Right, Bottom)
            Yield New Point2D(X, Bottom)
        End Function
    End Class
End Namespace
