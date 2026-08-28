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

Imports std = System.Math

''' <summary>
''' Barnes-Hut 空间划分树上的一个节点
''' </summary>
Friend Class SPNode

    ''' <summary>
    ''' 本节点所覆盖的点在索引排列中的区间 [start, finish)
    ''' </summary>
    Friend start As Integer
    Friend finish As Integer

    Friend isLeaf As Boolean

    ''' <summary>
    ''' 子节点，长度为 2^dim，未被占用的槽位为 <c>Nothing</c>
    ''' </summary>
    Friend children As SPNode()

    ''' <summary>
    ''' 子树质心，长度为 dim
    ''' </summary>
    Friend com As Double()

    ''' <summary>
    ''' cell 在各个维度上的宽度，长度为 dim
    ''' </summary>
    Friend width As Double()

    ''' <summary>
    ''' cell 的最大宽度，Barnes-Hut 判据直接使用的量
    ''' </summary>
    Friend maxWidth As Double

    ''' <summary>
    ''' 本子树所包含的点数
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property Count As Integer
        Get
            Return finish - start
        End Get
    End Property

    Friend Sub New([dim] As Integer)
        com = New Double([dim] - 1) {}
        width = New Double([dim] - 1) {}
        children = Nothing
        maxWidth = 0
    End Sub

    Friend Sub New([dim] As Integer, start As Integer, finish As Integer)
        Me.New([dim])
        Me.start = start
        Me.finish = finish
    End Sub
End Class

