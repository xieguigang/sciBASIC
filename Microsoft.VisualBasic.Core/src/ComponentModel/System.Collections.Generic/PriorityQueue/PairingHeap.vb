#Region "Microsoft.VisualBasic::ea471c810a5cba8a33755fb2df814644, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\PairingHeap.vb"

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
    '         Function: contains, decreaseKey, insert, isHeap, merge
    '                   mergePairs, removeMin, ToString
    ' 
    '         Sub: forEach
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Collection

    Public Class PairingHeap(Of T)

        ''' <summary>
        ''' 孩子堆容器。Add = push，RemoveAt(Count - 1) = pop，
        ''' 与 JS 原版数组（push/pop + 随机访问）语义一致。
        ''' </summary>
        Dim subheaps As List(Of PairingHeap(Of T))

        ''' <summary>节点元素值；Nothing 表示空哨兵节点</summary>
        Public elem As T

        ''' <summary>
        ''' 子树元素总数（含本节点）。
        ''' 迭代式统计以避免深树递归栈溢出；幽灵空节点不计入。
        ''' 注意：PriorityQueue.count 走独立的 O(1) 计数器，日常请优先使用它。
        ''' </summary>
        Public ReadOnly Property count() As Integer
            Get
                If Me.empty Then
                    Return 0
                End If

                Dim total As Integer = 0
                Dim nodeStack As New Stack(Of PairingHeap(Of T))
                Call nodeStack.Push(Me)

                While nodeStack.Count > 0
                    Dim node = nodeStack.Pop()
                    If node.empty Then
                        Continue While   ' 幽灵空节点不计入
                    End If
                    total += 1
                    For Each h As PairingHeap(Of T) In node.subheaps
                        Call nodeStack.Push(h)
                    Next
                End While

                Return total
            End Get
        End Property

        ''' <summary>本节点为堆根时的最小元素（即 elem 本身）</summary>
        Public ReadOnly Property min() As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.elem
            End Get
        End Property

        ''' <summary>空哨兵节点判定（elem Is Nothing）</summary>
        Public ReadOnly Property empty() As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.elem Is Nothing
            End Get
        End Property

        ''' <summary>空哨兵节点构造器</summary>
        Public Sub New()
            Me.subheaps = New List(Of PairingHeap(Of T))
            Me.elem = Nothing
        End Sub

        Public Sub New(elem As T)
            Me.subheaps = New List(Of PairingHeap(Of T))
            Me.elem = elem
        End Sub

        ''' <summary>
        ''' 结构化输出（s 表达式形式），仅用于调试。
        ''' </summary>
        ''' <param name="selector">a delegate function for cast target object as string</param>
        Public Overloads Function ToString(selector As IToString(Of T)) As String
            Dim str = ""
            Dim needComma = False

            For i As Integer = 0 To Me.subheaps.Count - 1
                Dim subheap As PairingHeap(Of T) = Me.subheaps(i)

                ' 对齐 JS 原版 if (!subheap.elem)：空哨兵节点跳过
                If subheap.elem Is Nothing Then
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

            ' 对齐 JS 原版 if (this.elem)：非空节点输出自身元素 + 孩子结构
            If Me.elem IsNot Nothing Then
                Return selector(Me.elem) & str
            Else
                Return str
            End If
        End Function

        ''' <summary>
        ''' 先序遍历全部元素并调用 f。迭代式实现，避免深树递归栈溢出；
        ''' 访问顺序与原递归版本一致。空哨兵节点跳过。
        ''' </summary>
        Public Sub forEach(f As Action(Of T, PairingHeap(Of T)))
            If Me.empty Then
                Return
            End If

            Dim nodeStack As New Stack(Of PairingHeap(Of T))
            Call nodeStack.Push(Me)

            While nodeStack.Count > 0
                Dim node = nodeStack.Pop()
                If node.empty Then
                    Continue While
                End If

                Call f(node.elem, node)

                ' 逆序压栈，保证出栈顺序与递归版（index 0 -> Count-1）一致
                For i As Integer = node.subheaps.Count - 1 To 0 Step -1
                    Call nodeStack.Push(node.subheaps(i))
                Next
            End While
        End Sub

        ''' <summary>
        ''' 按对象引用判断 h 是否在本子树中（迭代式）。
        ''' </summary>
        Public Function contains(h As PairingHeap(Of T)) As Boolean
            Dim nodeStack As New Stack(Of PairingHeap(Of T))
            Call nodeStack.Push(Me)

            While nodeStack.Count > 0
                Dim node = nodeStack.Pop()
                If node Is h Then
                    Return True
                End If
                For Each child As PairingHeap(Of T) In node.subheaps
                    Call nodeStack.Push(child)
                Next
            End While

            Return False
        End Function

        ''' <summary>
        ''' 堆性质检查（调试用）。迭代式实现；
        ''' 容忍幽灵空节点（它们会在下次 mergePairs 时被惰性清除）。
        ''' </summary>
        Public Function isHeap(lessThan As Func(Of T, T, Boolean)) As Boolean
            If Me.empty Then
                Return True
            End If

            Dim nodeStack As New Stack(Of PairingHeap(Of T))
            Call nodeStack.Push(Me)

            While nodeStack.Count > 0
                Dim node = nodeStack.Pop()
                If node.empty Then
                    Continue While
                End If

                For Each h As PairingHeap(Of T) In node.subheaps
                    If Not h.empty Then
                        If Not lessThan(node.elem, h.elem) Then
                            Return False
                        End If
                    End If
                    Call nodeStack.Push(h)
                Next
            End While

            Return True
        End Function

        Public Function insert(obj As T, lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            Return Me.merge(New PairingHeap(Of T)(obj), lessThan)
        End Function

        ''' <summary>
        ''' 合并两棵堆，返回合并后的堆根。O(1)。
        ''' 较小者作为根，另一棵整体挂为孩子。
        ''' </summary>
        ''' <param name="heap2"></param>
        ''' <param name="lessThan"></param>
        ''' <returns></returns>
        Public Function merge(heap2 As PairingHeap(Of T), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            ' 防御：自合并会产生环，导致后续遍历死循环
            If Me Is heap2 Then
                Return Me
            End If
            If Me.empty() Then
                Return heap2
            ElseIf heap2.empty() Then
                Return Me
            ElseIf lessThan(Me.elem, heap2.elem) Then
                Me.subheaps.Add(heap2)
                Return Me
            Else
                heap2.subheaps.Add(Me)
                Return heap2
            End If
        End Function

        ''' <summary>
        ''' 弹出堆根（最小元素），返回剩余元素合并后的新堆根。
        ''' 空堆返回新的空哨兵节点（类型稳定，便于 decreaseKey 安全回填）。
        ''' </summary>
        Public Function removeMin(lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.empty() Then
                Return New PairingHeap(Of T)()
            Else
                Return Me.mergePairs(lessThan)
            End If
        End Function

        ''' <summary>
        ''' 将本节点的全部孩子两两配对合并为一棵堆（删除堆根后的标准操作）。
        ''' 均摊 O(log n)。
        '''
        ''' 迭代式两趟配对，与原递归版本的合并调用序列完全等价：
        '''   递归版: result = merge(P0, merge(P1, ... merge(Pm-1, empty)))
        '''   其中 Pi = merge(c_(k-2i), c_(k-2i-1))（自孩子栈顶向栈底两两配对）
        ''' </summary>
        Public Function mergePairs(lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.subheaps.Count = 0 Then
                Return New PairingHeap(Of T)()
            End If

            ' ---- 第一趟：自孩子列表尾部向头部两两配对 ----
            Dim paired As New List(Of PairingHeap(Of T))
            Dim i As Integer = Me.subheaps.Count - 1

            While i > 0
                paired.Add(Me.subheaps(i).merge(Me.subheaps(i - 1), lessThan))
                i -= 2
            End While
            If i = 0 Then
                ' 奇数个孩子：最底下剩一个单独入列
                paired.Add(Me.subheaps(0))
            End If

            ' ---- 第二趟：自后向前依次左折叠合并 ----
            Dim result = paired(paired.Count - 1)
            For j As Integer = paired.Count - 2 To 0 Step -1
                result = paired(j).merge(result, lessThan)
            Next

            ' 释放本节点对孩子列表的引用（等价于原递归版的逐个 Pop）
            Call Me.subheaps.Clear()
            Return result
        End Function

        ''' <summary>
        ''' 将树中指定的 subheap 节点按键值降低为 newValue 后重新归堆。
        ''' 契约：newValue 必须 lessThan 于原键值（本方法不校验，与 JS 原版一致）。
        '''
        ''' 实现：把 subheap 当作子堆根弹掉（其孩子合并为 newHeap），再将
        ''' newHeap 的内容回填到 subheap 节点对象上以保持外部句柄引用有效，
        ''' 最后把携带新键的新节点合并入堆根。
        '''
        ''' 注意：若 subheap 是叶子节点，会在其父节点处遗留一个空哨兵孩子
        ''' （幽灵节点），由后续 mergePairs 惰性清除，不影响堆性质与出队顺序。
        ''' </summary>
        Public Function decreaseKey(subheap As PairingHeap(Of T), newValue As T, setHeapNode As Action(Of T, PairingHeap(Of T)), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            Dim newHeap = subheap.removeMin(lessThan)

            ' 将移除最小值后的子树内容回填到原节点对象上（保持外部引用有效）
            subheap.elem = newHeap.elem
            subheap.subheaps = newHeap.subheaps

            If setHeapNode IsNot Nothing AndAlso Not subheap.empty Then
                Call setHeapNode(subheap.elem, subheap)
            End If

            Dim pairingNode As New PairingHeap(Of T)(newValue)
            If setHeapNode IsNot Nothing Then
                Call setHeapNode(newValue, pairingNode)
            End If

            Return Me.merge(pairingNode, lessThan)
        End Function

        ''' <summary>
        ''' 按值查找节点（判等语义 = EqualityComparer(Of T).Default，
        ''' 即 x.Equals(elem)，对齐 java.util.PriorityQueue#remove(Object) 的
        ''' o.equals(e) 判等方向）。迭代式实现，O(n)。
        ''' </summary>
        ''' <returns>第一个匹配的节点；不存在返回 Nothing</returns>
        Friend Function findNode(x As T) As PairingHeap(Of T)
            Dim nodeStack As New Stack(Of PairingHeap(Of T))
            Call nodeStack.Push(Me)

            While nodeStack.Count > 0
                Dim node = nodeStack.Pop()
                If node.empty Then
                    Continue While   ' 幽灵空节点不参与匹配
                End If
                If EqualityComparer(Of T).Default.Equals(node.elem, x) Then
                    Return node
                End If
                For Each child As PairingHeap(Of T) In node.subheaps
                    Call nodeStack.Push(child)
                Next
            End While

            Return Nothing
        End Function

        ''' <summary>
        ''' 从树中摘除指定的 target 节点并返回修补后的子树根。
        ''' 摘除方式：target 的孩子经 mergePairs 合并后，原位顶替 target 在
        ''' 其父节点孩子列表中的槽位（孩子为空则直接删除槽位）。
        ''' 迭代式 DFS 实现，O(n)。
        ''' </summary>
        ''' <returns>修补后的子树根；未找到 target 时返回 Me 本身</returns>
        Friend Function removeNode(target As PairingHeap(Of T), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me Is target Then
                ' 目标即当前子树根：孩子合并后即为修补结果
                Dim result = Me.removeMin(lessThan)
                ' 已出树的节点将 elem 置空：外部过期句柄可通过 elem Is Nothing
                ' 廉价检测，防止对孤儿节点误调用 decreaseKey 等操作
                Me.elem = Nothing
                Return result
            End If

            ' 迭代式 DFS：nodeStack 保存待扫描节点，idxStack 保存对应节点
            ' 当前扫描到的孩子下标（两个栈同步压入/弹出）
            Dim nodeStack As New Stack(Of PairingHeap(Of T))
            Dim idxStack As New Stack(Of Integer)
            Call nodeStack.Push(Me)
            Call idxStack.Push(0)

            While nodeStack.Count > 0
                Dim node = nodeStack.Peek()
                Dim i As Integer = idxStack.Pop()

                If i >= node.subheaps.Count Then
                    ' 该节点孩子扫描完毕，出栈
                    Call nodeStack.Pop()
                    Continue While
                End If

                Call idxStack.Push(i + 1)
                Dim child = node.subheaps(i)

                If child Is target Then
                    ' 找到目标：孩子合并后的子树原位顶替（或删除空哨兵槽位）
                    Dim replacement = child.removeMin(lessThan)
                    child.elem = Nothing
                    If replacement.empty Then
                        Call node.subheaps.RemoveAt(i)
                    Else
                        node.subheaps(i) = replacement
                    End If
                    Return Me
                End If

                Call nodeStack.Push(child)
                Call idxStack.Push(0)
            End While

            Return Me
        End Function

    End Class

End Namespace
