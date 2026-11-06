' ---------------------------------------------------------------------------
' 半监督全图训练：只有一部分节点有标签，但所有节点都参与消息传递
'
' 这就是 GCN 论文标题里 "Semi-Supervised" 的全部含义：
'   损失只在有标签节点上算（掩码交叉熵），但特征传播覆盖整张图 ——
'   无标签节点的特征与结构信息通过 Â 进入了有标签节点的表示。
'
' 为什么损失/梯度统一走 Tensor.computeKernel.MaskedCrossEntropy 而不是手写循环：
'   1. 它内部是“逐行 log-sum-exp”，数值稳定；手写 exp 在没有减行最大值时会溢出；
'   2. 它把损失与梯度**都除以有效标签数**，于是不同标签量的实验之间损失可比，
'      学习率也不需要重新调；
'   3. GPU 后端会用融合内核覆盖同一个接口，主机侧代码不需要分叉。
'   （GNN.Trainer 里的手写版本这两点都没做，因此本工程不复用它。）
'
' 梯度生命周期：每个训练步 ResetGradients → Forward → 反向 → Step。
'   GCN 是全批量（整图一次前向就是一个 batch），所以反向里的梯度语义是“覆盖”而非
'   “累加”，但清零仍然必要 —— 一旦将来引入子图采样/多 batch，漏清零会立刻发散。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>某次评估的结果</summary>
Public Class EvaluationResult

    Friend Sub New(loss As Double, accuracy As Double, count As Integer)
        Me.Loss = loss
        Me.Accuracy = accuracy
        Me.Count = count
    End Sub

    ''' <summary>掩码位置上的平均交叉熵</summary>
    Public ReadOnly Property Loss As Double
    ''' <summary>掩码位置上的分类准确率（0~1）</summary>
    Public ReadOnly Property Accuracy As Double
    ''' <summary>参与统计的节点数</summary>
    Public ReadOnly Property Count As Integer

    Public Overrides Function ToString() As String
        Return $"loss={Loss:F4}, acc={Accuracy:P2} (n={Count})"
    End Function

End Class

''' <summary>逐 epoch 的训练历史（供报告、绘图与回归断言使用）</summary>
Public Class TrainingHistory

    Friend Sub New()
        Loss = New List(Of Double)()
        TrainAccuracy = New List(Of Double)()
        ValidationLoss = New List(Of Double)()
        ValidationAccuracy = New List(Of Double)()
    End Sub

    Public ReadOnly Property Loss As List(Of Double)
    Public ReadOnly Property TrainAccuracy As List(Of Double)
    Public ReadOnly Property ValidationLoss As List(Of Double)
    Public ReadOnly Property ValidationAccuracy As List(Of Double)

    ''' <summary>实际完成的 epoch 数（早停时小于请求值）</summary>
    Public Property TotalEpochs As Integer
    ''' <summary>是否被早停终止</summary>
    Public Property StoppedEarly As Boolean

    ''' <summary>清空历史（复用同一个训练器跑下一组实验时调用）。</summary>
    Friend Sub Reset()
        Call Loss.Clear()
        Call TrainAccuracy.Clear()
        Call ValidationLoss.Clear()
        Call ValidationAccuracy.Clear()

        TotalEpochs = 0
        StoppedEarly = False
    End Sub

    ''' <summary>初始损失（第一个 epoch）</summary>
    Public ReadOnly Property InitialLoss As Double
        Get
            Return If(Loss.Count > 0, Loss(0), Double.NaN)
        End Get
    End Property

    ''' <summary>最终损失（最后一个 epoch）</summary>
    Public ReadOnly Property FinalLoss As Double
        Get
            Return If(Loss.Count > 0, Loss(Loss.Count - 1), Double.NaN)
        End Get
    End Property

    ''' <summary>最优（最小）验证损失；没有验证集时退回训练损失</summary>
    Public ReadOnly Property BestValidationLoss As Double
        Get
            Dim source = If(ValidationLoss.Count > 0, ValidationLoss, Loss)
            Dim best = Double.MaxValue

            For Each value In source
                If value < best Then best = value
            Next

            Return If(source.Count > 0, best, Double.NaN)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"epochs={TotalEpochs}, loss {InitialLoss:F4}→{FinalLoss:F4}, early-stop={StoppedEarly}"
    End Function

