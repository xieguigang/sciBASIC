' Author:
' 
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

''' <summary>
''' H.264 CABAC 算术编码器与上下文状态管理。
''' </summary>
''' <remarks>
''' 算术编码部分按 ISO/IEC 14496-10 的 CABAC 编码流程实现（低位 low 以 9 位定点累积、
''' 区间 range 归一化到 [0x100, 0x1FF]、进位链用 outstanding 计数处理）；
''' 状态转移与区间查表则完全镜像 ffmpeg 解码器的用法
''' （libavcodec/cabac_functions.h 的 <c>get_cabac_inline</c>）：
''' 
''' + <c>RangeLPS = lpsRange(2 * (range And &amp;HC0) + state)</c>
''' + MPS 分支：<c>state = mlpsState(state)</c>
''' + LPS 分支：<c>state = mlpsState(state Xor 1)</c>
''' 
''' 只有与解码器使用同一套表与同一套索引，编码器与解码器的状态演化才能保持同步。
''' </remarks>
Friend Class H264Cabac

    Private ReadOnly bits As BitStreamWriter

    ''' <summary>
    ''' 1024 个上下文模型的状态字节，编码沿用 <c>state = 2 * pStateIdx + valMPS</c>。
    ''' </summary>
    Friend ReadOnly states As Byte()

    Private lowValue As Integer
    Private rangeValue As Integer
    Private outstandingCount As Integer
    Private firstBit As Boolean

    Friend Sub New(bits As BitStreamWriter)
        Me.bits = bits
        Me.states = New Byte(1023) {}
    End Sub

    ''' <summary>
    ''' 按 ffmpeg <c>ff_h264_init_cabac_states</c> 的公式初始化上下文状态。
    ''' </summary>
    ''' <param name="isI">是否为 I 切片（I 切片使用 <c>cabac_context_init_I</c>）</param>
    ''' <param name="cabacInitIdc">非 I 切片的 cabac_init_idc，取值 0 - 2</param>
    ''' <param name="sliceQp">切片量化参数，取值会被钳制到 0 - 51</param>
    Friend Sub initStates(isI As Boolean, cabacInitIdc As Integer, sliceQp As Integer)
        Dim qp As Integer = If(sliceQp < 0, 0, If(sliceQp > 51, 51, sliceQp))
        Dim mValues As Integer()
        Dim nValues As Integer()
        Dim offset As Integer = 0

        If isI Then
            mValues = H264CabacTables.ctxInitM_I
            nValues = H264CabacTables.ctxInitN_I
        Else
            offset = Math.Max(0, Math.Min(2, cabacInitIdc)) * 1024
            mValues = H264CabacTables.ctxInitM_PB
            nValues = H264CabacTables.ctxInitN_PB
        End If

        For i As Integer = 0 To 1023
            Dim pre As Integer = 2 * (((mValues(offset + i) * qp) >> 4) + nValues(offset + i)) - 127

            ' pre ^= pre >> 31 等价于：负数取 -pre-1
            If pre < 0 Then pre = -pre - 1
            If pre > 124 Then pre = 124 + (pre And 1)

            states(i) = CByte(pre)
        Next
    End Sub

    ''' <summary>
    ''' 开始一个切片的算术编码；会跳过码流的第一个比特
    ''' </summary>
    Friend Sub begin()
        lowValue = 0
        rangeValue = &H1FE
        outstandingCount = 0
        firstBit = True

        bits.skipFirstBit = True
    End Sub

    ''' <summary>
    ''' 编码一个使用上下文模型的语法元素位（bin）
    ''' </summary>
    Friend Sub encodeBin(ctx As Integer, bin As Integer)
        If H264Debug.Enabled Then H264Debug.Bins.Add($"c{ctx}={bin}")

        Dim s As Integer = states(ctx)
        Dim lps As Integer = H264CabacTables.lpsRange(2 * (rangeValue And &HC0) + s)

        rangeValue -= lps

        If bin = (s And 1) Then
            ' MPS 分支
            states(ctx) = H264CabacTables.mlpsState(s)
        Else
            ' LPS 分支：走上半个区间
            lowValue += rangeValue
            rangeValue = lps
            ' 解码器写的是 state = (mlps_state + 128)[s ^ -1]，即 *(mlps_state + 127 - s)，
            ' 因此 LPS 的后继状态来自表的另一半（lpsState）并按 127 - s 反向索引。
            ' 注意不能写成 mlpsState(s Xor 1)：MPS 概率不占优的位会立刻与解码器失去同步。
            states(ctx) = H264CabacTables.lpsState(127 - s)
        End If

        Call renorm()

        If H264Debug.Enabled Then H264Debug.Ranges.Add(rangeValue)
    End Sub

    ''' <summary>
    ''' 编码一个旁路（bypass）位：区间固定对半分，不使用上下文模型
    ''' </summary>
    Friend Sub encodeBypass(bit As Integer)
        If H264Debug.Enabled Then H264Debug.Bins.Add($"b{bit}")

        lowValue += lowValue

        If bit <> 0 Then
            lowValue += rangeValue
        End If

        If lowValue < &H200 Then
            Call putBit(0)
        ElseIf lowValue < &H400 Then
            outstandingCount += 1
            lowValue -= &H200
        Else
            Call putBit(1)
            lowValue -= &H400
        End If
    End Sub

    ''' <summary>
    ''' 编码一个旁路位表示的正负号：<paramref name="value"/> 的符号由 1 个旁路位承载
    ''' </summary>
    Friend Sub encodeBypassSign(value As Integer)
        ' 解码侧为 get_cabac_bypass_sign(CC, -magnitude)：
        ' 位为 0 → 负；位为 1 → 正
        Call encodeBypass(If(value >= 0, 1, 0))
    End Sub

    ''' <summary>
    ''' 编码终止位（end_of_slice_flag）；<paramref name="bit"/> 为 1 时结束切片数据
    ''' </summary>
    Friend Sub encodeTerminate(bit As Integer)
        If H264Debug.Enabled Then H264Debug.Bins.Add($"t{bit}")

        rangeValue -= 2

        If bit = 0 Then
            Call renorm()

            If H264Debug.Enabled Then H264Debug.Ranges.Add(rangeValue)

            Return
        End If

        lowValue += rangeValue
        rangeValue = 2

        Call renorm()

        ' 收尾：输出剩余的 3 个有效位，其中最低位固定为 1（即 rbsp_stop_one_bit）
        Call putBit((lowValue >> 9) And 1)
        Call bits.writeBits(((lowValue >> 7) And 3) Or 1, 2)
    End Sub

    ''' <summary>
    ''' 归一化：区间小于 0x100 时左移，并按 9 位定点输出低位
    ''' </summary>
    Private Sub renorm()
        While rangeValue < &H100
            If lowValue < &H100 Then
                Call putBit(0)
                lowValue <<= 1
            ElseIf lowValue < &H200 Then
                outstandingCount += 1
                lowValue = (lowValue - &H100) << 1
            Else
                Call putBit(1)
                lowValue = (lowValue - &H200) << 1
            End If

            rangeValue += rangeValue
        End While
    End Sub

    ''' <summary>
    ''' 输出一个比特以及待定（outstanding）的进位链
    ''' </summary>
    Private Sub putBit(b As Integer)
        Call bits.writeBit(b)

        While outstandingCount > 0
            Call bits.writeBit(If(b <> 0, 0, 1))
            outstandingCount -= 1
        End While
    End Sub

End Class