''' <summary>
''' Barnes-Hut 空间划分树（2^dim 叉树）
''' </summary>
''' <remarks>
''' 移植自 L. van der Maaten 的 bh_tsne 参考实现（SPTree）。
''' 
''' t-SNE 梯度中的斥力项需要对所有 N² 个点对求和，这是 O(N²) 复杂度的根源。
''' Barnes-Hut 的做法是把当前的低维嵌入组织成一棵空间划分树，
''' 当某个 cell 的角宽度（宽度 / 到质心的距离）小于阈值 theta 时，
''' 就用该 cell 的质心一次性近似整棵子树，从而把单次遍历降到 O(log N)，
''' 整体降到 O(N log N)。theta 越大近似越激进、速度越快、精度越低，
''' theta = 0 时退化为精确计算。
''' 
''' 建树过程对点索引数组做原地重排，使得任意节点的子树都对应一段连续区间，
''' 因此节点数仅为 O(N / leafSize)，内存开销相对 N 可以忽略。
''' </remarks>
Friend Class SPTree

    ''' <summary>
    ''' 叶子节点最多容纳的点数；超过则继续细分
    ''' </summary>
    Friend Const DEFAULT_LEAF_SIZE As Integer = 24

    Private ReadOnly mDim As Integer
    Private ReadOnly noChildren As Integer
    Private ReadOnly Y As Double()
    Private ReadOnly nPoints As Integer
    Private ReadOnly leafSize As Integer

    ''' <summary>
    ''' 点索引的排列，建树过程中被就地重排
    ''' </summary>
    Private ReadOnly idx As Integer()

    ''' <summary>
    ''' 按 BFS 顺序收集的全部节点；逆序遍历即为后序遍历
    ''' </summary>
    Private ReadOnly nodes As New List(Of SPNode)()

    Private root As SPNode

    ''' <summary>
    ''' 依据当前的低维嵌入构建空间划分树
    ''' </summary>
    ''' <param name="[dim]">嵌入维度，通常为 2 或 3</param>
    ''' <param name="Y">低维坐标，行主序的一维数组，长度为 N * dim（借用，内部不复制）</param>
    ''' <param name="N">样本数量</param>
    ''' <param name="leafSize">叶子节点容量</param>
    Friend Sub New([dim] As Integer, Y As Double(), N As Integer, Optional leafSize As Integer = DEFAULT_LEAF_SIZE)
        If [dim] <= 0 OrElse [dim] > 10 Then
            Throw New ArgumentOutOfRangeException(NameOf([dim]), $"Barnes-Hut tree only supports 1..10 dimensions, but got {[dim]}.")
        End If

        Me.mDim = [dim]
        Me.Y = Y
        Me.nPoints = N
        Me.noChildren = 1 << [dim]
        Me.leafSize = std.Max(4, leafSize)
        Me.idx = New Integer(N - 1) {}

        For i As Integer = 0 To N - 1
            Me.idx(i) = i
        Next

        Call Build()
    End Sub

    ''' <summary>
    ''' 建树：广度优先细分 + 自底向上汇总质心
    ''' </summary>
    Private Sub Build()
        root = New SPNode(mDim, 0, nPoints)

        If nPoints <= 0 Then
            Return
        End If

        ' 样本量不超过叶子容量时整棵树就是一个叶子，
        ' 否则下面的遍历会因为 children 为 Nothing 而在递归时报错
        If nPoints <= leafSize Then
            root.isLeaf = True
        End If

        nodes.Add(root)

        Dim queue As New Queue(Of SPNode)()
        queue.Enqueue(root)

        While queue.Count > 0
            Dim node = queue.Dequeue()

            If node.Count <= leafSize OrElse node.isLeaf Then
                node.isLeaf = True
                Continue While
            End If

            Call Subdivide(node)

            ' 宽度退化为 0（所有点重合）时无法细分，Subdivide 会将其标记为叶子
            If node.isLeaf Then
                Continue While
            End If

            For Each child As SPNode In node.children
                If child IsNot Nothing Then
                    nodes.Add(child)
                    queue.Enqueue(child)
                End If
            Next
        End While

        ' BFS 保证子节点的索引一定大于父节点，因此逆序遍历即为后序遍历，
        ' 可以安全地自底向上汇总质心
        For t As Integer = nodes.Count - 1 To 0 Step -1
            Call ComputeCenterOfMass(nodes(t))
        Next
    End Sub

    ''' <summary>
    ''' 把当前节点的点集按各维度的中点分配到 2^dim 个子节点之中
    ''' </summary>
    Private Sub Subdivide(node As SPNode)
        Dim lo = New Double(mDim - 1) {}
        Dim hi = New Double(mDim - 1) {}

        Call ComputeBounds(node.start, node.finish, lo, hi)

        ' 注意：不能命名为 mid，Mid 是 VB 的保留字
        Dim splitAt = New Double(mDim - 1) {}
        Dim maxW As Double = 0

        For d As Integer = 0 To mDim - 1
            splitAt(d) = (lo(d) + hi(d)) / 2
            node.width(d) = hi(d) - lo(d)

            If node.width(d) > maxW Then
                maxW = node.width(d)
            End If
        Next

        node.maxWidth = maxW

        ' 所有点重合时无法再细分，退化为叶子节点（否则会无限递归）
        If maxW <= 0 Then
            node.isLeaf = True
            Return
        End If

        Dim n As Integer = node.finish - node.start
        Dim codes = New Integer(n - 1) {}
        Dim counts = New Integer(noChildren - 1) {}

        For p As Integer = 0 To n - 1
            Dim offset As Integer = idx(node.start + p) * mDim
            Dim code As Integer = 0

            For d As Integer = 0 To mDim - 1
                If Y(offset + d) > splitAt(d) Then
                    code = code Or (1 << d)
                End If
            Next

            codes(p) = code
            counts(code) += 1
        Next

        ' 依据 code 做一次计数排序（稳定），令每个子节点都得到一段连续区间
        Dim offsets = New Integer(noChildren) {}
        For c As Integer = 0 To noChildren - 1
            offsets(c + 1) = offsets(c) + counts(c)
        Next

        Dim cursor = New Integer(noChildren - 1) {}
        Call System.Array.Copy(offsets, cursor, noChildren)

        Dim buf = New Integer(n - 1) {}
        For p As Integer = 0 To n - 1
            Dim c As Integer = codes(p)
            buf(cursor(c)) = idx(node.start + p)
            cursor(c) += 1
        Next

        Call System.Array.Copy(buf, 0, idx, node.start, n)

        Dim children = New SPNode(noChildren - 1) {}

        For c As Integer = 0 To noChildren - 1
            If counts(c) > 0 Then
                children(c) = New SPNode(mDim, node.start + offsets(c), node.start + offsets(c + 1))
            End If
        Next

        node.isLeaf = False
        node.children = children
    End Sub

    ''' <summary>
    ''' 计算 [start, finish) 区间内所有点的包围盒
    ''' </summary>
    Private Sub ComputeBounds(start As Integer, finish As Integer, lo As Double(), hi As Double())
        For d As Integer = 0 To mDim - 1
            lo(d) = Double.PositiveInfinity
            hi(d) = Double.NegativeInfinity
        Next

        For p As Integer = start To finish - 1
            Dim offset As Integer = idx(p) * mDim

            For d As Integer = 0 To mDim - 1
                Dim v As Double = Y(offset + d)

                If v < lo(d) Then lo(d) = v
                If v > hi(d) Then hi(d) = v
            Next
        Next
    End Sub

    ''' <summary>
    ''' 依据子节点（或叶子自身的点）汇总出本节点的质心
    ''' </summary>
    Private Sub ComputeCenterOfMass(node As SPNode)
        For d As Integer = 0 To mDim - 1
            node.com(d) = 0
        Next

        If node.isLeaf Then
            Dim n As Integer = node.finish - node.start

            If n <= 0 Then Return

            For p As Integer = node.start To node.finish - 1
                Dim offset As Integer = idx(p) * mDim

                For d As Integer = 0 To mDim - 1
                    node.com(d) += Y(offset + d)
                Next
            Next

            For d As Integer = 0 To mDim - 1
                node.com(d) /= n
            Next
        Else
            Dim total As Integer = 0

            For Each child As SPNode In node.children
                If child Is Nothing Then Continue For

                Dim w As Integer = child.finish - child.start

                If w <= 0 Then Continue For

                total += w

                For d As Integer = 0 To mDim - 1
                    node.com(d) += w * child.com(d)
                Next
            Next

            If total <= 0 Then Return

            For d As Integer = 0 To mDim - 1
                node.com(d) /= total
            Next
        End If
    End Sub

    ''' <summary>
    ''' 计算指定点的远场（斥力）作用力，结果累加到 <paramref name="negF"/>
    ''' </summary>
    ''' <param name="pointIndex">目标点索引</param>
    ''' <param name="theta">
    ''' Barnes-Hut 阈值：当 cell 的最大宽度 / 到质心的距离 &lt; theta 时，
    ''' 用质心近似整棵子树。取值越大越快越粗糙，取 0 退化为精确计算。
    ''' </param>
    ''' <param name="negF">斥力累加器，长度为 N * dim</param>
    ''' <param name="sumQ">配分函数 Z 的累加器（线程本地，调用方负责最终合并）</param>
    Friend Sub ComputeNonEdgeForces(pointIndex As Integer, theta As Double,
                                    negF As Double(), ByRef sumQ As Double)
        Call ComputeNonEdgeForces(root, pointIndex, theta, negF, sumQ)
    End Sub

    Private Sub ComputeNonEdgeForces(node As SPNode, pointIndex As Integer, theta As Double,
                                     negF As Double(), ByRef sumQ As Double)
        ' spend no time on empty nodes or self
        If node Is Nothing Then Return
        If node.finish <= node.start Then Return

        If node.isLeaf AndAlso node.Count = 1 AndAlso idx(node.start) = pointIndex Then
            Return
        End If

        Dim offset As Integer = pointIndex * mDim
        ' 注意：不要命名为 D，VB 大小写不敏感会与循环变量 d 冲突
        Dim d2sum As Double = 0

        For d As Integer = 0 To mDim - 1
            Dim tmp As Double = Y(offset + d) - node.com(d)
            d2sum += tmp * tmp
        Next

        If node.isLeaf OrElse node.maxWidth / std.Sqrt(d2sum) < theta Then
            Dim Q As Double = 1.0 / (1.0 + d2sum)
            Dim size As Integer = node.Count

            sumQ += size * Q

            Dim mult As Double = size * Q * Q

            For d As Integer = 0 To mDim - 1
                negF(offset + d) += mult * (Y(offset + d) - node.com(d))
            Next
        Else
            For Each child As SPNode In node.children
                Call ComputeNonEdgeForces(child, pointIndex, theta, negF, sumQ)
            Next
        End If
    End Sub

    ''' <summary>
    ''' 当前树上的节点总数（调试与性能诊断用）
    ''' </summary>
    ''' <returns></returns>
    Friend ReadOnly Property NodeCount As Integer
        Get
            Return nodes.Count
        End Get
    End Property
End Class
