#Region "Microsoft.VisualBasic::fa6355027c23fa06df287992dba2cb20, Data_science\MachineLearning\xgboost\TGBoost\GBM.vb"

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

    '   Total Lines: 343
    '    Code Lines: 202
    ' Comment Lines: 97
    '   Blank Lines: 44
    '     File Size: 15.54 KB


    '     Class GBM
    ' 
    '         Properties: eta, first_round_pred, loss, trees
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: predict, TGBoostIteration
    ' 
    '         Sub: fit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.My

Namespace train

    ''' <summary>
    ''' Tiny implement of Gradient Boosting tree
    ''' 
    ''' It is a Tiny implement of Gradient Boosting tree, based on 
    ''' XGBoost's scoring function and SLIQ's efficient tree building 
    ''' algorithm. TGBoost build the tree in a level-wise way as in 
    ''' SLIQ (by constructing Attribute list and Class list). 
    ''' Currently, TGBoost support parallel learning on single machine, 
    ''' the speed and memory consumption are comparable to XGBoost.
    '''
    ''' TGBoost supports most features As other library:
    '''
    ''' + Built-in loss , Square error loss for regression task, Logistic loss for classification task
    ''' + Early stopping, evaluate On validation Set And conduct early stopping
    ''' + Feature importance, output the feature importance after training
    ''' + Regularization , lambda, gamma
    ''' + Randomness, subsample，colsample
    ''' + Weighted loss Function , assign weight To Each sample
    '''
    ''' Another two features are novel:
    '''
    ''' + Handle missing value, XGBoost learn a direction For those 
    '''   With missing value, the direction Is left Or right. TGBoost 
    '''   take a different approach: it enumerate missing value go To 
    '''   left child, right child And missing value child, Then 
    '''   choose the best one. So TGBoost use Ternary Tree.
    ''' + Handle categorical feature, TGBoost order the categorical 
    '''   feature by their statistic (Gradient_sum / Hessian_sum) On 
    '''   Each tree node, Then conduct split finding As numeric 
    '''   feature.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/wepe/tgboost
    ''' </remarks>
    Public Class GBM : Inherits Model

        Private num_boost_round As Integer
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

        Shared ReadOnly logger As LogFile

        Public Overridable ReadOnly Property first_round_pred As Double
        Public Overridable ReadOnly Property eta As Double
        Public Overridable ReadOnly Property loss As Loss
        Public Overridable ReadOnly Property trees As New List(Of Tree)

        Shared Sub New()
            logger = FrameworkInternal.getLogger("XGBoostInfoLogging", split:=Sub(header, msg, level) VBDebugger.cat($"{header}({level}): {msg}", vbCrLf))

            ' save log file on application exit
            Call App.AddExitCleanHook(AddressOf logger.Save)
        End Sub

        Public Sub New()
        End Sub

        ''' <summary>
        ''' load model from file
        ''' </summary>
        ''' <param name="trees"></param>
        ''' <param name="loss"></param>
        ''' <param name="first_round_pred"></param>
        ''' <param name="eta"></param>
        Public Sub New(trees As List(Of Tree), loss As Loss, first_round_pred As Double, eta As Double)
            _trees = trees
            _loss = loss
            _first_round_pred = first_round_pred
            _eta = eta
        End Sub

        ''' <summary>
        ''' do model training
        ''' </summary>
        ''' <param name="trainset"></param>
        ''' <param name="valset"></param>
        ''' <param name="early_stopping_rounds"></param>
        ''' <param name="maximize"></param>
        ''' <param name="eval_metric">
        ''' <see cref="Metrics.mse"/> for regression problem
        ''' </param>
        ''' <param name="loss">
        ''' + logloss: <see cref="LogisticLoss"/> for classify problem
        ''' + squareloss: <see cref="SquareLoss"/> for regression problem
        ''' </param>
        ''' <param name="eta">
        ''' [learning_rate] Step size shrinkage used in update to prevents overfitting. 
        ''' After each boosting step, we can directly get the weights 
        ''' of new features, and eta shrinks the feature weights to make 
        ''' the boosting process more conservative. range: [0,1]
        ''' </param>
        ''' <param name="num_boost_round"></param>
        ''' <param name="max_depth">
        ''' Maximum depth of a tree. Increasing this value will make the model more
        ''' complex and more likely to overfit. 0 indicates no limit on depth. Beware
        ''' that XGBoost aggressively consumes memory when training a deep tree.
        ''' exact tree method requires non-zero value. range: [0,∞]
        ''' </param>
        ''' <param name="scale_pos_weight"></param>
        ''' <param name="rowsample"></param>
        ''' <param name="colsample"></param>
        ''' <param name="min_child_weight">
        ''' Minimum sum of instance weight (hessian) needed in a child. If the tree 
        ''' partition step results in a leaf node with the sum of instance weight less
        ''' than min_child_weight, then the building process will give up further 
        ''' partitioning. In linear regression task, this simply corresponds to 
        ''' minimum number of instances needed to be in each node. The larger min_child_weight
        ''' is, the more conservative the algorithm will be. range: [0,∞]
        ''' </param>
        ''' <param name="min_sample_split"></param>
        ''' <param name="lambda">
        ''' [reg_lambda] L2 regularization term on weights. Increasing this value
        ''' will make model more conservative.
        ''' </param>
        ''' <param name="gamma">
        ''' [min_split_loss] Minimum loss reduction required to make a further partition
        ''' on a leaf node of the tree. The larger gamma is, the more conservative the 
        ''' algorithm will be. range: [0,∞]
        ''' </param>
        ''' <param name="num_thread"></param>
        Public Overridable Sub fit(trainset As TrainData, valset As ValidationData,
                                   Optional early_stopping_rounds As Integer = 10,
                                   Optional maximize As Boolean = True,
                                   Optional eval_metric As Metrics = Metrics.auc,
                                   Optional loss As String = "logloss",
                                   Optional eta As Double = 0.3,
                                   Optional num_boost_round As Integer = 20,
                                   Optional max_depth As Integer = 7,
                                   Optional scale_pos_weight As Double = 1,
                                   Optional rowsample As Double = 0.8,
                                   Optional colsample As Double = 0.8,
                                   Optional min_child_weight As Double = 1,
                                   Optional min_sample_split As Integer = 5,
                                   Optional lambda As Double = 1,
                                   Optional gamma As Double = 0,
                                   Optional num_thread As Integer = -1)

            Me.num_boost_round = num_boost_round
            Me.max_depth = max_depth
            Me.rowsample = rowsample
            Me.colsample = colsample
            Me.lambda = lambda
            Me.gamma = gamma
            Me.min_sample_split = min_sample_split
            Me.num_thread = num_thread
            Me.eval_metric = eval_metric.Description
            Me.min_child_weight = min_child_weight
            Me.scale_pos_weight = scale_pos_weight

            _eta = eta

            Dim attribute_list As New AttributeList(trainset)
            Dim class_list As New ClassList(trainset)
            Dim row_sampler As New RowSampler(trainset.dataset_size, Me.rowsample)
            Dim col_sampler As New ColumnSampler(trainset.feature_dim, Me.colsample)
            Dim calculate_metric As IMetric = Metric.GetMetric(eval_metric)

            If loss.Equals("logloss") Then
                _loss = New LogisticLoss()
                _first_round_pred = 0.0
            ElseIf loss.Equals("squareloss") OrElse loss = "qlinearloss" Then
                _loss = If(loss = "squareloss", New SquareLoss(), New QLinearLoss())
                _first_round_pred = class_list.label.Average

                If eval_metric = Metrics.mse OrElse eval_metric = Metrics.mae Then
                    GBM.logger.info("Going to solve a regression model!")
                End If
            End If

            class_list.initialize_pred(_first_round_pred)
            class_list.update_grad_hess(_loss, Me.scale_pos_weight)

            'to evaluate on validation set and conduct early stopping
            Dim do_validation As Boolean
            Dim val_pred As Double()

            If valset Is Nothing Then
                do_validation = False
                valset = Nothing
                val_pred = Nothing
            Else
                do_validation = True
                val_pred = New Double(valset.dataset_size - 1) {}
                Arrays.fill(val_pred, _first_round_pred)
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

            For i As Integer = 0 To num_boost_round - 1
                If Not TGBoostIteration(
                    i,
                    attribute_list,
                    class_list,
                    row_sampler,
                    col_sampler,
                    calculate_metric,
                    do_validation,
                    valset,
                    val_pred,
                    maximize,
                    best_val_metric,
                    best_round,
                    become_worse_round,
                    early_stopping_rounds,
                    eval_metric
                ) Then

                    Exit For
                End If
            Next
        End Sub

        Private Function TGBoostIteration(i As Integer,
                                          ByRef attribute_list As AttributeList,
                                          ByRef class_list As ClassList,
                                          ByRef row_sampler As RowSampler,
                                          ByRef col_sampler As ColumnSampler,
                                          ByRef calculate_metric As IMetric,
                                          ByRef do_validation As Boolean,
                                          ByRef valset As ValidationData,
                                          ByRef val_pred As Double(),
                                          ByRef maximize As Boolean,
                                          ByRef best_val_metric As Double,
                                          ByRef best_round As Integer,
                                          ByRef become_worse_round As Integer,
                                          ByRef early_stopping_rounds As Integer,
                                          ByRef eval_metric As Metrics) As Boolean

            Dim tree As New Tree(min_sample_split, min_child_weight, max_depth, colsample, rowsample, lambda, gamma, num_thread, attribute_list.cat_features_cols)

            tree.fit(attribute_list, class_list, row_sampler, col_sampler)
            'when finish building this tree, update the class_list.pred, grad, hess
            class_list.update_pred(_eta)
            class_list.update_grad_hess(_loss, Me.scale_pos_weight)

            'save this tree
            trees.Add(tree)
            GBM.logger.log(MSG_TYPES.INF, String.Format("current tree has {0:D} nodes,including {1:D} nan tree nodes", tree.nodes_cnt, tree.nan_nodes_cnt))

            'print training information
            If eval_metric = Metrics.none Then
                GBM.logger.log(MSG_TYPES.FINEST, String.Format("TGBoost round {0:D}", i))
                Return True
            End If

            Dim train_metric = calculate_metric(_loss.transform(class_list.pred), class_list.label)

            If Not do_validation Then
                GBM.logger.log(MSG_TYPES.INF, String.Format("TGBoost round {0:D},train-{1}:{2:F6}", i, eval_metric, train_metric))
                Return True
            End If

            Dim cur_tree_pred As Double() = tree.predict(valset.origin_feature)

            For n = 0 To val_pred.Length - 1
                val_pred(n) += _eta * cur_tree_pred(n)
            Next

            Dim val_metric = calculate_metric(_loss.transform(val_pred), valset.label)

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
                    Return False
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
                    Return False
                End If
            End If

            Return True
        End Function

        Public Overridable Function predict(features As Single()()) As Double()
            Dim pred = New Double(features.Length - 1) {}

            GBM.logger.info($"feature set: {features.GetHashCode}")
            GBM.logger.info("TGBoost start predicting...")

            For i = 0 To pred.Length - 1
                pred(i) += _first_round_pred
            Next

            For Each tree As Tree In _trees
                Dim cur_tree_pred As Double() = tree.predict(features)

                For i = 0 To pred.Length - 1
                    pred(i) += _eta * cur_tree_pred(i)
                Next
            Next

            Return _loss.transform(pred)
        End Function
    End Class
End Namespace
