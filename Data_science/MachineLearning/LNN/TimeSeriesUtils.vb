Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 时间序列处理工具
''' </summary>
Public Module TimeSeriesUtils

    ''' <summary>
    ''' 创建滑动窗口数据集
    ''' </summary>
    ''' <param name="data">原始时间序列数据</param>
    ''' <param name="windowSize">窗口大小</param>
    ''' <param name="forecastHorizon">预测步长</param>
    ''' <returns>输入窗口和目标值</returns>
    Public Function CreateSlidingWindowDataset(data As Double(), windowSize As Integer, forecastHorizon As Integer) As (inputs As List(Of Tensor), targets As List(Of Tensor))
        Dim inputs As New List(Of Tensor)()
        Dim targets As New List(Of Tensor)()

        For i = 0 To data.Length - windowSize - forecastHorizon
            ' 创建输入窗口
            Dim inputWindow = New Tensor(windowSize)
            For j = 0 To windowSize - 1
                inputWindow(j) = data(i + j)
            Next

            ' 创建目标值
            Dim targetValue = New Tensor(forecastHorizon)
            For j = 0 To forecastHorizon - 1
                targetValue(j) = data(i + windowSize + j)
            Next

            inputs.Add(inputWindow)
            targets.Add(targetValue)
        Next

        Return (inputs, targets)
    End Function

    ''' <summary>
    ''' 归一化数据到[0, 1]范围
    ''' </summary>
    Public Function Normalize(data As Double()) As (normalized As Double(), min As Double, max As Double)
        Dim minVal = data.Min()
        Dim maxVal = data.Max()
        Dim range = maxVal - minVal

        If range = 0 Then
            Return (data.Select(Function(x) 0.5).ToArray(), minVal, maxVal)
        End If

        Dim normalized = data.Select(Function(x) (x - minVal) / range).ToArray()
        Return (normalized, minVal, maxVal)
    End Function

    ''' <summary>
    ''' 反归一化
    ''' </summary>
    Public Function Denormalize(normalized As Double(), min As Double, max As Double) As Double()
        Dim range = max - min
        Return normalized.Select(Function(x) x * range + min).ToArray()
    End Function

    ''' <summary>
    ''' 标准化数据（零均值，单位方差）
    ''' </summary>
    Public Function Standardize(data As Double()) As (standardized As Double(), mean As Double, std As Double)
        Dim mean = data.Average()
        Dim variance = data.Select(Function(x) (x - mean) * (x - mean)).Average()
        Dim std As Double = System.Math.Sqrt(variance)

        If std = 0 Then
            Return (data.Select(Function(x) 0.0).ToArray(), mean, std)
        End If

        Dim standardized = data.Select(Function(x) (x - mean) / std).ToArray()
        Return (standardized, mean, std)
    End Function

    ''' <summary>
    ''' 反标准化
    ''' </summary>
    Public Function Destandardize(standardized As Double(), mean As Double, std As Double) As Double()
        Return standardized.Select(Function(x) x * std + mean).ToArray()
    End Function

    ''' <summary>
    ''' 生成正弦波数据
    ''' </summary>
    Public Function GenerateSineWave(length As Integer, frequency As Double, amplitude As Double, phase As Double, noiseLevel As Double) As Double()
        Dim data = New Double(length - 1) {}
        Dim random As New Random(42)

        For i = 0 To length - 1
            data(i) = amplitude * std.Sin(2 * std.PI * frequency * i + phase)
            If noiseLevel > 0 Then
                data(i) += (random.NextDouble() * 2 - 1) * noiseLevel
            End If
        Next

        Return data
    End Function

    ''' <summary>
    ''' 计算预测指标
    ''' </summary>
    Public Function CalculateMetrics(predicted As Double(), actual As Double()) As (mse As Double, mae As Double, rmse As Double, mape As Double)
        Dim n = predicted.Length
        Dim sumSquaredError = 0.0
        Dim sumAbsoluteError = 0.0
        Dim sumAbsolutePercentError = 0.0

        For i = 0 To n - 1
            Dim [error] = predicted(i) - actual(i)
            sumSquaredError += [error] ^ 2
            sumAbsoluteError += std.Abs([error])

            If actual(i) <> 0 Then
                sumAbsolutePercentError += std.Abs([error] / actual(i))
            End If
        Next

        Dim mse = sumSquaredError / n
        Dim mae = sumAbsoluteError / n
        Dim rmse = std.Sqrt(mse)
        Dim mape = sumAbsolutePercentError / n * 100

        Return (mse, mae, rmse, mape)
    End Function

End Module
