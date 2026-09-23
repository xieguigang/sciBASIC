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

Imports System.IO
Imports System.Text

''' <summary>
''' ISO BMFF（MP4）的 box 写入器：所有字段都是<b>大端</b>序，box 尺寸在写出过程中自动回填。
''' </summary>
''' <remarks>
''' 结构对照 ffmpeg 的 libavformat/movenc.c（例如 <c>mov_write_mvhd_tag</c> @4551、
''' <c>mov_write_tkhd_tag</c> @4041、<c>mov_write_stbl_tag</c> @3439）。
''' MP4 的头部体量很小（每个采样只占 4 - 8 字节索引），因此整棵 moov 树先在内存里组装完，
''' 再一次性写出；逐帧的图像数据则始终留在临时文件里顺序拷贝，内存占用不随帧数增长。
''' </remarks>
Friend Class Mp4BoxWriter
    Implements IDisposable

    Private ReadOnly ms As New MemoryStream

    Friend ReadOnly Property length As Long
        Get
            Return ms.Length
        End Get
    End Property

    ''' <summary>写出一个普通 box：尺寸 + 类型 + 由回调填充的内容</summary>
    Friend Sub box(typeName As String, payload As Action(Of Mp4BoxWriter))
        Dim start As Long = ms.Position

        Call writeU32(0)                    ' 尺寸占位
        Call writeType(typeName)

        If payload IsNot Nothing Then Call payload(Me)

        patchU32(start, CLng(ms.Position) - start)
    End Sub

    ''' <summary>写出一个 full box：尺寸 + 类型 + version/flags + 由回调填充的内容</summary>
    Friend Sub fullBox(typeName As String, version As Integer, flags As Integer, payload As Action(Of Mp4BoxWriter))
        Call box(typeName,
                 Sub(w)
                     w.writeU8(version)
                     w.writeU24(flags)
                     If payload IsNot Nothing Then payload(w)
                 End Sub)
    End Sub

    Friend Sub writeU8(value As Integer)
        ms.WriteByte(CByte(value And &HFF))
    End Sub

    Friend Sub writeU16(value As Integer)
        Call writeU8(value >> 8)
        Call writeU8(value)
    End Sub

    Friend Sub writeU24(value As Integer)
        Call writeU8(value >> 16)
        Call writeU8(value >> 8)
        Call writeU8(value)
    End Sub

    Friend Sub writeU32(value As Long)
        Call writeU8(CInt((value >> 24) And &HFF))
        Call writeU8(CInt((value >> 16) And &HFF))
        Call writeU8(CInt((value >> 8) And &HFF))
        Call writeU8(CInt(value And &HFF))
    End Sub

    Friend Sub writeU64(value As Long)
        Call writeU32(value >> 32)
        Call writeU32(value And &HFFFFFFFFL)
    End Sub

    ''' <summary>写出 4 个 ASCII 字符（box 类型名或 fourcc）</summary>
    Friend Sub writeType(typeName As String)
        If typeName.Length <> 4 Then
            Throw New ArgumentException($"a box type should be exactly 4 characters, but got '{typeName}'", NameOf(typeName))
        End If

        Call ms.Write(Encoding.ASCII.GetBytes(typeName), 0, 4)
    End Sub

    Friend Sub writeBytes(value As Byte())
        If value IsNot Nothing AndAlso value.Length > 0 Then
            Call ms.Write(value, 0, value.Length)
        End If
    End Sub

    Friend Sub writeZeros(count As Integer)
        For i As Integer = 1 To count
            ms.WriteByte(0)
        Next
    End Sub

    ''' <summary>写出以 0 结尾的 C 字符串</summary>
    Friend Sub writeCString(value As String)
        Call writeBytes(Encoding.ASCII.GetBytes(value))
        ms.WriteByte(0)
    End Sub

    ''' <summary>写出视频 sample entry 固定使用的单位矩阵（16.16 定点）</summary>
    Friend Sub writeUnityMatrix()
        Call writeU32(&H10000L) : Call writeU32(0L) : Call writeU32(0L)
        Call writeU32(0L) : Call writeU32(&H10000L) : Call writeU32(0L)
        Call writeU32(0L) : Call writeU32(0L) : Call writeU32(&H40000000L)
    End Sub

    ''' <summary>回填一个 32 位尺寸字段</summary>
    Private Sub patchU32(position As Long, value As Long)
        Dim current As Long = ms.Position

        Call ms.Seek(position, SeekOrigin.Begin)
        Call writeU32(value)
        Call ms.Seek(current, SeekOrigin.Begin)
    End Sub

    Friend Function toArray() As Byte()
        Return ms.ToArray
    End Function

    Friend Sub writeTo(stream As Stream)
        Dim data As Byte() = ms.ToArray
        Call stream.Write(data, 0, data.Length)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Call ms.Dispose()
    End Sub

End Class
