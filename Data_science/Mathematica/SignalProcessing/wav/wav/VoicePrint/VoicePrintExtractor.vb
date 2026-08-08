#Region "Microsoft.VisualBasic::7f2917afee003edc3555b344b08df4bf, Data_science\Mathematica\SignalProcessing\wav\wav\VoicePrint\VoicePrintExtractor.vb"

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

    '   Total Lines: 399
    '    Code Lines: 206 (51.63%)
    ' Comment Lines: 120 (30.08%)
    '    - Xml Docs: 47.50%
    ' 
    '   Blank Lines: 73 (18.30%)
    '     File Size: 14.43 KB


    ' Class VoicePrintOptions
    ' 
    '     Properties: ApplyCMVN, DeltaN, FrameHopMs, FrameSizeMs, IncludeC0
    '                 L2Normalize, NumMelFilters, PreEmphasisCoeff, TargetDimension, UseDelta
    '                 UseDeltaDelta
    ' 
    ' Class VoicePrintResult
    ' 
    '     Properties: DurationSec, NumFrames, NumMfcc, SampleRate, Vector
    ' 
    '     Function: ToString
    ' 
    ' Class VoicePrintExtractor
    ' 
    '     Function: AdjustDimension, CalculateNumMfcc, ComputeVoiceprintVector, Extract, ExtractDetailed
    ' 
    '     Sub: AppendStatistics, ApplyCMVN, L2NormalizeVector
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

' ============================================================================
' VoicePrintExtractor.vb - 声纹向量提取器
'
' 从 PCM 音频样本中提取固定维度的声纹向量。
'
' 提取流水线：
'   1. 预加重滤波（高通，提升高频分量）
'   2. 分帧（25ms 帧，10ms 帧移）
'   3. 逐帧处理：
'      a. Hamming 加窗
'      b. FFT → 功率谱
'      c. Mel 滤波器组 → Mel 能量
'      d. 对数压缩
'      e. DCT → MFCC 系数
'   4. CMVN（倒谱均值方差归一化，可选）
'   5. Delta / Delta-Delta 差分系数
'   6. 统计聚合：每个系数在所有帧上的 mean / std / skewness / kurtosis
'   7. L2 归一化
'   8. 维度调整（截断或补零到目标维度）
'
' 维度计算：
'   n_mfcc = ceil(targetDim / (nOrders × nStats))
'   nOrders = 1 + (UseDelta ? 1 : 0) + (UseDeltaDelta ? 1 : 0)
'   nStats = 4 (mean, std, skew, kurt)
'
'   例：targetDim=192, 3 orders, 4 stats → n_mfcc = 192/12 = 16
' ============================================================================

''' <summary>
''' 声纹提取参数配置。
''' </summary>
Public Class VoicePrintOptions

    ''' <summary>目标向量维度，默认 192。</summary>
    Public Property TargetDimension As Integer = 192

    ''' <summary>帧长（毫秒），标准值 25ms。</summary>
    Public Property FrameSizeMs As Double = 25.0

    ''' <summary>帧移（毫秒），标准值 10ms。</summary>
    Public Property FrameHopMs As Double = 10.0

    ''' <summary>预加重系数，典型值 0.95~0.97。</summary>
    Public Property PreEmphasisCoeff As Double = 0.97

    ''' <summary>Mel 滤波器数量，典型值 20~40。</summary>
    Public Property NumMelFilters As Integer = 26

    ''' <summary>是否计算 Delta（一阶差分）系数。</summary>
    Public Property UseDelta As Boolean = True

    ''' <summary>是否计算 Delta-Delta（二阶差分）系数。</summary>
    Public Property UseDeltaDelta As Boolean = True

    ''' <summary>是否包含 MFCC 的 c0 系数（第 0 维，代表帧能量）。</summary>
    Public Property IncludeC0 As Boolean = False

    ''' <summary>是否应用 CMVN（倒谱均值方差归一化）。</summary>
    Public Property ApplyCMVN As Boolean = True

    ''' <summary>是否对最终向量做 L2 归一化。</summary>
    Public Property L2Normalize As Boolean = True

    ''' <summary>Delta 计算的窗口半宽，典型值 2。</summary>
    Public Property DeltaN As Integer = 2

