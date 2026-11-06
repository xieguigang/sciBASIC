Option Strict On
Option Explicit On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Snappy test support
'  Data\BinaryData\BinaryData\test\SnappyTests\TestSupport.vb
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.IO.Snappy

''' <summary>
''' 测试断言与辅助函数，统一走 <see cref="DebugAudit"/> 记录结果。
''' </summary>
Friend Module TestSupport

    Private _checks As Integer

    ''' <summary>已执行的断言总数（用于在测试报告中记录每个用例组的覆盖量）。</summary>
    Friend ReadOnly Property Checks As Integer
        Get
            Return _checks
        End Get
    End Property

    Friend Sub ResetChecks()
        _checks = 0
    End Sub

    Private Sub Count()
        _checks += 1
    End Sub

    ''' <summary>把字节数组格式化为 <c>0A 01 61</c> 形式的十六进制文本。</summary>
    Friend Function ToHex(bytes As Byte()) As String
        If bytes Is Nothing Then Return "<null>"

        Dim parts As New List(Of String)()

        For Each b As Byte In bytes
            parts.Add(b.ToString("X2"))
        Next

        Return String.Join(" ", parts)
    End Function

    ''' <summary>把 <c>0A 01 61</c> 形式的十六进制文本解析为字节数组。</summary>
    Friend Function HexBytes(hex As String) As Byte()
        Dim parts As String() = hex.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim buffer(parts.Length - 1) As Byte

        For i As Integer = 0 To parts.Length - 1
            buffer(i) = Convert.ToByte(parts(i), 16)
        Next

        Return buffer
    End Function

    ''' <summary>创建指定长度的重复字节序列。</summary>
    Friend Function Repeat(value As Byte, count As Integer) As Byte()
        If count <= 0 Then Return New Byte() {}

        Dim result(count - 1) As Byte

        For i As Integer = 0 To count - 1
            result(i) = value
        Next

        Return result
    End Function

    ''' <summary>用固定种子生成确定性的随机字节。</summary>
    Friend Function RandomBytes(seed As Integer, count As Integer) As Byte()
        If count <= 0 Then Return New Byte() {}

        Dim result(count - 1) As Byte
        Dim rng As New Random(seed)
        rng.NextBytes(result)

        Return result
    End Function

    ''' <summary>生成指定长度、可高度压缩的 UTF-8 文本数据。</summary>
    Friend Function TextBytes(size As Integer) As Byte()
        If size <= 0 Then Return New Byte() {}

        Dim phrase As String = "Snappy compression keeps the LZ77 pipeline simple and fast. "
        Dim builder As New StringBuilder()

        While builder.Length < size
            builder.Append(phrase)
        End While

        Dim all As Byte() = Encoding.UTF8.GetBytes(builder.ToString())
        Dim result(size - 1) As Byte
        Buffer.BlockCopy(all, 0, result, 0, size)

        Return result
    End Function

    ''' <summary>断言两个字节数组完全一致。</summary>
    Friend Sub AssertBytesEqual(expected As Byte(), actual As Byte(), message As String)
        Dim equal As Boolean = BytesEqual(expected, actual)

        Call Count()

        If equal Then
            Call DebugAudit.Test(True, message)
        Else
            Call DebugAudit.Test(False, $"{message}：期望 [{ToHex(expected)}]，实际 [{ToHex(actual)}]")
        End If
    End Sub

    ''' <summary>断言两个值相等。</summary>
    Friend Sub AssertEqual(Of T)(expected As T, actual As T, message As String)
        Call Count()

        If EqualityComparer(Of T).Default.Equals(expected, actual) Then
            Call DebugAudit.Test(True, message)
        Else
            Call DebugAudit.Test(False, $"{message}：期望 {DebugAudit.ValueText(expected)}，实际 {DebugAudit.ValueText(actual)}")
        End If
    End Sub

    ''' <summary>断言条件成立。</summary>
    Friend Sub AssertTrue(condition As Boolean, message As String)
        Call Count()
        Call DebugAudit.Test(condition, message)
    End Sub

    ''' <summary>断言条件不成立。</summary>
    Friend Sub AssertFalse(condition As Boolean, message As String)
        Call Count()
        Call DebugAudit.Test(Not condition, message)
    End Sub

    ''' <summary>断言执行指定的代码块会抛出 <typeparamref name="TException"/>。</summary>
    Friend Sub AssertThrows(Of TException As Exception)(code As Action, message As String)
        Call Count()

        Try
            Call code()
            Call DebugAudit.Test(False, $"{message}：期望抛出 {GetType(TException).Name}，但执行成功")
        Catch ex As TException
            Call DebugAudit.Test(True, message)
        Catch ex As Exception
            Call DebugAudit.Test(False, $"{message}：期望抛出 {GetType(TException).Name}，实际抛出 {ex.GetType().Name}")
        End Try
    End Sub

    ''' <summary>判断两个字节数组是否逐字节相等。</summary>
    Friend Function BytesEqual(expected As Byte(), actual As Byte()) As Boolean
        If expected Is Nothing OrElse actual Is Nothing Then Return expected Is actual
        If expected.Length <> actual.Length Then Return False

        For i As Integer = 0 To expected.Length - 1
            If expected(i) <> actual(i) Then Return False
        Next

        Return True
    End Function

    ''' <summary>拼接若干字节数组。</summary>
    Friend Function Concat(ParamArray parts As Byte()()) As Byte()
        Dim total As Integer = 0

        For Each part As Byte() In parts
            If part IsNot Nothing Then total += part.Length
        Next

        Dim result(total - 1) As Byte
        Dim offset As Integer = 0

        For Each part As Byte() In parts
            If part IsNot Nothing AndAlso part.Length > 0 Then
                Buffer.BlockCopy(part, 0, result, offset, part.Length)
                offset += part.Length
            End If
        Next

        Return result
    End Function

    ''' <summary>截取字节数组的一段。</summary>
    Friend Function Slice(bytes As Byte(), offset As Integer, count As Integer) As Byte()
        If count <= 0 Then Return New Byte() {}

        Dim result(count - 1) As Byte
        Buffer.BlockCopy(bytes, offset, result, 0, count)

        Return result
    End Function

    ''' <summary>把流中的剩余数据全部读出。</summary>
    Friend Function ReadAll(stream As Stream, Optional bufferSize As Integer = 81920) As Byte()
        Using output As New MemoryStream()
            Dim buffer(bufferSize - 1) As Byte

            Do
                Dim n As Integer = stream.Read(buffer, 0, buffer.Length)
                If n <= 0 Then Exit Do
                output.Write(buffer, 0, n)
            Loop

            Return output.ToArray()
        End Using
    End Function

    ''' <summary>通过 <see cref="SnappyOutputStream"/> 生成分块流。</summary>
    Friend Function Frame(data As Byte(), Optional chunkSize As Integer = 65536) As Byte()
        Using output As New MemoryStream()
            Using compressor As New SnappyOutputStream(output, leaveOpen:=True, chunkSize:=chunkSize)
                compressor.Write(data, 0, data.Length)
            End Using

            Return output.ToArray()
        End Using
    End Function

    ''' <summary>通过 <see cref="SnappyInputStream"/> 解开分块流。</summary>
    Friend Function Unframe(compressed As Byte()) As Byte()
        Using input As New MemoryStream(compressed)
            Using decompressor As New SnappyInputStream(input, leaveOpen:=True)
                Return ReadAll(decompressor)
            End Using
        End Using
    End Function

    ''' <summary>把 32 位无符号整数写成小端字节序。</summary>
    Friend Function UInt32LE(value As UInteger) As Byte()
        Return New Byte() {
            CByte(value And &HFFUI),
            CByte((value >> 8) And &HFFUI),
            CByte((value >> 16) And &HFFUI),
            CByte((value >> 24) And &HFFUI)
        }
    End Function

    ''' <summary>构造一个带 1 字节类型 + 3 字节小端长度的分块头和数据。</summary>
    Friend Function MakeChunk(chunkType As Byte, payload As Byte()) As Byte()
        Dim result(4 + payload.Length - 1) As Byte

        result(0) = chunkType
        result(1) = CByte(payload.Length And &HFF)
        result(2) = CByte((payload.Length >> 8) And &HFF)
        result(3) = CByte((payload.Length >> 16) And &HFF)
        Buffer.BlockCopy(payload, 0, result, 4, payload.Length)

        Return result
    End Function

    ''' <summary>构造未压缩数据分块（类型 0x01）。</summary>
    Friend Function MakeUncompressedChunk(data As Byte()) As Byte()
        Dim crc As UInteger = Crc32C.Mask(Crc32C.ComputeHash(data))
        Return MakeChunk(&H1, Concat(UInt32LE(crc), data))
    End Function

    ''' <summary>构造压缩数据分块（类型 0x00）。</summary>
    Friend Function MakeCompressedChunk(data As Byte()) As Byte()
        Dim crc As UInteger = Crc32C.Mask(Crc32C.ComputeHash(data))
        Return MakeChunk(&H0, Concat(UInt32LE(crc), Snappy.Compress(data)))
    End Function

    ''' <summary>使用调用方给定的 CRC 构造数据分块，便于测试校验失败。</summary>
    Friend Function MakeDataChunk(chunkType As Byte, crc As UInteger, payload As Byte()) As Byte()
        Return MakeChunk(chunkType, Concat(UInt32LE(crc), payload))
    End Function

End Module
