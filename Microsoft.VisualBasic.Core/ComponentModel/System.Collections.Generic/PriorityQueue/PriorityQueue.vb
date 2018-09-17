#Region "Microsoft.VisualBasic::9dbc70c8a7df8057d1d1449a5c94baf7, Microsoft.VisualBasic.Core\ComponentModel\System.Collections.Generic\PriorityQueue\PriorityQueue.vb"

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

    '     Class PriorityQueue
    ' 
    '         Properties: Count
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Contains, Dequeue, GetEnumerator, IEnumerable_GetEnumerator, Peek
    '                   ToString
    ' 
    '         Sub: Add, Clear, Enqueue, Remove, Sort
    ' 
    ' 
    ' /********************************************************************************/

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    ''' <summary>
    ''' An Interface for the PriorityQueue Class.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PriorityQueue(Of T As IComparable)
        Implements IEnumerable(Of T)

        Protected list As New List(Of T)

        Public Overridable ReadOnly Property Count() As Integer
            Get
                Return list.Count
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(source As IEnumerable(Of T))
            list = source.ToList
            list.Sort()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Enqueue(queueItem As T)
            Call list.Add(queueItem)
            Call list.Sort()
        End Sub

        ''' <summary>
        ''' Add without sort
        ''' </summary>
        ''' <param name="x"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(x As T)
            Call list.Add(x)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Sort()
            Call list.Sort()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Clear()
            Call list.Clear()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Sub Remove(o As T)
            Call list.Remove(o)
        End Sub

        ''' <summary>
        ''' Poll
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function Dequeue() As T
            Dim frontItem As T = list(0)
            list.RemoveAt(0)
            Return frontItem
        End Function

        Public Overridable Function Peek() As T
            Dim frontItem As T = list(0)
            Return frontItem
        End Function

        Public Overrides Function ToString() As String
            Return $"Queue {list.Count} items, 1st_item:={Peek.GetJson}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function Contains(queueItem As T) As Boolean
            Return list.Contains(queueItem)
        End Function

        Public Overridable Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In list
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
