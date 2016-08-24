#Region "Microsoft.VisualBasic::03e8ae70b1fe69d82e6065fed280550d, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\Render\GraphicsTools.vb"

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

Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing.Drawing2D
Imports System.Drawing

''' <summary>
''' Provides tools for graphics processing
''' </summary>
''' <remarks>
''' 2007 José Manuel Menéndez Poo
''' Visit my blog for upgrades and other renderers - www.menendezpoo.com
''' </remarks>
Friend NotInheritable Class GraphicsTools
	Private Sub New()
	End Sub
	''' <summary>
	''' Creates a rounded rectangle from the specified rectangle and radius
	''' </summary>
	''' <param name="rectangle">Base rectangle</param>
	''' <param name="radius">Radius of the corners</param>
	''' <returns>Rounded rectangle as a GraphicsPath</returns>
	Public Shared Function CreateRoundRectangle(rectangle As Rectangle, radius As Integer) As GraphicsPath
		Dim path As New GraphicsPath()

		Dim l As Integer = rectangle.Left
		Dim t As Integer = rectangle.Top
		Dim w As Integer = rectangle.Width
		Dim h As Integer = rectangle.Height
		Dim d As Integer = radius << 1

		path.AddArc(l, t, d, d, 180, 90)
		' topleft
		path.AddLine(l + radius, t, l + w - radius, t)
		' top
		path.AddArc(l + w - d, t, d, d, 270, 90)
		' topright
		path.AddLine(l + w, t + radius, l + w, t + h - radius)
		' right
		path.AddArc(l + w - d, t + h - d, d, d, 0, 90)
		' bottomright
		path.AddLine(l + w - radius, t + h, l + radius, t + h)
		' bottom
		path.AddArc(l, t + h - d, d, d, 90, 90)
		' bottomleft
		path.AddLine(l, t + h - radius, l, t + radius)
		' left
		path.CloseFigure()

		Return path
	End Function

	''' <summary>
	''' Creates a rectangle rounded on the top
	''' </summary>
	''' <param name="rectangle">Base rectangle</param>
	''' <param name="radius">Radius of the top corners</param>
	''' <returns>Rounded rectangle (on top) as a GraphicsPath object</returns>
	Public Shared Function CreateTopRoundRectangle(rectangle As Rectangle, radius As Integer) As GraphicsPath
		Dim path As New GraphicsPath()

		Dim l As Integer = rectangle.Left
		Dim t As Integer = rectangle.Top
		Dim w As Integer = rectangle.Width
		Dim h As Integer = rectangle.Height
		Dim d As Integer = radius << 1

		path.AddArc(l, t, d, d, 180, 90)
		' topleft
		path.AddLine(l + radius, t, l + w - radius, t)
		' top
		path.AddArc(l + w - d, t, d, d, 270, 90)
		' topright
		path.AddLine(l + w, t + radius, l + w, t + h)
		' right
		path.AddLine(l + w, t + h, l, t + h)
		' bottom
		path.AddLine(l, t + h, l, t + radius)
		' left
		path.CloseFigure()

		Return path
	End Function

	''' <summary>
	''' Creates a rectangle rounded on the bottom
	''' </summary>
	''' <param name="rectangle">Base rectangle</param>
	''' <param name="radius">Radius of the bottom corners</param>
	''' <returns>Rounded rectangle (on bottom) as a GraphicsPath object</returns>
	Public Shared Function CreateBottomRoundRectangle(rectangle As Rectangle, radius As Integer) As GraphicsPath
		Dim path As New GraphicsPath()

		Dim l As Integer = rectangle.Left
		Dim t As Integer = rectangle.Top
		Dim w As Integer = rectangle.Width
		Dim h As Integer = rectangle.Height
		Dim d As Integer = radius << 1

		path.AddLine(l + radius, t, l + w - radius, t)
		' top
		path.AddLine(l + w, t + radius, l + w, t + h - radius)
		' right
		path.AddArc(l + w - d, t + h - d, d, d, 0, 90)
		' bottomright
		path.AddLine(l + w - radius, t + h, l + radius, t + h)
		' bottom
		path.AddArc(l, t + h - d, d, d, 90, 90)
		' bottomleft
		path.AddLine(l, t + h - radius, l, t + radius)
		' left
		path.CloseFigure()

		Return path
	End Function
End Class
