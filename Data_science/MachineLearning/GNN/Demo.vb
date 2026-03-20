Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 时序图神经网络演示程序
''' 展示如何使用T-GCN模型进行时间序列预测
''' </summary>
Public Module TemporalGNNDemo

    ''' <summary>
    ''' 主入口点
    ''' </summary>
    Public Sub Main()
        Console.WriteLine("="c, 60)
        Console.WriteLine("时序图神经网络 (Temporal GNN) 演示程序")
        Console.WriteLine("="c, 60)
        Console.WriteLine()

        ' 演示1: 基本的时间序列预测
        Console.WriteLine("【演示1】交通流量预测示例")
        Console.WriteLine("-"c, 60)
        DemoTrafficPrediction()

        Console.WriteLine()

        ' 演示2: 股票价格预测
        Console.WriteLine("【演示2】股票价格预测示例")
        Console.WriteLine("-"c, 60)
        DemoStockPrediction()

        Console.WriteLine()

        ' 演示3: 传感器网络数据预测
        Console.WriteLine("【演示3】传感器网络数据预测示例")
        Console.WriteLine("-"c, 60)
        DemoSensorNetworkPrediction()

        Console.WriteLine()
        Console.WriteLine("演示完成！")
    End Sub

    ''' <summary>
    ''' 交通流量预测演示
    ''' 模拟多个路口的交通流量数据，预测未来流量
    ''' </summary>
    Private Sub DemoTrafficPrediction()
        ' 参数设置
        Dim numNodes = 5  ' 5个路口
        Dim numTimeSteps = 100  ' 100个时间步
        Dim windowSize = 10  ' 使用过去10个时间步预测
        Dim horizon = 1  ' 预测下一个时间步
        Dim featureDim = 1  ' 每个路口1个特征（流量）

        Console.WriteLine($"节点数: {numNodes}, 时间步: {numTimeSteps}")
        Console.WriteLine($"窗口大小: {windowSize}, 预测步长: {horizon}")

        ' 生成模拟数据
        Dim data = GenerateTrafficData(numNodes, numTimeSteps)
        Console.WriteLine($"生成模拟数据完成，形状: [{data.Shape(0)}, {data.Shape(1)}]")

        ' 创建滑动窗口数据集
        Dim dataset = TimeSeriesUtils.CreateSlidingWindowDataset(data, windowSize, horizon, numNodes)
        Console.WriteLine($"创建数据集完成，样本数: {dataset.Count}")

        ' 创建T-GCN模型
        Dim model = New TGCNModel(
            featureDim,      ' 输入特征维度
            16,              ' GCN隐藏层维度
            32,              ' GRU隐藏层维度
            numNodes * featureDim  ' 输出维度
        )
        Console.WriteLine("T-GCN模型创建完成")

        ' 创建优化器
        Dim parameters = model.GetParameters()
        Dim gradients = model.GetGradients()
        Dim optimizer = New AdamOptimizer(parameters, gradients, 0.001F)

        ' 创建训练器
        Dim trainer = New TemporalGNNTrainer(model, optimizer)

        ' 训练模型
        Console.WriteLine()
        trainer.Train(dataset, 0.8, 50, printEvery:=10)

        ' 评估
        Console.WriteLine()
        Console.WriteLine("模型评估:")
        EvaluateModel(model, dataset, numNodes)
    End Sub

    ''' <summary>
    ''' 股票价格预测演示
    ''' 模拟多只股票的价格走势，预测未来价格
    ''' </summary>
    Private Sub DemoStockPrediction()
        ' 参数设置
        Dim numStocks = 4  ' 4只股票
        Dim numTimeSteps = 200  ' 200个交易日
        Dim windowSize = 20  ' 使用过去20天数据
        Dim horizon = 5  ' 预测未来5天

        Console.WriteLine($"股票数: {numStocks}, 交易日: {numTimeSteps}")
        Console.WriteLine($"窗口大小: {windowSize}, 预测步长: {horizon}")

        ' 生成模拟股票数据
        Dim data = GenerateStockData(numStocks, numTimeSteps)
        Console.WriteLine($"生成模拟数据完成")

        ' 创建数据集
        Dim dataset = TimeSeriesUtils.CreateSlidingWindowDataset(data, windowSize, horizon, numStocks)

        ' 创建A3T-GCN模型（带注意力机制）
        Dim model = New A3TGCNModel(
            1,               ' 输入特征维度（价格）
            8,               ' GCN隐藏层
            16,              ' GRU隐藏层
            numStocks        ' 输出维度
        )
        Console.WriteLine("A3T-GCN模型创建完成（带时间注意力）")

        ' 创建优化器和训练器
        Dim parameters = model.GetParameters()
        Dim gradients = model.GetGradients()
        Dim optimizer = New AdamOptimizer(parameters, gradients, 0.0005F)
        Dim trainer = New TemporalGNNTrainer(model, optimizer)

        ' 训练
        Console.WriteLine()
        trainer.Train(dataset, 0.8, 30, printEvery:=10)
    End Sub

    ''' <summary>
    ''' 传感器网络数据预测演示
    ''' 模拟多个传感器的温度数据
    ''' </summary>
    Private Sub DemoSensorNetworkPrediction()
        ' 参数设置
        Dim numSensors = 6  ' 6个传感器
        Dim numTimeSteps = 150
        Dim windowSize = 12

        Console.WriteLine($"传感器数: {numSensors}, 时间步: {numTimeSteps}")

        ' 生成传感器数据
        Dim data = GenerateSensorData(numSensors, numTimeSteps)

        ' 创建时序图数据集
        Dim dataset = New TemporalGraphDataset()

        For startIdx = 0 To numTimeSteps - windowSize - 1
            Dim tg = New TemporalGraph(windowSize)

            For t = 0 To windowSize - 1
                Dim actualTimeStep = startIdx + t
                Dim nodeFeatures = New Tensor(numSensors, 1)

                For s = 0 To numSensors - 1
                    nodeFeatures(s, 0) = data(actualTimeStep, s)
                Next

                Dim graph = New Graph(nodeFeatures)

                ' 构建传感器网络拓扑（环形+星形）
                For i = 0 To numSensors - 1
                    graph.AddUndirectedEdge(i, (i + 1) Mod numSensors, 1.0F)
                Next
                ' 添加中心节点连接
                For i = 1 To numSensors - 1
                    graph.AddUndirectedEdge(0, i, 0.5F)
                Next

                tg(t) = graph
            Next

            ' 目标：预测下一个时间步所有传感器的值
            Dim target = New Single(numSensors - 1) {}
            For s = 0 To numSensors - 1
                target(s) = data(startIdx + windowSize, s)
            Next

            dataset.Add(tg, target)
        Next

        Console.WriteLine($"数据集创建完成，样本数: {dataset.Count}")

        ' 创建模型
        Dim model = New TGCNModel(1, 8, 16, numSensors)

        ' 训练
        Dim parameters = model.GetParameters()
        Dim gradients = model.GetGradients()
        Dim optimizer = New AdamOptimizer(parameters, gradients, 0.002F)
        Dim trainer = New TemporalGNNTrainer(model, optimizer)

        Console.WriteLine()
        trainer.Train(dataset, 0.8, 40, printEvery:=10)
    End Sub

#Region "数据生成函数"

    ''' <summary>
    ''' 生成模拟交通流量数据
    ''' </summary>
    Private Function GenerateTrafficData(numNodes As Integer, numTimeSteps As Integer) As Tensor
        Dim random = New Random(42)
        Dim data = New Tensor(numTimeSteps, numNodes)

        ' 每个节点有不同的基础流量和周期性
        Dim baseFlows = New Single() {100, 150, 80, 120, 90}
        Dim periods = New Single() {24, 24, 12, 24, 24}  ' 周期（小时）
        Dim phases = New Single() {0, 2, 4, 1, 3}  ' 相位偏移

        For t = 0 To numTimeSteps - 1
            For n = 0 To numNodes - 1
                ' 基础流量 + 周期性变化 + 随机噪声
                Dim periodic = 30 * std.Sin(2 * std.PI * (t + phases(n)) / periods(n))
                Dim noise = (CSng(random.NextDouble()) - 0.5F) * 20
                data(t, n) = baseFlows(n) + CSng(periodic) + noise
            Next
        Next

        Return data
    End Function

    ''' <summary>
    ''' 生成模拟股票数据
    ''' </summary>
    Private Function GenerateStockData(numStocks As Integer, numTimeSteps As Integer) As Tensor
        Dim random = New Random(123)
        Dim data = New Tensor(numTimeSteps, numStocks)

        ' 每只股票有不同的初始价格和趋势
        Dim prices = New Single() {100, 50, 200, 75}
        Dim trends = New Single() {0.01F, 0.005F, -0.002F, 0.008F}
        Dim volatilities = New Single() {0.02F, 0.03F, 0.015F, 0.025F}

        For t = 0 To numTimeSteps - 1
            For s = 0 To numStocks - 1
                If t = 0 Then
                    data(t, s) = prices(s)
                Else
                    ' 几何布朗运动模型
                    Dim drift = trends(s)
                    Dim shock = volatilities(s) * CSng(random.NextDouble() - 0.5) * 2
                    data(t, s) = data(t - 1, s) * (1 + drift + shock)
                End If
            Next
        Next

        Return data
    End Function

    ''' <summary>
    ''' 生成模拟传感器数据
    ''' </summary>
    Private Function GenerateSensorData(numSensors As Integer, numTimeSteps As Integer) As Tensor
        Dim random = New Random(456)
        Dim data = New Tensor(numTimeSteps, numSensors)

        ' 温度数据：日周期性 + 随机波动
        Dim baseTemps = New Single() {20, 22, 18, 25, 21, 23}

        For t = 0 To numTimeSteps - 1
            For s = 0 To numSensors - 1
                ' 日周期性变化
                Dim daily = 5 * std.Sin(2 * std.PI * t / 24)
                ' 传感器间的相关性
                Dim correlation = If(s > 0, 0.3F * (data(t, s - 1) - baseTemps(s - 1)), 0)
                ' 随机噪声
                Dim noise = (CSng(random.NextDouble()) - 0.5F) * 2

                data(t, s) = baseTemps(s) + CSng(daily) + correlation + noise
            Next
        Next

        Return data
    End Function

