Option Strict On
Option Explicit On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Snappy framing format tests
'  Data\BinaryData\BinaryData\test\SnappyTests\SnappyFramedTests.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Snappy

Friend Module SnappyFramedTests

    ''' <summary>分块流的往返、分块类型、跳过分块、分块读取与 leaveOpen 行为。</summary>
    Friend Sub RoundTripAndChunks()
        Dim sizes As Integer() = {0, 1, 3, 1000, 65535, 65536, 65537, 200000}

        For Each size As Integer In sizes
            Dim data As Byte() = TestSupport.RandomBytes(2000 + size, size)
            Dim framed As Byte() = TestSupport.Frame(data)

            TestSupport.AssertBytesEqual(data, TestSupport.Unframe(framed), $"分块流随机 {size} 字节：往返")
        Next

        Dim text As Byte() = TestSupport.TextBytes(150000)
        Dim textFramed As Byte() = TestSupport.Frame(text)

        TestSupport.AssertBytesEqual(SnappyVectors.StreamIdentifier(), TestSupport.Slice(textFramed, 0, 10), "流标识前缀")
        TestSupport.AssertEqual(CByte(&H0), textFramed(10), "可压缩数据使用压缩分块")
        TestSupport.AssertBytesEqual(text, TestSupport.Unframe(textFramed), "文本分块流往返")

        Dim random As Byte() = TestSupport.RandomBytes(99, 1000)
        Dim randomFramed As Byte() = TestSupport.Frame(random)

        TestSupport.AssertEqual(CByte(&H1), randomFramed(10), "不可压缩数据使用未压缩分块")
        TestSupport.AssertBytesEqual(random, TestSupport.Unframe(randomFramed), "随机分块流往返")

        Dim smallChunks As Byte() = TestSupport.Frame(text, chunkSize:=1024)
        TestSupport.AssertBytesEqual(text, TestSupport.Unframe(smallChunks), "自定义分块大小往返")

        For Each readSize As Integer In New Integer() {1, 3, 7, 65536}
            Using input As New MemoryStream(textFramed)
                Using stream As New SnappyInputStream(input)
                    TestSupport.AssertBytesEqual(text, TestSupport.ReadAll(stream, readSize), $"分块读取 {readSize} 字节")
                End Using
            End Using
        Next

        TestSupport.AssertBytesEqual(
            New Byte() {},
            TestSupport.Unframe(TestSupport.Concat(SnappyVectors.StreamIdentifier(), SnappyVectors.EmptyUncompressedChunk())),
            "空未压缩分块")
        TestSupport.AssertBytesEqual(
            New Byte() {},
            TestSupport.Unframe(TestSupport.Concat(SnappyVectors.StreamIdentifier(), SnappyVectors.EmptyCompressedChunk())),
            "空压缩分块")

        Dim payload As Byte() = Encoding.ASCII.GetBytes("hello snappy")
        Dim withSkippable As Byte() = TestSupport.Concat(
            SnappyVectors.StreamIdentifier(),
            SnappyVectors.SkippableChunk(),
            SnappyVectors.PaddingChunk(),
            TestSupport.MakeUncompressedChunk(payload))

        TestSupport.AssertBytesEqual(payload, TestSupport.Unframe(withSkippable), "可跳过分块与填充分块")
        TestSupport.AssertBytesEqual(
            payload,
            TestSupport.Unframe(TestSupport.Concat(SnappyVectors.StreamIdentifier(), TestSupport.MakeCompressedChunk(payload))),
            "压缩分块解码")

        Dim keepOpen As New MemoryStream(textFramed)

        Using stream As New SnappyInputStream(keepOpen, leaveOpen:=True)
            TestSupport.AssertBytesEqual(text, TestSupport.ReadAll(stream), "leaveOpen 读取")
        End Using

        TestSupport.AssertTrue(keepOpen.CanRead, "leaveOpen 保留输入流")
        keepOpen.Dispose()

        Dim closeInput As New MemoryStream(textFramed)

        Using stream As New SnappyInputStream(closeInput, leaveOpen:=False)
        End Using

        TestSupport.AssertFalse(closeInput.CanRead, "Dispose 关闭输入流")

        Using output As New MemoryStream()
            Using compressor As New SnappyOutputStream(output, leaveOpen:=True)
                TestSupport.AssertTrue(compressor.CanWrite, "输出流 CanWrite")
                TestSupport.AssertFalse(compressor.CanRead, "输出流 CanRead")
                TestSupport.AssertFalse(compressor.CanSeek, "输出流 CanSeek")
                TestSupport.AssertThrows(Of NotSupportedException)(
                    Sub()
                        Dim ignored As Integer = compressor.Read(New Byte(0) {}, 0, 0)
                    End Sub,
                    "输出流 Read 抛出异常")
                TestSupport.AssertThrows(Of NotSupportedException)(Sub() compressor.Seek(0, SeekOrigin.Begin), "输出流 Seek 抛出异常")
            End Using

            TestSupport.AssertTrue(output.CanWrite, "leaveOpen 保留输出流")
        End Using
    End Sub

    ''' <summary>分块流的格式错误、校验失败与非法分块类型必须被拒绝。</summary>
    Friend Sub Errors()
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() TestSupport.Unframe(New Byte() {}), "空分块流缺少流标识")
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.MakeUncompressedChunk(Encoding.ASCII.GetBytes("x"))),
            "缺少流标识")
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.HexBytes("FF 06 00 00 73 4E 61 50 70 5A")),
            "错误的流标识载荷")
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.HexBytes("FF 05 00 00 73 4E 61 50 70")),
            "错误的流标识长度")

        Dim data As Byte() = Encoding.ASCII.GetBytes("checksum")
        Dim badCrc As Byte() = TestSupport.MakeUncompressedChunk(data)
        badCrc(4) = CByte(badCrc(4) Xor &HFF)
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.Concat(SnappyVectors.StreamIdentifier(), badCrc)),
            "未压缩分块 CRC 校验失败")

        Dim badCompressedCrc As Byte() = TestSupport.MakeCompressedChunk(data)
        badCompressedCrc(5) = CByte(badCompressedCrc(5) Xor &HFF)
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.Concat(SnappyVectors.StreamIdentifier(), badCompressedCrc)),
            "压缩分块 CRC 校验失败")

        Dim unknown As Byte() = TestSupport.MakeChunk(&H2, New Byte() {})
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.Concat(SnappyVectors.StreamIdentifier(), unknown)),
            "保留的不可跳过分块类型")

        Dim framed As Byte() = TestSupport.Frame(TestSupport.TextBytes(5000))
        TestSupport.AssertThrows(Of InvalidDataException)(
            Sub() TestSupport.Unframe(TestSupport.Slice(framed, 0, framed.Length - 1)),
            "截断的分块流")

        TestSupport.AssertThrows(Of ArgumentOutOfRangeException)(
            Sub()
                Dim bad As SnappyOutputStream = New SnappyOutputStream(New MemoryStream(), chunkSize:=0)
            End Sub,
            "分块大小为零")
        TestSupport.AssertThrows(Of ArgumentOutOfRangeException)(
            Sub()
                Dim bad As SnappyOutputStream = New SnappyOutputStream(New MemoryStream(), chunkSize:=65537)
            End Sub,
            "分块大小过大")
    End Sub

End Module
