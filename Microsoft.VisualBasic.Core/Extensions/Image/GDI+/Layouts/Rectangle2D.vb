#Region "Microsoft.VisualBasic::e6e08858ebd53e1cf616447fd793dbce, Microsoft.VisualBasic.Core\Extensions\Image\GDI+\Layouts\Rectangle2D.vb"

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
    '         Properties: CenterX, CenterY, Height, Rectangle, Width
    ' 
    '         Constructor: (+6 Overloads) Sub New
    ' 
    '         Function: Clone, contains, Equals, intersection, intersectLine
    '                   ToString
    ' 
    '         Sub: add, grow, setRect
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports sys = System.Math

' $Id: mxRectangle.java,v 1.1 2012/11/15 13:26:39 gaudenz Exp $
' Copyright (c) 2007-2010, Gaudenz Alder, David Benson

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Implements a 2-dimensional rectangle with double precision coordinates.
    ''' </summary>
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
            Get
                Return X + Width / 2
            End Get
        End Property

        ''' <summary>
        ''' Returns the y-coordinate of the center.
        ''' </summary>
        ''' <returns> Returns the y-coordinate of the center. </returns>
        Public Overridable ReadOnly Property CenterY As Double
            Get
                Return Y + Height / 2
            End Get
        End Property

        ''' <summary>
        ''' Constructs a new rectangle at (0, 0) with the width and height set to 0.
        ''' </summary>
        Public Sub New()
            Me.New(0, 0, 0, 0)
        End Sub

        ''' <summary>
        ''' Constructs a copy of the given rectangle.
        ''' </summary>
        ''' <param name="rect"> Rectangle to construct a copy of. </param>
        Public Sub New(rect As Rectangle)
            Me.New(rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        ''' <summary>
        ''' Constructs a copy of the given rectangle.
        ''' </summary>
        ''' <param name="rect"> Rectangle to construct a copy of. </param>
        Public Sub New(rect As RectangleF)
            Me.New(rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        ''' <summary>
        ''' Constructs a copy of the given rectangle.
        ''' </summary>
        ''' <param name="rect"> Rectangle to construct a copy of. </param>
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

        Sub New(width%, height%)
            Call Me.New(0, 0, width, height)
        End Sub

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

        ''' <summary>
        ''' Adds the given rectangle to this rectangle.
        ''' </summary>
        Public Overridable Sub add(rect As Rectangle2D)
            If rect IsNot Nothing Then
                Dim minX As Double = sys.Min(X, rect.X)
                Dim minY As Double = sys.Min(Y, rect.Y)
                Dim maxX As Double = sys.Max(X + Width, rect.X + rect.Width)
                Dim maxY As Double = sys.Max(Y + Height, rect.Y + rect.Height)

                X = minX
                Y = minY
                Width = maxX - minX
                Height = maxY - minY
            End If
        End Sub

        ''' <summary>
        ''' Grows the rectangle by the given amount, that is, this method subtracts
        ''' the given amount from the x- and y-coordinates and adds twice the amount
        ''' to the width and height.
        ''' </summary>
        ''' <param name="amount"> Amount by which the rectangle should be grown. </param>
        Public Overridable Sub grow(amount As Double)
            X -= amount
            Y -= amount
            Width += 2 * amount
            Height += 2 * amount
        End Sub

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
        Public Shared Function intersection(x0 As Double, y0 As Double, x1 As Double, y1 As Double, x2 As Double, y2 As Double, x3 As Double, y3 As Double) As Point2D
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
        ''' Returns the bounds as a new rectangle.
        ''' </summary>
        ''' <returns> Returns a new rectangle for the bounds. </returns>
        Public Overridable ReadOnly Property Rectangle As RectangleF
            Get
                Dim ix As Integer = CInt(Fix(sys.Round(X)))
                Dim iy As Integer = CInt(Fix(sys.Round(Y)))
                Dim iw As Integer = CInt(Fix(sys.Round(Width - ix + X)))
                Dim ih As Integer = CInt(Fix(sys.Round(Height - iy + Y)))

                Return New RectangleF(ix, iy, iw, ih)
            End Get
        End Property

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
    End Class
End Namespace