#End Region

#Region "评估函数"

    ''' <summary>
    ''' 评估模型性能
    ''' </summary>
    Private Sub EvaluateModel(model As TemporalGNNModel, dataset As TemporalGraphDataset, numNodes As Integer)
        model.SetTraining(False)

        Dim totalMAE As Single = 0
        Dim totalRMSE As Single = 0
        Dim count As Integer = 0

        ' 评估最后10个样本
        Dim evalStart = std.Max(0, dataset.Count - 10)

        For i = evalStart To dataset.Count - 1
            Dim graph = dataset(i)
            Dim target = dataset.GetLabel(i)

            Dim prediction = model.Forward(graph)

            ' 创建目标张量
            Dim targetTensor = New Tensor(prediction.Shape)
            For j = 0 To target.Length - 1
                targetTensor(j) = target(j)
            Next

            totalMAE += TimeSeriesUtils.MAE(prediction, targetTensor)
            totalRMSE += TimeSeriesUtils.RMSE(prediction, targetTensor)
            count += 1
        Next

        Console.WriteLine($"  MAE: {totalMAE / count:F4}")
        Console.WriteLine($"  RMSE: {totalRMSE / count:F4}")
    End Sub

#End Region

End Module

''' <summary>
''' 简单使用示例
''' 展示最基本的使用方法
''' </summary>
Public Module SimpleExample

    ''' <summary>
    ''' 最简单的使用示例
    ''' </summary>
    Public Sub SimpleDemo()
        ' 步骤1: 创建时序图数据
        Dim numTimeSteps = 5
        Dim numNodes = 3
        Dim featureDim = 2

        Dim temporalGraph = New TemporalGraph(numTimeSteps)

        ' 为每个时间步创建图快照
        For t = 0 To numTimeSteps - 1
            ' 创建节点特征
            Dim features = New Tensor(numNodes, featureDim)
            For i = 0 To numNodes - 1
                For j = 0 To featureDim - 1
                    features(i, j) = CSng((t + 1) * (i + 1) + j * 0.1)
                Next
            Next

            ' 创建图
            Dim graph = New Graph(features)
            graph.AddUndirectedEdge(0, 1)
            graph.AddUndirectedEdge(1, 2)
            graph.AddUndirectedEdge(0, 2)

            temporalGraph(t) = graph
        Next

        ' 步骤2: 创建T-GCN模型
        Dim model = New TGCNModel(
            featureDim,  ' 输入特征维度
            4,           ' GCN隐藏层维度
            8,           ' GRU隐藏层维度
            1            ' 输出维度（预测一个值）
        )

        ' 步骤3: 前向传播
        Dim output = model.Forward(temporalGraph)

        Console.WriteLine($"输入: {numTimeSteps}个时间步, {numNodes}个节点, {featureDim}维特征")
        Console.WriteLine($"输出: {output.Shape(0)} x {output.Shape(1)}")
        Console.WriteLine($"预测值: {output(0, 0):F4}")

        ' 步骤4: 训练（如果有标签数据）
        ' 创建优化器
        Dim parameters = model.GetParameters()
        Dim gradients = model.GetGradients()
        Dim optimizer = New AdamOptimizer(parameters, gradients, 0.001F)

        ' 计算损失和梯度
        Dim target = 5.0F  ' 目标值
        Dim loss = (output(0, 0) - target) * (output(0, 0) - target)
        Dim gradient = New Tensor(1, 1)
        gradient(0, 0) = 2 * (output(0, 0) - target)

        ' 反向传播
        model.Backward(gradient, temporalGraph)

        ' 更新参数
        optimizer.Step()

        Console.WriteLine($"损失: {loss:F4}")
        Console.WriteLine("训练步骤完成！")
    End Sub

End Module
