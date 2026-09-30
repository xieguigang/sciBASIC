Option Strict On
Option Explicit On

Imports System.IO
Imports Xunit

''' <summary>
''' LZ4 block and frame decompression tests. The corpus files were produced by
''' the reference python-lz4 encoder, hand-crafted vectors come from the
''' LZ4 block/frame format specification.
''' </summary>
Public Class Lz4Tests

    Private Shared ReadOnly TestData As String = Path.Combine(AppContext.BaseDirectory, "TestData")
    Private Shared ReadOnly SharedDir As String = Path.Combine(TestData, "shared")
    Private Shared ReadOnly Lz4Dir As String = Path.Combine(TestData, "lz4")

    ' ------------------------------------------------------------------ corpus

    <Fact>
    Public Sub FrameCorpus_AllFiles_DecodeToTheOriginalBytes()
        Dim files = Directory.GetFiles(Lz4Dir, "*.lz4frame")
        Assert.NotEmpty(files)
        For Each f In files
            Dim name = Path.GetFileName(f)
            Dim original = File.ReadAllBytes(Path.Combine(SharedDir, name.Split("."c)(0) & ".bin"))
            Dim result = Lz4.Decompress(File.OpenRead(f))
            Assert.True(result.SequenceEqual(original), $"frame mismatch: {name}")
        Next
    End Sub

    <Fact>
    Public Sub BlockCorpus_AllFiles_DecodeToTheOriginalBytes()
        Dim files = Directory.GetFiles(Lz4Dir, "*.lz4block")
        Assert.NotEmpty(files)
        For Each f In files
            Dim name = Path.GetFileName(f)
            Dim original = File.ReadAllBytes(Path.Combine(SharedDir, name.Split("."c)(0) & ".bin"))
            Dim compressed = File.ReadAllBytes(f)
            Dim result = Lz4.DecodeBlock(compressed, original.Length)
            Assert.True(result.SequenceEqual(original), $"block mismatch: {name}")
        Next
    End Sub

    <Theory>
    <InlineData(1)>
    <InlineData(3)>
    <InlineData(7)>
    <InlineData(65536)>
    Public Sub FrameCorpus_ChunkedReads_Match(chunkSize As Integer)
        Dim f = Path.Combine(Lz4Dir, "text500k.default.lz4frame")
        Dim original = File.ReadAllBytes(Path.Combine(SharedDir, "text500k.bin"))
        Using stream As New Lz4Stream(File.OpenRead(f))
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
    Public Sub ReadByte_WalksTheWholeFrame()
        Dim f = Path.Combine(Lz4Dir, "text64k.hc16.lz4frame")
        Dim original = File.ReadAllBytes(Path.Combine(SharedDir, "text64k.bin"))
        Using stream As New Lz4Stream(File.OpenRead(f))
            Dim ms As New MemoryStream()
            Do
                Dim b = stream.ReadByte()
                If b < 0 Then Exit Do
                ms.WriteByte(CByte(b))
            Loop
            Assert.True(ms.ToArray().SequenceEqual(original))
        End Using
    End Sub

    ' ----------------------------------------------------- hand-crafted vectors

    <Fact>
    Public Sub Block_LiteralsOnly()
        ' token 0x40 = literal length 4, no match follows
        Dim src As Byte() = {&H40, 65, 66, 67, 68}
        Assert.Equal("ABCD", System.Text.Encoding.ASCII.GetString(Lz4.DecodeBlock(src, 4)))
    End Sub

    <Fact>
    Public Sub Block_ExtendedLiteralLength()
        ' token 0xF0 + one extension byte (5) => 20 literals
        Dim literals = System.Text.Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRST")
        Dim src(21) As Byte
        src(0) = &HF0
        src(1) = 5
        Array.Copy(literals, 0, src, 2, literals.Length)
        Assert.Equal(literals, Lz4.DecodeBlock(src, literals.Length))
    End Sub

    <Fact>
    Public Sub Block_RunLengthWithOffsetOne()
        ' token 0x1D = 1 literal + match of (13+4) bytes, offset 1
        Dim src As Byte() = {&H1D, 65, &H1, &H0}
        Dim result = Lz4.DecodeBlock(src, 18)
        Assert.Equal(New String("A"c, 18), System.Text.Encoding.ASCII.GetString(result))
    End Sub

    <Fact>
    Public Sub Block_OverlappingMatchWithOffsetThree()
        ' token 0x63 = 6 literals + match of (3+4) bytes, offset 3
        Dim src As Byte() = {&H63, 97, 98, 99, 100, 101, 102, &H3, &H0}
        Dim result = Lz4.DecodeBlock(src, 13)
        Assert.Equal("abcdefdefdefd", System.Text.Encoding.ASCII.GetString(result))
    End Sub

    <Fact>
    Public Sub Block_ExtendedMatchLength()
        ' token 0x1F = 1 literal + match base 19, extensions 255 + 26 => 300 bytes, offset 1
        Dim src As Byte() = {&H1F, 88, &H1, &H0, &HFF, &H1A}
        Dim result = Lz4.DecodeBlock(src, 301)
        Assert.Equal(301, result.Length)
        Assert.All(result, Sub(b) Assert.Equal(88, b))
    End Sub

    <Fact>
    Public Sub Block_EmptyInput_DecodesToEmpty()
        Assert.Empty(Lz4.DecodeBlock({}, 0))
    End Sub

    <Fact>
    Public Sub Block_ZeroOffset_IsRejected()
        Dim src As Byte() = {&H40, 65, 66, 67, 68, &H0, &H0}
        Assert.Throws(Of InvalidDataException)(Function() Lz4.DecodeBlock(src, 8))
    End Sub

    <Fact>
    Public Sub Block_OffsetBeyondWindow_IsRejected()
        Dim src As Byte() = {&H20, 65, 66, &H3, &H0}
        Assert.Throws(Of InvalidDataException)(Function() Lz4.DecodeBlock(src, 6))
    End Sub

    ' -------------------------------------------------------- frame behaviour

    <Fact>
    Public Sub Frame_MinimalHandCrafted()
        Dim frame = BuildFrame(flg:=&H60, bd:=&H40,
                               blockWords:={7UI, 0UI},
                               blocks:={New Byte() {&H60, 65, 66, 67, 68, 69, 70}, New Byte() {}})
        Using ms As New MemoryStream(frame)
            Assert.Equal("ABCDEF", System.Text.Encoding.ASCII.GetString(Lz4.Decompress(ms)))
        End Using
    End Sub

    <Fact>
    Public Sub Frame_StoredBlock()
        ' stored block: high bit of the size word set, data is literal
        Dim frame = BuildFrame(flg:=&H60, bd:=&H40,
                               blockWords:={&H80000000UI Or 6UI, 0UI},
                               blocks:={System.Text.Encoding.ASCII.GetBytes("ABCDEF"), New Byte() {}})
        Using ms As New MemoryStream(frame)
            Assert.Equal("ABCDEF", System.Text.Encoding.ASCII.GetString(Lz4.Decompress(ms)))
        End Using
    End Sub

    <Fact>
    Public Sub Frame_SkippableFrameIsSkipped()
        Dim minimal = BuildFrame(flg:=&H60, bd:=&H40,
                                 blockWords:={7UI, 0UI},
                                 blocks:={New Byte() {&H60, 65, 66, 67, 68, 69, 70}, New Byte() {}})
        Dim junk As Byte() = {&H50, &H2A, &H4D, &H18, &H4, &H0, &H0, &H0, &HDE, &HAD, &HBE, &HAF}
        Using ms As New MemoryStream(junk.Concat(minimal).ToArray())
            Assert.Equal("ABCDEF", System.Text.Encoding.ASCII.GetString(Lz4.Decompress(ms)))
        End Using
    End Sub

    <Fact>
    Public Sub Frame_ConcatenatedFrames()
        Dim one = BuildFrame(flg:=&H60, bd:=&H40,
                             blockWords:={7UI, 0UI},
                             blocks:={New Byte() {&H60, 65, 66, 67, 68, 69, 70}, New Byte() {}})
        Using ms As New MemoryStream(one.Concat(one).ToArray())
            Assert.Equal("ABCDEFABCDEF", System.Text.Encoding.ASCII.GetString(Lz4.Decompress(ms)))
        End Using
    End Sub

    <Fact>
    Public Sub Frame_ContentChecksumMismatch_IsRejected()
        ' content-checksum frame for "ABCDEFabcdef" with the checksum incremented by one
        Dim block As Byte() = {&HC0, 65, 66, 67, 68, 69, 70, 97, 98, 99, 100, 101, 102}
        Dim content = System.Text.Encoding.ASCII.GetString(block, 1, 12)
        Dim checksum = XxHash32.ComputeHash(System.Text.Encoding.ASCII.GetBytes(content), 0, 12)
        Dim frame = BuildFrame(flg:=&H64, bd:=&H40,
                               blockWords:={CUInt(block.Length), 0UI},
                               blocks:={block, New Byte() {}},
                               contentChecksum:=checksum + 1UI)
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(frame)
                    Return Lz4.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Frame_HeaderChecksumMismatch_IsRejected()
        Dim frame = BuildFrame(flg:=&H60, bd:=&H40,
                               blockWords:={7UI, 0UI},
                               blocks:={New Byte() {&H60, 65, 66, 67, 68, 69, 70}, New Byte() {}})
        frame(6) = CByte(frame(6) Xor 1)       ' corrupt HC
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(frame)
                    Return Lz4.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Frame_ContentChecksumCorruptedInCorpus_IsRejected()
        Dim corpusFile = Path.Combine(Lz4Dir, "text64k.nosize_cc.lz4frame")
        Dim data = File.ReadAllBytes(corpusFile)
        data(data.Length - 1) = CByte(data(data.Length - 1) Xor &HFF)
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(data)
                    Return Lz4.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Frame_TruncatedStream_IsRejected()
        Dim data = File.ReadAllBytes(Path.Combine(Lz4Dir, "text64k.default.lz4frame"))
        Dim cut = data.Take(data.Length \ 2).ToArray()
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(cut)
                    Return Lz4.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Frame_UnknownMagic_IsRejected()
        Dim garbage As Byte() = {&H11, &H11, &H11, &H11, &H60, &H40, &H0}
        Assert.Throws(Of InvalidDataException)(
            Function()
                Using ms As New MemoryStream(garbage)
                    Return Lz4.Decompress(ms)
                End Using
            End Function)
    End Sub

    <Fact>
    Public Sub Frame_EmptyInputStream_ReturnsNoData()
        Using ms As New MemoryStream()
            Assert.Empty(Lz4.Decompress(ms))
        End Using
    End Sub

    <Fact>
    Public Sub Frame_LegacyFormat()
        ' legacy magic + [4-byte LE compressed size][lz4 block]
        Dim payload = System.Text.Encoding.ASCII.GetBytes("ABCDEFabcdef")
        Dim block(payload.Length) As Byte
        block(0) = &HC0    ' 12 literals, no match
        Array.Copy(payload, 0, block, 1, payload.Length)

        Dim ms As New MemoryStream()
        ms.Write({&H2, &H21, &H4C, &H18}, 0, 4)
        ms.Write(WordLE(CUInt(block.Length)), 0, 4)
        ms.Write(block, 0, block.Length)
        ms.Position = 0

        Using stream As New Lz4Stream(ms)
            Dim result As New MemoryStream()
            stream.CopyTo(result)
            Assert.Equal("ABCDEFabcdef", System.Text.Encoding.ASCII.GetString(result.ToArray()))
        End Using
    End Sub

    ' ------------------------------------------------------------ stream rules

    <Fact>
    Public Sub Stream_ReportedCapabilities()
        Using stream As New Lz4Stream(New MemoryStream())
            Assert.True(stream.CanRead)
            Assert.False(stream.CanSeek)
            Assert.False(stream.CanWrite)
            Assert.Throws(Of NotSupportedException)(Function() stream.Length)
            Assert.Throws(Of NotSupportedException)(Sub() stream.Position = 0)
            Assert.Throws(Of NotSupportedException)(Sub() stream.Write({}, 0, 0))
        End Using
    End Sub

    <Fact>
    Public Sub Stream_Dispose_ClosesTheUnderlyingStream_UnlessLeaveOpen()
        Dim inner As New MemoryStream()
        Using New Lz4Stream(inner)
        End Using
        Assert.Throws(Of ObjectDisposedException)(Function() inner.ReadByte())

        Dim inner2 As New MemoryStream()
        Using New Lz4Stream(inner2, leaveOpen:=True)
        End Using
        Assert.Equal(-1, inner2.ReadByte())   ' still readable
    End Sub

    ' -------------------------------------------------------------- helpers

    Friend Shared Function WordLE(v As UInteger) As Byte()
        Return {CByte(v And &HFFUI), CByte((v >> 8) And &HFFUI), CByte((v >> 16) And &HFFUI), CByte((v >> 24) And &HFFUI)}
    End Function

    Friend Shared Function BuildFrame(flg As Byte, bd As Byte,
                                       blockWords As UInteger(),
                                       blocks As Byte()(),
                                       Optional contentChecksum As UInteger? = Nothing) As Byte()
        Dim ms As New MemoryStream()
        ms.Write({&H4, &H22, &H4D, &H18}, 0, 4)   ' frame magic, little endian

        Dim header As New List(Of Byte) From {flg, bd}
        ms.WriteByte(flg)
        ms.WriteByte(bd)

        If (flg And &H8) <> 0 Then
            ' content size present: always write the length of the blocks' payload
            Dim size As ULong = 0UL
            For Each b In blocks
                size += CULng(b.Length)
            Next
            For i = 0 To 7
                Dim bb = CByte((size >> (8 * i)) And &HFFUL)
                ms.WriteByte(bb)
                header.Add(bb)
            Next
        End If

        Dim hc = CByte((XxHash32.ComputeHash(header.ToArray(), 0, header.Count) >> 8) And &HFFUI)
        ms.WriteByte(hc)

        For i = 0 To blocks.Length - 1
            Dim w = WordLE(blockWords(i))
            ms.Write(w, 0, 4)
            ms.Write(blocks(i), 0, blocks(i).Length)
        Next

        If contentChecksum.HasValue Then
            Dim w = WordLE(contentChecksum.Value)
            ms.Write(w, 0, 4)
        End If

        Return ms.ToArray()
    End Function

End Class
