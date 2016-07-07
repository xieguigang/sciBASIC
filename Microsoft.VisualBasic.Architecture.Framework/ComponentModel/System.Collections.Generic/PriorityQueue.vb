#Region "c4686235b9c0efdd4f52e308c97e2201, ..\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\PriorityQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
'@file PriorityQueue.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date September 27, 2013
'@brief PriorityQueue Interface
'@version 2.0
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
'An Interface for the PriorityQueue Class.
'
'

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Collections

Public Class PriorityQueue(Of T As IComparable)

    Dim m_data As List(Of T)

    Public Sub New()
		Me.m_data = New List(Of T)()
	End Sub

	Public Sub Enqueue(queueItem As T)
		m_data.Add(queueItem)
		m_data.Sort()
	End Sub

	Public Sub Clear()
		m_data.Clear()
	End Sub

    Public Function Dequeue() As T
		Dim frontItem As T = m_data(0)
		m_data.RemoveAt(0)
		Return frontItem
	End Function

	Public Function Peek() As T
		Dim frontItem As T = m_data(0)
		Return frontItem
	End Function

    Public Function Contains(queueItem As T) As Boolean
        Return m_data.Contains(queueItem)
    End Function

    Public ReadOnly Property Count() As Integer
		Get
			Return m_data.Count
		End Get
	End Property
End Class
