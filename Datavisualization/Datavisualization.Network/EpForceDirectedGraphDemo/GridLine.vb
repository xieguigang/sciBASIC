'! 
'@file GridLine.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief GridLine Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the GridLine Class.
'
'

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Drawing

Namespace EpForceDirectedGraphDemo
	Class GridLine
		Public fromX As Integer, fromY As Integer, toX As Integer, toY As Integer
		Public pen As Pen

		Public Sub New(iFromX As Integer, iFromY As Integer, iToX As Integer, iToY As Integer)
			Me.fromX = iFromX + 9
			Me.fromY = iFromY + 9
			Me.toX = iToX + 9
			Me.toY = iToY + 9
			pen = New Pen(Color.Yellow)


			pen.Width = 2
		End Sub
		Public Sub [Set](iFromX As Integer, iFromY As Integer, iToX As Integer, iToY As Integer)
			Me.fromX = iFromX + 9
			Me.fromY = iFromY + 9
			Me.toX = iToX + 9
			Me.toY = iToY + 9
		End Sub
		Public Sub DrawLine(iPaper As Graphics)
			iPaper.DrawLine(pen, fromX, fromY, toX, toY)

		End Sub


		Public Sub Dispose()
			If Me.pen IsNot Nothing Then
				Me.pen.Dispose()
			End If

		End Sub
	End Class
End Namespace
