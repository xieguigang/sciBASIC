Imports Microsoft.VisualBasic.Language.Java

Namespace train
    Public Class ClassList
        Public dataset_size As Integer
        Public label As Double()
        Public corresponding_tree_node As TreeNode()
        Public pred As Double()
        Public grad As Double()
        Public hess As Double()

        Public Sub New(data As TrainData)
            dataset_size = data.dataset_size
            label = data.label
            pred = New Double(dataset_size - 1) {}
            grad = New Double(dataset_size - 1) {}
            hess = New Double(dataset_size - 1) {}
            corresponding_tree_node = New TreeNode(dataset_size - 1) {}
        End Sub

        Public Overridable Sub initialize_pred(first_round_pred As Double)
            Arrays.fill(pred, first_round_pred)
        End Sub

        Public Overridable Sub update_pred(eta As Double)
            For i = 0 To dataset_size - 1
                pred(i) += eta * corresponding_tree_node(i).leaf_score
            Next
        End Sub

        Public Overridable Sub update_grad_hess(loss As Loss, scale_pos_weight As Double)
            grad = loss.grad(pred, label)
            hess = loss.hess(pred, label)

            If scale_pos_weight <> 1.0 Then
                For i = 0 To dataset_size - 1

                    If label(i) = 1 Then
                        grad(i) *= scale_pos_weight
                        hess(i) *= scale_pos_weight
                    End If
                Next
            End If
        End Sub

        Public Overridable Sub sampling(row_mask As List(Of Double?))
            For i = 0 To dataset_size - 1
                grad(i) *= row_mask(i).Value
                hess(i) *= row_mask(i).Value
            Next
        End Sub

        'TODO
        'parallel each col's calculation
        Public Overridable Sub update_grad_hess_missing_for_tree_node(missing_value_attribute_list As Integer()())
            For col = 0 To missing_value_attribute_list.Length - 1

                For Each i In missing_value_attribute_list(col)
                    Dim treenode As TreeNode = corresponding_tree_node(i)

                    If Not treenode.is_leaf Then
                        treenode.Grad_missing(col) += grad(i)
                        treenode.Hess_missing(col) += hess(i)
                    End If
                Next
            Next
        End Sub

        Public Overridable Sub update_Grad_Hess_numsample_for_tree_node()
            For i = 0 To dataset_size - 1
                Dim treenode As TreeNode = corresponding_tree_node(i)

                If Not treenode.is_leaf Then
                    treenode.Grad_add(grad(i))
                    treenode.Hess_add(hess(i))
                    treenode.num_sample_add(1)
                End If
            Next
        End Sub

        Public Overridable Sub update_corresponding_tree_node(attribute_list As AttributeList)
            For i = 0 To dataset_size - 1
                Dim treenode As TreeNode = corresponding_tree_node(i)

                If Not treenode.is_leaf Then
                    Dim split_feature As Integer = treenode.split_feature
                    Dim nan_go_to As Double = treenode.nan_go_to
                    Dim val As Double = attribute_list.origin_feature(i)(split_feature)
                    'consider categorical feature
                    If attribute_list.cat_features_cols.Contains(split_feature) Then
                        Dim left_child_catvalue As List(Of Double?) = treenode.split_left_child_catvalue

                        If val = Data.NULL Then
                            If nan_go_to = 0 Then
                                corresponding_tree_node(i) = treenode.nan_child
                            ElseIf nan_go_to = 1 Then
                                corresponding_tree_node(i) = treenode.left_child
                            Else
                                corresponding_tree_node(i) = treenode.right_child
                            End If
                        ElseIf left_child_catvalue.Contains(val) Then
                            corresponding_tree_node(i) = treenode.left_child
                        Else
                            corresponding_tree_node(i) = treenode.right_child
                        End If
                    Else
                        Dim split_threshold As Double = treenode.split_threshold

                        If val = Data.NULL Then
                            If nan_go_to = 0 Then
                                corresponding_tree_node(i) = treenode.nan_child
                            ElseIf nan_go_to = 1 Then
                                corresponding_tree_node(i) = treenode.left_child
                            Else
                                corresponding_tree_node(i) = treenode.right_child
                            End If
                        ElseIf val <= split_threshold Then
                            corresponding_tree_node(i) = treenode.left_child
                        Else
                            corresponding_tree_node(i) = treenode.right_child
                        End If
                    End If
                End If
            Next
        End Sub
    End Class
End Namespace
