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

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>输入脉冲编码方式</summary>
Public Enum SpikeEncoding
    ''' <summary>频率编码：强度 → 发放频率（泊松近似）</summary>
    RateCoding
    ''' <summary>时延编码：强度 → 首脉冲时间（TTFS，确定性）</summary>
    LatencyCoding
    ''' <summary>
    ''' 直接电流注入：强度 → 持续输入电流（不做脉冲采样）。
    ''' 适合回归任务与稳态输入（如基因表达状态量）。
    ''' </summary>
    DirectCurrent
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

    ''' <summary>
    ''' 直接电流编码：把连续输入逐元素线性缩放到 [0, iMax] 后，作为整个时间窗的
    ''' <b>持续输入电流</b>注入（每个时间步都注入同一电流，而不是采样成脉冲）。
    '''
    ''' 与频率/时延编码的区别：输出<b>不是</b> 0/1 脉冲序列，而是 t 步的连续电流序列，
    ''' 语义上等价于"模拟恒流注入"。对回归任务（例如以表达状态量驱动脉冲网络）
    ''' 这是最简洁、最稳定的驱动方式，也是 readme 中 <c>mode = "current"</c> 的实现。
    '''
    ''' 注意：本函数不做 MinMax 归一化，调用方需保证 x 已落在 [0,1]（输出与编码器约定一致）。
    ''' </summary>
    ''' <param name="x">连续输入 [batch, features]，取值范围建议 [0,1]</param>
    ''' <param name="T">仿真时间步数</param>
    ''' <param name="iMax">电流上限（缩放系数），默认 1.0 表示直接使用输入值</param>
    ''' <returns>T 个时间步的输入电流张量序列（每个形状与 x 相同，内容为 x·iMax 的独立副本）</returns>
    Public Function DirectCurrentEncode(x As Tensor, T As Integer,
                                        Optional iMax As Double = 1.0) As List(Of Tensor)
        If x Is Nothing Then
            Throw New ArgumentNullException(NameOf(x))
        End If
        If T <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(T), "仿真步数必须为正整数")
        End If

        Dim scaled = If(iMax = 1.0, x.Data, x.Data.Select(Function(v) v * iMax).ToArray())
        Dim seq As New List(Of Tensor)()

        For n = 1 To T
            ' 每步一份独立副本：避免下游就地写入（如扰动注入）时相互污染
            seq.Add(Tensor.Wrap(CType(scaled.Clone(), Double()), x.Shape))
        Next

        Return seq
    End Function

End Module


