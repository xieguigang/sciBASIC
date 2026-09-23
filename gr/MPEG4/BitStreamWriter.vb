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
''' MSB-first 位流写入器，负责 H.264 的位级语法写出与 RBSP 防竞争字节插入。
''' </summary>
''' <remarks>
''' H.264 的语法元素是按"高位在前"的顺序串起来的，因此这里统一按 MSB-first 累积到字节。
''' 除了常规的无符号/有符号指数哥伦布编码之外，还提供 CABAC 切片所需的
''' <c>cabac_alignment_one_bit</c>（用 1 补齐到字节边界）与 RBSP 结尾比特。
''' </remarks>
Friend Class BitStreamWriter

    Private buf As Byte()
    Private len As Integer      ' 已经写满的字节数
    Private cur As Integer      ' 正在累积的字节（低位待填）
    Private nbits As Integer    ' cur 中已经确定的位数，取值 0 - 7

    ''' <summary>
    ''' CABAC 算术编码器要求丢弃码流的第一个比特（解码器初始化时该比特被隐式置 1）。
    ''' </summary>
    Friend Property skipFirstBit As Boolean

    Private firstBitSkipped As Boolean

    Friend Sub New(Optional capacity As Integer = 4096)
        Me.buf = New Byte(Math.Max(capacity, 16) - 1) {}
    End Sub

    ''' <summary>
    ''' 已写出的字节数（未完成的字节按 1 个字节计入）
    ''' </summary>
    Friend ReadOnly Property length As Integer
        Get
            Return len + If(nbits > 0, 1, 0)
        End Get
    End Property

    ''' <summary>
    ''' 已经写出的比特总数
    ''' </summary>
    Friend ReadOnly Property bitLength As Long
        Get
            Return CLng(len) * 8 + nbits
        End Get
    End Property

    Friend Sub writeBit(b As Integer)
        If skipFirstBit AndAlso Not firstBitSkipped Then
            firstBitSkipped = True
            Return
        End If

        cur = (cur << 1) Or (b And 1)
        nbits += 1

        If nbits = 8 Then flushByte()
    End Sub

    Friend Sub writeBits(value As Long, bits As Integer)
        While bits > 0
            Dim take As Integer = Math.Min(8 - nbits, bits)
            Dim shift As Integer = bits - take
            Dim mask As Long = (1L << take) - 1L
            Dim part As Integer = CInt((value >> shift) And mask)

            cur = (cur << take) Or part
            nbits += take
            bits -= take

            If nbits = 8 Then flushByte()
        End While
    End Sub

    ''' <summary>
    ''' 无符号指数哥伦布编码 ue(v)
    ''' </summary>
    Friend Sub writeUe(value As Long)
        Dim n As Long = value + 1L
        Dim leading As Integer = 0
        Dim t As Long = n

        While t > 1L
            t >>= 1
            leading += 1
        End While

        If leading > 0 Then Call writeBits(0, leading)

        Call writeBits(n, leading + 1)
    End Sub

    ''' <summary>
    ''' 有符号指数哥伦布编码 se(v)：正值映射为 2v-1，非正值映射为 -2v
    ''' </summary>
    Friend Sub writeSe(value As Long)
        If value > 0L Then
            Call writeUe(2L * value - 1L)
        Else
            Call writeUe(-2L * value)
        End If
    End Sub

    ''' <summary>
    ''' CABAC 切片数据前的对齐：补 1 到字节边界
    ''' </summary>
    Friend Sub alignWithOnes()
        While nbits <> 0
            Call writeBit(1)
        End While
    End Sub

    ''' <summary>
    ''' 补 0 到字节边界，但不写 rbsp_stop_one_bit
    ''' </summary>
    Friend Sub alignWithZeros()
        While nbits <> 0
            Call writeBit(0)
        End While
    End Sub

    ''' <summary>
    ''' rbsp_trailing_bits()：一个停止位 1 加若干 0 补齐到字节边界
    ''' </summary>
    Friend Sub writeRbspTrailingBits()
        Call writeBit(1)
        Call alignWithZeros()
    End Sub

    Private Sub flushByte()
        ensure(len + 1)

        buf(len) = CByte(cur And &HFF)
        len += 1
        cur = 0
        nbits = 0
    End Sub

    Private Sub ensure(size As Integer)
        If size <= buf.Length Then Return

        Dim next_ As Byte() = New Byte(Math.Max(size, buf.Length * 2) - 1) {}
        Call Array.Copy(buf, next_, len)
        buf = next_
    End Sub

    ''' <summary>
    ''' 结束并返回已经写出的字节（未完成的字节按 0 补齐）
    ''' </summary>
    Friend Function toArray() As Byte()
        If nbits > 0 Then
            Call flushByte()
        End If

        Dim out As Byte() = New Byte(len - 1) {}
        Call Array.Copy(buf, out, len)

        Return out
    End Function

    ''' <summary>
    ''' 按 H.264 的要求插入防竞争字节：0x000000 - 0x000003 之间插入 0x03
    ''' </summary>
    Friend Shared Function escapeRbsp(rbsp As Byte()) As Byte()
        Dim out As New List(Of Byte)(rbsp.Length + 16)
        Dim zeros As Integer = 0

        For Each b As Byte In rbsp
            If zeros >= 2 AndAlso b <= &H3 Then
                out.Add(&H3)
                zeros = 0
            End If

            out.Add(b)

            If b = 0 Then
                zeros += 1
            Else
                zeros = 0
            End If
        Next

        Return out.ToArray
    End Function

    ''' <summary>
    ''' 组装一个 NAL 单元：起始码 + NAL 头 + 防竞争处理后的 RBSP
    ''' </summary>
    Friend Shared Function nalUnit(rbsp As Byte(), nalRefIdc As Integer, nalType As Integer, Optional annexB As Boolean = True) As Byte()
        Dim payload As Byte() = escapeRbsp(rbsp)
        Dim head As Integer = ((nalRefIdc And 3) << 5) Or (nalType And &H1F)
        Dim offset As Integer = If(annexB, 4, 0)
        Dim out As Byte() = New Byte(offset + 1 + payload.Length - 1) {}

        If annexB Then
            out(0) = 0
            out(1) = 0
            out(2) = 0
            out(3) = 1
        End If

        out(offset) = CByte(head)
        Call Array.Copy(payload, 0, out, offset + 1, payload.Length)

        Return out
    End Function

End Class
