Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks

Namespace train.kernels

    ''' <summary>
    ''' 扁平化（编译后）的树，用于批量预测。
    '''
    ''' 旧实现的预测路径是「每棵树一次 PLINQ + 每样本一个 <c>PredictCallable</c> 对象 + <c>OrderBy</c> 排序」，
    ''' 复杂度 <c>O(T · N log N)</c> 且产生 <c>T × N</c> 次堆分配。编译后每个节点只占用几条平行数组，
    ''' 预测变成纯粹的整数索引跳转与一次浮点比较，整体降为 <c>O(T · N)</c> 且零分配。
    ''' </summary>
    Public Class FlatTree

        Private ReadOnly feature As Integer()
        Private ReadOnly threshold As Double()
        Private ReadOnly left As Integer()
        Private ReadOnly right As Integer()
        Private ReadOnly nanDefault As Integer()
        Private ReadOnly isLeaf As Boolean()
        Private ReadOnly leaf As Double()
        ''' <summary>
        ''' 类别分裂节点进入左子树的取值集合（已升序排序，用于二分查找）；数值节点为 <c>Nothing</c>
        ''' </summary>
        Private ReadOnly catValues As Double()()

        ''' <summary>
        ''' 节点数
        ''' </summary>
        Public ReadOnly Property NodeCount As Integer

        Private Sub New(nodeCount As Integer, feature As Integer(), threshold As Double(),
                        left As Integer(), right As Integer(), nanDefault As Integer(),
                        isLeaf As Boolean(), leaf As Double(), catValues As Double()())
            Me.NodeCount = nodeCount
            Me.feature = feature
            Me.threshold = threshold
            Me.left = left
            Me.right = right
            Me.nanDefault = nanDefault
            Me.isLeaf = isLeaf
            Me.leaf = leaf
            Me.catValues = catValues
        End Sub

        ''' <summary>
        ''' 把一棵 <see cref="TreeNode"/> 树编译为扁平数组
        ''' </summary>
        ''' <param name="root"></param>
        ''' <returns></returns>
        Public Shared Function Compile(root As TreeNode) As FlatTree
            If root Is Nothing Then
                Throw New ArgumentNullException(NameOf(root))
            End If

            Dim ids As New Dictionary(Of TreeNode, Integer)()
            Dim nodes As New List(Of TreeNode)()
            Dim queue As New Queue(Of TreeNode)()

            ids(root) = 0
            Call nodes.Add(root)
            Call queue.Enqueue(root)

            While queue.Count > 0
                Dim cur = queue.Dequeue()

                If cur.is_leaf Then
                    Continue While
                End If

                For Each child In {cur.left_child, cur.right_child, cur.nan_child}

                    If child IsNot Nothing AndAlso Not ids.ContainsKey(child) Then
                        ids(child) = nodes.Count
                        Call nodes.Add(child)
                        Call queue.Enqueue(child)
                    End If
                Next
            End While

            Dim count As Integer = nodes.Count
            Dim featureArr As Integer() = New Integer(count - 1) {}
            Dim thresholdArr As Double() = New Double(count - 1) {}
            Dim leftArr As Integer() = New Integer(count - 1) {}
            Dim rightArr As Integer() = New Integer(count - 1) {}
            Dim nanArr As Integer() = New Integer(count - 1) {}
            Dim isLeafArr As Boolean() = New Boolean(count - 1) {}
            Dim leafArr As Double() = New Double(count - 1) {}
            Dim catArr As Double()() = New Double(count - 1)() {}

            For i As Integer = 0 To count - 1
                Dim node = nodes(i)

                isLeafArr(i) = node.is_leaf

                If node.is_leaf Then
                    leafArr(i) = node.leaf_score
                    leftArr(i) = -1
                    rightArr(i) = -1
                    nanArr(i) = -1
                    featureArr(i) = -1
                    Continue For
                End If

                featureArr(i) = node.split_feature
                leftArr(i) = idOf(ids, node.left_child)
                rightArr(i) = idOf(ids, node.right_child)

                If node.split_left_child_catvalue IsNot Nothing Then
                    Dim values = node.split_left_child_catvalue.ToArray()

                    Call Array.Sort(values)
                    catArr(i) = values
                Else
                    thresholdArr(i) = node.split_threshold
                End If

                Select Case CInt(node.nan_go_to)
                    Case SplitCandidate.NanDirectionChild
                        nanArr(i) = idOf(ids, node.nan_child)
                    Case SplitCandidate.NanDirectionLeft
                        nanArr(i) = leftArr(i)
                    Case SplitCandidate.NanDirectionRight
                        nanArr(i) = rightArr(i)
                    Case Else
                        ' 训练集无缺失：沿用旧实现的启发式——进入样本更多的子节点
                        Dim lc = If(node.left_child Is Nothing, 0, node.left_child.num_sample)
                        Dim rc = If(node.right_child Is Nothing, 0, node.right_child.num_sample)

                        nanArr(i) = If(lc > rc, leftArr(i), rightArr(i))
                End Select
            Next

            Return New FlatTree(count, featureArr, thresholdArr, leftArr, rightArr,
                                nanArr, isLeafArr, leafArr, catArr)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function idOf(ids As Dictionary(Of TreeNode, Integer), node As TreeNode) As Integer
            If node Is Nothing Then
                Return -1
            End If

            Dim id As Integer

            If ids.TryGetValue(node, id) Then
                Return id
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' 单样本预测
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        Public Function Predict(row As Single()) As Double
            Dim i As Integer = 0

            While Not isLeaf(i)
                Dim f As Integer = feature(i)

                If f < 0 OrElse f >= row.Length Then
                    Return leaf(i)
                End If

                Dim v As Single = row(f)

                If ColumnBlock.IsMissing(v) Then
                    i = nanDefault(i)
                ElseIf catValues(i) IsNot Nothing Then
                    i = If(Array.BinarySearch(catValues(i), CDbl(v)) >= 0, left(i), right(i))
                ElseIf v <= threshold(i) Then
                    i = left(i)
                Else
                    i = right(i)
                End If

                If i < 0 Then
                    Return 0
                End If
            End While

            Return leaf(i)
        End Function

        ''' <summary>
        ''' 批量预测（旧实现的顺序由 <c>features</c> 索引决定，这里天然保持顺序）
        ''' </summary>
        ''' <param name="features"></param>
        ''' <returns></returns>
        Public Function Predict(features As Single()()) As Double()
            Dim ret As Double() = New Double(features.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                ret(i) = Predict(features(i))
            Next

            Return ret
        End Function

        ''' <summary>
        ''' 批量预测并把结果累加到 <paramref name="output"/>（并行安全：每行只写自己的槽位）
        ''' </summary>
        ''' <param name="features"></param>
        ''' <param name="output"></param>
        ''' <param name="scale">学习率等缩放系数</param>
        Public Sub Accumulate(features As Single()(), output As Double(), scale As Double)
            For i As Integer = 0 To output.Length - 1
                output(i) += scale * Predict(features(i))
            Next
        End Sub

        ''' <summary>
        ''' 多棵树并行批量累加：按行并行、内层遍历全部树，只派发一次任务
        ''' </summary>
        ''' <param name="trees"></param>
        ''' <param name="features"></param>
        ''' <param name="output"></param>
        ''' <param name="scale"></param>
        ''' <param name="numThread"></param>
        Public Shared Sub AccumulateAll(trees As FlatTree(), features As Single()(), output As Double(),
                                        scale As Double, numThread As Integer)
            Dim options As New ParallelOptions With {
                .MaxDegreeOfParallelism = If(numThread <= 0, -1, numThread)
            }

            Call Parallel.For(0, features.Length, options,
                Sub(i As Integer)
                    Dim row = features(i)
                    Dim sum As Double = 0

                    For t As Integer = 0 To trees.Length - 1
                        sum += trees(t).Predict(row)
                    Next

                    output(i) += scale * sum
                End Sub)
        End Sub
    End Class
End Namespace
