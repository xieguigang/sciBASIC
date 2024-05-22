#Region "Microsoft.VisualBasic::68cd2215a0190dd396794899d2ff0a68, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\PairingHeap.vb"

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

    '   Total Lines: 166
    '    Code Lines: 127 (76.51%)
    ' Comment Lines: 18 (10.84%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 21 (12.65%)
    '     File Size: 6.00 KB


    '     Class PairingHeap
    ' 
    '         Properties: count, empty, min
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: contains, decreaseKey, Insert, isHeap, merge
    '                   mergePairs, removeMin, ToString
    ' 
    '         Sub: forEach
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.JavaScript
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Collection

    ''' <summary>
    ''' from: https://gist.github.com/nervoussystem
    ''' ``{elem:object, subheaps:[array of heaps]}``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>
    ''' 堆对象应该没有节点删除的操作吧？
    ''' </remarks>
    Public Class PairingHeap(Of T)

        Dim subheaps As Stack(Of PairingHeap(Of T))

        Public elem As T

        Public ReadOnly Property count() As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Me.empty Then
                    Return 0
                Else
                    Return 1 + subheaps _
                        .reduce(Function(n As Double, h As PairingHeap(Of T))
                                    Return n + h.count()
                                End Function, 0)
                End If
            End Get
        End Property

        Public ReadOnly Property min() As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.elem
            End Get
        End Property

        Public ReadOnly Property empty() As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.elem Is Nothing
            End Get
        End Property

        Public Sub New(elem As T)
            Me.subheaps = New Stack(Of PairingHeap(Of T))
            Me.elem = elem
        End Sub

        Public Overloads Function ToString(selector As IToString(Of T)) As String
            Dim str = ""
            Dim needComma = False

            For i As Integer = 0 To Me.subheaps.Count - 1
                Dim subheap As PairingHeap(Of T) = Me.subheaps(i)
                If Not subheap.elem Is Nothing Then
                    needComma = False
                    Continue For
                End If
                If needComma Then
                    str = str & ","
                End If
                str = str & subheap.ToString(selector)
                needComma = True
            Next

            If str <> "" Then
                str = "(" & str & ")"
            End If

            If Me.elem Is Nothing Then
                Return selector(Me.elem) & str
            Else
                Return str
            End If
        End Function

        Public Sub forEach(f As Action(Of T, PairingHeap(Of T)))
            If Not Me.empty() Then
                f(Me.elem, Me)
                Me.subheaps.DoEach(Sub(s) s.forEach(f))
            End If
        End Sub

        Public Function contains(h As PairingHeap(Of T)) As Boolean
            If Me Is h Then
                Return True
            End If
            For i As Integer = 0 To Me.subheaps.Count - 1
                If Me.subheaps(i).contains(h) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function isHeap(lessThan As Func(Of T, T, Boolean)) As Boolean
            Return Me.subheaps.All(Function(h) lessThan(Me.elem, h.elem) AndAlso h.isHeap(lessThan))
        End Function

        Public Function Insert(obj As T, lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            Return Me.merge(New PairingHeap(Of T)(obj), lessThan)
        End Function

        ''' <summary>
        ''' 优先度比较大的对象将会先被push到stack之中
        ''' </summary>
        ''' <param name="heap2"></param>
        ''' <param name="lessThan"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 会在这个函数始终构建出一个二叉树的结构
        ''' </remarks>
        Public Function merge(heap2 As PairingHeap(Of T), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.empty() Then
                Return heap2
            ElseIf heap2.empty() Then
                Return Me
            ElseIf lessThan(Me.elem, heap2.elem) Then
                Me.subheaps.Push(heap2)
                Return Me
            Else
                heap2.subheaps.Push(Me)
                Return heap2
            End If
        End Function

        Public Function removeMin(lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.empty() Then
                Return Nothing
            Else
                Return Me.mergePairs(lessThan)
            End If
        End Function

        Public Function mergePairs(lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.subheaps.Count = 0 Then
                Return New PairingHeap(Of T)(Nothing)
            ElseIf Me.subheaps.Count = 1 Then
                Return Me.subheaps(0)
            Else
                Dim firstPair = Me.subheaps.Pop().merge(Me.subheaps.Pop(), lessThan)
                Dim remaining = Me.mergePairs(lessThan)
                Return firstPair.merge(remaining, lessThan)
            End If
        End Function

        Public Function decreaseKey(subheap As PairingHeap(Of T), newValue As T, setHeapNode As Action(Of T, PairingHeap(Of T)), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            Dim newHeap = subheap.removeMin(lessThan)
            'reassign subheap values to preserve tree
            subheap.elem = newHeap.elem
            subheap.subheaps = newHeap.subheaps
            If setHeapNode IsNot Nothing AndAlso newHeap.elem IsNot Nothing Then
                setHeapNode(subheap.elem, subheap)
            End If
            Dim pairingNode = New PairingHeap(Of T)(newValue)
            Call setHeapNode(newValue, pairingNode)
            Return Me.merge(pairingNode, lessThan)
        End Function
    End Class

End Namespace
