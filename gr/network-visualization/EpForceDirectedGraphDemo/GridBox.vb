#Region "Microsoft.VisualBasic::c46bf4eafc61cc5789516e2099b82d60, ..\sciBASIC#\gr\Datavisualization.Network\EpForceDirectedGraphDemo\GridBox.vb"

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

'! 
'@file GridBox.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief GridBox Interface
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
'An Interface for the GridBox Class.
'
'

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Drawing

Namespace EpForceDirectedGraphDemo
	Enum BoxType
		Normal
		Pinned
	End Enum

	Class GridBox
		Implements IDisposable
		Public x As Integer, y As Integer, width As Integer, height As Integer
		Public brush As SolidBrush
		Public boxRec As Rectangle
		Public boxType As BoxType
		Public Sub New(iX As Integer, iY As Integer, iType As BoxType)
			Me.x = iX
			Me.y = iY
			Me.boxType = iType
			Select Case iType
				Case BoxType.Normal
					brush = New SolidBrush(Color.Black)
					Exit Select
				Case BoxType.Pinned
					brush = New SolidBrush(Color.Red)
					Exit Select

			End Select
			width = 18
			height = 18
			boxRec = New Rectangle(x, y, width, height)
		End Sub

		Public Sub [Set](iX As Integer, iY As Integer)
			Me.x = iX
			Me.y = iY
			boxRec.X = x
			boxRec.Y = y
		End Sub
		Public Sub DrawBox(iPaper As Graphics)
			If boxType = BoxType.Pinned Then
				boxRec.Width = 26
				boxRec.Height = 26
			Else
				boxRec.Width = 18
				boxRec.Height = 18
			End If
			iPaper.FillRectangle(brush, boxRec)

		End Sub


		Public Sub SetNormalBox()
			If Me.brush IsNot Nothing Then
				Me.brush.Dispose()
			End If
			Me.brush = New SolidBrush(Color.WhiteSmoke)
			Me.boxType = BoxType.Normal
		End Sub

		Public Sub SetEndBox()
			If Me.brush IsNot Nothing Then
				Me.brush.Dispose()
			End If
			Me.brush = New SolidBrush(Color.Red)
			Me.boxType = BoxType.Pinned
		End Sub


		Public Sub Dispose() Implements IDisposable.Dispose
			If Me.brush IsNot Nothing Then
				Me.brush.Dispose()
			End If

		End Sub
	End Class
End Namespace
