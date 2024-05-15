#Region "Microsoft.VisualBasic::c8809eaa65649884863b5c3583a8d7bb, Data_science\MachineLearning\xgboost\TGBoost\Tree.vb"

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

    '   Total Lines: 475
    '    Code Lines: 344
    ' Comment Lines: 48
    '   Blank Lines: 83
    '     File Size: 22.26 KB


    '     Class Tree
    ' 
    '         Properties: root
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: calculate_leaf_score, calculate_split_gain, predict
    ' 
    '         Sub: build, clean_up, fit
    '         Class ProcessEachNumericFeature
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: run
    ' 
    '         Class ProcessEachCategoricalFeature
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: run
    '             Class ComparatorAnonymousInnerClass
    ' 
    '                 Function: compare
    ' 
    ' 
    ' 
    '         Class PredictCallable
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: [call]
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Threads
Imports stdNum = System.Math

Namespace train

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
        Private alive_nodes As New List(Of TreeNode)()
        'number of tree node of this tree
        Public nodes_cnt As Integer = 0
        'number of nan tree node of this tree
        Public nan_nodes_cnt As Integer = 0

        Public Overridable ReadOnly Property root As TreeNode

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
                       cat_features_cols As IEnumerable(Of Integer))

            Me.min_sample_split = min_sample_split
            Me.min_child_weight = min_child_weight
            Me.max_depth = max_depth
            Me.colsample = colsample
            Me.rowsample = rowsample
            Me.lambda = lambda
            Me.gamma = gamma
            Me.cat_features_cols = cat_features_cols.AsList

            If num_thread = -1 Then
                Me.num_thread = App.CPUCoreNumbers
            Else
                Me.num_thread = num_thread
            End If

            'to avoid divide zero
            Me.lambda = stdNum.Max(Me.lambda, 0.00001)
        End Sub

        Private Function calculate_leaf_score(G As Double, H As Double) As Double
            'According to xgboost, the leaf score is : - G / (H+lambda)
            Return -G / (H + lambda)
        End Function

        Private Function calculate_split_gain(G_left As Double, H_left As Double, G_nan As Double, H_nan As Double, G_total As Double, H_total As Double) As Double()
            'According to xgboost, the scoring function is:
            '     gain = 0.5 * (GL^2/(HL+lambda) + GR^2/(HR+lambda) - (GL+GR)^2/(HL+HR+lambda)) - gamma
            'this gain is the loss reduction, We want it to be as large as possible.
            Dim G_right = G_total - G_left - G_nan
            Dim H_right = H_total - H_left - H_nan

            'if we let those with missing value go to a nan child
            Dim gain_1 = 0.5 * (stdNum.Pow(G_left, 2) / (H_left + lambda) + stdNum.Pow(G_right, 2) / (H_right + lambda) + stdNum.Pow(G_nan, 2) / (H_nan + lambda) - stdNum.Pow(G_total, 2) / (H_total + lambda)) - gamma

            'uncomment this line, then we use xgboost's method to deal with missing value
            'gain_1 = -Double.MAX_VALUE;

            'if we let those with missing value go to left child
            Dim gain_2 = 0.5 * (stdNum.Pow(G_left + G_nan, 2) / (H_left + H_nan + lambda) + stdNum.Pow(G_right, 2) / (H_right + lambda) - stdNum.Pow(G_total, 2) / (H_total + lambda)) - gamma

            'if we let those with missing value go to right child
            Dim gain_3 = 0.5 * (stdNum.Pow(G_left, 2) / (H_left + lambda) + stdNum.Pow(G_right + G_nan, 2) / (H_right + H_nan + lambda) - stdNum.Pow(G_total, 2) / (H_total + lambda)) - gamma
            Dim nan_go_to As Double
            Dim gain = stdNum.Max(gain_1, stdNum.Max(gain_2, gain_3))

            If gain_1 = gain Then
                nan_go_to = 0 'nan child
            ElseIf gain_2 = gain Then
                nan_go_to = 1 'left child
            Else
                nan_go_to = 2 'right child
            End If

            'in this case, the trainset does not contains nan samples
            If H_nan = 0 AndAlso G_nan = 0 Then
                nan_go_to = 3
            End If

            Return New Double() {nan_go_to, gain}
        End Function

        Public Overridable Sub fit(attribute_list As AttributeList, class_list As ClassList, row_sampler As RowSampler, col_sampler As ColumnSampler)
            'when we start to fit a tree, we first conduct row and column sampling
            col_sampler.shuffle()
            row_sampler.shuffle()
            class_list.sampling(row_sampler.row_mask)

            'then we create the root node, initialize histogram(Gradient sum and Hessian sum)
            Dim root_node As New TreeNode(1, 1, attribute_list.feature_dim, False)

            root_node.Grad_setter(class_list.grad.Sum)
            root_node.Hess_setter(class_list.hess.Sum)
            _root = root_node

            'put it into the alive_node, and fill the class_list, all data are assigned to root node initially
            alive_nodes.Add(root_node)

            For i As Integer = 0 To class_list.dataset_size - 1
                class_list.corresponding_tree_node(i) = root_node
            Next

            'update Grad_missing Hess_missing for root node
            class_list.update_grad_hess_missing_for_tree_node(attribute_list.missing_value_attribute_list)

            'then build the tree util there is no alive tree_node to split
            Me.build(attribute_list, class_list, col_sampler)
            clean_up()
        End Sub

        Friend Class ProcessEachNumericFeature
            Inherits ThreadStart

            Private ReadOnly outerInstance As Tree
            Public col As Integer
            Public attribute_list As AttributeList
            Public class_list As ClassList

            Public Sub New(outerInstance As Tree, col As Integer, attribute_list As AttributeList, class_list As ClassList)
                Me.outerInstance = outerInstance
                Me.col = col
                Me.attribute_list = attribute_list
                Me.class_list = class_list
            End Sub

            Public Overrides Sub run()
                For interval As Integer = 0 To attribute_list.cutting_inds(col).Length - 1 - 1
                    'update the corresponding treenode's G_left,H_left with this inds's sample
                    Dim inds As Integer() = attribute_list.cutting_inds(col)(interval)
                    Dim nodes As HashSet(Of TreeNode) = New HashSet(Of TreeNode)()

                    For Each ind In inds
                        Dim treenode As TreeNode = class_list.corresponding_tree_node(ind)

                        If treenode.is_leaf Then
                            Continue For
                        End If

                        nodes.Add(treenode)
                        treenode.G_left(col) += class_list.grad(ind)
                        treenode.H_left(col) += class_list.hess(ind)
                    Next
                    'update each treenode's best split using this feature
                    For Each node As TreeNode In nodes
                        Dim G_left As Double = node.G_left(col)
                        Dim H_left As Double = node.H_left(col)
                        Dim G_total As Double = node.Grad
                        Dim H_total As Double = node.Hess
                        Dim G_nan As Double = node.Grad_missing(col)
                        Dim H_nan As Double = node.Hess_missing(col)
                        Dim ret As Double() = outerInstance.calculate_split_gain(G_left, H_left, G_nan, H_nan, G_total, H_total)
                        Dim nan_go_to = ret(0)
                        Dim gain = ret(1)
                        node.update_best_split(col, attribute_list.cutting_thresholds(col)(interval), gain, nan_go_to)
                    Next
                Next
            End Sub
        End Class

        Friend Class ProcessEachCategoricalFeature
            Inherits ThreadStart

            Private ReadOnly outerInstance As Tree
            Public col As Integer
            Public attribute_list As AttributeList
            Public class_list As ClassList

            Public Sub New(outerInstance As Tree, col As Integer, attribute_list As AttributeList, class_list As ClassList)
                Me.outerInstance = outerInstance
                Me.col = col
                Me.attribute_list = attribute_list
                Me.class_list = class_list
            End Sub

            Public Overrides Sub run()
                Dim nodes As New HashSet(Of TreeNode)()
                Dim colkey As String = col.ToString

                For interval As Integer = 0 To attribute_list.cutting_inds(CInt(col)).Length - 1
                    'update the corresponding treenode's cat_feature_col_value_GH
                    Dim inds As Integer() = attribute_list.cutting_inds(col)(interval)
                    Dim cat_value As String = CStr(attribute_list.cutting_thresholds(col)(interval))

                    For Each ind As Integer In inds
                        Dim treeNode As TreeNode = class_list.corresponding_tree_node(ind)

                        SyncLock treeNode
                            If treeNode.is_leaf Then
                                Continue For
                            End If

                            If Not nodes.Contains(treeNode) Then
                                nodes.Add(treeNode)
                                treeNode.cat_feature_col_value_GH(key:=colkey) = New Dictionary(Of String, Double())
                            End If

                            Dim colVal = treeNode.cat_feature_col_value_GH.ComputeIfAbsent(colkey, Function() New Dictionary(Of String, Double()))

                            If colVal.ContainsKey(cat_value) Then
                                colVal(key:=cat_value)(0) += class_list.grad(ind)
                                colVal(key:=cat_value)(1) += class_list.hess(ind)
                            Else
                                colVal.put(cat_value, New Double() {class_list.grad(ind), class_list.hess(ind)})
                            End If
                        End SyncLock
                    Next
                Next

                For Each node As TreeNode In nodes
                    Dim node_GH = node.cat_feature_col_value_GH.GetValueOrNull(colkey)

                    If node_GH Is Nothing Then
                        node_GH = New Dictionary(Of String, Double())
                    End If

                    Dim catvalue_GdivH As Double()() = RectangularArray.Matrix(Of Double)(node_GH.Count, 4)
                    Dim i = 0
                    Dim catkey As String

                    For Each catvalue As String In node_GH.Keys
                        catkey = catvalue.ToString
                        catvalue_GdivH(i)(0) = catvalue
                        catvalue_GdivH(i)(1) = node_GH(catkey)(0)
                        catvalue_GdivH(i)(2) = node_GH(catkey)(1)
                        catvalue_GdivH(i)(3) = catvalue_GdivH(i)(1) / catvalue_GdivH(i)(2)
                        i += 1
                    Next

                    Call Array.Sort(catvalue_GdivH, New ComparatorAnonymousInnerClass)

                    Dim G_total As Double = node.Grad
                    Dim H_total As Double = node.Hess
                    Dim G_nan As Double = node.Grad_missing(col)
                    Dim H_nan As Double = node.Hess_missing(col)
                    Dim G_left As Double = 0
                    Dim H_left As Double = 0
                    Dim best_split As Integer = -1
                    Dim best_gain = -Double.MaxValue
                    Dim best_nan_go_to As Double = -1

                    For i = 0 To catvalue_GdivH.Length - 1
                        G_left += catvalue_GdivH(i)(1)
                        H_left += catvalue_GdivH(i)(2)
                        Dim ret As Double() = outerInstance.calculate_split_gain(G_left, H_left, G_nan, H_nan, G_total, H_total)
                        Dim nan_go_to = ret(0)
                        Dim gain = ret(1)

                        If gain > best_gain Then
                            best_gain = gain
                            best_split = i
                            best_nan_go_to = nan_go_to
                        End If
                    Next

                    Dim left_child_catvalue As New List(Of Integer)()

                    For i = 0 To best_split
                        left_child_catvalue.Add(CInt(catvalue_GdivH(i)(0)))
                    Next

                    node.set_categorical_feature_best_split(col, left_child_catvalue, best_gain, best_nan_go_to)
                Next
            End Sub

            Private Class ComparatorAnonymousInnerClass : Implements IComparer(Of Double())

                Public Function compare(a As Double(), b As Double()) As Integer Implements IComparer(Of Double()).Compare
                    Return a(3).CompareTo(b(3))
                End Function
            End Class
        End Class

        Private Sub build(attribute_list As AttributeList, class_list As ClassList, col_sampler As ColumnSampler)
            While alive_nodes.Count > 0
                nodes_cnt += alive_nodes.Count

                'parallelly scan and process each selected attribute list
                Dim pool As New List(Of ThreadStart)

                For Each col As Integer In col_sampler.col_selected
                    If attribute_list.cat_features_cols.Contains(col) Then
                        pool.Add(New Tree.ProcessEachCategoricalFeature(Me, col, attribute_list, class_list))
                    Else
                        pool.Add(New Tree.ProcessEachNumericFeature(Me, col, attribute_list, class_list))
                    End If
                Next

                Call ThreadStart.execute(pool)

                'once had scan all column, we can get the best (feature,threshold,gain) for each alive tree node
                Dim cur_level_node_size = alive_nodes.Count
                Dim new_tree_nodes As New List(Of TreeNode)()

                'time consumption: 0.0x ms
                For i = 0 To cur_level_node_size - 1
                    'pop each alive treenode
                    Dim treenode As TreeNode = alive_nodes.Poll

                    'consider categorical feature
                    Dim ret As List(Of Double) = treenode.get_best_feature_threshold_gain()
                    Dim best_feature = ret(0)
                    Dim best_gain = ret(1)
                    Dim best_nan_go_to = ret(2)
                    Dim best_threshold As Double = 0
                    Dim left_child_catvalue As New List(Of Double)()

                    If cat_features_cols.Contains(CInt(best_feature)) Then
                        For j = 3 To ret.Count - 1
                            left_child_catvalue.Add(ret(j))
                        Next
                    Else
                        best_threshold = ret(3)
                    End If

                    If best_gain <= 0 Then
                        'this node is leaf node
                        Dim leaf_score = Me.calculate_leaf_score(treenode.Grad, treenode.Hess)
                        treenode.leaf_node_setter(leaf_score, True)
                    Else
                        'this node is internal node
                        Dim left_child As TreeNode = New TreeNode(3 * treenode.index - 1, treenode.depth + 1, treenode.feature_dim, False)
                        Dim right_child As TreeNode = New TreeNode(3 * treenode.index + 1, treenode.depth + 1, treenode.feature_dim, False)
                        Dim nan_child As TreeNode = Nothing

                        If best_nan_go_to = 0 Then
                            'this case we create the nan child
                            nan_child = New TreeNode(3 * treenode.index, treenode.depth + 1, treenode.feature_dim, False)
                            nan_nodes_cnt += 1
                        End If
                        'consider categorical feature
                        If cat_features_cols.Contains(CInt(best_feature)) Then
                            treenode.internal_node_setter(best_feature, left_child_catvalue, best_nan_go_to, nan_child, left_child, right_child, False)
                        Else
                            treenode.internal_node_setter(best_feature, best_threshold, best_nan_go_to, nan_child, left_child, right_child, False)
                        End If

                        new_tree_nodes.Add(left_child)
                        new_tree_nodes.Add(right_child)

                        If nan_child IsNot Nothing Then
                            new_tree_nodes.Add(nan_child)
                        End If
                    End If
                Next

                'update class_list.corresponding_tree_node
                class_list.update_corresponding_tree_node(attribute_list)

                'update (Grad,Hess,num_sample) for each new tree node
                class_list.update_Grad_Hess_numsample_for_tree_node()

                'update Grad_missing, Hess_missing for each new tree node
                'time consumption: 5ms
                class_list.update_grad_hess_missing_for_tree_node(attribute_list.missing_value_attribute_list)

                'process the new tree nodes
                'satisfy max_depth? min_child_weight? min_sample_split?
                'if yes, it is leaf node, calculate its leaf score
                'if no, put into self.alive_node
                While new_tree_nodes.Count <> 0
                    Dim treenode As TreeNode = new_tree_nodes.Poll

                    If treenode.depth >= max_depth OrElse treenode.Hess < min_child_weight OrElse treenode.num_sample <= min_sample_split Then
                        treenode.leaf_node_setter(Me.calculate_leaf_score(treenode.Grad, treenode.Hess), True)
                    Else
                        alive_nodes.Add(treenode)
                    End If
                End While
            End While
        End Sub

        Friend Class PredictCallable
            Inherits Callable(Of Double)

            Private ReadOnly outerInstance As Tree
            Friend feature As Single()

            Public Sub New(outerInstance As Tree, feature As Single())
                Me.outerInstance = outerInstance
                Me.feature = feature
            End Sub

            Public Overrides Function [call]() As Double
                Dim cur_tree_node As TreeNode = outerInstance.root

                While Not cur_tree_node.is_leaf

                    If feature(cur_tree_node.split_feature) = Data.NA Then
                        'it is missing value
                        If cur_tree_node.nan_go_to = 0 Then
                            cur_tree_node = cur_tree_node.nan_child
                        ElseIf cur_tree_node.nan_go_to = 1 Then
                            cur_tree_node = cur_tree_node.left_child
                        ElseIf cur_tree_node.nan_go_to = 2 Then
                            cur_tree_node = cur_tree_node.right_child
                        Else
                            'trainset has not missing value for this feature,
                            ' so we should decide which branch the testset's missing value go to
                            If cur_tree_node.left_child.num_sample > cur_tree_node.right_child.num_sample Then
                                cur_tree_node = cur_tree_node.left_child
                            Else
                                cur_tree_node = cur_tree_node.right_child
                            End If
                        End If
                    Else
                        'not missing value
                        ' consider split_feature categorical or numeric
                        If cur_tree_node.split_left_child_catvalue IsNot Nothing Then
                            If cur_tree_node.split_left_child_catvalue.Contains(feature(cur_tree_node.split_feature)) Then
                                cur_tree_node = cur_tree_node.left_child
                            Else
                                cur_tree_node = cur_tree_node.right_child
                            End If
                        Else

                            If feature(cur_tree_node.split_feature) <= cur_tree_node.split_threshold Then
                                cur_tree_node = cur_tree_node.left_child
                            Else
                                cur_tree_node = cur_tree_node.right_child
                            End If
                        End If
                    End If
                End While

                Return cur_tree_node.leaf_score
            End Function
        End Class

        ''' <summary>
        ''' 这个并行函数返回的数据与原始数据的顺序保持一致
        ''' </summary>
        ''' <param name="features"></param>
        ''' <returns></returns>
        Public Overridable Function predict(features As Single()()) As Double()
            Dim ret As Double() = features _
                .SeqIterator() _
                .AsParallel _
                .Select(Function(i)
                            Return (idx:=i.i, New Tree.PredictCallable(Me, i.value).call)
                        End Function) _
                .OrderBy(Function(i) i.idx) _
                .Select(Function(i) i.Item2) _
                .ToArray

            Return ret
        End Function

        Private Sub clean_up()
            alive_nodes = Nothing
        End Sub
    End Class
End Namespace
