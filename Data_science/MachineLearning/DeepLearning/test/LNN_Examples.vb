''' <summary>
''' 液态神经网络(LNN)使用示例
''' 展示如何使用LiquidNeuralNetwork模块进行时间序列分析
''' </summary>
Imports System.IO
Imports Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Public Class LNNUsageExamples

    ''' <summary>
    ''' 示例1: 基础时间序列预测
    ''' </summary>
    Public Shared Sub BasicTimeSeriesPrediction()
        Console.WriteLine("=" & vbNewLine & "示例1: 基础时间序列预测" & vbNewLine & "=")

        ' 步骤1: 准备数据
        ' 生成带有噪声的正弦波数据
        Dim dataLength As Integer = 1000
        Dim rawData(dataLength - 1) As Double
        Dim random As New Random(42)

        For i = 0 To dataLength - 1
            ' 正弦波 + 小噪声
            rawData(i) = std.Sin(2 * std.PI * i / 50.0) + (random.NextDouble() - 0.5) * 0.1
        Next

        Console.WriteLine($"数据长度: {dataLength}")

        ' 步骤2: 数据预处理
        ' 归一化到[0, 1]范围
        Dim minVal = rawData.Min()
        Dim maxVal = rawData.Max()
        Dim normalizedData(dataLength - 1) As Double

        For i = 0 To dataLength - 1
            normalizedData(i) = (rawData(i) - minVal) / (maxVal - minVal)
        Next

        ' 步骤3: 创建滑动窗口数据集
        Dim windowSize As Integer = 20    ' 使用过去20个时间步
        Dim forecastHorizon As Integer = 1 ' 预测下一个时间步

        Dim inputs As New List(Of Tensor)()
        Dim targets As New List(Of Tensor)()

        For i = 0 To dataLength - windowSize - forecastHorizon
            ' 输入窗口
            Dim inputTensor = New Tensor(windowSize)
            For j = 0 To windowSize - 1
                inputTensor(j) = normalizedData(i + j)
            Next

            ' 目标值
            Dim targetTensor = New Tensor(forecastHorizon)
            targetTensor(0) = normalizedData(i + windowSize)

            inputs.Add(inputTensor)
            targets.Add(targetTensor)
        Next

        Console.WriteLine($"样本数量: {inputs.Count}")

        ' 步骤4: 划分训练集和测试集
        Dim splitRatio As Double = 0.8
        Dim splitIndex As Integer = CInt(inputs.Count * splitRatio)

        Dim trainInputs = inputs.Take(splitIndex).ToList()
        Dim trainTargets = targets.Take(splitIndex).ToList()
        Dim testInputs = inputs.Skip(splitIndex).ToList()
        Dim testTargets = targets.Skip(splitIndex).ToList()

        Console.WriteLine($"训练样本: {trainInputs.Count}, 测试样本: {testInputs.Count}")

        ' 步骤5: 创建液态神经网络
        Dim lnn As New LiquidNeuralNetwork(
            inputSize:=windowSize,
            hiddenSize:=16,
            outputSize:=forecastHorizon,
            numLiquidLayers:=1,
            activationType:="tanh",
            outputActivation:="sigmoid",  ' 输出范围[0,1]匹配归一化数据
            seed:=42
        )

        ' 配置网络参数
        lnn.DefaultDt = 0.05          ' 时间步长
        lnn.SolverType = "rk4"        ' 使用RK4求解器
        lnn.RecordHistory = False     ' 不记录历史（节省内存）

        Console.WriteLine($"网络参数数量: {lnn.GetParameterCount()}")

        ' 步骤6: 创建训练器
        Dim trainer As New LNNTrainer(lnn, learningRate:=0.01)
        trainer.OptimizerType = "adam"
        trainer.UseGradientClipping = True
        trainer.GradientClipValue = 1.0

        ' 步骤7: 训练模型
        Console.WriteLine(vbNewLine & "开始训练...")

        Dim epochs As Integer = 30
        Dim batchSize As Integer = 32

        For epoch = 1 To epochs
            Dim epochLoss As Double = 0
            Dim batchCount As Integer = 0

            ' 随机打乱训练数据
            Dim indices = Enumerable.Range(0, trainInputs.Count).OrderBy(Function(x) random.Next()).ToList()

            For batchStart = 0 To trainInputs.Count - 1 Step batchSize
                Dim batchEnd = std.Min(batchStart + batchSize, trainInputs.Count)
                Dim batchLoss As Double = 0

                For i = batchStart To batchEnd - 1
                    Dim idx = indices(i)
                    lnn.ResetState()  ' 每个样本重置状态
                    Dim loss = trainer.TrainStep(trainInputs(idx), trainTargets(idx))
                    batchLoss += loss
                Next

                epochLoss += batchLoss / (batchEnd - batchStart)
                batchCount += 1
            Next

            epochLoss /= batchCount

            If epoch Mod 5 = 0 OrElse epoch = 1 Then
                Console.WriteLine($"Epoch {epoch}/{epochs}, Loss: {epochLoss:F6}")
            End If
        Next

        ' 步骤8: 测试和评估
        Console.WriteLine(vbNewLine & "测试模型...")

        Dim predictions As New List(Of Double)()
        Dim actuals As New List(Of Double)()

        For i = 0 To testInputs.Count - 1
            lnn.ResetState()
            Dim predicted = lnn.Forward(testInputs(i))
            predictions.Add(predicted(0))
            actuals.Add(testTargets(i)(0))
        Next

        ' 计算评估指标
        Dim mse As Double = 0, mae As Double = 0
        For i = 0 To predictions.Count - 1
            Dim [error] = predictions(i) - actuals(i)
            mse += [error] ^ 2
            mae += std.Abs([error])
        Next
        mse /= predictions.Count
        mae /= predictions.Count

        Console.WriteLine($"测试结果:")
        Console.WriteLine($"  MSE: {mse:F6}")
        Console.WriteLine($"  MAE: {mae:F6}")
        Console.WriteLine($"  RMSE: {std.Sqrt(mse):F6}")

        ' 步骤9: 显示预测示例
        Console.WriteLine(vbNewLine & "预测示例（前10个）:")
        Console.WriteLine("  序号    预测值      实际值      误差")
        Console.WriteLine("  ----    ------      ------      ----")
        For i = 0 To std.Min(9, predictions.Count - 1)
            Console.WriteLine($"  {i + 1,4}    {predictions(i):F4}      {actuals(i):F4}      {std.Abs(predictions(i) - actuals(i)):F4}")
        Next

        lnn.Dispose()
        Console.WriteLine(vbNewLine & "示例1完成!" & vbNewLine)
    End Sub

    ''' <summary>
    ''' 示例2: 多步预测
    ''' </summary>
    Public Shared Sub MultiStepForecast()
        Console.WriteLine("=" & vbNewLine & "示例2: 多步预测" & vbNewLine & "=")

        ' 生成数据
        Dim dataLength As Integer = 500
        Dim data(dataLength - 1) As Double

        For i = 0 To dataLength - 1
            ' 复合波形: sin + cos
            data(i) = std.Sin(2 * std.PI * i / 30.0) + 0.5 * std.Cos(2 * std.PI * i / 15.0)
        Next

        ' 归一化
        Dim minVal = data.Min()
        Dim maxVal = data.Max()
        For i = 0 To dataLength - 1
            data(i) = (data(i) - minVal) / (maxVal - minVal)
        Next

        ' 创建数据集 - 预测未来5步
        Dim windowSize As Integer = 30
        Dim forecastHorizon As Integer = 5

        Dim inputs As New List(Of Tensor)()
        Dim targets As New List(Of Tensor)()

        For i = 0 To dataLength - windowSize - forecastHorizon
            Dim inputTensor = New Tensor(windowSize)
            For j = 0 To windowSize - 1
                inputTensor(j) = data(i + j)
            Next

            Dim targetTensor = New Tensor(forecastHorizon)
            For j = 0 To forecastHorizon - 1
                targetTensor(j) = data(i + windowSize + j)
            Next

            inputs.Add(inputTensor)
            targets.Add(targetTensor)
        Next

        Console.WriteLine($"数据准备完成: {inputs.Count} 个样本")

        ' 创建网络
        Dim lnn As New LiquidNeuralNetwork(
            inputSize:=windowSize,
            hiddenSize:=32,
            outputSize:=forecastHorizon,
            numLiquidLayers:=2,
            activationType:="tanh",
            outputActivation:="tanh",
            seed:=123
        )

        lnn.DefaultDt = 0.1
        lnn.SolverType = "rk4"

        Console.WriteLine($"网络配置: {windowSize} -> 32x2 -> {forecastHorizon}")
        Console.WriteLine($"参数数量: {lnn.GetParameterCount()}")

        ' 训练
        Dim trainer As New LNNTrainer(lnn, 0.005)

        Dim trainSize = CInt(inputs.Count * 0.8)
        Dim epochs As Integer = 50

        Console.WriteLine(vbNewLine & "训练中...")
        For epoch = 1 To epochs
            Dim totalLoss As Double = 0

            For i = 0 To trainSize - 1
                lnn.ResetState()
                totalLoss += trainer.TrainStep(inputs(i), targets(i))
            Next

            If epoch Mod 10 = 0 OrElse epoch = 1 Then
                Console.WriteLine($"Epoch {epoch}, Avg Loss: {totalLoss / trainSize:F6}")
            End If
        Next

        ' 测试多步预测
        Console.WriteLine(vbNewLine & "多步预测结果:")
        lnn.ResetState()
        Dim testIdx = trainSize
        Dim testInput = inputs(testIdx)
        Dim testTarget = targets(testIdx)
        Dim prediction = lnn.Forward(testInput)

        Console.WriteLine("  步骤    预测值      实际值")
        Console.WriteLine("  ----    ------      ------")
        For i = 0 To forecastHorizon - 1
            Console.WriteLine($"  {i + 1,4}    {prediction(i):F4}      {testTarget(i):F4}")
        Next

        lnn.Dispose()
        Console.WriteLine(vbNewLine & "示例2完成!" & vbNewLine)
    End Sub

    ''' <summary>
    ''' 示例3: 自定义ODE求解器比较
    ''' </summary>
    Public Shared Sub CompareODESolvers()
        Console.WriteLine("=" & vbNewLine & "示例3: ODE求解器比较" & vbNewLine & "=")

        ' 创建相同的网络配置
        Dim inputSize As Integer = 10
        Dim hiddenSize As Integer = 16
        Dim outputSize As Integer = 1

        Dim solvers As String() = {"euler", "heun", "rk4"}
        Dim results As New Dictionary(Of String, Double)()

        ' 生成测试数据
        Dim testInput = Tensor.Random({inputSize}, -1.0F, 1.0F, 42)
        Dim testTarget = Tensor.Random({outputSize}, 0.0F, 1.0F, 42)

        Console.WriteLine("比较不同ODE求解器的性能:")
        Console.WriteLine()

        For Each solver In solvers
            ' 创建网络
            Dim lnn As New LiquidNeuralNetwork(inputSize, hiddenSize, outputSize, 1, "tanh", "tanh", 42)
            lnn.SolverType = solver
            lnn.DefaultDt = 0.1

            Dim trainer As New LNNTrainer(lnn, 0.01)

            ' 训练
            Dim startTime = DateTime.Now
            For epoch = 1 To 20
                lnn.ResetState()
                trainer.TrainStep(testInput, testTarget)
            Next
            Dim elapsed = (DateTime.Now - startTime).TotalMilliseconds

            ' 最终预测
            lnn.ResetState()
            Dim output = lnn.Forward(testInput)
            Dim finalLoss = LNNTrainer.MSE(output, testTarget)

            results.Add(solver, finalLoss)

            Console.WriteLine($"  {solver.ToUpper(),-6}: Loss = {finalLoss:F6}, Time = {elapsed:F2}ms")

            lnn.Dispose()
        Next

        Console.WriteLine(vbNewLine & "示例3完成!" & vbNewLine)
    End Sub

    ''' <summary>
    ''' 示例4: 状态历史记录和可视化数据导出
    ''' </summary>
    Public Shared Sub StateHistoryExample()
        Console.WriteLine("=" & vbNewLine & "示例4: 状态历史记录" & vbNewLine & "=")

        ' 创建网络
        Dim lnn As New LiquidNeuralNetwork(
            inputSize:=5,
            hiddenSize:=8,
            outputSize:=1,
            numLiquidLayers:=1,
            seed:=42
        )

        lnn.RecordHistory = True  ' 启用历史记录
        lnn.DefaultDt = 0.1

        ' 处理序列
        Dim seqLength As Integer = 20
        Dim inputSeq = Tensor.Random({seqLength, 5}, -1.0F, 1.0F, 42)

        Console.WriteLine("处理时间序列...")
        Dim outputSeq = lnn.ProcessSequence(inputSeq)

        ' 输出状态历史
        Console.WriteLine(vbNewLine & "状态历史记录:")
        Console.WriteLine($"  记录了 {lnn.StateHistory.Count} 个状态")

        ' 导出状态历史到CSV
        Dim csvPath As String = "/home/z/my-project/download/lnn_state_history.csv"
        Using writer As New StreamWriter(csvPath)
            ' 写入表头
            Dim header As New System.Text.StringBuilder("timestep")
            For i = 0 To lnn.HiddenSize - 1
                header.Append($",hidden_{i}")
            Next
            header.Append(",output")
            writer.WriteLine(header.ToString())

            ' 写入数据
            For t = 0 To lnn.StateHistory.Count - 1
                Dim state = lnn.StateHistory(t)
                Dim line As New System.Text.StringBuilder($"{t}")
                For i = 0 To state.Length - 1
                    line.Append($",{state(i):F6}")
                Next
                ' 获取对应的输出
                line.Append($",{outputSeq(t, 0):F6}")
                writer.WriteLine(line.ToString())
            Next
        End Using

        Console.WriteLine($"  状态历史已保存到: {csvPath}")

        lnn.Dispose()
        Console.WriteLine(vbNewLine & "示例4完成!" & vbNewLine)
    End Sub

    ''' <summary>
    ''' 示例5: 不同激活函数的影响
    ''' </summary>
    Public Shared Sub CompareActivationFunctions()
        Console.WriteLine("=" & vbNewLine & "示例5: 激活函数比较" & vbNewLine & "=")

        Dim activations As String() = {"tanh", "sigmoid", "relu"}
        Dim results As New Dictionary(Of String, Double)()

        ' 准备数据
        Dim data = TimeSeriesUtils.GenerateSineWave(200, 0.1, 1.0, 0.0, 0.05)
        Dim normalized = TimeSeriesUtils.Normalize(data)

        Dim windowSize As Integer = 10
        Dim inputs As New List(Of Tensor)()
        Dim targets As New List(Of Tensor)()

        For i = 0 To data.Length - windowSize - 1
            Dim inputTensor = New Tensor(windowSize)
            For j = 0 To windowSize - 1
                inputTensor(j) = normalized.normalized(i + j)
            Next

            Dim targetTensor = New Tensor(1)
            targetTensor(0) = normalized.normalized(i + windowSize)

            inputs.Add(inputTensor)
            targets.Add(targetTensor)
        Next

        Dim trainSize = CInt(inputs.Count * 0.8)

        Console.WriteLine("比较不同激活函数的效果:")
        Console.WriteLine()

        For Each activation In activations
            Dim lnn As New LiquidNeuralNetwork(
                inputSize:=windowSize,
                hiddenSize:=16,
                outputSize:=1,
                numLiquidLayers:=1,
                activationType:=activation,
                outputActivation:="sigmoid",
                seed:=42
            )

            Dim trainer As New LNNTrainer(lnn, 0.01)

            ' 训练
            For epoch = 1 To 30
                For i = 0 To trainSize - 1
                    lnn.ResetState()
                    trainer.TrainStep(inputs(i), targets(i))
                Next
            Next

            ' 测试
            Dim testLoss As Double = 0
            For i = trainSize To inputs.Count - 1
                lnn.ResetState()
                Dim output = lnn.Forward(inputs(i))
                testLoss += LNNTrainer.MSE(output, targets(i))
            Next
            testLoss /= (inputs.Count - trainSize)

            results.Add(activation, testLoss)
            Console.WriteLine($"  {activation,-8}: Test Loss = {testLoss:F6}")

            lnn.Dispose()
        Next

        Console.WriteLine(vbNewLine & "示例5完成!" & vbNewLine)
    End Sub

    ''' <summary>
    ''' 运行所有示例
    ''' </summary>
    Public Shared Sub RunAllExamples()
        Console.WriteLine(vbNewLine & "========================================")
        Console.WriteLine("  液态神经网络(LNN)使用示例集")
        Console.WriteLine("========================================" & vbNewLine)

        BasicTimeSeriesPrediction()
        MultiStepForecast()
        CompareODESolvers()
        StateHistoryExample()
        CompareActivationFunctions()

        Console.WriteLine("========================================")
        Console.WriteLine("  所有示例运行完成!")
        Console.WriteLine("========================================")
    End Sub

End Class
