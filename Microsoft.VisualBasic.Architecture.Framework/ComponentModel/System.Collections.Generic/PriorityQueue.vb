#Region "Microsoft.VisualBasic::dd1a4cf1aea349912a72f2493b78fe67, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\PriorityQueue.vb"

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

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    ''' <summary>
    ''' An Interface for the PriorityQueue Class.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PriorityQueue(Of T As IComparable)
        Implements IEnumerable(Of T)

        Dim list As New List(Of T)

        Public ReadOnly Property Count() As Integer
            Get
                Return list.Count
            End Get
        End Property

        Public Sub Enqueue(queueItem As T)
            Call list.Add(queueItem)
            Call list.Sort()
        End Sub

        Public Sub Clear()
            Call list.Clear()
        End Sub

        Public Sub Remove(o As T)
            Call list.Remove(o)
        End Sub

        ''' <summary>
        ''' Poll
        ''' </summary>
        ''' <returns></returns>
        Public Function Dequeue() As T
            Dim frontItem As T = list(0)
            list.RemoveAt(0)
            Return frontItem
        End Function

        Public Function Peek() As T
            Dim frontItem As T = list(0)
            Return frontItem
        End Function

        Public Overrides Function ToString() As String
            Return $"Queue {list.Count} items, 1st_item:={Peek.GetJson}"
        End Function

        Public Function Contains(queueItem As T) As Boolean
            Return list.Contains(queueItem)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In list
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
