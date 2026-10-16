#Region "Microsoft.VisualBasic::68900ee6c1238930d999e5dc0afe2137, Data_science\Mathematica\SignalProcessing\SignalProcessing\MathUtils.vb"

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

    '   Total Lines: 182
    '    Code Lines: 96 (52.75%)
    ' Comment Lines: 58 (31.87%)
    '    - Xml Docs: 84.48%
    ' 
    '   Blank Lines: 28 (15.38%)
    '     File Size: 6.69 KB


    ' Module MathUtils
    ' 
    '     Function: AutoCorrelation, BlockPower, Convolve, Decimate, Envelope, FirstDerivative
    '               Gaussian, LerpClamped, LinearInterpolation, Mean, Median, MexicanHatWavelet
    '               MovingAverage, MovingAverageSmooth, Percentile, Quantile, Rms
    '               SampleOrZero, SavitzkyGolaySmooth, SecondDerivative
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
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

        ' the legacy contract of this function is the odd window size, the moving
        ' average of an odd window is mathematically identical to the original inline
        ' implementation, so the computation is forwarded to the O(n) linear time
        ' implementation: <see cref="MovingAverage"/>.
        '
        ' NOTE: the forwarding changes the summation order (running sum instead of a
        ' fresh reduction on each position), so the result may differ in the last few
        ' bits of the floating point mantissa.
        Return MovingAverage(data, windowSize)
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

