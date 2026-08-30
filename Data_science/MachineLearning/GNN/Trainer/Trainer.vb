#Region "Microsoft.VisualBasic::d456c2617f66159a097d722d647ce115, Data_science\MachineLearning\GNN\Trainer\Trainer.vb"

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

    '   Total Lines: 136
    '    Code Lines: 74 (54.41%)
    ' Comment Lines: 35 (25.74%)
    '    - Xml Docs: 77.14%
    ' 
    '   Blank Lines: 27 (19.85%)
    '     File Size: 4.58 KB


    ' Class Trainer
    ' 
    '     Properties: TrainLossHistory, ValAccuracyHistory
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Evaluate, TrainEpoch
    ' 
    '     Sub: Train
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 训练器
''' 封装了训练循环、验证和测试逻辑
''' </summary>
Public Class Trainer
    Private ReadOnly _model As GNNModel
    Private ReadOnly _optimizer As Optimizer
    Private ReadOnly _lossType As LossType

    ''' <summary>
    ''' 训练损失历史
    ''' </summary>
    Public ReadOnly Property TrainLossHistory As List(Of Single) = New List(Of Single)()

    ''' <summary>
    ''' 验证准确率历史
    ''' </summary>
    Public ReadOnly Property ValAccuracyHistory As List(Of Single) = New List(Of Single)()

    Public Sub New(model As GNNModel, optimizer As Optimizer, Optional lossType As LossType = LossType.CrossEntropy)
        _model = model
        _optimizer = optimizer
        _lossType = lossType
    End Sub

    ''' <summary>
    ''' 训练一个epoch
    ''' </summary>
    ''' <param name="graph">图数据</param>
    ''' <param name="labels">节点标签</param>
    ''' <param name="trainMask">训练集掩码（标记哪些节点用于训练）</param>
    ''' <returns>平均训练损失</returns>
    Public Function TrainEpoch(graph As Graph, labels As Integer(), trainMask As Boolean()) As Single
        _model.SetTraining(True)
        _optimizer.ZeroGrad()

        ' 前向传播
        Dim probs = _model.Forward(graph.NodeFeatures, graph)

        ' 计算损失（只在训练节点上）
        Dim totalLoss As Single = 0
        Dim trainCount = 0

        ' 创建梯度张量
        Dim gradient = New Tensor(probs.Shape)

        For i = 0 To labels.Length - 1
            If Not trainMask(i) Then Continue For

            trainCount += 1

            ' 计算交叉熵损失及其梯度
            ' 对于Softmax + CrossEntropy，梯度 = probs - one_hot(label)
            For j = 0 To probs.Shape(1) - 1
                If j = labels(i) Then
                    totalLoss -= CSng(std.Log(std.Max(probs(i, j), 0.0000001F)))
                    gradient(i, j) = probs(i, j) - 1
                Else
                    gradient(i, j) = probs(i, j)
                End If
            Next
        Next

        Dim avgLoss = totalLoss / trainCount
        TrainLossHistory.Add(avgLoss)

        ' 反向传播
        _model.Backward(gradient, graph)

        ' 参数更新
        _optimizer.Step()

        Return avgLoss
    End Function

    ''' <summary>
    ''' 评估模型
    ''' </summary>
    ''' <param name="graph">图数据</param>
    ''' <param name="labels">节点标签</param>
    ''' <param name="evalMask">评估集掩码</param>
    ''' <returns>准确率</returns>
    Public Function Evaluate(graph As Graph, labels As Integer(), evalMask As Boolean()) As Single
        _model.SetTraining(False)

        Dim probs = _model.Forward(graph.NodeFeatures, graph)

        Dim correct = 0
        Dim total = 0

        For i = 0 To labels.Length - 1
            If Not evalMask(i) Then Continue For

            total += 1

            ' 找到预测的类别
            Dim predictedClass = 0
            Dim maxProb = probs(i, 0)
            For j = 1 To probs.Shape(1) - 1
                If probs(i, j) > maxProb Then
                    maxProb = probs(i, j)
                    predictedClass = j
                End If
            Next

            If predictedClass = labels(i) Then correct += 1
        Next

        Return If(total > 0, CSng(correct) / total, 0)
    End Function

    ''' <summary>
    ''' 完整训练流程
    ''' </summary>
    Public Sub Train(graph As Graph, labels As Integer(), trainMask As Boolean(), valMask As Boolean(), epochs As Integer, Optional printEvery As Integer = 10)
        Console.WriteLine($"开始训练，共 {epochs} 个epoch")
        Console.WriteLine(New String("-"c, 50))

        For epoch = 0 To epochs - 1
            Dim loss = TrainEpoch(graph, labels, trainMask)
            Dim valAcc = Evaluate(graph, labels, valMask)
            ValAccuracyHistory.Add(valAcc)

            If (epoch + 1) Mod printEvery = 0 OrElse epoch = 0 Then
                Dim trainAcc = Evaluate(graph, labels, trainMask)
                Console.WriteLine($"Epoch {epoch + 1,4}/{epochs}: " & $"Loss = {loss:F4}, Train Acc = {trainAcc:P2}, Val Acc = {valAcc:P2}")
            End If
        Next

        Console.WriteLine(New String("-"c, 50))
        Console.WriteLine("训练完成！")
    End Sub
End Class

