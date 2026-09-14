' ============================================================================
' Encoder.vb — 脉冲编码器（Spike Encoder）
'
' 把连续值输入编码为离散脉冲序列，是 SNN 信息流的入口（readme 第二节）：
'
'   频率编码（Rate Coding）
'     强度 → 发放频率。逐时间步以概率 p = x·dt 发放伯努利脉冲（泊松近似）。
'     信息容量最高（T 步可表达 O(T) 个等级），但统计噪声随 T 降低、时延随 T 增大。
'
'   时延编码（Latency Coding / TTFS）
'     强度 → 首脉冲时间。t_first = −τ·ln(x)，输入越强脉冲来得越早。
'     极致稀疏（每特征至多 1 个脉冲）、延迟低、且为确定性编码。
' ============================================================================

Imports std = System.Math
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace SpikingNN

    ''' <summary>输入脉冲编码方式</summary>
    Public Enum SpikeEncoding
        ''' <summary>频率编码：强度 → 发放频率（泊松近似）</summary>
        RateCoding
        ''' <summary>时延编码：强度 → 首脉冲时间（TTFS，确定性）</summary>
        LatencyCoding
    End Enum

    ''' <summary>脉冲编码器库</summary>
    Public Module SpikeEncoders

        ''' <summary>
        ''' 频率编码（泊松近似）。
        ''' </summary>
        ''' <param name="x">连续输入 [batch, features]，取值范围建议 [0,1]</param>
        ''' <param name="T">仿真时间步数</param>
        ''' <param name="rng">伯努利采样随机源</param>
        ''' <param name="dt">单步时长（缩放发放概率），默认 1.0</param>
        ''' <returns>T 个时间步的脉冲张量序列，每个 [batch, features]</returns>
        Public Function RateEncode(x As Tensor, T As Integer,
                                   rng As Random, Optional dt As Double = 1.0) As List(Of Tensor)
            Dim seq As New List(Of Tensor)()

            For T = 1 To T
                Dim spk = New Tensor(x.Shape)
                For i = 0 To x.Length - 1
                    If rng.NextDouble() < x.Data(i) * dt Then
                        spk.Data(i) = 1.0
                    End If
                Next
                seq.Add(spk)
            Next

            Return seq
        End Function

        ''' <summary>
        ''' 时延编码（TTFS, time-to-first-spike）。
        ''' t_first = −τ·ln(max(x, eps))：x→1 早发放，x→0 晚发放或不发放。
        ''' 确定性编码——数值梯度自检依赖这一性质。
        ''' </summary>
        ''' <param name="tau">编码时间常数（步）</param>
        ''' <param name="eps">下限截断，防止 ln(0)</param>
        Public Function LatencyEncode(x As Tensor, T As Integer,
                                      Optional tau As Double = 5.0,
                                      Optional eps As Double = 0.01) As List(Of Tensor)
            Dim seq As New List(Of Tensor)()
            For T = 1 To T
                seq.Add(New Tensor(x.Shape))
            Next

            For i = 0 To x.Length - 1
                Dim v = std.Max(x.Data(i), eps)
                Dim tFirst = CInt(std.Round(-tau * std.Log(v)))
                If tFirst >= 0 AndAlso tFirst < T Then
                    seq(tFirst).Data(i) = 1.0
                End If
                ' tFirst 超出 [0, T) → 该特征在本窗口内不发放
            Next

            Return seq
        End Function

    End Module

End Namespace
