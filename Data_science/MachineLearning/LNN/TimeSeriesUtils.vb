#Region "Microsoft.VisualBasic::a89634344dffd6e133d06304854199d7, Data_science\MachineLearning\LNN\TimeSeriesUtils.vb"

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

    '   Total Lines: 131
    '    Code Lines: 78 (59.54%)
    ' Comment Lines: 30 (22.90%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 23 (17.56%)
    '     File Size: 4.64 KB


    ' Module TimeSeriesUtils
    ' 
    '     Function: CalculateMetrics, CreateSlidingWindowDataset, Denormalize, Destandardize, GenerateSineWave
    '               Normalize, Standardize
    ' 
    ' /********************************************************************************/

#End Region

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
        ' data.Min() / data.Max() 各是一次完整枚举（两次 LINQ 遍历）；
        ' 下面一趟同时求出极值，再把结果一次写进结果数组。
        Dim n = data.Length

        If n = 0 Then Return (New Double() {}, 0.0, 0.0)

        Dim minVal = data(0)
        Dim maxVal = data(0)

        For i = 1 To n - 1
            Dim v = data(i)

            If v < minVal Then minVal = v
            If v > maxVal Then maxVal = v
        Next

        Dim range = maxVal - minVal
        Dim normalized(n - 1) As Double

        If range = 0 Then
            ' 全常量序列：归一化后退化为 0.5（与旧实现一致）
            For i = 0 To n - 1
                normalized(i) = 0.5
            Next

            Return (normalized, minVal, maxVal)
        End If

        For i = 0 To n - 1
            normalized(i) = (data(i) - minVal) / range
        Next

        Return (normalized, minVal, maxVal)
    End Function

    ''' <summary>
    ''' 反归一化
    ''' </summary>
    Public Function Denormalize(normalized As Double(), min As Double, max As Double) As Double()
        Dim range = max - min
        Dim result(normalized.Length - 1) As Double

        For i = 0 To result.Length - 1
            result(i) = normalized(i) * range + min
        Next

        Return result
    End Function

    ''' <summary>
    ''' 标准化数据（零均值，单位方差）
    ''' </summary>
    Public Function Standardize(data As Double()) As (standardized As Double(), mean As Double, std As Double)
        ' 全部改为显式循环：LINQ 版本每一步都要分配闭包与迭代器
        ' （data.Select(...).Average() 是"委托 + 迭代器 + 再枚举一遍"），
        ' 对长序列来说这些分配与虚调用开销远大于算术本身。
        ' 累加次序与 LINQ Average 一致（下标由小到大），因此结果逐位相同。
        Dim n = data.Length

        If n = 0 Then Return (New Double() {}, 0.0, 0.0)

        Dim sum As Double = 0

        For i = 0 To n - 1
            sum += data(i)
        Next

        Dim mean = sum / n
        Dim sq As Double = 0

        For i = 0 To n - 1
            Dim d = data(i) - mean
            sq += d * d
        Next

        Dim variance = sq / n
        Dim std As Double = System.Math.Sqrt(variance)
        Dim standardized(n - 1) As Double

        If std = 0 Then
            ' 全常量序列：标准化后退化为全零（与旧实现一致）
            Return (standardized, mean, std)
        End If

        For i = 0 To n - 1
            standardized(i) = (data(i) - mean) / std
        Next

        Return (standardized, mean, std)
    End Function

    ''' <summary>
    ''' 反标准化
    ''' </summary>
    Public Function Destandardize(standardized As Double(), mean As Double, std As Double) As Double()
        Dim result(standardized.Length - 1) As Double

        For i = 0 To result.Length - 1
            result(i) = standardized(i) * std + mean
        Next

        Return result
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
