#Region "Microsoft.VisualBasic::85954bf60c639eb23fa6a14944c534ee, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\PriorityQueue.vb"

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

    '   Total Lines: 187
    '    Code Lines: 115 (61.50%)
    ' Comment Lines: 42 (22.46%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 30 (16.04%)
    '     File Size: 6.75 KB


    '     Class PriorityQueue
    ' 
    '         Properties: count, empty, isHeap, top
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: peek, poll, pop, push, remove
    '                   ToString
    ' 
    '         Sub: clear, forEach, reduceKey
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

        ''' <summary>堆根；Nothing 元素即为空哨兵节点</summary>
        Dim root As PairingHeap(Of T)

        ''' <summary>元素比较委托：lessThan(a, b) = True 表示 a 应排在 b 之前</summary>
        Dim lessThan As Func(Of T, T, Boolean)

        ''' <summary>元素计数器：使 count 达到 Java size() 的 O(1)</summary>
        Dim _count As Integer

        ''' <summary>队列元素个数，O(1)（对齐 java.util.PriorityQueue#size()）</summary>
        Public ReadOnly Property count() As Integer
            Get
                Return _count
            End Get
        End Property

        ''' <summary>队首元素（lessThan 定义下的最小元素）；空队列返回 Nothing</summary>
        Public ReadOnly Property top() As T
            Get
                If Me.empty() Then
                    Return Nothing
                End If
                Return Me.root.elem
            End Get
        End Property

        Public ReadOnly Property empty() As Boolean
            Get
                Return Me.root Is Nothing OrElse Me.root.elem Is Nothing
            End Get
        End Property

        ''' <summary>堆性质自检（调试用）；空队列为空真 True</summary>
        Public ReadOnly Property isHeap() As Boolean
            Get
                If Me.root Is Nothing Then
                    Return True
                End If
                Return Me.root.isHeap(Me.lessThan)
            End Get
        End Property

        Public Sub New(lessThan As Func(Of T, T, Boolean))
            If lessThan Is Nothing Then
                Throw New ArgumentNullException(NameOf(lessThan))
            End If
            Me.root = New PairingHeap(Of T)()
            Me.lessThan = lessThan
            Me._count = 0
        End Sub

        Sub New(compare As IComparer(Of T))
            Call Me.New(lessThan:=Function(l, r) compare.Compare(l, r) < 0)
        End Sub

        Sub New(source As IEnumerable(Of T), lessThan As Func(Of T, T, Boolean))
            Call Me.New(lessThan)
            For Each element As T In source.SafeQuery
                Call Me.push(element)
            Next
        End Sub

        ''' <summary>
        ''' 批量入队，返回最后入队元素的节点句柄（供 reduceKey / contains 使用）。
        ''' 禁止 Nothing 元素（对齐 Java NPE 语义）。
        ''' </summary>
        Public Function push(ParamArray args As T()) As PairingHeap(Of T)
            Dim lastNode As PairingHeap(Of T) = Nothing

            For Each arg As T In args
                If arg Is Nothing Then
                    Throw New ArgumentException(
                        "PriorityQueue 不允许插入 Nothing 元素" &
                        "（对齐 java.util.PriorityQueue 的 null 禁止语义）",
                        NameOf(args))
                End If

                Dim pairingNode As New PairingHeap(Of T)(arg)

                If Me.empty Then
                    Me.root = pairingNode
                Else
                    Me.root = Me.root.merge(pairingNode, Me.lessThan)
                End If

                _count += 1
                lastNode = pairingNode
            Next

            Return lastNode
        End Function

        ''' <summary>
        ''' 检索但不移除队首元素；空队列返回 Nothing。
        ''' 对齐 java.util.PriorityQueue#peek()。
        ''' </summary>
        Public Function peek() As T
            Return Me.top
        End Function

        ''' <summary>
        ''' 检索并移除队首元素；空队列返回 Nothing。
        ''' 对齐 java.util.PriorityQueue#poll()。均摊 O(log n)。
        ''' </summary>
        Public Function poll() As T
            Return Me.pop()
        End Function

        ''' <summary>
        ''' 移除全部元素，O(1)。
        ''' 对齐 java.util.PriorityQueue#clear()。
        ''' </summary>
        Public Sub clear()
            Me.root = New PairingHeap(Of T)()
            _count = 0
        End Sub

        ''' <summary>
        ''' 移除一个与 x 判等（EqualityComparer(Of T).Default，即 x.Equals(elem)）
        ''' 的元素。对齐 java.util.PriorityQueue#remove(Object)：
        '''   - 成功移除返回 True，不存在返回 False
        '''   - x 为 Nothing 时返回 False（对齐 OpenJDK indexOf 的 null 处理）
        '''   - 只移除一个匹配实例；多个相同元素时移除遍历序最先命中的那个
        ''' 复杂度 O(n)（任意元素删除的理论下界）。
        ''' </summary>
        Public Function remove(x As T) As Boolean
            If x Is Nothing Then
                Return False
            End If
            If Me.empty Then
                Return False
            End If

            Dim node = Me.root.findNode(x)
            If node Is Nothing Then
                Return False
            End If

            Me.root = Me.root.removeNode(node, Me.lessThan)
            _count -= 1
            Return True
        End Function

        Public Sub forEach(f As Action(Of T, PairingHeap(Of T)))
            Me.root.forEach(f)
        End Sub

        ''' <summary>
        ''' 移除并返回队首元素；空队列返回 Nothing。均摊 O(log n)。
        ''' </summary>
        Public Function pop() As T
            If Me.empty() Then
                Return Nothing
            End If
            Dim obj = Me.root.min()
            Me.root = Me.root.removeMin(Me.lessThan)
            _count -= 1
            Return obj
        End Function

        ''' <summary>
        ''' 将树中节点 heapNode 的键值降低为 newKey 并重新归堆（Java 无此能力，
        ''' 属于 pairing heap 的超集功能，典型用途：Dijkstra/Prim 的惰性删除替代）。
        ''' 契约：newKey 必须 lessThan 于原键值。
        ''' </summary>
        Public Sub reduceKey(heapNode As PairingHeap(Of T), newKey As T, setHeapNode As Action(Of T, PairingHeap(Of T)))
            Me.root = Me.root.decreaseKey(heapNode, newKey, setHeapNode, Me.lessThan)
        End Sub

        Public Overloads Function ToString(selector As IToString(Of T)) As String
            Return Me.root.ToString(selector)
        End Function

    End Class

End Namespace
