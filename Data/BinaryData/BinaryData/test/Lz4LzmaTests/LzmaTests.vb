Option Strict On
Option Explicit On

Imports System.IO
Imports Xunit

''' <summary>
''' LZMA (alone format) decompression tests. The corpus files were produced by
''' CPython's liblzma binding with various lc/lp/pb and dictionary settings.
''' </summary>
Public Class LzmaTests

    Private Shared ReadOnly TestData As String = Path.Combine(AppContext.BaseDirectory, "TestData")
    Private Shared ReadOnly SharedDir As String = Path.Combine(TestData, "shared")
    Private Shared ReadOnly LzmaDir As String = Path.Combine(TestData, "lzma")

    ' ------------------------------------------------------------------ corpus

    <Fact>
    Public Sub Corpus_AllFiles_DecodeToTheOriginalBytes()
        Dim files = Directory.GetFiles(LzmaDir, "*.lzma")
        Assert.NotEmpty(files)
        For Each f In files
            Dim name = Path.GetFileName(f)
            Dim original = File.ReadAllBytes(Path.Combine(SharedDir, name.Split("."c)(0) & ".bin"))
            Dim result = Lzma.Decompress(File.OpenRead(f))
            Assert.True(result.SequenceEqual(original), $"lzma mismatch: {name}")
        Next
    End Sub

    <Theory>
    <InlineData(1)>
    <InlineData(3)>
    <InlineData(7)>
    <InlineData(65536)>
    Public Sub Corpus_ChunkedReads_Match(chunkSize As Integer)
        Dim original = File.ReadAllBytes(Path.Combine(SharedDir, "text500k.bin"))
        Using stream As New LzmaStream(File.OpenRead(Path.Combine(LzmaDir, "text500k.p6.lzma")))
            Dim ms As New MemoryStream()
            Dim buf(chunkSize - 1) As Byte
            Do
                Dim n = stream.Read(buf, 0, buf.Length)
                If n = 0 Then Exit Do
                ms.Write(buf, 0, n)
            Loop
            Assert.True(ms.ToArray().SequenceEqual(original))
        End Using
    End Sub

    <Fact>
    Public Sub Corpus_ReadByte_WalksTheWholeStream()
        Dim original = File.ReadAllBytes(Path.Combine(SharedDir, "mixed150k.bin"))
        Using stream As New LzmaStream(File.OpenRead(Path.Combine(LzmaDir, "mixed150k.p6.lzma")))
            Dim ms As New MemoryStream()
            Do
                Dim b = stream.ReadByte()
                If b < 0 Then Exit Do
                ms.WriteByte(CByte(b))
            Loop
            Assert.True(ms.ToArray().SequenceEqual(original))
        End Using
    End Sub

    ' ------------------------------------------------------- header handling

    <Fact>
    Public Sub Header_KnownSize_IsHonoured()
        ' python writes 0xFFFF.. (unknown) — patch in the true size; the decoder
        ' must stop at that size even though the end marker still follows.
        Dim original = File.ReadAllBytes(Path.Combine(SharedDir, "text64k.bin"))
        Dim data = File.ReadAllBytes(Path.Combine(LzmaDir, "text64k.p6.lzma"))
        Array.Copy(BitConverter.GetBytes(CULng(original.Length)), 0, data, 5, 8)
        Using ms As New MemoryStream(data)
            Assert.True(Lzma.Decompress(ms).SequenceEqual(original))
        End Using
    End Sub

    <Fact>
    Public Sub Header_DeclaredSizeTooLarge_IsRejected()
        Dim data = File.ReadAllBytes(Path.Combine(LzmaDir, "text64k.p6.lzma"))
        Array.Copy(BitConverter.GetBytes(CULng(data.Length * 100)), 0, data, 5, 8)
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(data)
                    Return Lzma.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Header_InvalidPropertiesByte_IsRejected()
        Dim data = File.ReadAllBytes(Path.Combine(LzmaDir, "text64k.p6.lzma"))
        data(0) = 231         ' > 224
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(data)
                    Return Lzma.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Header_TruncatedHeader_IsRejected()
        Dim data = File.ReadAllBytes(Path.Combine(LzmaDir, "text64k.p6.lzma")).Take(7).ToArray()
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(data)
                    Return Lzma.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub TruncatedCompressedData_IsRejected()
        Dim data = File.ReadAllBytes(Path.Combine(LzmaDir, "text500k.p6.lzma"))
        Dim cut = data.Take(data.Length \ 2).ToArray()
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(cut)
                    Return Lzma.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub EmptyInputStream_IsRejected()
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream()
                    Return Lzma.Decompress(ms)
                End Using
            End Function)
    End Sub

    ' ------------------------------------------------------------ stream rules

    <Fact>
    Public Sub Stream_ReportedCapabilitiesAndProperties()
        Using stream As New LzmaStream(File.OpenRead(Path.Combine(LzmaDir, "text64k.p6.lzma")))
            ' preset 6 -> props 0x5D -> lc=3, lp=0, pb=2, dictionary 8 MiB
            Assert.True(stream.CanRead)
            Assert.False(stream.CanSeek)
            Assert.False(stream.CanWrite)
            Assert.Equal(3, stream.Lc)
            Assert.Equal(0, stream.Lp)
            Assert.Equal(2, stream.Pb)
            Assert.Equal(8 * 1024 * 1024, stream.DictionarySize)
            Assert.Null(stream.DeclaredSize)
            Assert.Throws(Of NotSupportedException)(Function() stream.Length)
            Assert.Throws(Of NotSupportedException)(Sub() stream.Position = 0)
            Assert.Throws(Of NotSupportedException)(Sub() stream.Write({}, 0, 0))
        End Using
    End Sub

    <Fact>
    Public Sub Stream_KnownSizeIsExposed()
        Dim original = File.ReadAllBytes(Path.Combine(SharedDir, "text64k.bin"))
        Dim data = File.ReadAllBytes(Path.Combine(LzmaDir, "text64k.p6.lzma"))
        Array.Copy(BitConverter.GetBytes(CULng(original.Length)), 0, data, 5, 8)
        Using stream As New LzmaStream(New MemoryStream(data))
            Assert.Equal(CULng(original.Length), stream.DeclaredSize)
        End Using
    End Sub

    <Fact>
    Public Sub Stream_Dispose_ClosesTheUnderlyingStream_UnlessLeaveOpen()
        Dim corpusFile = Path.Combine(LzmaDir, "text64k.p6.lzma")

        Dim inner As New MemoryStream(File.ReadAllBytes(corpusFile))
        Using New LzmaStream(inner)
        End Using
        Assert.Throws(Of ObjectDisposedException)(Function() inner.ReadByte())

        Dim inner2 As New MemoryStream(File.ReadAllBytes(corpusFile))
        Using New LzmaStream(inner2, leaveOpen:=True)
        End Using
        ' the constructor consumed the 13-byte header plus the 5-byte range
        ' coder init; the underlying stream stays readable and positioned there
        Assert.Equal(18L, inner2.Position)
        Assert.True(inner2.ReadByte() >= 0)
    End Sub

    <Fact>
    Public Sub Stream_ReadAfterFullDrain_ReturnsZeroForever()
        Using stream As New LzmaStream(File.OpenRead(Path.Combine(LzmaDir, "tiny.p0.lzma")))
            Dim first As Byte() = {}
            Using ms As New MemoryStream()
                stream.CopyTo(ms)
                first = ms.ToArray()
            End Using
            Dim buf(15) As Byte
            For i = 1 To 3
                Assert.Equal(0, stream.Read(buf, 0, buf.Length))
            Next
            Assert.NotEmpty(first)
        End Using
    End Sub

    <Fact>
    Public Sub Stream_ZeroCountRead_ReturnsZeroWithoutBlocking()
        Using stream As New LzmaStream(File.OpenRead(Path.Combine(LzmaDir, "tiny.p0.lzma")))
            Assert.Equal(0, stream.Read({}, 0, 0))
        End Using
    End Sub

End Class
