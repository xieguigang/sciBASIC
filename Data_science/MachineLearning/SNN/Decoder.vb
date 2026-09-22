#Region "Microsoft.VisualBasic::d1dde9ab91a36abee0d61d352052315b, Data_science\MachineLearning\SNN\Decoder.vb"

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

    '   Total Lines: 213
    '    Code Lines: 135 (63.38%)
    ' Comment Lines: 42 (19.72%)
    '    - Xml Docs: 59.52%
    ' 
    '   Blank Lines: 36 (16.90%)
    '     File Size: 7.87 KB


    ' Module SpikeDecoders
    ' 
    '     Function: ActiveNeurons, ConcatRateAndPotential, dataLength, FiringRate, MembranePotential
    '               RasterText, SpikeCounts, TotalSpikeCount
    ' 
    '     Sub: AssertHistory
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' Decoder.vb — 脉冲解码器（Spike Decoder）
'
' SNN 的输出是离散 0/1 脉冲序列，而回归/分类目标通常是连续值，
' 因此需要在仿真结束后把"脉冲域"重新映射回"值域"：
'
'   方案 A · 膜电位读取（推荐用于回归）
'     y = u_last        直接读取末步膜电位。梯度路径平滑、不依赖发放次数，
'                       避免了"脉冲过稀 → 梯度为零"的死亡神经元问题。
'   方案 B · 发放率解码
'     y = (Σ_t S[t]) / T   统计时间窗内的平均发放率（频率编码的天然逆运算）。
'   方案 C · 拼接解码
'     y = concat(rate, u_last)  同时提供"发放统计"与"瞬时状态"两类信息。
'
' 本模块只做"读取/汇总"，不含任何可训练参数；可训练的线性映射由
' LinearReadout（回归解码头）完成，两者组合即为 readme 中的 DECODE_OUTPUT。
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>脉冲序列 → 连续值的解码工具</summary>
Public Module SpikeDecoders

#Region "计数与发放率"

    ''' <summary>
    ''' 方案 B：时间窗内脉冲计数 Σ_t S[t]（形状 [batch, Units]）。
    ''' </summary>
    Public Function SpikeCounts(sHistory As List(Of Tensor)) As Tensor
        AssertHistory(sHistory)

        Dim shape = sHistory(0).Shape
        Dim acc(dataLength(shape) - 1) As Double

        For Each s In sHistory
            Dim d = s.Data
            For i = 0 To acc.Length - 1
                acc(i) += d(i)
            Next
        Next

        Dim counts = Tensor.Wrap(acc, shape)
        Return counts
    End Function

    ''' <summary>
    ''' 方案 B：平均发放率 (Σ_t S[t]) / T（形状 [batch, Units]，取值 [0,1]）。
    ''' </summary>
    Public Function FiringRate(sHistory As List(Of Tensor)) As Tensor
        AssertHistory(sHistory)
        Return SpikeCounts(sHistory) / CSng(sHistory.Count)
    End Function

    ''' <summary>整个仿真窗内的脉冲总数（诊断用标量）</summary>
    Public Function TotalSpikeCount(sHistory As List(Of Tensor)) As Double
        If sHistory Is Nothing OrElse sHistory.Count = 0 Then Return 0.0

        Dim total = 0.0
        For Each s In sHistory
            Dim d = s.Data
            For i = 0 To d.Length - 1
                total += d(i)
            Next
        Next
        Return total
    End Function

    ''' <summary>曾被激活（至少发放一次）的神经元索引</summary>
    Public Function ActiveNeurons(sHistory As List(Of Tensor)) As Integer()
        If sHistory Is Nothing OrElse sHistory.Count = 0 Then Return New Integer() {}

        Dim units = sHistory(0).Shape(1)
        Dim fired(units - 1) As Boolean
        For Each s In sHistory
            Dim d = s.Data
            For b = 0 To s.Shape(0) - 1
                Dim off = b * units
                For j = 0 To units - 1
                    If d(off + j) > 0.0 Then fired(j) = True
                Next
            Next
        Next

        Dim list As New List(Of Integer)()
        For j = 0 To units - 1
            If fired(j) Then list.Add(j)
        Next
        Return list.ToArray()
    End Function

#End Region