End Class


''' <summary>
''' 声纹提取结果（包含向量和元数据）。
''' </summary>
Public Class VoicePrintResult

    ''' <summary>声纹向量。</summary>
    Public Property Vector As Double()

    ''' <summary>处理的帧数。</summary>
    Public Property NumFrames As Integer

    ''' <summary>每帧的 MFCC 系数个数。</summary>
    Public Property NumMfcc As Integer

    ''' <summary>采样率（Hz）。</summary>
    Public Property SampleRate As Integer

    ''' <summary>音频时长（秒）。</summary>
    Public Property DurationSec As Double

    Public Overrides Function ToString() As String
        Return $"VoicePrint(dim={Vector.Length}, frames={NumFrames}, mfcc={NumMfcc}, sr={SampleRate}, dur={DurationSec:F2}s)"
    End Function

End Class


''' <summary>
''' 声纹向量提取器。
''' 纯 DSP 实现，不依赖任何文件 I/O。
''' 输入为归一化的音频样本数组，输出为固定维度的声纹向量。
''' </summary>
Public Class VoicePrintExtractor

    ''' <summary>
    ''' 从音频样本中提取声纹向量。
    ''' </summary>
    ''' <param name="samples">归一化到 [-1.0, 1.0] 的音频样本。</param>
    ''' <param name="sampleRate">采样率（Hz）。</param>
    ''' <param name="options">提取参数，为 Nothing 时使用默认值。</param>
    ''' <returns>目标维度的声纹向量。</returns>
    Public Function Extract(samples As Single(), sampleRate As Integer,
                            Optional options As VoicePrintOptions = Nothing) As Double()
        Return ExtractDetailed(samples, sampleRate, options).Vector
    End Function

    ''' <summary>
    ''' 从音频样本中提取声纹向量，返回包含元数据的详细结果。
    ''' </summary>
    Public Function ExtractDetailed(samples As Single(), sampleRate As Integer,
                                    Optional options As VoicePrintOptions = Nothing) As VoicePrintResult
        If options Is Nothing Then options = New VoicePrintOptions()

        Dim result As New VoicePrintResult With {
            .SampleRate = sampleRate,
            .DurationSec = If(samples IsNot Nothing, samples.Length / CDbl(sampleRate), 0.0)
        }

        ' 空输入：返回零向量
        If samples Is Nothing OrElse samples.Length = 0 Then
            result.Vector = New Double(options.TargetDimension - 1) {}
            result.NumFrames = 0
            result.NumMfcc = 0
            Return result
        End If

        ' --- Single → Double 转换 ---
        Dim signal(samples.Length - 1) As Double
        For i As Integer = 0 To samples.Length - 1
            signal(i) = CDbl(samples(i))
        Next

        ' --- Step 1: 预加重 ---
        signal = SignalProcessing.ApplyPreEmphasis(signal, options.PreEmphasisCoeff)

        ' --- Step 2: 分帧 ---
        Dim frameSize As Integer = CInt(options.FrameSizeMs * sampleRate / 1000.0)
        Dim hopSize As Integer = CInt(options.FrameHopMs * sampleRate / 1000.0)
        If frameSize <= 0 Then frameSize = 1
        If hopSize <= 0 Then hopSize = frameSize

        Dim frames As List(Of Double()) = SignalProcessing.FrameSignal(signal, frameSize, hopSize)
        result.NumFrames = frames.Count

        If frames.Count = 0 Then
            result.Vector = New Double(options.TargetDimension - 1) {}
            result.NumMfcc = 0
            Return result
        End If

        ' --- FFT 大小（>= frameSize 的最小 2 的幂）---
        Dim fftSize As Integer = SignalProcessing.NextPowerOf2(frameSize)
        Dim numBins As Integer = fftSize \ 2 + 1

        ' --- Mel 滤波器组 ---
        Dim melBank As New MelFilterbank(options.NumMelFilters, fftSize, sampleRate)

        ' --- 计算每帧的 MFCC 系数个数 ---
        Dim numMfcc As Integer = CalculateNumMfcc(options)
        result.NumMfcc = numMfcc

        ' --- Step 3: 逐帧 MFCC 提取 ---
        Dim window As Double() = SignalProcessing.CreateHammingWindow(frameSize)
        Dim mfccList As New List(Of Double())(frames.Count)

        ' DCT 输出个数：如果不含 c0 则多取一个然后丢弃 c0
        Dim dctCount As Integer = numMfcc + If(options.IncludeC0, 0, 1)

        For Each frame As Double() In frames
            ' Hamming 加窗
            Dim windowed(frameSize - 1) As Double
            For i As Integer = 0 To frameSize - 1
                windowed(i) = frame(i) * window(i)
            Next

            ' FFT（补零到 fftSize）
            Dim real(fftSize - 1) As Double
            Dim imag(fftSize - 1) As Double
            Array.Copy(windowed, real, frameSize)
            SignalProcessing.FFT(real, imag)

            ' 功率谱
            Dim psd As Double() = SignalProcessing.PowerSpectrum(real, imag, numBins)

            ' Mel 滤波器组
            Dim melEnergies As Double() = melBank.Apply(psd)

            ' 对数压缩
            For i As Integer = 0 To melEnergies.Length - 1
                melEnergies(i) = std.Log(melEnergies(i) + 0.0000000001)
            Next

            ' DCT → MFCC
            Dim mfcc As Double() = SignalProcessing.DCT(melEnergies, dctCount)

            ' 去除 c0（如果配置）
            If Not options.IncludeC0 Then
                Dim mfccTrimmed(numMfcc - 1) As Double
                Array.Copy(mfcc, 1, mfccTrimmed, 0, numMfcc)
                mfccList.Add(mfccTrimmed)
            Else
                mfccList.Add(mfcc)
            End If
        Next

        ' 转为数组
        Dim mfccArray As Double()() = mfccList.ToArray()

        ' --- Step 4: CMVN ---
        If options.ApplyCMVN Then
            ApplyCMVN(mfccArray)
        End If

        ' --- Step 5: Delta / Delta-Delta ---
        Dim deltaArray As Double()() = Nothing
        Dim deltaDeltaArray As Double()() = Nothing

        If options.UseDelta Then
            deltaArray = SignalProcessing.ComputeDelta(mfccArray, options.DeltaN)
        End If

        If options.UseDeltaDelta Then
            Dim baseForDelta As Double()() = If(deltaArray, mfccArray)
            deltaDeltaArray = SignalProcessing.ComputeDelta(baseForDelta, options.DeltaN)
        End If

        ' --- Step 6: 统计聚合 ---
        Dim vector As Double() = ComputeVoiceprintVector(
            mfccArray, deltaArray, deltaDeltaArray, numMfcc, options)

        ' --- Step 7: L2 归一化 ---
        If options.L2Normalize Then
            L2NormalizeVector(vector)
        End If

        ' --- Step 8: 维度调整 ---
        result.Vector = AdjustDimension(vector, options.TargetDimension)
        Return result
    End Function

    ' ========================================================================
    ' 内部方法
    ' ========================================================================

    ''' <summary>
    ''' 根据目标维度和配置计算需要的 MFCC 系数个数。
    ''' nMfcc = ceil(targetDim / (nOrders × nStats))
    ''' </summary>
    Private Function CalculateNumMfcc(options As VoicePrintOptions) As Integer
        Dim nOrders As Integer = 1 + (If(options.UseDelta, 1, 0)) + (If(options.UseDeltaDelta, 1, 0))
        Dim nStats As Integer = 4  ' mean, std, skew, kurt
        Dim nMfcc As Integer = CInt(std.Ceiling(options.TargetDimension / CDbl(nOrders * nStats)))
        Return std.Max(1, nMfcc)
    End Function

    ''' <summary>
    ''' 将 MFCC、Delta、Delta-Delta 的统计量拼接为声纹向量。
    ''' 每个系数贡献 4 个统计量（mean, std, skew, kurt）。
    ''' </summary>
    Private Function ComputeVoiceprintVector(
        mfcc As Double()(),
        delta As Double()(),
        deltaDelta As Double()(),
        numMfcc As Integer,
        options As VoicePrintOptions) As Double()

        Dim nOrders As Integer = 1 +
            (If(delta IsNot Nothing, 1, 0)) +
            (If(deltaDelta IsNot Nothing, 1, 0))
        Dim nStats As Integer = 4
        Dim totalDim As Integer = numMfcc * nOrders * nStats
        Dim vector(totalDim - 1) As Double
        Dim offset As Integer = 0

        ' 静态 MFCC 统计
        AppendStatistics(mfcc, numMfcc, vector, offset)
        offset += numMfcc * nStats

        ' Delta 统计
        If delta IsNot Nothing Then
            AppendStatistics(delta, numMfcc, vector, offset)
            offset += numMfcc * nStats
        End If

        ' Delta-Delta 统计
        If deltaDelta IsNot Nothing Then
            AppendStatistics(deltaDelta, numMfcc, vector, offset)
            offset += numMfcc * nStats
        End If

        Return vector
    End Function

    ''' <summary>
    ''' 对每个 MFCC 系数在所有帧上计算 4 个统计量，写入向量的指定位置。
    ''' </summary>
    Private Sub AppendStatistics(features As Double()(), numCoeffs As Integer,
                                  vector As Double(), offset As Integer)
        If features.Length = 0 Then Return

        For k As Integer = 0 To numCoeffs - 1
            ' 收集第 k 个系数在所有帧上的值
            Dim coeffValues(features.Length - 1) As Double
            For t As Integer = 0 To features.Length - 1
                coeffValues(t) = features(t)(k)
            Next

            ' 计算统计量
            Dim mean As Double, std As Double, skew As Double, kurt As Double
            SignalProcessing.ComputeStatistics(coeffValues, mean, std, skew, kurt)

            ' 写入向量
            Dim baseIdx As Integer = offset + k * 4
            vector(baseIdx) = mean
            vector(baseIdx + 1) = std
            vector(baseIdx + 2) = skew
            vector(baseIdx + 3) = kurt
        Next
    End Sub

    ''' <summary>
    ''' 倒谱均值方差归一化：对每个系数在所有帧上做零均值单位方差。
    ''' </summary>
    Private Sub ApplyCMVN(features As Double()())
        If features.Length = 0 Then Return
        Dim numCoeffs As Integer = features(0).Length

        For k As Integer = 0 To numCoeffs - 1
            ' 均值
            Dim mean As Double = 0.0
            For t As Integer = 0 To features.Length - 1
                mean += features(t)(k)
            Next
            mean /= features.Length

            ' 方差
            Dim variance As Double = 0.0
            For t As Integer = 0 To features.Length - 1
                Dim diff As Double = features(t)(k) - mean
                variance += diff * diff
            Next
            variance /= features.Length
            Dim std As Double = std.Sqrt(variance)
            If std < 1.0E-10 Then std = 1.0

            ' 归一化
            For t As Integer = 0 To features.Length - 1
                features(t)(k) = (features(t)(k) - mean) / std
            Next
        Next
    End Sub

    ''' <summary>
    ''' L2 归一化：向量除以其欧几里得范数。
    ''' </summary>
    Private Sub L2NormalizeVector(vector As Double())
        Dim norm As Double = 0.0
        For i As Integer = 0 To vector.Length - 1
            norm += vector(i) * vector(i)
        Next
        norm = std.Sqrt(norm)
        If norm > 1.0E-10 Then
            For i As Integer = 0 To vector.Length - 1
                vector(i) /= norm
            Next
        End If
    End Sub

    ''' <summary>
    ''' 将向量截断或补零到目标维度。
    ''' </summary>
    Private Function AdjustDimension(vector As Double(), targetDim As Integer) As Double()
        If vector.Length = targetDim Then
            Return vector
        ElseIf vector.Length > targetDim Then
            ' 截断
            Dim result(targetDim - 1) As Double
            Array.Copy(vector, result, targetDim)
            Return result
        Else
            ' 补零
            Dim result(targetDim - 1) As Double
            Array.Copy(vector, result, vector.Length)
            Return result
        End If
    End Function

End Class