End Class

''' <summary>
''' 半监督节点分类训练器：整图全批量 + 掩码交叉熵 + AdamW + 可选早停。
''' </summary>
Public Class SemiSupervisedTrainer

    Private ReadOnly _network As GCNNetwork
    Private ReadOnly _optimizer As GCNAdamW

    Public Sub New(network As GCNNetwork, optimizer As GCNAdamW)
        If network Is Nothing Then Throw New ArgumentNullException(NameOf(network))
        If optimizer Is Nothing Then Throw New ArgumentNullException(NameOf(optimizer))

        _network = network
        _optimizer = optimizer
    End Sub

    ''' <summary>网络</summary>
    Public ReadOnly Property Network As GCNNetwork
        Get
            Return _network
        End Get
    End Property

    ''' <summary>优化器</summary>
    Public ReadOnly Property Optimizer As GCNAdamW
        Get
            Return _optimizer
        End Get
    End Property

    ''' <summary>最近一次 <see cref="Train"/> 的历史</summary>
    Public ReadOnly Property History As New TrainingHistory()

    ''' <summary>
    ''' 训练。
    ''' </summary>
    ''' <param name="features">节点特征 <c>[nodes, inFeatures]</c></param>
    ''' <param name="adjacency">对称归一化邻接</param>
    ''' <param name="labels">节点标签；无效节点填负数（会被自动跳过）</param>
    ''' <param name="trainMask">训练节点掩码（长度 = 节点数）</param>
    ''' <param name="valMask">验证节点掩码；Nothing 表示不做验证/早停</param>
    ''' <param name="epochs">最大 epoch 数</param>
    ''' <param name="earlyStopping">早停器；Nothing 表示跑满 <paramref name="epochs"/></param>
    ''' <param name="printEvery">每多少个 epoch 打印一次进度（0 表示静默）</param>
    Public Function Train(features As Tensor,
                          adjacency As NormalizedAdjacency,
                          labels As Integer(),
                          trainMask As Boolean(),
                          Optional valMask As Boolean() = Nothing,
                          Optional epochs As Integer = 200,
                          Optional earlyStopping As EarlyStopping = Nothing,
                          Optional printEvery As Integer = 10) As TrainingHistory

        If features Is Nothing Then Throw New ArgumentNullException(NameOf(features))
        If adjacency Is Nothing Then Throw New ArgumentNullException(NameOf(adjacency))
        If labels Is Nothing Then Throw New ArgumentNullException(NameOf(labels))
        If trainMask Is Nothing Then Throw New ArgumentNullException(NameOf(trainMask))
        If epochs <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(epochs))

        Call History.Reset()

        If earlyStopping IsNot Nothing Then Call earlyStopping.Reset()

        If printEvery > 0 Then
            Console.WriteLine($"GCN 训练：{epochs} epoch（上限），lr={_optimizer.LearningRate:G4}, " &
                              $"wd={_optimizer.WeightDecay:G4}, dropout={_network.DropoutRate:P0}, " &
                              $"传播={adjacency.EffectiveMode}")
            Console.WriteLine(New String("-"c, 72))
        End If

        For epoch = 1 To epochs
            Dim loss = TrainEpoch(features, adjacency, labels, trainMask)

            If Double.IsNaN(loss) OrElse Double.IsInfinity(loss) Then
                Throw New InvalidOperationException(
                    $"第 {epoch} 个 epoch 的损失为非有限值（{loss}）—— 通常是学习率过大或输入特征未标准化")
            End If

            Dim trainEval = Evaluate(features, adjacency, labels, trainMask)
            Dim valEval = If(valMask Is Nothing, Nothing, Evaluate(features, adjacency, labels, valMask))

            Call History.Loss.Add(loss)
            Call History.TrainAccuracy.Add(trainEval.Accuracy)

            If valEval IsNot Nothing Then
                Call History.ValidationLoss.Add(valEval.Loss)
                Call History.ValidationAccuracy.Add(valEval.Accuracy)
            End If

            History.TotalEpochs = epoch

            If printEvery > 0 AndAlso (epoch = 1 OrElse epoch Mod printEvery = 0 OrElse epoch = epochs) Then
                If valEval Is Nothing Then
                    Console.WriteLine($"  epoch {epoch,4}/{epochs}  loss={loss:F4}  train_acc={trainEval.Accuracy:P2}")
                Else
                    Console.WriteLine($"  epoch {epoch,4}/{epochs}  loss={loss:F4}  train_acc={trainEval.Accuracy:P2}  " &
                                      $"val_loss={valEval.Loss:F4}  val_acc={valEval.Accuracy:P2}")
                End If
            End If

            If earlyStopping IsNot Nothing Then
                ' 有验证集时监控验证损失（更贴近泛化），否则退回训练损失
                Dim monitor = If(valEval IsNot Nothing, valEval.Loss, loss)

                If earlyStopping.ShouldStop(monitor, epoch, _network.GetParameters()) Then
                    History.StoppedEarly = True

                    If printEvery > 0 Then
                        Console.WriteLine($"  早停：第 {epoch} 个 epoch 起验证损失连续 {earlyStopping.Patience} 次未改善，" &
                                          $"回滚到 epoch {earlyStopping.BestEpoch}（{earlyStopping.BestValue:F4}）")
                    End If

                    Exit For
                End If
            End If
        Next

        If earlyStopping IsNot Nothing Then
            Call earlyStopping.RestoreBest(_network.GetParameters())
        End If

        _network.SetTraining(False)

        If printEvery > 0 Then
            Console.WriteLine(New String("-"c, 72))
            Console.WriteLine($"训练结束：{History}")
        End If

        Return History
    End Function

    ''' <summary>
    ''' 单个 epoch：清零梯度 → 前向 → 掩码交叉熵 → 反向 → 参数更新。
    ''' </summary>
    ''' <returns>训练节点上的平均交叉熵</returns>
    Public Function TrainEpoch(features As Tensor,
                               adjacency As NormalizedAdjacency,
                               labels As Integer(),
                               trainMask As Boolean()) As Double

        _network.SetTraining(True)
        _network.ResetGradients()

        Dim logits = _network.Forward(features, adjacency)
        Dim dLogits As Tensor = Nothing
        Dim loss = Tensor.computeKernel.MaskedCrossEntropy(logits, labels, trainMask, dLogits)

        Call _network.Backward(dLogits)
        Call _optimizer.Step()

        Return loss
    End Function

    ''' <summary>
    ''' 在掩码节点上评估损失与准确率（不更新参数）。
    ''' </summary>
    Public Function Evaluate(features As Tensor,
                             adjacency As NormalizedAdjacency,
                             labels As Integer(),
                             mask As Boolean()) As EvaluationResult

        _network.SetTraining(False)

        Dim logits = _network.Forward(features, adjacency)
        Dim dLogits As Tensor = Nothing
        Dim loss = Tensor.computeKernel.MaskedCrossEntropy(logits, labels, mask, dLogits)
        Dim predicted = _network.Predict(logits)
        Dim vocab = logits.Shape(1)

        Dim correct = 0
        Dim total = 0

        For i = 0 To predicted.Length - 1
            ' 与 MaskedCrossEntropy 的有效位置判据保持一致，否则损失与准确率的样本集不同
            If mask IsNot Nothing AndAlso i < mask.Length AndAlso Not mask(i) Then Continue For
            If i >= labels.Length Then Continue For

            Dim label = labels(i)

            If label < 0 OrElse label >= vocab Then Continue For

            total += 1

            If predicted(i) = label Then correct += 1
        Next

        Return New EvaluationResult(loss, If(total > 0, correct / CDbl(total), 0.0), total)
    End Function

End Class
