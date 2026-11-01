Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train.kernels
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
        Private tree_method As TreeMethod
        Private max_bin As Integer
        Private random_seed As Integer?
        Private best_iteration As Integer = -1
        Private best_score As Double = Double.NaN

        Shared ReadOnly logger As LogFile

        Public Overridable ReadOnly Property first_round_pred As Double
        Public Overridable ReadOnly Property eta As Double
        Public Overridable ReadOnly Property loss As Loss
        Public Overridable ReadOnly Property trees As New List(Of Tree)

        ''' <summary>
        ''' 早停选出的最优轮次（从 0 开始）；<c>-1</c> 表示未启用早停或未产生最优轮次。
        ''' </summary>
        Public Overridable ReadOnly Property bestIteration As Integer
            Get
                Return best_iteration
            End Get
        End Property

        ''' <summary>
        ''' 早停最优轮次对应的验证集指标
        ''' </summary>
        Public Overridable ReadOnly Property bestScore As Double
            Get
                Return best_score
            End Get
        End Property

        ''' <summary>
        ''' 本模型使用的分裂查找算法
        ''' </summary>
        Public Overridable ReadOnly Property treeMethod As TreeMethod
            Get
                Return tree_method
            End Get
        End Property

        Shared Sub New()
            logger = FrameworkInternal.getLogger("XGBoostInfoLogging", split:=Sub(header, msg, level) VBDebugger.WriteLine($"{header}({level}): {msg}"))

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
        ''' 按名称解析损失函数；未知名称直接抛 <see cref="ArgumentException"/>，
        ''' 而不是像旧实现那样留下 <c>Nothing</c> 让后续步骤抛 <see cref="NullReferenceException"/>。
        ''' </summary>
        ''' <param name="loss"></param>
        ''' <returns></returns>
        Public Shared Function resolveLoss(loss As String) As Loss
            Dim name As String = If(loss, "").ToLowerInvariant.Trim

            Select Case name
                Case "logloss", "logistic", "binary:logistic", "reg:logistic"
                    Return New LogisticLoss()
                Case "squareloss", "squarederror", "reg:squarederror", "reg:linear", "mse"
                    Return New SquareLoss()
                Case "qlinearloss", "pseudo-huber", "reg:absoluteerror", "mae"
                    Return New QLinearLoss()
                Case Else
                    Throw New ArgumentException(
                        $"不支持的损失函数 '{loss}'；可用取值：logloss / squareloss / qlinearloss。",
                        NameOf(loss))
            End Select
        End Function

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
        ''' complex and more likely to overfit. 0 indicates no limit on depth (capped at 30).
        ''' </param>
        ''' <param name="scale_pos_weight"></param>
        ''' <param name="rowsample">
        ''' 每轮重新抽取的样本比例（伯努利抽样）
        ''' </param>
        ''' <param name="colsample"></param>
        ''' <param name="min_child_weight">
        ''' Minimum sum of instance weight (hessian) needed in a child.
        ''' </param>
        ''' <param name="min_sample_split"></param>
        ''' <param name="lambda">
        ''' [reg_lambda] L2 regularization term on weights.
        ''' </param>
        ''' <param name="gamma">
        ''' [min_split_loss] Minimum loss reduction required to make a further partition.
        ''' </param>
        ''' <param name="num_thread">
        ''' 并行度；<c>-1</c> 取 CPU 核心数，<c>1</c> 表示串行
        ''' </param>
        ''' <param name="tree_method">
        ''' <see cref="TreeMethod.Exact"/>（默认，与旧实现数值一致）或
        ''' <see cref="TreeMethod.Hist"/>（分桶直方图，高基数特征更快）
        ''' </param>
        ''' <param name="max_bin">直方图模式下的最大分桶数</param>
        ''' <param name="random_seed">随机种子，便于复现实验</param>
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
                                  Optional num_thread As Integer = -1,
                                  Optional tree_method As TreeMethod = TreeMethod.Exact,
                                  Optional max_bin As Integer = 256,
                                  Optional random_seed As Integer? = Nothing)

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
            Me.tree_method = tree_method
            Me.max_bin = max_bin
            Me.random_seed = random_seed

            _eta = eta

            Dim attribute_list As New AttributeList(trainset)
            Dim class_list As New ClassList(trainset)
            Dim row_sampler As New RowSampler(trainset.dataset_size, Me.rowsample, random_seed)
            Dim col_sampler As New ColumnSampler(trainset.feature_dim, Me.colsample, random_seed)
            Dim calculate_metric As IMetric = Metric.GetMetric(eval_metric)

            _loss = resolveLoss(loss)
            _first_round_pred = _loss.base_score(class_list.label)

            GBM.logger.info($"TGBoost loss={_loss.Name}, base_score={_first_round_pred:F6}, tree_method={tree_method}")

            class_list.initialize_pred(_first_round_pred)
            class_list.update_grad_hess(_loss, Me.scale_pos_weight)

            'to evaluate on validation set and conduct early stopping
            Dim do_validation As Boolean
            Dim val_pred As Double()

            If valset Is Nothing Then
                do_validation = False
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
            GBM.logger.info("TGBoost start training (" & SimdOptions.Describe() & ")")

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

                    ' 回滚到最优轮次：旧实现只打日志、保留了 best_round 之后的全部树，
                    ' 导致「日志里的 best round」与「实际模型」不一致。
                    Call rollbackTo(best_round)

                    Me.best_iteration = best_round
                    Me.best_score = best_val_metric

                    Exit For
                End If

                If i = num_boost_round - 1 Then
                    Me.best_iteration = If(do_validation, best_round, num_boost_round - 1)
                    Me.best_score = best_val_metric
                End If
            Next
        End Sub

        ''' <summary>
        ''' 丢弃 <paramref name="round"/> 之后训练出来的树
        ''' </summary>
        ''' <param name="round"></param>
        Private Sub rollbackTo(round As Integer)
            Dim keep As Integer = round + 1

            If keep < 1 Then
                keep = 1
            End If

            If _trees.Count > keep Then
                GBM.logger.info($"early stopping: 回滚 {_trees.Count - keep} 棵多余的树（保留前 {keep} 棵）")
                Call _trees.RemoveRange(keep, _trees.Count - keep)
            End If
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

            Dim tree As New Tree(min_sample_split, min_child_weight, max_depth, colsample, rowsample,
                                 lambda, gamma, num_thread, attribute_list.cat_features_cols,
                                 tree_method, max_bin)

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
            Dim improved As Boolean

            If maximize Then
                improved = val_metric > best_val_metric
            Else
                improved = val_metric < best_val_metric
            End If

            If improved Then
                best_val_metric = val_metric
                best_round = i
                become_worse_round = 0
            Else
                become_worse_round += 1
            End If

            ' 旧实现使用 > ，比 early_stopping_rounds 多跑一轮
            If become_worse_round >= early_stopping_rounds Then
                GBM.logger.log(MSG_TYPES.INF, String.Format("TGBoost training stop,best round is {0:D},best val-{1} is {2:F6}", best_round, eval_metric, best_val_metric))
                Return False
            End If

            Return True
        End Function

        Public Function predict(data As TestData) As Double()
            Return predict(data.origin_feature)
        End Function

        ''' <summary>
        ''' 批量预测。
        '''
        ''' 旧实现是「外层按树循环 + 每棵树内部一次 PLINQ + OrderBy 排序恢复顺序」，
        ''' 复杂度 <c>O(T · N log N)</c> 且产生 <c>T × N</c> 次堆分配。现在把每棵树编译为
        ''' 扁平数组后，按行并行、内层遍历全部树，只派发一次任务。
        ''' </summary>
        ''' <param name="features"></param>
        ''' <returns></returns>
        Public Overridable Function predict(features As Single()()) As Double()
            Dim pred = New Double(features.Length - 1) {}

            For i As Integer = 0 To pred.Length - 1
                pred(i) = _first_round_pred
            Next

            If _trees.Count = 0 Then
                Return _loss.transform(pred)
            End If

            Dim flats As FlatTree() = New FlatTree(_trees.Count - 1) {}

            For i As Integer = 0 To _trees.Count - 1
                flats(i) = _trees(i).Flat
            Next

            Call FlatTree.AccumulateAll(flats, features, pred, _eta, num_thread)

            Return _loss.transform(pred)
        End Function
    End Class
End Namespace
