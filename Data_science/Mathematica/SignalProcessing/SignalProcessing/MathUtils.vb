Imports std = System.Math

''' <summary>
''' 数学辅助工具类，提供信号处理和数值计算所需的基础函数
''' </summary>
Public Module MathUtils

    ''' <summary>
    ''' 高斯函数
    ''' </summary>
    ''' <param name="x">自变量</param>
    ''' <param name="center">中心位置</param>
    ''' <param name="sigma">标准差</param>
    ''' <param name="height">峰高</param>
    ''' <returns>高斯函数值</returns>
    Public Function Gaussian(x As Double, center As Double, sigma As Double, height As Double) As Double
        Dim z As Double = (x - center) / sigma
        Return height * std.Exp(-0.5 * z * z)
    End Function

    ''' <summary>
    ''' 墨西哥帽小波（Ricker Wavelet）函数
    ''' 这是高斯函数的二阶导数，用于CentWave算法
    ''' </summary>
    ''' <param name="x">自变量</param>
    ''' <param name="center">中心位置</param>
    ''' <param name="sigma">尺度参数</param>
    ''' <returns>小波函数值</returns>
    Public Function MexicanHatWavelet(x As Double, center As Double,
                                             sigma As Double) As Double
        Dim z As Double = (x - center) / sigma
        Dim z2 As Double = z * z
        ' ψ(x) = (1 - x²) * exp(-x²/2) * 2/(√3 * σ * π^(1/4))
        ' 归一化系数可以省略，因为我们只关心相对值
        Return (1.0 - z2) * std.Exp(-0.5 * z2)
    End Function

    ''' <summary>
    ''' 移动平均平滑
    ''' </summary>
    ''' <param name="data">输入数据</param>
    ''' <param name="windowSize">窗口大小（必须为奇数）</param>
    ''' <returns>平滑后的数据</returns>
    Public Function MovingAverageSmooth(data As Double(), windowSize As Integer) As Double()
        If data Is Nothing OrElse data.Length = 0 Then Return New Double() {}
        If windowSize < 2 Then Return CType(data.Clone(), Double())
        If windowSize Mod 2 = 0 Then windowSize += 1 ' 确保窗口为奇数

        Dim result(data.Length - 1) As Double
        Dim halfW As Integer = windowSize \ 2

        For i As Integer = 0 To data.Length - 1
            Dim sum As Double = 0.0
            Dim count As Integer = 0
            For j As Integer = std.Max(0, i - halfW) To std.Min(data.Length - 1, i + halfW)
                sum += data(j)
                count += 1
            Next
            result(i) = sum / count
        Next

        Return result
    End Function

    ''' <summary>
    ''' Savitzky-Golay平滑（二次多项式，5点窗口的简化实现）
    ''' 相比移动平均，能更好地保留峰形特征
    ''' </summary>
    ''' <param name="data">输入数据</param>
    ''' <returns>平滑后的数据</returns>
    Public Function SavitzkyGolaySmooth(data As Double()) As Double()
        If data Is Nothing OrElse data.Length < 5 Then
            Return If(data Is Nothing, New Double() {}, CType(data.Clone(), Double()))
        End If

        ' 5点二次Savitzky-Golay卷积核
        ' 由最小二乘法拟合二次多项式推导得到
        Dim kernel As Double() = {-0.085714, 0.342857, 0.514286, 0.342857, -0.085714}
        Dim result(data.Length - 1) As Double

        For i As Integer = 0 To data.Length - 1
            Dim sum As Double = 0.0
            For k As Integer = -2 To 2
                Dim idx As Integer = std.Max(0, std.Min(data.Length - 1, i + k))
                sum += kernel(k + 2) * data(idx)
            Next
            result(i) = sum
        Next

        Return result
    End Function

    ''' <summary>
    ''' 一维卷积运算
    ''' </summary>
    ''' <param name="signal">输入信号</param>
    ''' <param name="kernel">卷积核</param>
    ''' <returns>卷积结果</returns>
    Public Function Convolve(signal As Double(), kernel As Double()) As Double()
        If signal Is Nothing OrElse kernel Is Nothing Then Return New Double() {}
        If signal.Length = 0 OrElse kernel.Length = 0 Then Return New Double() {}

        Dim result(signal.Length - 1) As Double
        Dim halfK As Integer = kernel.Length \ 2

        For i As Integer = 0 To signal.Length - 1
            Dim sum As Double = 0.0
            For k As Integer = 0 To kernel.Length - 1
                Dim idx As Integer = i - halfK + k
                If idx >= 0 AndAlso idx < signal.Length Then
                    sum += signal(idx) * kernel(k)
                End If
            Next
            result(i) = sum
        Next

        Return result
    End Function

    ''' <summary>
    ''' 计算一阶导数（中心差分法）
    ''' </summary>
    ''' <param name="data">输入数据</param>
    ''' <param name="dt">采样间隔</param>
    ''' <returns>一阶导数</returns>
    Public Function FirstDerivative(data As Double(), dt As Double) As Double()
        If data Is Nothing OrElse data.Length < 2 Then
            Return New Double() {}
        End If

        Dim deriv(data.Length - 1) As Double

        ' 中心差分（内部点）
        For i As Integer = 1 To data.Length - 2
            deriv(i) = (data(i + 1) - data(i - 1)) / (2.0 * dt)
        Next

        ' 前向差分（第一个点）
        deriv(0) = (data(1) - data(0)) / dt

        ' 后向差分（最后一个点）
        deriv(data.Length - 1) = (data(data.Length - 1) - data(data.Length - 2)) / dt

        Return deriv
    End Function

    ''' <summary>
    ''' 计算二阶导数（中心差分法）
    ''' </summary>
    Public Function SecondDerivative(data As Double(), dt As Double) As Double()
        If data Is Nothing OrElse data.Length < 3 Then
            Return New Double() {}
        End If

        Dim deriv(data.Length - 1) As Double
        Dim dt2 As Double = dt * dt

        ' 中心差分（内部点）
        For i As Integer = 1 To data.Length - 2
            deriv(i) = (data(i + 1) - 2.0 * data(i) + data(i - 1)) / dt2
        Next

        ' 边界处理
        deriv(0) = deriv(1)
        deriv(data.Length - 1) = deriv(data.Length - 2)

        Return deriv
    End Function

    ''' <summary>
    ''' 线性插值
    ''' </summary>
    Public Function LinearInterpolation(x0 As Double, y0 As Double,
                                                x1 As Double, y1 As Double,
                                                x As Double) As Double
        If std.Abs(x1 - x0) < Double.Epsilon Then
            Return (y0 + y1) / 2.0
        End If
        Return y0 + (y1 - y0) * (x - x0) / (x1 - x0)
    End Function

End Module