Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My
Imports train.kernels
Imports std = System.Math

Namespace train

    ''' <summary>
    ''' 单棵梯度提升树。
    '''
    ''' 建树逻辑已迁移到 <see cref="TreeBuilder"/>（exact / hist 双路径 + 行分区），
    ''' 本类保留公共契约：<see cref="root"/>、<see cref="predict(Single()())"/>、
    ''' <see cref="nodes_cnt"/> / <see cref="nan_nodes_cnt"/>，以及
    ''' <see cref="fit(AttributeList, ClassList, RowSampler, ColumnSampler)"/>。
    ''' </summary>
    Public Class Tree

        Private min_sample_split As Integer
        Private min_child_weight As Double
        Private max_depth As Integer
        Private colsample As Double
        Private rowsample As Double
        Private lambda As Double
        Private gamma As Double
        Private num_thread As Integer
        Private cat_features_cols As List(Of Integer)
        Private tree_method As TreeMethod
        Private max_bin As Integer

        'number of tree node of this tree
        Public nodes_cnt As Integer = 0
        'number of nan tree node of this tree
        Public nan_nodes_cnt As Integer = 0

        ''' <summary>
        ''' 编译后的扁平整树，用于批量预测（惰性构建）
        ''' </summary>
        Private flatTree As FlatTree

        Public Overridable ReadOnly Property root As TreeNode

        ''' <summary>
        ''' 用于预测的低阶表示；首次访问时从 <see cref="root"/> 编译
        ''' </summary>
        Public ReadOnly Property Flat As FlatTree
            Get
                If flatTree Is Nothing Then
                    SyncLock Me
                        If flatTree Is Nothing AndAlso root IsNot Nothing Then
                            flatTree = FlatTree.Compile(root)
                        End If
                    End SyncLock
                End If

                Return flatTree
            End Get
        End Property

        ''' <summary>
        ''' 本树建树时使用的分裂查找算法
        ''' </summary>
        Public ReadOnly Property method As TreeMethod
            Get
                Return tree_method
            End Get
        End Property

        Public Sub New(root As TreeNode)
            _root = root
            num_thread = App.CPUCoreNumbers
        End Sub

        Public Sub New(min_sample_split As Integer,
                       min_child_weight As Double,
                       max_depth As Integer,
                       colsample As Double,
                       rowsample As Double,
                       lambda As Double,
                       gamma As Double,
                       num_thread As Integer,
                       cat_features_cols As IEnumerable(Of Integer),
                       Optional tree_method As TreeMethod = TreeMethod.Exact,
                       Optional max_bin As Integer = 256)

            Me.min_sample_split = min_sample_split
            Me.min_child_weight = min_child_weight
            Me.max_depth = max_depth
            Me.colsample = colsample
            Me.rowsample = rowsample
            Me.lambda = lambda
            Me.gamma = gamma
            Me.cat_features_cols = cat_features_cols.AsList
            Me.tree_method = tree_method
            Me.max_bin = max_bin

            If num_thread = -1 Then
                Me.num_thread = App.CPUCoreNumbers
            Else
                Me.num_thread = num_thread
            End If

            'to avoid divide zero
            Me.lambda = std.Max(Me.lambda, 0.00001)
        End Sub

        ''' <summary>
        ''' 训练一棵树
        ''' </summary>
        ''' <param name="attribute_list">列块（跨树共享，只读）</param>
        ''' <param name="class_list">梯度/海森与标签</param>
        ''' <param name="row_sampler">行采样器；每棵树重新抽样一次</param>
        ''' <param name="col_sampler">列采样器；每棵树重新抽样一次</param>
        Public Overridable Sub fit(attribute_list As AttributeList, class_list As ClassList,
                                   row_sampler As RowSampler, col_sampler As ColumnSampler)

            Dim mask As Boolean() = If(row_sampler Is Nothing, Nothing, row_sampler.Resample())
            Dim colSelected As List(Of Integer) = Nothing

            If col_sampler IsNot Nothing Then
                Call col_sampler.shuffle()
                colSelected = col_sampler.SelectedColumns()
            End If

            Dim builder As New TreeBuilder(attribute_list.Columns, lambda, gamma, max_depth,
                                           min_child_weight, min_sample_split,
                                           tree_method, max_bin, num_thread)
            Dim result As BuildTree = builder.Build(class_list.grad, class_list.hess, mask,
                                                    colSelected, class_list.dataset_size)

            _root = result.Root
            nodes_cnt = result.NodeCount
            nan_nodes_cnt = result.NanNodeCount
            flatTree = Nothing

            ' 回填「行 -> 叶节点」：ClassList.update_pred 依赖该映射累加叶子输出
            Dim partitioner = result.Partitioner

            For Each node As BuildNode In result.Nodes

                If node.IsLeaf AndAlso node.Materialized IsNot Nothing Then

                    For Each row As Integer In partitioner.RowsOf(node.Id)
                        class_list.corresponding_tree_node(row) = node.Materialized
                    Next
                End If
            Next
        End Sub

        ''' <summary>
        ''' 批量预测。返回顺序与 <paramref name="features"/> 一致。
        ''' </summary>
        ''' <param name="features"></param>
        ''' <returns></returns>
        Public Overridable Function predict(features As Single()()) As Double()
            Dim flatTree = Me.Flat

            If flatTree Is Nothing Then
                Return New Double(features.Length - 1) {}
            End If

            Return flatTree.Predict(features)
        End Function
    End Class
End Namespace
