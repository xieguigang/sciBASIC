#Region "Microsoft.VisualBasic::f676060ae2602122277763bcdf8aa7e1, Data_science\MachineLearning\SNN\Encoder.vb"

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

    '   Total Lines: 120
    '    Code Lines: 54 (45.00%)
    ' Comment Lines: 52 (43.33%)
    '    - Xml Docs: 67.31%
    ' 
    '   Blank Lines: 14 (11.67%)
    '     File Size: 5.53 KB


    ' Enum SpikeEncoding
    ' 
    '     DirectCurrent, LatencyCoding, RateCoding
    ' 
    '  
    ' 
    ' 
    ' 
    ' Module SpikeEncoders
    ' 
    '     Function: DirectCurrentEncode, LatencyEncode, RateEncode
    ' 
    ' /********************************************************************************/

#End Region

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
    ''' <param name="shareBuffer">
    ''' 是否让所有时间步共享同一份电流缓冲（默认 <c>False</c>，保持"每步独立副本"的既有语义）。
    ''' <para>
    ''' 置为 <c>True</c> 可省掉 T 份 <c>[batch, features]</c> 的分配与拷贝
    ''' （T=30 时为 30 次分配），代价是调用方<b>不得再对返回的张量就地写入</b>
    ''' （恒流注入场景下每步内容本就完全相同，且融合算子只读该输入）。
    ''' </para>
    ''' </param>
    ''' <returns>T 个时间步的输入电流张量序列（每个形状与 x 相同，内容为 x·iMax）</returns>
    Public Function DirectCurrentEncode(x As Tensor, T As Integer,
                                        Optional iMax As Double = 1.0,
                                        Optional shareBuffer As Boolean = False) As List(Of Tensor)
        If x Is Nothing Then
            Throw New ArgumentNullException(NameOf(x))
        End If
        If T <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(T), "仿真步数必须为正整数")
        End If

        Dim scaled = If(iMax = 1.0, x.Data, MapScale(x.Data, iMax))
        Dim seq As New List(Of Tensor)()

        If shareBuffer Then
            ' 恒流：所有时间步的电流完全相同，一份缓冲即可（下游只读）
            Dim currentBuffer = Tensor.Wrap(scaled, x.Shape)

            For n = 1 To T
                seq.Add(currentBuffer)
            Next

            Return seq
        End If

        For n = 1 To T
            ' 每步一份独立副本：避免下游就地写入（如扰动注入）时相互污染
            seq.Add(Tensor.Wrap(CType(scaled.Clone(), Double()), x.Shape))
        Next

        Return seq
    End Function

    ''' <summary>逐元素缩放（与 <c>LINQ Select</c> 等价，但避免中间迭代器与装箱开销）。</summary>
    Private Function MapScale(src As Double(), factor As Double) As Double()
        Dim dst(src.Length - 1) As Double

        For i As Integer = 0 To src.Length - 1
            dst(i) = src(i) * factor
        Next

        Return dst
    End Function

End Module
