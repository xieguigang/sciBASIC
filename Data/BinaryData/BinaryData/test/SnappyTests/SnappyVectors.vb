Option Strict On
Option Explicit On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Snappy known-answer vectors
'  Data\BinaryData\BinaryData\test\SnappyTests\SnappyVectors.vb
' /********************************************************************************/

Imports System.Collections.Generic
Imports System.Text

''' <summary>一个原始块的已知答案向量。</summary>
Friend NotInheritable Class RawVector

    Friend ReadOnly Name As String
    Friend ReadOnly Compressed As Byte()
    Friend ReadOnly Expected As Byte()

    Friend Sub New(name As String, compressed As Byte(), expected As Byte())
        Me.Name = name
        Me.Compressed = compressed
        Me.Expected = expected
    End Sub

End Class

''' <summary>
''' 手工构造的标准 Snappy 字节序列。这些向量用于验证解码器与官方格式兼容，
''' 而不是仅仅验证“压缩后能自己解开”。
''' </summary>
Friend Module SnappyVectors

    Friend Function RawVectors() As RawVector()
        Dim vectors As New List(Of RawVector)()

        vectors.Add(New RawVector("empty", TestSupport.HexBytes("00"), New Byte() {}))
        vectors.Add(New RawVector("literal-a", TestSupport.HexBytes("01 00 61"), Encoding.ASCII.GetBytes("a")))
        vectors.Add(New RawVector("copy1-run-5", TestSupport.HexBytes("05 00 61 01 01"), TestSupport.Repeat(&H61, 5)))
        vectors.Add(New RawVector("copy1-run-10", TestSupport.HexBytes("0A 00 61 15 01"), TestSupport.Repeat(&H61, 10)))
        vectors.Add(New RawVector("copy1-run-12", TestSupport.HexBytes("0C 00 61 1D 01"), TestSupport.Repeat(&H61, 12)))
        vectors.Add(New RawVector("copy4-single", TestSupport.HexBytes("02 00 61 03 01 00 00 00"), TestSupport.Repeat(&H61, 2)))
        vectors.Add(New RawVector("copy2-run-300", TestSupport.HexBytes("AC 02 00 78 FE 01 00 FE 01 00 FE 01 00 FE 01 00 AA 01 00"), TestSupport.Repeat(&H78, 300)))

        ' literal 长度 60：高 6 位 = 59，直接编码长度减一
        Dim literal60 As Byte() = New Byte(61) {}
        literal60(0) = &H3C
        literal60(1) = &HEC
        For i As Integer = 2 To 61
            literal60(i) = &H4C
        Next
        vectors.Add(New RawVector("literal-60", literal60, TestSupport.Repeat(&H4C, 60)))

        ' literal 长度 61：高 6 位 = 60，后续 1 字节存放长度减一
        Dim literal61 As Byte() = New Byte(63) {}
        literal61(0) = &H3D
        literal61(1) = &HF0
        literal61(2) = &H3C
        For i As Integer = 3 To 63
            literal61(i) = &H4D
        Next
        vectors.Add(New RawVector("literal-61", literal61, TestSupport.Repeat(&H4D, 61)))

        ' literal 长度 300：高 6 位 = 61，后续 2 字节小端存放长度减一
        Dim literal300 As Byte() = New Byte(304) {}
        literal300(0) = &HAC
        literal300(1) = &H02
        literal300(2) = &HF4
        literal300(3) = &H2B
        literal300(4) = &H01
        For i As Integer = 5 To 304
            literal300(i) = &H4E
        Next
        vectors.Add(New RawVector("literal-300", literal300, TestSupport.Repeat(&H4E, 300)))

        Return vectors.ToArray()
    End Function

    ''' <summary>分块流的 stream identifier。</summary>
    Friend Function StreamIdentifier() As Byte()
        Return TestSupport.HexBytes("FF 06 00 00 73 4E 61 50 70 59")
    End Function

    ''' <summary>未压缩的空数据分块（CRC32C 空数据掩码 = 0xA282EAD8）。</summary>
    Friend Function EmptyUncompressedChunk() As Byte()
        Return TestSupport.HexBytes("01 04 00 00 D8 EA 82 A2")
    End Function

    ''' <summary>压缩的空数据分块（原始块为单个 0x00 字节）。</summary>
    Friend Function EmptyCompressedChunk() As Byte()
        Return TestSupport.HexBytes("00 05 00 00 D8 EA 82 A2 00")
    End Function

    ''' <summary>可跳过分块（类型 0x80）。</summary>
    Friend Function SkippableChunk() As Byte()
        Return TestSupport.HexBytes("80 03 00 00 61 62 63")
    End Function

    ''' <summary>填充分块（类型 0xFE）。</summary>
    Friend Function PaddingChunk() As Byte()
        Return TestSupport.HexBytes("FE 02 00 00 78 79")
    End Function

End Module
