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
''' 【开发期诊断工具】按 ffmpeg 的 <c>get_cabac_inline</c> 逐字镜像实现的 CABAC 位读取器。
''' </summary>
''' <remarks>
''' 用途只有一个：把编码器写出的码流反解回 bin 序列，从而定位「编码器与解码器在哪一个 bin 上分叉」。
''' 它不参与编码/解码流程，也不会被任何正式 API 引用；一旦码流通过验收即可删除本文件。
''' 
''' 实现严格对照 libavcodec/cabac.c 的 <c>ff_init_cabac_decoder</c>、cabac_functions.h 的
''' <c>get_cabac_inline</c> 与 <c>refill</c>：
''' 
''' + 初始化：<c>low = (b0 &lt;&lt; 18) + (b1 &lt;&lt; 10) + (1 &lt;&lt; 9)</c>，<c>range = 0x1FE</c>
''' + 每读一位：<c>range -= RangeLPS</c>，<c>mask = ((range &lt;&lt; 17) - low) &gt;&gt; 31</c>，
'''   <c>low -= (range &lt;&lt; 17) And mask</c>，<c>range += (RangeLPS - range) And mask</c>，
'''   <c>state = mlpsState(state Xor mask)</c>，返回位为 <c>state And 1</c>
''' + 归一化后当低位 16 位耗尽时补入 2 字节（<c>low += (b &lt;&lt; 9) + (b' &lt;&lt; 1)</c>）
''' </remarks>
Public Class H264CabacDebugReader

    Private ReadOnly data As Byte()
    Private ReadOnly states As Byte()
    Private lowValue As Integer
    Private rangeValue As Integer
    Private bytePos As Integer

    ''' <summary>
    ''' 用一段已经去掉防竞争字节的 RBSP 数据（从 slice data 的起点开始）与切片 QP 初始化
    ''' </summary>
    Sub New(sliceData As Byte(), sliceQp As Integer)
        Me.data = sliceData
        Me.states = New Byte(1023) {}

        Dim qp As Integer = If(sliceQp < 0, 0, If(sliceQp > 51, 51, sliceQp))
        Dim mValues As Integer() = H264CabacTables.ctxInitM_I
        Dim nValues As Integer() = H264CabacTables.ctxInitN_I

        For i As Integer = 0 To 1023
            Dim pre As Integer = 2 * (((mValues(i) * qp) >> 4) + nValues(i)) - 127

            If pre < 0 Then pre = -pre - 1
            If pre > 124 Then pre = 124 + (pre And 1)

            states(i) = CByte(pre)
        Next

        Me.bytePos = 2
        Me.rangeValue = &H1FE
        ' 注意：VB 的左移按左操作数类型求值，Byte 左移会溢出，必须显式转 Integer
        Me.lowValue = (CInt(data(0)) << 18) + (CInt(data(1)) << 10) + (1 << 9)
    End Sub

    ''' <summary>
    ''' 读取一位，上下文状态由调用方按期望的语法元素给出
    ''' </summary>
    Public Function readBin(ctx As Integer) As Integer
        Dim s As Integer = states(ctx)
        Dim lps As Integer = H264CabacTables.lpsRange(2 * (rangeValue And &HC0) + s)

        rangeValue -= lps

        Dim mask As Integer = ((rangeValue << 17) - lowValue) >> 31

        lowValue -= (rangeValue << 17) And mask
        rangeValue += (lps - rangeValue) And mask

        Dim bit As Integer

        If mask = 0 Then
            states(ctx) = H264CabacTables.mlpsState(s)
            bit = s And 1
        Else
            ' LPS：(mlps_state + 128)[s ^ -1] 等价于 mlps_state[127 - s]
            states(ctx) = H264CabacTables.lpsState(127 - s)
            bit = (s And 1) Xor 1
        End If

        ' 归一化：与解码器的 norm_shift 表等价，并且低位耗尽时补 2 字节
        Dim shift As Integer = 0

        While rangeValue < &H100
            rangeValue <<= 1
            lowValue <<= 1
            shift += 1
        End While

        If shift > 0 AndAlso (lowValue And &HFFFF) = 0 Then
            If bytePos + 1 < data.Length Then
                lowValue += (CInt(data(bytePos)) << 9) + (CInt(data(bytePos + 1)) << 1)
                bytePos += 2
            End If
        End If

        Return bit
    End Function

    ''' <summary>编码器使用的 CABAC 终止位（end_of_slice_flag）读取</summary>
    Public Function readTerminate() As Integer
        rangeValue -= 2

        Dim value As Integer

        If lowValue < (rangeValue << 17) Then
            value = 0
        Else
            lowValue -= rangeValue << 17
            rangeValue = 2
            value = 1
        End If

        Dim shift As Integer = 0

        While rangeValue < &H100
            rangeValue <<= 1
            lowValue <<= 1
            shift += 1
        End While

        If shift > 0 AndAlso (lowValue And &HFFFF) = 0 Then
            If bytePos + 1 < data.Length Then
                lowValue += (CInt(data(bytePos)) << 9) + (CInt(data(bytePos + 1)) << 1)
                bytePos += 2
            End If
        End If

        Return value
    End Function

End Class
