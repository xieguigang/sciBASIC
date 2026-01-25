#Region "Microsoft.VisualBasic::e0c2dc7b9025bba44357f067ec3e5789, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\PriorityQueue.vb"

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

'   Total Lines: 127
'    Code Lines: 67 (52.76%)
' Comment Lines: 40 (31.50%)
'    - Xml Docs: 35.00%
' 
'   Blank Lines: 20 (15.75%)
'     File Size: 3.86 KB


'     Class PriorityQueue
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: count, empty, isHeap, pop, push
'                   top, ToString
' 
'         Sub: forEach, reduceKey
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Collection

    ''' <summary>
    ''' a min priority queue backed by a pairing heap
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PriorityQueue(Of T)

        Dim root As PairingHeap(Of T)
        Dim lessThan As Func(Of T, T, Boolean)

        ''' <summary>
        ''' number of elements in queue
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property count() As Integer
            Get
                If root Is Nothing Then
                    Return 0
                End If
                Return Me.root.count()
            End Get
        End Property

        ''' <summary>
        ''' the top element (the min element as defined by lessThan)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property top() As T
            Get
                If Me.empty() Then
                    Return Nothing
                End If
                Return Me.root.elem
            End Get
        End Property

        ''' <summary>
        ''' true if no more elements in queue
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property empty() As Boolean
            Get
                Return Me.root Is Nothing OrElse Me.root.elem Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' check heap condition (for testing)
        ''' </summary>
        ''' <returns>true if queue is in valid state</returns>
        Public ReadOnly Property isHeap() As Boolean
            Get
                If root Is Nothing Then
                    Return False
                End If
                Return Me.root.isHeap(Me.lessThan)
            End Get
        End Property

        ''' <summary>
        ''' ```
        ''' priority = a &lt; b
        ''' ```
        ''' </summary>
        ''' <param name="lessThan"></param>
        Public Sub New(lessThan As Func(Of T, T, Boolean))
            Me.lessThan = lessThan
        End Sub

        Sub New(source As IEnumerable(Of T), lessThan As Func(Of T, T, Boolean))
            Call Me.New(lessThan)

            For Each element As T In source.SafeQuery
                Call Me.push(element)
            Next
        End Sub

        ''' <summary>
        ''' put things on the heap
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function push(ParamArray args As T()) As PairingHeap(Of T)
            Dim pairingNode As PairingHeap(Of T) = Nothing
            Dim arg As T
            Dim lastNode As PairingHeap(Of T) = Nothing

            For i As Integer = 0 To args.Length - 1
                arg = args(i)
                pairingNode = New PairingHeap(Of T)(arg)

                If Me.empty Then
                    root = pairingNode
                Else
                    root = root.merge(pairingNode, Me.lessThan)
                End If

                lastNode = pairingNode
            Next

            ' 返回最后插入的节点句柄
            Return lastNode
        End Function

        ''' <summary>
        ''' apply f to each element of the queue
        ''' </summary>
        ''' <param name="f">function to apply</param>
        Public Sub forEach(f As Action(Of T, PairingHeap(Of T)))
            Me.root.forEach(f)
        End Sub

        ''' <summary>
        ''' remove and return the min element from the queue
        ''' </summary>
        ''' <returns></returns>
        Public Function pop() As T
            If Me.empty() Then
                Return Nothing
            End If
            Dim obj = Me.root.min()
            Me.root = Me.root.removeMin(Me.lessThan)
            Return obj
        End Function

        ''' <summary>
        ''' reduce the key value of the specified heap node
        ''' </summary>
        ''' <param name="heapNode"></param>
        ''' <param name="newKey"></param>
        ''' <param name="setHeapNode"></param>
        Public Sub reduceKey(heapNode As PairingHeap(Of T), newKey As T, setHeapNode As Action(Of T, PairingHeap(Of T)))
            Me.root = Me.root.decreaseKey(heapNode, newKey, setHeapNode, Me.lessThan)
        End Sub

        Public Overloads Function ToString(selector As IToString(Of T)) As String
            Return Me.root.ToString(selector)
        End Function

    End Class
End Namespace
