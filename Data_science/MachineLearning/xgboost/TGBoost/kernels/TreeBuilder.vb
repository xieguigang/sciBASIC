Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace train

    ''' <summary>
    ''' 建树时使用的分裂查找算法。
    ''' </summary>
    Public Enum TreeMethod
        ''' <summary>
        ''' 精确前缀扫描。语义与旧的 SLIQ 实现一致（同样的阈值、同样的增益），
        ''' 因此可作为重构前后的结果对照基线。
        ''' </summary>
        Exact = 0
        ''' <summary>
        ''' <c>max_bin</c> 分桶直方图。高基数（连续型）特征上显著更快，
        ''' 但会引入量化误差，需要配合回归评测使用。
        ''' </summary>
        Hist = 1
    End Enum
End Namespace

Namespace train.kernels

    ''' <summary>
    ''' 建树结果。
    ''' </summary>
    Friend Class BuildTree
        ''' <summary>
        ''' 物化后的树根
        ''' </summary>
        Public Property Root As TreeNode
        ''' <summary>
        ''' 全部节点（含叶节点），用于回填「行 -&gt; 叶节点」映射
        ''' </summary>
        Public Property Nodes As List(Of BuildNode)
        Public Property Partitioner As RowPartitioner
        Public Property NodeCount As Integer
        Public Property NanNodeCount As Integer
    End Class

    ''' <summary>
    ''' 逐层（level-wise）建树器。
    '''
    ''' 与旧 <c>Tree.build</c> 的差别：
    ''' + 节点统计量在「分区完成后立刻计算」，每层只扫一遍本层节点的行（旧实现是 3 遍全量 N）；
    ''' + 分裂查找交给 <see cref="SplitFinderBase"/> 的 exact / hist 实现，并行度受 <c>num_thread</c> 控制；
    ''' + 不做「每层每列一个 Task」，也避免 <c>List.Poll</c>（<c>RemoveAt(0)</c>）的 O(n²) 出队；
    ''' + 建树结束后一次性物化为 <see cref="TreeNode"/>，保持 <c>Tree.root</c> 与
    '''   <c>ModelSerializer</c> 的既有契约。
    ''' </summary>
    Friend Class TreeBuilder

        Private ReadOnly columns As ColumnBlock
        Private ReadOnly lambda As Double
        Private ReadOnly gamma As Double
        Private ReadOnly maxDepth As Integer
        Private ReadOnly minChildWeight As Double
        Private ReadOnly minSamples As Integer
        Private ReadOnly method As TreeMethod
        Private ReadOnly maxBin As Integer
        Private ReadOnly numThread As Integer

        ''' <summary>
        ''' 树的最大深度硬上限：防止 <c>max_depth = 0</c>（不限制）时节点编号
        ''' <c>3i±1</c> 溢出 <see cref="Integer"/>，也防止病态数据把树长成链。
        ''' </summary>
        Private Const MAX_DEPTH_HARD_LIMIT As Integer = 30

        Public Sub New(columns As ColumnBlock, lambda As Double, gamma As Double, maxDepth As Integer,
                       minChildWeight As Double, minSamples As Integer,
                       method As TreeMethod, maxBin As Integer, numThread As Integer)

            Me.columns = columns
            Me.lambda = If(lambda <= 0, 0.00001, lambda)
            Me.gamma = gamma
            Me.maxDepth = If(maxDepth <= 0, MAX_DEPTH_HARD_LIMIT, std.Min(maxDepth, MAX_DEPTH_HARD_LIMIT))
            Me.minChildWeight = minChildWeight
            Me.minSamples = minSamples
            Me.method = method
            Me.maxBin = If(maxBin <= 1, 256, maxBin)
            Me.numThread = numThread
        End Sub

        Private Function createFinder() As SplitFinderBase
            If method = TreeMethod.Hist Then
                Return New HistogramSplitFinder(columns, lambda, gamma, minChildWeight, minSamples, numThread, maxBin)
            Else
                Return New ExactSplitFinder(columns, lambda, gamma, minChildWeight, minSamples, numThread)
            End If
        End Function

        ''' <summary>
        ''' 构建一棵树
        ''' </summary>
        ''' <param name="grad">一阶梯度和</param>
        ''' <param name="hess">二阶梯度和</param>
        ''' <param name="mask">行采样掩码，<c>Nothing</c> 表示全量参与</param>
        ''' <param name="colSelected">本棵树参与分裂评估的特征列</param>
        ''' <param name="rowCount">样本数</param>
        Public Function Build(grad As Double(), hess As Double(), mask As Boolean(),
                              colSelected As List(Of Integer), rowCount As Integer) As BuildTree

            Dim partitioner As New RowPartitioner(rowCount)
            Dim nodes As New List(Of BuildNode)()
            Dim root As New BuildNode(0, 1, 1)

            Call nodes.Add(root)
            Call partitioner.AssignAll(root.Id)
            Call computeStats(root, partitioner, grad, hess, mask)

            Dim finder = createFinder()
            Dim alive As New List(Of BuildNode)
            Dim nanNodes As Integer = 0

            Call alive.Add(root)

            While alive.Count > 0
                Dim candidates As SplitCandidate() = finder.FindAll(alive, partitioner, grad, hess, mask, colSelected)
                Dim nextAlive As New List(Of BuildNode)()
                Dim created As New List(Of BuildNode)()

                For i As Integer = 0 To alive.Count - 1
                    Dim node = alive(i)
                    Dim candidate = candidates(i)

                    If candidate Is Nothing OrElse Not candidate.HasSplit Then
                        Call makeLeaf(node)
                        Continue For
                    End If

                    Dim leftNode As New BuildNode(nodes.Count, node.Depth + 1, 3 * node.Index - 1)
                    Dim rightNode As New BuildNode(nodes.Count + 1, node.Depth + 1, 3 * node.Index + 1)

                    Call nodes.Add(leftNode)
                    Call nodes.Add(rightNode)

                    Dim nanNode As BuildNode = Nothing

                    If candidate.NanDirection = SplitCandidate.NanDirectionChild Then
                        nanNode = New BuildNode(nodes.Count, node.Depth + 1, 3 * node.Index)
                        Call nodes.Add(nanNode)
                        nanNodes += 1
                    End If

                    node.Feature = candidate.Feature
                    node.Threshold = candidate.Threshold
                    node.LeftCategories = candidate.LeftCategories
                    node.NanDirection = candidate.NanDirection
                    node.Left = leftNode
                    node.Right = rightNode
                    node.NanChild = nanNode

                    Dim nanId As Integer = If(nanNode Is Nothing, -1, nanNode.Id)
                    Dim assign As Func(Of Integer, Integer) = createAssigner(candidate, leftNode.Id, rightNode.Id, nanId)

                    Call partitioner.Split(node.Id, leftNode.Id, rightNode.Id, nanId, assign)

                    ' 统计推迟到本层循环结束后并行执行：新建节点的行集合互不相交
                    Call created.Add(leftNode)
                    Call created.Add(rightNode)

                    If nanNode IsNot Nothing Then
                        Call created.Add(nanNode)
                    End If
                Next

                Call computeStatsAll(created, partitioner, grad, hess, mask)

                For Each child As BuildNode In created

                    If shouldStop(child) Then
                        Call makeLeaf(child)
                    Else
                        Call nextAlive.Add(child)
                    End If
                Next

                alive = nextAlive
            End While

            Dim ret As New BuildTree With {
                .Nodes = nodes,
                .Partitioner = partitioner,
                .NodeCount = nodes.Count,
                .NanNodeCount = nanNodes
            }

            ret.Root = materialize(root)
            Return ret
        End Function

        ''' <summary>
        ''' 构造「行 -&gt; 子节点」判定函数
        ''' </summary>
        Private Function createAssigner(candidate As SplitCandidate, leftId As Integer, rightId As Integer,
                                        nanId As Integer) As Func(Of Integer, Integer)
            Dim feature As Integer = candidate.Feature
            Dim origin = columns.Origin
            Dim nanDirection As Integer = candidate.NanDirection
            Dim threshold As Single = candidate.Threshold

            If candidate.IsCategorical Then
                Dim cats = columns.CategoryValues(feature)
                Dim codes = columns.CategoryCodes(feature)
                Dim leftFlag As Boolean() = New Boolean(cats.Length - 1) {}

                For Each code As Integer In candidate.LeftCategories
                    leftFlag(code) = True
                Next

                Return Function(row As Integer) As Integer
                           Dim v As Single = origin(row)(feature)

                           If ColumnBlock.IsMissing(v) Then
                               Return nanChildOf(nanDirection, leftId, rightId, nanId)
                           End If

                           Dim code As Integer

                           If codes.TryGetValue(v, code) AndAlso leftFlag(code) Then
                               Return leftId
                           Else
                               Return rightId
                           End If
                       End Function
            Else
                Return Function(row As Integer) As Integer
                           Dim v As Single = origin(row)(feature)

                           If ColumnBlock.IsMissing(v) Then
                               Return nanChildOf(nanDirection, leftId, rightId, nanId)
                           End If

                           If v <= threshold Then
                               Return leftId
                           Else
                               Return rightId
                           End If
                       End Function
            End If
        End Function

        ''' <summary>
        ''' 缺失值应落入的子节点。
        '''
        ''' <c>NanDirectionNone</c>（训练集在该列上无缺失）保持旧实现的语义：落入右子节点
        ''' （训练期本就无缺失样本，此分支不会被执行；预测期由 <see cref="FlatTree"/> 用样本数启发式决定）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function nanChildOf(nanDirection As Integer, leftId As Integer, rightId As Integer,
                                           nanId As Integer) As Integer
            Select Case nanDirection
                Case SplitCandidate.NanDirectionChild
                    Return nanId
                Case SplitCandidate.NanDirectionLeft
                    Return leftId
                Case Else
                    Return rightId
            End Select
        End Function

        Private Sub computeStats(node As BuildNode, partitioner As RowPartitioner,
                                 grad As Double(), hess As Double(), mask As Boolean())

            Dim rows = partitioner.RowsOf(node.Id)
            Dim g As Double = 0
            Dim h As Double = 0
            Dim hall As Double = 0
            Dim count As Integer = 0

            For Each row As Integer In rows
                Dim hv As Double = hess(row)

                hall += hv

                If mask Is Nothing OrElse mask(row) Then
                    g += grad(row)
                    h += hv
                    count += 1
                End If
            Next

            node.Grad = g
            node.Hess = h
            node.HessAll = hall
            node.NSamples = count
        End Sub

        ''' <summary>
        ''' 并行统计一批新建节点的梯度/海森和。
        '''
        ''' 新建节点的行集合互不相交，因此各节点可以完全并行地累加，没有共享写入；
        ''' 旧实现把这一步放在逐节点分裂的循环里串行执行，是每层最主要的串行段之一。
        ''' </summary>
        Private Sub computeStatsAll(created As List(Of BuildNode), partitioner As RowPartitioner,
                                    grad As Double(), hess As Double(), mask As Boolean())

            If created.Count = 0 Then
                Return
            End If

            If created.Count = 1 OrElse Not SimdOptions.IsActive OrElse numThread = 1 Then

                For Each node As BuildNode In created
                    Call computeStats(node, partitioner, grad, hess, mask)
                Next

                Return
            End If

            ' 注意：回调第二个参数是**开区间终点**（exclusive），不是长度
            Call SimdParallel.ForRange(created.Count,
                Sub(start As Integer, ends As Integer)

                    For i As Integer = start To ends - 1
                        Call computeStats(created(i), partitioner, grad, hess, mask)
                    Next
                End Sub,
                minLength:=4)
        End Sub

        Private Function shouldStop(node As BuildNode) As Boolean
            If node.Depth >= maxDepth Then
                Return True
            End If

            If node.HessAll < minChildWeight Then
                Return True
            End If

            Return node.NSamples <= minSamples
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub makeLeaf(node As BuildNode)
            node.IsLeaf = True
            node.LeafScore = -node.Grad / (node.Hess + lambda)
        End Sub

        ''' <summary>
        ''' 把建树期的轻量节点物化为 <see cref="TreeNode"/>（保持三叉树编号与公共字段语义）
        ''' </summary>
        Private Function materialize(node As BuildNode) As TreeNode
            Dim treeNode As New TreeNode(node.Index, node.Depth, node.IsLeaf) With {
                .num_sample = node.NSamples
            }

            node.Materialized = treeNode

            If node.IsLeaf Then
                Call treeNode.leaf_node_setter(node.LeafScore, True)
                Return treeNode
            End If

            treeNode.split_feature = node.Feature
            treeNode.nan_go_to = node.NanDirection

            If node.LeftCategories IsNot Nothing Then
                Dim cats = columns.CategoryValues(node.Feature)
                Dim list As Double() = New Double(node.LeftCategories.Length - 1) {}

                For k As Integer = 0 To node.LeftCategories.Length - 1
                    list(k) = cats(node.LeftCategories(k))
                Next

                Call treeNode.set_left_categories(list)
            Else
                treeNode.split_threshold = node.Threshold
            End If

            treeNode.left_child = materialize(node.Left)
            treeNode.right_child = materialize(node.Right)

            If node.NanChild IsNot Nothing Then
                treeNode.nan_child = materialize(node.NanChild)
            End If

            Return treeNode
        End Function
    End Class
End Namespace
