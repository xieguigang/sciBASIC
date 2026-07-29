' ============================================================================
' SignalProcessing.vb - 数字信号处理原语
'
' 提供 FFT、DCT、Mel 滤波器组、分帧、预加重、加窗、Delta 计算和统计量
' 计算等底层 DSP 功能，为声纹特征提取服务。
'
' 所有方法均为纯数学运算，不依赖任何文件 I/O。
' ============================================================================

Imports System.Math

''' <summary>
''' 信号处理工具模块，提供静态方法供声纹提取流水线调用。
''' </summary>
Public Module SignalProcessing

    ' ========================================================================
    ' 预加重滤波器
    ' ========================================================================

    ''' <summary>
    ''' 对信号应用一阶预加重高通滤波器：y[n] = x[n] - coeff * x[n-1]
    ''' 这提升了高频分量，使频谱更平坦，有利于后续处理。
    ''' </summary>
    ''' <param name="samples">输入信号（归一化到 [-1, 1]）。</param>
    ''' <param name="coeff">预加重系数，典型值 0.95 ~ 0.97。</param>
    ''' <returns>滤波后的信号。</returns>
    Public Function ApplyPreEmphasis(samples As Double(), coeff As Double) As Double()
        If samples Is Nothing OrElse samples.Length = 0 Then Return samples
        Dim output(samples.Length - 1) As Double
        output(0) = samples(0)
        For i As Integer = 1 To samples.Length - 1
            output(i) = samples(i) - coeff * samples(i - 1)
        Next
        Return output
    End Function

    ' ========================================================================
    ' 分帧
    ' ========================================================================

    ''' <summary>
    ''' 将信号分割为重叠的帧。
    ''' </summary>
    ''' <param name="samples">输入信号。</param>
    ''' <param name="frameSize">每帧的采样点数。</param>
    ''' <param name="hopSize">帧移（相邻帧起始位置间隔的采样点数）。</param>
    ''' <returns>帧列表，每帧是一个 Double 数组。不足一帧的尾部用零填充。</returns>
    Public Function FrameSignal(samples As Double(), frameSize As Integer, hopSize As Integer) As List(Of Double())
        Dim frames As New List(Of Double())
        If samples Is Nothing OrElse samples.Length = 0 Then Return frames
        If frameSize <= 0 Then frameSize = samples.Length
        If hopSize <= 0 Then hopSize = frameSize

        ' 信号短于一帧：补零到 frameSize，返回单帧
        If samples.Length <= frameSize Then
            Dim frame(frameSize - 1) As Double
            Array.Copy(samples, frame, samples.Length)
            frames.Add(frame)
            Return frames
        End If

        Dim numFrames As Integer = (samples.Length - frameSize) \ hopSize + 1

        For i As Integer = 0 To numFrames - 1
            Dim start As Integer = i * hopSize
            Dim frame(frameSize - 1) As Double
            Array.Copy(samples, start, frame, 0, frameSize)
            frames.Add(frame)
        Next

        ' 处理尾部剩余样本（补零到一帧）
        Dim lastStart As Integer = (numFrames - 1) * hopSize + frameSize
        If lastStart < samples.Length Then
            Dim frame(frameSize - 1) As Double
            Dim remaining As Integer = samples.Length - lastStart
            Array.Copy(samples, lastStart, frame, 0, remaining)
            frames.Add(frame)
        End If

        Return frames
    End Function

    ' ========================================================================
    ' 窗函数
    ' ========================================================================

    ''' <summary>
    ''' 生成 Hamming 窗：w[n] = 0.54 - 0.46 * cos(2πn / (N-1))
    ''' </summary>
    Public Function CreateHammingWindow(size As Integer) As Double()
        Dim window(size - 1) As Double
        If size = 1 Then
            window(0) = 1.0
            Return window
        End If
        For i As Integer = 0 To size - 1
            window(i) = 0.54 - 0.46 * Cos(2.0 * PI * i / (size - 1))
        Next
        Return window
    End Function

    ' ========================================================================
    ' FFT（Radix-2 Cooley-Tukey 迭代实现）
    ' ========================================================================

    ''' <summary>
    ''' 就地执行 Radix-2 Cooley-Tukey FFT。
    ''' 输入长度必须为 2 的幂。
    ''' </summary>
    ''' <param name="real">实部数组（输入/输出）。</param>
    ''' <param name="imag">虚部数组（输入/输出）。</param>
    Public Sub FFT(real As Double(), imag As Double())
        Dim n As Integer = real.Length
        If n <= 1 Then Return
        If (n And (n - 1)) <> 0 Then
            Throw New ArgumentException("FFT size must be a power of 2, got " & n)
        End If

        ' --- 位反转排列 ---
        Dim j As Integer = 0
        For i As Integer = 0 To n - 2
            If i < j Then
                Dim tr As Double = real(i) : real(i) = real(j) : real(j) = tr
                Dim ti As Double = imag(i) : imag(i) = imag(j) : imag(j) = ti
            End If
            Dim m As Integer = n >> 1
            Do While (j And m) <> 0
                j = j Xor m
                m = m >> 1
            Loop
            j = j Or m
        Next

        ' --- Cooley-Tukey 蝶形运算 ---
        Dim size As Integer = 2
        Do While size <= n
            Dim halfSize As Integer = size >> 1
            Dim tableStep As Double = 2.0 * PI / size

            For i As Integer = 0 To halfSize - 1
                Dim angle As Double = -i * tableStep
                Dim wr As Double = Cos(angle)
                Dim wi As Double = Sin(angle)

                Dim j2 As Integer = i
                Do While j2 < n
                    Dim k As Integer = j2 + halfSize
                    Dim tr As Double = wr * real(k) - wi * imag(k)
                    Dim ti As Double = wr * imag(k) + wi * real(k)
                    real(k) = real(j2) - tr
                    imag(k) = imag(j2) - ti
                    real(j2) = real(j2) + tr
                    imag(j2) = imag(j2) + ti
                    j2 += size
                Loop
            Next

            size *= 2
        Loop
    End Sub

    ' ========================================================================
    ' 功率谱
    ' ========================================================================

    ''' <summary>
    ''' 从 FFT 结果计算功率谱：P[k] = Re[k]^2 + Im[k]^2
    ''' </summary>
    ''' <param name="real">FFT 实部。</param>
    ''' <param name="imag">FFT 虚部。</param>
    ''' <param name="numBins">输出的频带数（通常为 fftSize/2 + 1）。</param>
    Public Function PowerSpectrum(real As Double(), imag As Double(), numBins As Integer) As Double()
        Dim psd(numBins - 1) As Double
        For i As Integer = 0 To numBins - 1
            psd(i) = real(i) * real(i) + imag(i) * imag(i)
        Next
        Return psd
    End Function

    ' ========================================================================
    ' DCT-II（离散余弦变换）
    ' ========================================================================

    ''' <summary>
    ''' 计算 DCT-II，用于从 Mel 对数能量导出 MFCC。
    ''' 使用正交归一化系数：
    '''   k=0: sqrt(1/N)
    '''   k>0: sqrt(2/N)
    ''' </summary>
    ''' <param name="input">输入数据（如 Mel 对数能量）。</param>
    ''' <param name="numCoeffs">输出的系数个数。</param>
    Public Function DCT(input As Double(), numCoeffs As Integer) As Double()
        Dim n As Integer = input.Length
        Dim output(numCoeffs - 1) As Double

        For k As Integer = 0 To numCoeffs - 1
            Dim sumVal As Double = 0.0
            For i As Integer = 0 To n - 1
                sumVal += input(i) * Cos(PI / n * (i + 0.5) * k)
            Next
            If k = 0 Then
                output(k) = sumVal * Sqrt(1.0 / n)
            Else
                output(k) = sumVal * Sqrt(2.0 / n)
            End If
        Next
        Return output
    End Function

    ' ========================================================================
    ' Delta（差分）计算
    ' ========================================================================

    ''' <summary>
    ''' 计算特征序列的 Delta（一阶差分）系数。
    ''' 公式：delta[t] = Σ_{n=1}^{N} n * (feat[t+n] - feat[t-n]) / (2 * Σ n²)
    ''' 边界处使用复制填充。
    ''' </summary>
    ''' <param name="features">特征矩阵 [frame][coeff]。</param>
    ''' <param name="N">差分窗口半宽，典型值 2。</param>
    Public Function ComputeDelta(features As Double()(), N As Integer) As Double()()
        Dim numFrames As Integer = features.Length
        If numFrames = 0 Then Return CType(Array.CreateInstance(GetType(Double()), 0), Double()())
        Dim numCoeffs As Integer = features(0).Length
        Dim delta(numFrames - 1)() As Double

        ' 分母：2 * Σ_{n=1}^{N} n²
        Dim denom As Double = 0.0
        For n As Integer = 1 To N
            denom += 2.0 * n * n
        Next

        For t As Integer = 0 To numFrames - 1
            delta(t) = New Double(numCoeffs - 1) {}
            For k As Integer = 0 To numCoeffs - 1
                Dim sumVal As Double = 0.0
                For n As Integer = 1 To N
                    Dim idxPlus As Integer = Min(t + n, numFrames - 1)
                    Dim idxMinus As Integer = Max(t - n, 0)
                    sumVal += n * (features(idxPlus)(k) - features(idxMinus)(k))
                Next
                delta(t)(k) = sumVal / denom
            Next
        Next
        Return delta
    End Function

    ' ========================================================================
    ' 统计量计算
    ' ========================================================================

    ''' <summary>
    ''' 计算数据的均值、标准差、偏度和峰度（超额峰度 = 峰度 - 3）。
    ''' </summary>
    Public Sub ComputeStatistics(data As Double(), ByRef mean As Double, ByRef std As Double,
                                  ByRef skewness As Double, ByRef kurtosis As Double)
        Dim n As Integer = data.Length
        If n = 0 Then
            mean = 0.0 : std = 0.0 : skewness = 0.0 : kurtosis = 0.0
            Return
        End If

        ' 均值
        Dim sumVal As Double = 0.0
        For i As Integer = 0 To n - 1
            sumVal += data(i)
        Next
        mean = sumVal / n

        ' 方差
        Dim sumSq As Double = 0.0
        For i As Integer = 0 To n - 1
            Dim diff As Double = data(i) - mean
            sumSq += diff * diff
        Next
        Dim variance As Double = sumSq / n
        std = Sqrt(variance)

        If std < 1.0E-10 Then
            skewness = 0.0
            kurtosis = 0.0
            Return
        End If

        ' 偏度和峰度（标准化三阶矩和四阶矩）
        Dim sumCube As Double = 0.0
        Dim sumQuad As Double = 0.0
        For i As Integer = 0 To n - 1
            Dim normDiff As Double = (data(i) - mean) / std
            Dim sq As Double = normDiff * normDiff
            sumCube += sq * normDiff
            sumQuad += sq * sq
        Next
        skewness = sumCube / n
        kurtosis = sumQuad / n - 3.0  ' 超额峰度
    End Sub

    ' ========================================================================
    ' 辅助方法
    ' ========================================================================

    ''' <summary>
    ''' 返回不小于 n 的最小 2 的幂。
    ''' </summary>
    Public Function NextPowerOf2(n As Integer) As Integer
        Dim p As Integer = 1
        Do While p < n
            p = p << 1
        Loop
        Return p
    End Function

End Module


' ============================================================================
' MelFilterbank - Mel 尺度三角滤波器组
' ============================================================================

''' <summary>
''' Mel 尺度三角滤波器组，用于将功率谱转换为 Mel 频率能量。
'''
''' Mel 尺度模拟人耳对频率的感知：低频分辨率高，高频分辨率低。
''' 公式：mel = 2595 * log10(1 + hz/700)
''' </summary>
Public Class MelFilterbank

    Private ReadOnly _filters As Double()()
    Private ReadOnly _numFilters As Integer
    Private ReadOnly _fftSize As Integer

    ''' <summary>
    ''' 创建 Mel 滤波器组。
    ''' </summary>
    ''' <param name="numFilters">滤波器数量，典型值 20~40。</param>
    ''' <param name="fftSize">FFT 大小（必须为 2 的幂）。</param>
    ''' <param name="sampleRate">采样率（Hz）。</param>
    ''' <param name="lowFreq">最低频率（Hz），默认 0。</param>
    ''' <param name="highFreq">最高频率（Hz），默认为奈奎斯特频率。</param>
    Public Sub New(numFilters As Integer, fftSize As Integer, sampleRate As Integer,
                   Optional lowFreq As Integer = 0, Optional highFreq As Integer = -1)

        If highFreq < 0 Then highFreq = sampleRate \ 2
        _numFilters = numFilters
        _fftSize = fftSize

        ' 在 Mel 尺度上均匀分布 numFilters+2 个点
        Dim lowMel As Double = HzToMel(lowFreq)
        Dim highMel As Double = HzToMel(highFreq)

        Dim melPoints(numFilters + 1) As Double
        Dim hzPoints(numFilters + 1) As Double
        Dim binPoints(numFilters + 1) As Integer

        For i As Integer = 0 To numFilters + 1
            melPoints(i) = lowMel + (highMel - lowMel) * i / (numFilters + 1)
            hzPoints(i) = MelToHz(melPoints(i))
            binPoints(i) = CInt(Math.Floor((fftSize + 1) * hzPoints(i) / sampleRate))
        Next

        Dim numBins As Integer = fftSize \ 2 + 1
        _filters = New Double(numFilters - 1)() {}

        ' 为每个滤波器创建三角窗
        For f As Integer = 0 To numFilters - 1
            _filters(f) = New Double(numBins - 1) {}
            Dim leftBin As Integer = binPoints(f)
            Dim centerBin As Integer = binPoints(f + 1)
            Dim rightBin As Integer = binPoints(f + 2)

            For k As Integer = leftBin To rightBin
                If k < 0 OrElse k >= numBins Then Continue For

                If k <= centerBin Then
                    ' 上升沿
                    Dim span As Integer = Math.Max(1, centerBin - leftBin)
                    _filters(f)(k) = CDbl(k - leftBin) / span
                Else
                    ' 下降沿
                    Dim span As Integer = Math.Max(1, rightBin - centerBin)
                    _filters(f)(k) = CDbl(rightBin - k) / span
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 将功率谱通过 Mel 滤波器组，得到每个滤波器的能量值。
    ''' </summary>
    Public Function Apply(powerSpectrum As Double()) As Double()
        Dim output(_numFilters - 1) As Double
        For f As Integer = 0 To _numFilters - 1
            Dim sumVal As Double = 0.0
            Dim len As Integer = Math.Min(powerSpectrum.Length, _filters(f).Length)
            For k As Integer = 0 To len - 1
                sumVal += powerSpectrum(k) * _filters(f)(k)
            Next
            output(f) = sumVal
        Next
        Return output
    End Function

    ''' <summary>Hz → Mel 尺度转换。</summary>
    Public Shared Function HzToMel(hz As Double) As Double
        Return 2595.0 * Math.Log10(1.0 + hz / 700.0)
    End Function

    ''' <summary>Mel → Hz 尺度转换。</summary>
    Public Shared Function MelToHz(mel As Double) As Double
        Return 700.0 * (Math.Pow(10.0, mel / 2595.0) - 1.0)
    End Function

End Class