#Region "膜电位读取"

    ''' <summary>
    ''' 方案 A：膜电位读取。返回末步膜电位的独立副本（梯度路径平滑，回归首选）。
    ''' </summary>
    ''' <param name="uLast">末步膜电位（<c>ULast</c> 或 <c>HLast</c>）</param>
    Public Function MembranePotential(uLast As Tensor) As Tensor
        If uLast Is Nothing Then
            Throw New ArgumentNullException(NameOf(uLast))
        End If
        Return New Tensor(uLast.ToDoubleArray(), uLast.Shape)
    End Function

#End Region

#Region "拼接解码"

    ''' <summary>
    ''' 方案 C：把发放率与末步膜电位沿特征维拼接为 [batch, 2·Units]。
    ''' 供 LinearReadout 做"发放统计 + 瞬时状态"的联合回归。
    ''' </summary>
    Public Function ConcatRateAndPotential(rate As Tensor, membranePotential As Tensor) As Tensor
        If rate Is Nothing OrElse membranePotential Is Nothing Then
            Throw New ArgumentNullException(NameOf(rate))
        End If
        If rate.Rank <> 2 OrElse membranePotential.Rank <> 2 Then
            Throw New ArgumentException("拼接解码要求两个输入均为二维张量 [batch, features]")
        End If
        If rate.Shape(0) <> membranePotential.Shape(0) Then
            Throw New ArgumentException("发放率与膜电位的 batch 大小不一致")
        End If

        Dim batch = rate.Shape(0)
        Dim a = rate.Shape(1)
        Dim b = membranePotential.Shape(1)
        Dim rd = rate.Data
        Dim pd = membranePotential.Data

        Dim out(batch * (a + b) - 1) As Double
        For n = 0 To batch - 1
            Array.Copy(rd, n * a, out, n * (a + b), a)
            Array.Copy(pd, n * b, out, n * (a + b) + a, b)
        Next

        Return Tensor.Wrap(out, batch, a + b)
    End Function

#End Region

#Region "可视化"

    ''' <summary>
    ''' 把脉冲轨迹渲染成 ASCII 栅格图（行 = 神经元分组，列 = 时间步，█ = 本步有发放），
    ''' 便于在控制台快速判断"静默 / 持续发放 / 稀疏传播"等动力学形态。
    ''' </summary>
    ''' <param name="sHistory">脉冲轨迹 S[t]</param>
    ''' <param name="neuronsPerRow">每行折叠的神经元数量</param>
    ''' <param name="maxRows">最多输出的行数</param>
    ''' <param name="sampleIndex">批量中的样本下标</param>
    Public Function RasterText(sHistory As List(Of Tensor),
                               Optional neuronsPerRow As Integer = 16,
                               Optional maxRows As Integer = 8,
                               Optional sampleIndex As Integer = 0) As String
        If sHistory Is Nothing OrElse sHistory.Count = 0 Then
            Return "(empty spike history)"
        End If
        If neuronsPerRow <= 0 Then neuronsPerRow = 16

        Dim units = sHistory(0).Shape(1)
        Dim steps = sHistory.Count
        Dim groups = CInt(std.Ceiling(units / CDbl(neuronsPerRow)))
        If maxRows > 0 AndAlso groups > maxRows Then groups = maxRows

        Dim sb As New StringBuilder()
        For g = 0 To groups - 1
            sb.Append($"    g{g.ToString("D2")}|")
            For t = 0 To steps - 1
                Dim d = sHistory(t).Data
                Dim any = False
                Dim from = g * neuronsPerRow
                Dim upto = std.Min(from + neuronsPerRow, units) - 1
                Dim off = sampleIndex * units

                For j = from To upto
                    If d(off + j) > 0.0 Then
                        any = True
                        Exit For
                    End If
                Next

                sb.Append(If(any, "█"c, "·"c))
            Next
            sb.AppendLine("|")
        Next

        Return sb.ToString()
    End Function

#End Region

#Region "内部工具"

    Private Function dataLength(shape As Integer()) As Integer
        Dim n = 1
        For Each d In shape
            n *= d
        Next
        Return n
    End Function

    Private Sub AssertHistory(sHistory As List(Of Tensor))
        If sHistory Is Nothing OrElse sHistory.Count = 0 Then
            Throw New ArgumentException("脉冲轨迹为空，无法解码", NameOf(sHistory))
        End If
    End Sub

#End Region

End Module