#Region "signal filters and statistics"

    ''' <summary>
    ''' centered moving average with a edge clamping.
    ''' 
    ''' the window radius is ``Max(1, window \ 2)``, so an even window size is also
    ''' supported: ``MovingAverage(x, 128)`` uses the radius 64.
    ''' 
    ''' ### the difference from <see cref="MovingAverageSmooth"/>
    ''' 
    ''' ``MovingAverageSmooth`` is the legacy entry which forces the window size into an
    ''' odd number (an even input is increased by one), this function uses the window
    ''' size as it is. both of them share the same centered window and the same edge
    ''' clamping, therefore for an odd window size the results are mathematically equal.
    ''' </summary>
    ''' <param name="x">the input signal</param>
    ''' <param name="window">the window size in samples</param>
    Public Function MovingAverage(x As Double(), window As Integer) As Double()
        Dim n As Integer = x.Length
        Dim result As Double() = New Double(n - 1) {}
        Dim half As Integer = std.Max(1, window \ 2)
        Dim sum As Double = 0
        Dim count As Integer = 0

        ' running sum initialization
        For i As Integer = 0 To std.Min(half, n - 1)
            sum += x(i)
            count += 1
        Next

        For i As Integer = 0 To n - 1
            result(i) = sum / count

            ' advance the window
            Dim drop As Integer = i - half
            Dim add As Integer = i + half + 1

            If drop >= 0 Then
                sum -= x(drop)
                count -= 1
            End If

            If add < n Then
                sum += x(add)
                count += 1
            End If
        Next

        Return result
    End Function

    ''' <summary>
    ''' rectify the signal and then smooth it by a moving average window,
    ''' this is the amplitude envelope of the given analog signal.
    ''' </summary>
    ''' <param name="x">the input signal</param>
    ''' <param name="window">the window size of the low pass filter</param>
    Public Function Envelope(x As Single(), Optional window As Integer = 64) As Double()
        Dim n As Integer = x.Length
        Dim rect As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            rect(i) = std.Abs(x(i))
        Next

        Return MovingAverage(rect, window)
    End Function

    ''' <summary>
    ''' decimate the signal by the factor ``k`` using a simple averaging low pass filter
    ''' </summary>
    ''' <param name="x">the input signal</param>
    ''' <param name="k">the decimation factor</param>
    Public Function Decimate(x As Double(), k As Integer) As Double()
        Dim n As Integer = x.Length \ k
        Dim result As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim sum As Double = 0

            For j As Integer = 0 To k - 1
                sum += x(i * k + j)
            Next

            result(i) = sum / k
        Next

        Return result
    End Function

    ''' <summary>
    ''' normalized auto correlation of the given signal on the lag range ``[minLag, maxLag]``
    ''' </summary>
    ''' <param name="x">the input signal, should be already decimated by the caller</param>
    ''' <param name="minLag">the minimal lag in samples</param>
    ''' <param name="maxLag">the maximal lag in samples</param>
    ''' <param name="maxCount">the maximal number of the samples which are used in the correlation</param>
    Public Function AutoCorrelation(x As Double(), minLag As Integer, maxLag As Integer,
                                    Optional maxCount As Integer = 65536) As Double()

        Dim n As Integer = std.Min(x.Length, maxCount)
        Dim mean As Double = 0

        For i As Integer = 0 To n - 1
            mean += x(i)
        Next

        mean /= n

        Dim variance As Double = 0

        For i As Integer = 0 To n - 1
            Dim d As Double = x(i) - mean

            variance += d * d
        Next

        Dim result As Double() = New Double(maxLag - minLag) {}

        For lag As Integer = minLag To maxLag
            Dim sum As Double = 0

            For i As Integer = 0 To n - lag - 1
                sum += (x(i) - mean) * (x(i + lag) - mean)
            Next

            result(lag - minLag) = If(variance = 0, 0, sum / variance)
        Next

        Return result
    End Function

    ''' <summary>
    ''' the value at the given percentile position ``p`` in range ``[0,1]``,
    ''' the input data does not need to be sorted.
    ''' 
    ''' this is the exact percentile by the sorted interpolation (the "linear" type of
    ''' the R quantile), please note that it is **not** the same thing as the GK
    ''' estimation of ``Microsoft.VisualBasic.Math.Quantile`` which is an approximate
    ''' query on a data stream. the exact percentile must buffer and sort the whole
    ''' sample data.
    ''' </summary>
    ''' <param name="data">the sample data, it is not modified</param>
    ''' <param name="p">the percentile position in range ``[0,1]``</param>
    Public Function Percentile(data As Double(), p As Double) As Double
        If data.Length = 0 Then
            Return Double.NaN
        End If

        Dim sorted As Double() = data.ToArray
        Call Array.Sort(sorted)

        Return Quantile(sorted, p)
    End Function

    ''' <summary>
    ''' the exact percentile of a single precision signal, it is the overload of
    ''' <see cref="Percentile(Double(), Double)"/> for the sample buffers which are
    ''' loaded from the wave files.
    ''' </summary>
    ''' <param name="data">the sample data, it is not modified</param>
    ''' <param name="p">the percentile position in range ``[0,1]``</param>
    Public Function Percentile(data As Single(), p As Double) As Double
        If data.Length = 0 Then
            Return Double.NaN
        End If

        Dim sorted As Double() = New Double(data.Length - 1) {}

        For i As Integer = 0 To data.Length - 1
            sorted(i) = data(i)
        Next

        Call Array.Sort(sorted)

        Return Quantile(sorted, p)
    End Function

    ''' <summary>
    ''' the quantile value on an already sorted data array, ``p`` in range ``[0,1]``
    ''' </summary>
    ''' <param name="sorted">the ascending sorted sample data</param>
    ''' <param name="p">the quantile position in range ``[0,1]``</param>
    Public Function Quantile(sorted As Double(), p As Double) As Double
        If sorted.Length = 0 Then
            Return Double.NaN
        End If

        If p <= 0 Then
            Return sorted(Scan0)
        ElseIf p >= 1 Then
            Return sorted(sorted.Length - 1)
        End If

        Dim pos As Double = p * (sorted.Length - 1)
        Dim i As Integer = CInt(std.Floor(pos))
        Dim f As Double = pos - i

        If i >= sorted.Length - 1 Then
            Return sorted(sorted.Length - 1)
        End If

        Return sorted(i) * (1 - f) + sorted(i + 1) * f
    End Function

    ''' <summary>
    ''' the median value of the given signal, it is the percentile at the position 0.5.
    ''' 
    ''' ### the difference from the linq extension ``Median``
    ''' 
    ''' the core library provides the linq extension ``Median`` for ``IEnumerable(Of Double)``,
    ''' this function is a plain module function and it is **not** an extension method, so
    ''' both of them can be imported at the same place without the overload ambiguity.
    ''' the interpolation rule of an even length sample is also different.
    ''' </summary>
    ''' <param name="x">the sample data, it is not modified</param>
    Public Function Median(x As Double()) As Double
        Return Percentile(x, 0.5)
    End Function

    ''' <summary>
    ''' the arithmetic mean of the given signal
    ''' </summary>
    ''' <param name="x">the sample data</param>
    Public Function Mean(x As Single()) As Double
        If x.Length = 0 Then
            Return Double.NaN
        End If

        Dim sum As Double = 0

        For i As Integer = 0 To x.Length - 1
            sum += x(i)
        Next

        Return sum / x.Length
    End Function

    ''' <summary>
    ''' the arithmetic mean of a double precision signal, it is the overload of
    ''' <see cref="Mean(Single())"/> for the computed intermediate data.
    ''' </summary>
    ''' <param name="x">the sample data</param>
    Public Function Mean(x As Double()) As Double
        If x.Length = 0 Then
            Return Double.NaN
        End If

        Dim sum As Double = 0

        For i As Integer = 0 To x.Length - 1
            sum += x(i)
        Next

        Return sum / x.Length
    End Function

    ''' <summary>
    ''' the root mean square of the given signal
    ''' </summary>
    ''' <param name="x">the sample data</param>
    Public Function Rms(x As Single()) As Double
        If x.Length = 0 Then
            Return Double.NaN
        End If

        Dim sum As Double = 0

        For i As Integer = 0 To x.Length - 1
            sum += x(i) * x(i)
        Next

        Return std.Sqrt(sum / x.Length)
    End Function

    ''' <summary>
    ''' the root mean square of a data block, a robust intensity measurement of the signal
    ''' </summary>
    ''' <param name="x">the sample data</param>
    ''' <param name="start">the zero based start position of the block</param>
    ''' <param name="end">the zero based end position (exclusive) of the block</param>
    Public Function BlockPower(x As Single(), start As Integer, [end] As Integer) As Double
        [end] = std.Min([end], x.Length)
        start = std.Max(0, start)

        Dim sum As Double = 0
        Dim n As Integer = [end] - start

        If n <= 0 Then
            Return 0
        End If

        For i As Integer = start To [end] - 1
            sum += x(i) * x(i)
        Next

        Return std.Sqrt(sum / n)
    End Function

#End Region

#Region "signal sampling"

    ''' <summary>
    ''' linear interpolation sample of a signal at a fractional position, the position
    ''' is clamped into the signal range.
    ''' 
    ''' this is the float position sampler which is used by the scan line alignment and
    ''' the raster folding. do not confuse it with <see cref="SampleOrZero"/> which takes
    ''' an integer index and returns zero outside of the signal range.
    ''' </summary>
    ''' <param name="x">the sample data</param>
    ''' <param name="pos">the fractional sample position</param>
    Public Function LerpClamped(x As Single(), pos As Double) As Double
        If pos <= 0 Then
            Return x(Scan0)
        End If

        If pos >= x.Length - 1 Then
            Return x(x.Length - 1)
        End If

        Dim i As Integer = CInt(std.Floor(pos))
        Dim f As Double = pos - i

        Return x(i) * (1 - f) + x(i + 1) * f
    End Function

    ''' <summary>
    ''' the sample value at the given integer index, zero is returned when the index is
    ''' outside of the signal range.
    ''' 
    ''' do not confuse it with <see cref="LerpClamped"/> which takes a fractional position
    ''' and clamps the position into the signal range instead of returning zero.
    ''' </summary>
    ''' <param name="data">the sample data</param>
    ''' <param name="i">the integer sample index</param>
    Public Function SampleOrZero(data As Single(), i As Integer) As Double
        If i < 0 OrElse i >= data.Length Then
            Return 0
        End If

        Return data(i)
    End Function

#End Region

End Module
