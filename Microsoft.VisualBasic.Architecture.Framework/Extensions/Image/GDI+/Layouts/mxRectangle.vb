Imports Microsoft.VisualBasic
Imports System

''' <summary>
''' $Id: mxRectangle.java,v 1.1 2012/11/15 13:26:39 gaudenz Exp $
''' Copyright (c) 2007-2010, Gaudenz Alder, David Benson
''' </summary>
Namespace com.mxgraph.util


	''' <summary>
	''' Implements a 2-dimensional rectangle with double precision coordinates.
	''' </summary>
	Public Class mxRectangle
		Inherits mxPoint

		''' 
		Private Const serialVersionUID As Long = -3793966043543578946L

		''' <summary>
		''' Holds the width and the height. Default is 0.
		''' </summary>
		Protected Friend width, height As Double

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
		Public Sub New(ByVal rect As java.awt.geom.Rectangle2D)
			Me.New(rect.X, rect.Y, rect.Width, rect.Height)
		End Sub

		''' <summary>
		''' Constructs a copy of the given rectangle.
		''' </summary>
		''' <param name="rect"> Rectangle to construct a copy of. </param>
		Public Sub New(ByVal rect As mxRectangle)
			Me.New(rect.X, rect.Y, rect.Width, rect.Height)
		End Sub

		''' <summary>
		''' Constructs a rectangle using the given parameters.
		''' </summary>
		''' <param name="x"> X-coordinate of the new rectangle. </param>
		''' <param name="y"> Y-coordinate of the new rectangle. </param>
		''' <param name="width"> Width of the new rectangle. </param>
		''' <param name="height"> Height of the new rectangle. </param>
		Public Sub New(ByVal x As Double, ByVal y As Double, ByVal width As Double, ByVal height As Double)
			MyBase.New(x, y)

			Width = width
			Height = height
		End Sub

		''' <summary>
		''' Returns the width of the rectangle.
		''' </summary>
		''' <returns> Returns the width. </returns>
		Public Overridable Property Width As Double
			Get
				Return width
			End Get
			Set(ByVal value As Double)
				width = value
			End Set
		End Property


		''' <summary>
		''' Returns the height of the rectangle.
		''' </summary>
		''' <returns> Returns the height. </returns>
		Public Overridable Property Height As Double
			Get
				Return height
			End Get
			Set(ByVal value As Double)
				height = value
			End Set
		End Property


		''' <summary>
		''' Sets this rectangle to the specified values
		''' </summary>
		''' <param name="x"> the new x-axis position </param>
		''' <param name="y"> the new y-axis position </param>
		''' <param name="w"> the new width of the rectangle </param>
		''' <param name="h"> the new height of the rectangle </param>
		Public Overridable Sub setRect(ByVal x As Double, ByVal y As Double, ByVal w As Double, ByVal h As Double)
			Me.x = x
			Me.y = y
			Me.width = w
			Me.height = h
		End Sub

		''' <summary>
		''' Adds the given rectangle to this rectangle.
		''' </summary>
		Public Overridable Sub add(ByVal rect As mxRectangle)
			If rect IsNot Nothing Then
				Dim minX As Double = Math.Min(x, rect.x)
				Dim minY As Double = Math.Min(y, rect.y)
				Dim maxX As Double = Math.Max(x + width, rect.x + rect.width)
				Dim maxY As Double = Math.Max(y + height, rect.y + rect.height)

				x = minX
				y = minY
				width = maxX - minX
				height = maxY - minY
			End If
		End Sub

		''' <summary>
		''' Returns the x-coordinate of the center.
		''' </summary>
		''' <returns> Returns the x-coordinate of the center. </returns>
		Public Overridable Property CenterX As Double
			Get
				Return X + Width / 2
			End Get
		End Property

		''' <summary>
		''' Returns the y-coordinate of the center.
		''' </summary>
		''' <returns> Returns the y-coordinate of the center. </returns>
		Public Overridable Property CenterY As Double
			Get
				Return Y + Height / 2
			End Get
		End Property

		''' <summary>
		''' Grows the rectangle by the given amount, that is, this method subtracts
		''' the given amount from the x- and y-coordinates and adds twice the amount
		''' to the width and height.
		''' </summary>
		''' <param name="amount"> Amount by which the rectangle should be grown. </param>
		Public Overridable Sub grow(ByVal amount As Double)
			x -= amount
			y -= amount
			width += 2 * amount
			height += 2 * amount
		End Sub

		''' <summary>
		''' Returns true if the given point is contained in the rectangle.
		''' </summary>
		''' <param name="x"> X-coordinate of the point. </param>
		''' <param name="y"> Y-coordinate of the point. </param>
		''' <returns> Returns true if the point is contained in the rectangle. </returns>
		Public Overridable Function contains(ByVal x As Double, ByVal y As Double) As Boolean
			Return (Me.x <= x AndAlso Me.x + width >= x AndAlso Me.y <= y AndAlso Me.y + height >= y)
		End Function

		''' <summary>
		''' Returns the point at which the specified point intersects the perimeter 
		''' of this rectangle or null if there is no intersection.
		''' </summary>
		''' <param name="x0"> the x co-ordinate of the first point of the line </param>
		''' <param name="y0"> the y co-ordinate of the first point of the line </param>
		''' <param name="x1"> the x co-ordinate of the second point of the line </param>
		''' <param name="y1"> the y co-ordinate of the second point of the line </param>
		''' <returns> the point at which the line intersects this rectangle, or null
		''' 			if there is no intersection </returns>
		Public Overridable Function intersectLine(ByVal x0 As Double, ByVal y0 As Double, ByVal x1 As Double, ByVal y1 As Double) As mxPoint
			Dim result As mxPoint = Nothing

			result = mxUtils.intersection(x, y, x + width, y, x0, y0, x1, y1)

			If result Is Nothing Then result = mxUtils.intersection(x + width, y, x + width, y + height, x0, y0, x1, y1)

			If result Is Nothing Then result = mxUtils.intersection(x + width, y + height, x, y + height, x0, y0, x1, y1)

			If result Is Nothing Then result = mxUtils.intersection(x, y, x, y + height, x0, y0, x1, y1)

			Return result
		End Function

		''' <summary>
		''' Returns the bounds as a new rectangle.
		''' </summary>
		''' <returns> Returns a new rectangle for the bounds. </returns>
		Public Overridable Property Rectangle As java.awt.Rectangle
			Get
				Dim ix As Integer = CInt(Fix(Math.Round(x)))
				Dim iy As Integer = CInt(Fix(Math.Round(y)))
				Dim iw As Integer = CInt(Fix(Math.Round(width - ix + x)))
				Dim ih As Integer = CInt(Fix(Math.Round(height - iy + y)))
    
				Return New java.awt.Rectangle(ix, iy, iw, ih)
			End Get
		End Property

		''' 
		''' <summary>
		''' Returns true if the given object equals this rectangle.
		''' </summary>
		Public Overrides Function Equals(ByVal obj As Object) As Boolean
			If TypeOf obj Is mxRectangle Then
				Dim ___rect As mxRectangle = CType(obj, mxRectangle)

				Return ___rect.X = X AndAlso ___rect.Y = Y AndAlso ___rect.Width = Width AndAlso ___rect.Height = Height
			End If

			Return False
		End Function

		''' <summary>
		''' Returns a new instance of the same rectangle.
		''' </summary>
		Public Overrides Function clone() As Object
			Dim ___clone As mxRectangle = CType(MyBase.clone(), mxRectangle)

			___clone.Width = Width
			___clone.Height = Height

			Return ___clone
		End Function

		''' <summary>
		''' Returns the <code>String</code> representation of this
		''' <code>mxRectangle</code>. </summary>
		''' <returns> a <code>String</code> representing this
		''' <code>mxRectangle</code>. </returns>
		Public Overrides Function ToString() As String
			Return Me.GetType().Name & "[x=" & x & ",y=" & y & ",w=" & width & ",h=" & height & "]"
		End Function
	End Class

End Namespace