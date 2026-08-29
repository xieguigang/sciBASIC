
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' 图分类训练器
''' 用于图级别分类任务的训练
''' </summary>
Public Class GraphClassificationTrainer
    Private ReadOnly _model As GraphClassificationModel
    Private ReadOnly _optimizer As Optimizer

    Public ReadOnly Property TrainLossHistory As List(Of Single) = New List(Of Single)()
    Public ReadOnly Property ValAccuracyHistory As List(Of Single) = New List(Of Single)()

    Public Sub New(model As GraphClassificationModel, optimizer As Optimizer)
        _model = model
        _optimizer = optimizer
    End Sub

    ''' <summary>
    ''' 训练一个epoch
    ''' </summary>
    Public Function TrainEpoch(dataset As GraphDataset, trainIndices As Integer()) As Single
        _model.SetTraining(True)

        Dim totalLoss As Single = 0

        For Each idx In trainIndices
            _optimizer.ZeroGrad()

            Dim graph = dataset.Graphs(idx)
            Dim label = dataset.Labels(idx)

            ' 前向传播
            Dim probs = _model.Forward(graph.NodeFeatures, graph)

            ' 计算损失和梯度
            Dim gradient = New Tensor(probs.Shape)
            Dim loss As Single = 0

            For j = 0 To probs.Shape(1) - 1
                If j = label Then
                    loss = -CSng(std.Log(std.Max(probs(0, j), 0.0000001F)))
                    gradient(0, j) = probs(0, j) - 1
                Else
                    gradient(0, j) = probs(0, j)
                End If
            Next

            totalLoss += loss

            ' 反向传播
            _model.Backward(gradient, graph)

            ' 参数更新
            _optimizer.Step()
        Next

        Dim avgLoss = totalLoss / trainIndices.Length
        TrainLossHistory.Add(avgLoss)

        Return avgLoss
    End Function

    ''' <summary>
    ''' 评估模型
    ''' </summary>
    Public Function Evaluate(dataset As GraphDataset, evalIndices As Integer()) As Single
        _model.SetTraining(False)

        Dim correct = 0

        For Each idx In evalIndices
            Dim graph = dataset.Graphs(idx)
            Dim label = dataset.Labels(idx)

            Dim probs = _model.Forward(graph.NodeFeatures, graph)

            ' 找到预测类别
            Dim predictedClass = 0
            Dim maxProb = probs(0, 0)
            For j = 1 To probs.Shape(1) - 1
                If probs(0, j) > maxProb Then
                    maxProb = probs(0, j)
                    predictedClass = j
                End If
            Next

            If predictedClass = label Then correct += 1
        Next

        Return CSng(correct) / evalIndices.Length
    End Function

    ''' <summary>
    ''' 完整训练流程
    ''' </summary>
    Public Sub Train(dataset As GraphDataset, trainRatio As Single, epochs As Integer, Optional printEvery As Integer = 10)
        Dim totalSamples = dataset.Count
        Dim trainSize As Integer = totalSamples * trainRatio

        ' 随机划分训练集和验证集
        Dim indices = Enumerable.Range(0, totalSamples).ToArray()
        Dim random = New Random(42)
        For i = indices.Length - 1 To 1 Step -1
            Dim j = random.Next(i + 1)
            Dim ij = (indices(j), indices(i))
            indices(i) = ij.Item1
            indices(j) = ij.Item2
        Next

        Dim trainIndices = indices.Take(trainSize).ToArray()
        Dim valIndices = indices.Skip(trainSize).ToArray()

        Console.WriteLine($"训练样本: {trainIndices.Length}, 验证样本: {valIndices.Length}")
        Console.WriteLine(New String("-"c, 50))

        For epoch = 0 To epochs - 1
            Dim loss = TrainEpoch(dataset, trainIndices)
            Dim valAcc = Evaluate(dataset, valIndices)
            Dim trainAcc = Evaluate(dataset, trainIndices)

            ValAccuracyHistory.Add(valAcc)

            If (epoch + 1) Mod printEvery = 0 OrElse epoch = 0 Then
                Console.WriteLine($"Epoch {epoch + 1,4}/{epochs}: " & $"Loss = {loss:F4}, Train Acc = {trainAcc:P2}, Val Acc = {valAcc:P2}")
            End If
        Next

        Console.WriteLine(New String("-"c, 50))
        Console.WriteLine("训练完成！")
    End Sub
End Class
