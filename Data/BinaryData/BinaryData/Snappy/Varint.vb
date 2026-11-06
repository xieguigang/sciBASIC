Option Strict On
Option Explicit On

Imports System.IO

Namespace Snappy

    ''' <summary>
    ''' LEB128 无符号 varint 的读写辅助。Snappy 的原始块以 varint 表示未压缩长度。
    ''' </summary>
    Friend Module Varint

        ''' <summary>
        ''' 读取一个最多 5 字节的 32 位 varint。
        ''' </summary>
        ''' <param name="src">源缓冲区。</param>
        ''' <param name="pos">读取游标，方法返回后指向 varint 之后的第一个字节。</param>
        ''' <param name="limit">本次读取允许访问的上界（不包含）。</param>
        ''' <exception cref="InvalidDataException">数据被截断、过长或超出 32 位范围。</exception>
        Friend Function Read(src As Byte(), ByRef pos As Integer, limit As Integer) As UInteger
            Dim result As ULong = 0UL
            Dim shift As Integer = 0

            For i As Integer = 0 To 4
                If pos >= limit Then
                    Throw New InvalidDataException("Snappy: 数据被截断，无法读取长度前缀。")
                End If

                Dim b As Integer = CInt(src(pos))
                pos += 1
                result = result Or (CULng(b And &H7F) << shift)

                If (b And &H80) = 0 Then
                    If result > UInteger.MaxValue Then
                        Throw New InvalidDataException("Snappy: 长度前缀超出 32 位范围。")
                    End If
                    Return CUInt(result)
                End If

                shift += 7
            Next

            Throw New InvalidDataException("Snappy: 长度前缀过长（超过 5 字节）。")
        End Function

        ''' <summary>
        ''' 把一个 32 位无符号整数写成 varint。
        ''' </summary>
        ''' <param name="value">要写入的值。</param>
        ''' <param name="buffer">目标缓冲区，调用方需保证空间充足（最多 5 字节）。</param>
        ''' <param name="pos">写入游标，方法返回后指向写入内容之后的第一个字节。</param>
        Friend Sub Write(value As UInteger, buffer As Byte(), ByRef pos As Integer)
            Dim v As UInteger = value

            While v >= &H80UI
                buffer(pos) = CByte((v And &H7FUI) Or &H80UI)
                pos += 1
                v >>= 7
            End While

            buffer(pos) = CByte(v)
            pos += 1
        End Sub

    End Module
End Namespace
