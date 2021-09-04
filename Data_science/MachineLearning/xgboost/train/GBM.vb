Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.My

Namespace train
    Public Class GBM

        Private trees_Renamed As List(Of Tree) = New List(Of Tree)()

        Private eta_Renamed As Double
        Private num_boost_round As Integer

        Private first_round_pred_Renamed As Double

        Private loss_Renamed As Loss
        Private max_depth As Integer
        Private rowsample As Double
        Private colsample As Double
        Private lambda As Double
        Private min_sample_split As Integer
        Private gamma As Double
        Private num_thread As Integer
        Private min_child_weight As Double
        Private scale_pos_weight As Double
        Private eval_metric As String
        Private Shared logger As LogFile = FrameworkInternal.getLogger("InfoLogging")

        Public Sub New()
        End Sub

        Public Sub New(trees As List(Of Tree), loss As Loss, first_round_pred As Double, eta As Double)
            trees_Renamed = trees
            loss_Renamed = loss
            first_round_pred_Renamed = first_round_pred
            eta_Renamed = eta
        End Sub

        Public Overridable Sub fit(file_training As String, file_validation As String, categorical_features As IEnumerable(Of String),
                                   early_stopping_rounds As Integer,
                                   maximize As Boolean,
                                   eval_metric As String,
                                   loss As String,
                                   eta As Double,
                                   num_boost_round As Integer,
                                   max_depth As Integer,
                                   scale_pos_weight As Double,
                                   rowsample As Double,
                                   colsample As Double,
                                   min_child_weight As Double,
                                   min_sample_split As Integer,
                                   lambda As Double,
                                   gamma As Double,
                                   num_thread As Integer)
            eta_Renamed = eta
            Me.num_boost_round = num_boost_round
            Me.max_depth = max_depth
            Me.rowsample = rowsample
            Me.colsample = colsample
            Me.lambda = lambda
            Me.gamma = gamma
            Me.min_sample_split = min_sample_split
            Me.num_thread = num_thread
            Me.eval_metric = eval_metric
            Me.min_child_weight = min_child_weight
            Me.scale_pos_weight = scale_pos_weight
            Dim trainset As TrainData = New TrainData(file_training, categorical_features)
            Dim attribute_list As AttributeList = New AttributeList(trainset)
            Dim class_list As ClassList = New ClassList(trainset)
            Dim row_sampler As RowSampler = New RowSampler(trainset.dataset_size, Me.rowsample)
            Dim col_sampler As ColumnSampler = New ColumnSampler(trainset.feature_dim, Me.colsample)
            trainset = Nothing

            If loss.Equals("logloss") Then
                loss_Renamed = New LogisticLoss()
                first_round_pred_Renamed = 0.0
            ElseIf loss.Equals("squareloss") Then
                loss_Renamed = New SquareLoss()
                first_round_pred_Renamed = Me.average(class_list.label)
            End If

            class_list.initialize_pred(first_round_pred_Renamed)
            class_list.update_grad_hess(loss_Renamed, Me.scale_pos_weight)

            'to evaluate on validation set and conduct early stopping
            Dim do_validation As Boolean
            Dim valset As ValidationData
            Dim val_pred As Double()

            If file_validation.Equals("") Then
                do_validation = False
                valset = Nothing
                val_pred = Nothing
            Else
                do_validation = True
                valset = New ValidationData(file_validation)
                val_pred = New Double(valset.dataset_size - 1) {}
                Arrays.fill(val_pred, first_round_pred_Renamed)
            End If

            Dim best_val_metric As Double
            Dim best_round = 0
            Dim become_worse_round = 0

            If maximize Then
                best_val_metric = -Double.MaxValue
            Else
                best_val_metric = Double.MaxValue
            End If

            'Start learning
            GBM.logger.info("TGBoost start training")

            For i = 0 To num_boost_round - 1
                Dim tree As Tree = New Tree(min_sample_split, min_child_weight, max_depth, colsample, rowsample, lambda, gamma, num_thread, attribute_list.cat_features_cols)
                tree.fit(attribute_list, class_list, row_sampler, col_sampler)
                'when finish building this tree, update the class_list.pred, grad, hess
                class_list.update_pred(eta_Renamed)
                class_list.update_grad_hess(loss_Renamed, Me.scale_pos_weight)


                'save this tree
                trees_Renamed.Add(tree)
                GBM.logger.log(MSG_TYPES.INF, String.Format("current tree has {0:D} nodes,including {1:D} nan tree nodes", tree.nodes_cnt, tree.nan_nodes_cnt))

                'print training information
                If eval_metric.Equals("") Then
                    GBM.logger.log(MSG_TYPES.FINEST, String.Format("TGBoost round {0:D}", i))
                Else
                    Dim train_metric = Me.calculate_metric(eval_metric, loss_Renamed.transform(class_list.pred), class_list.label)

                    If Not do_validation Then
                        GBM.logger.log(MSG_TYPES.INF, String.Format("TGBoost round {0:D},train-{1}:{2:F6}", i, eval_metric, train_metric))
                    Else
                        Dim cur_tree_pred As Double() = tree.predict(valset.origin_feature)

                        For n = 0 To val_pred.Length - 1
                            val_pred(n) += eta_Renamed * cur_tree_pred(n)
                        Next

                        Dim val_metric = Me.calculate_metric(eval_metric, loss_Renamed.transform(val_pred), valset.label)
                        GBM.logger.log(MSG_TYPES.INF, String.Format("TGBoost round {0:D},train-{1}:{2:F6},val-{3}:{4:F6}", i, eval_metric, train_metric, eval_metric, val_metric))
                        'check whether to early stop
                        If maximize Then
                            If val_metric > best_val_metric Then
                                best_val_metric = val_metric
                                best_round = i
                                become_worse_round = 0
                            Else
                                become_worse_round += 1
                            End If

                            If become_worse_round > early_stopping_rounds Then
                                GBM.logger.log(MSG_TYPES.INF, String.Format("TGBoost training stop,best round is {0:D},best val-{1} is {2:F6}", i, eval_metric, best_val_metric))
                                Exit For
                            End If
                        Else

                            If val_metric < best_val_metric Then
                                best_val_metric = val_metric
                                best_round = i
                                become_worse_round = 0
                            Else
                                become_worse_round += 1
                            End If

                            If become_worse_round > early_stopping_rounds Then
                                GBM.logger.log(MSG_TYPES.INF, String.Format("TGBoost training stop,best round is {0:D},best val-{1} is {2:F6}", i, eval_metric, best_val_metric))
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next
        End Sub

        Public Overridable Function predict(features As Single()()) As Double()
            GBM.logger.info("TGBoost start predicting...")
            Dim pred = New Double(features.Length - 1) {}

            For i = 0 To pred.Length - 1
                pred(i) += first_round_pred_Renamed
            Next

            For Each tree As Tree In trees_Renamed
                Dim cur_tree_pred As Double() = tree.predict(features)

                For i = 0 To pred.Length - 1
                    pred(i) += eta_Renamed * cur_tree_pred(i)
                Next
            Next

            Return loss_Renamed.transform(pred)
        End Function

        Public Overridable Sub predict(file_test As String, file_output As String)
            Dim testdata As TestData = New TestData(file_test)
            Dim preds = Me.predict(testdata.origin_feature)
            Dim strs = New String(preds.Length - 1) {}

            For i = 0 To strs.Length - 1
                strs(i) = preds(i).ToString()
            Next

            Call String.Join(vbLf, strs).SaveTo(file_output)
        End Sub

        Private Function calculate_metric(eval_metric As String, pred As Double(), label As Double()) As Double
            If eval_metric.Equals("acc") Then
                Return Metric.accuracy(pred, label)
            ElseIf eval_metric.Equals("error") Then
                Return Metric.error(pred, label)
            ElseIf eval_metric.Equals("mse") Then
                Return Metric.mean_square_error(pred, label)
            ElseIf eval_metric.Equals("mae") Then
                Return Metric.mean_absolute_error(pred, label)
            ElseIf eval_metric.Equals("auc") Then
                Return Metric.auc(pred, label)
            Else
                Throw New NotImplementedException()
            End If
        End Function

        Private Function average(vals As Double()) As Double
            Dim sum = 0.0

            For Each v In vals
                sum += v
            Next

            Return sum / vals.Length
        End Function

        Public Overridable ReadOnly Property first_round_pred As Double
            Get
                Return first_round_pred_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property eta As Double
            Get
                Return eta_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property loss As Loss
            Get
                Return loss_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property trees As List(Of Tree)
            Get
                Return trees_Renamed
            End Get
        End Property
    End Class
End Namespace
