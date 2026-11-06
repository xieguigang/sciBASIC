Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' 编码层的独立测试：用规范字节序列验证 RLE / bit-packed、DELTA 系列、BYTE_STREAM_SPLIT 与词典索引解码。
''' </summary>
Friend Module EncodingTests

    Friend Sub Run()
        Call RunBitPacking()
        Call RunBitPackingZeroWidth()
        Call RunLegacyBitPacked()
        Call RunDeltaBinaryPacked()
        Call RunDeltaLengthByteArray()
        Call RunDeltaByteArray()
        Call RunByteStreamSplit()
        Call RunDictionaryIndices()
        Call RunPlainEncoding()
        Call RunCodec()
    End Sub

    Private Sub RunBitPacking()
        For bitWidth As Integer = 1 To 8
            Dim maxValue As Integer = (1 << bitWidth) - 1
            Dim values As Integer() = New Integer(99) {}

            For i As Integer = 0 To values.Length - 1
                values(i) = (i * 7 + i \ 13) Mod (maxValue + 1)
            Next

            values(10) = maxValue
            values(11) = maxValue
            values(12) = maxValue
            values(13) = maxValue
            values(14) = maxValue
            values(15) = maxValue
            values(16) = maxValue
            values(17) = maxValue
            values(18) = maxValue

            Dim buffer As New MemoryStream()

            Call Encodings.BitPacking.WriteRleBitPackedHybrid(buffer, values, bitWidth)

            Dim data As Byte() = buffer.ToArray()
            Dim reader As New Encodings.ByteReader(data)
            Dim decoded As Integer() = Encodings.BitPacking.ReadRleBitPackedHybrid(reader, values.Length, bitWidth)

            Call TestSupport.AssertArray(values, decoded, $"RLE/bit-packed hybrid 往返一致（bit width {bitWidth}）")
        Next
    End Sub

    Private Sub RunBitPackingZeroWidth()
        Dim values As Integer() = New Integer(19) {}
        Dim buffer As New MemoryStream()

        Call Encodings.BitPacking.WriteRleBitPackedHybrid(buffer, values, 0)

        Dim data As Byte() = buffer.ToArray()
        Dim reader As New Encodings.ByteReader(data)
        Dim decoded As Integer() = Encodings.BitPacking.ReadRleBitPackedHybrid(reader, values.Length, 0)

        Call TestSupport.AssertArray(values, decoded, "bit width 为 0 的级别流解码正确")
    End Sub

    Private Sub RunLegacyBitPacked()
        ' 3 bit 宽度、MSB first：[1, 2, 3] -> 0x29 0x80
        Dim data As Byte() = {&H29, &H80}
        Dim reader As New Encodings.ByteReader(data)
        Dim decoded As Integer() = Encodings.BitPacking.ReadBitPackedLegacy(reader, 3, 3)

        Call TestSupport.AssertArray(New Integer() {1, 2, 3}, decoded, "旧式 BIT_PACKED 级别解码正确")
    End Sub

    Private Sub RunDeltaBinaryPacked()
        ' block=128, miniblocks=4, total=5, first=10，minDelta=1，bitwidth=0 -> 10..14
        Dim data As Byte() = {&H80, &H1, &H4, &H5, &H14, &H2, &H0}
        Dim reader As New Encodings.ByteReader(data)
        Dim decoded As Long() = Encodings.DeltaEncoding.ReadDeltaBinaryPacked(reader)

        Call TestSupport.AssertArray(New Long() {10, 11, 12, 13, 14}, decoded, "DELTA_BINARY_PACKED 解码正确")
    End Sub

    Private Sub RunDeltaLengthByteArray()
        Dim buffer As New List(Of Byte)

        Call buffer.AddRange(New Byte() {&H80, &H1, &H4, &H2, &H6, &H1, &H0}) ' 长度流: 3, 2
        Call buffer.AddRange(System.Text.Encoding.ASCII.GetBytes("abcde"))

        Dim reader As New Encodings.ByteReader(buffer.ToArray())
        Dim decoded As Byte()() = Encodings.DeltaEncoding.ReadDeltaLengthByteArray(reader)

        Call TestSupport.AssertEqual(2, decoded.Length, "DELTA_LENGTH_BYTE_ARRAY 元素数量正确")
        Call TestSupport.AssertEqual("abc", System.Text.Encoding.ASCII.GetString(decoded(0)), "DELTA_LENGTH_BYTE_ARRAY 第 1 项正确")
        Call TestSupport.AssertEqual("de", System.Text.Encoding.ASCII.GetString(decoded(1)), "DELTA_LENGTH_BYTE_ARRAY 第 2 项正确")
    End Sub

    Private Sub RunDeltaByteArray()
        Dim buffer As New List(Of Byte)

        Call buffer.AddRange(New Byte() {&H80, &H1, &H4, &H2, &H0, &H4, &H0}) ' 前缀长度: 0, 2
        Call buffer.AddRange(New Byte() {&H80, &H1, &H4, &H2, &H6, &H3, &H0}) ' 后缀长度: 3, 1
        Call buffer.AddRange(System.Text.Encoding.ASCII.GetBytes("abcd"))

        Dim reader As New Encodings.ByteReader(buffer.ToArray())
        Dim decoded As Byte()() = Encodings.DeltaEncoding.ReadDeltaByteArray(reader)

        Call TestSupport.AssertEqual(2, decoded.Length, "DELTA_BYTE_ARRAY 元素数量正确")
        Call TestSupport.AssertEqual("abc", System.Text.Encoding.ASCII.GetString(decoded(0)), "DELTA_BYTE_ARRAY 第 1 项正确")
        Call TestSupport.AssertEqual("abd", System.Text.Encoding.ASCII.GetString(decoded(1)), "DELTA_BYTE_ARRAY 第 2 项正确")
    End Sub

    Private Sub RunByteStreamSplit()
        ' 1.0f -> 00 00 80 3F, 2.0f -> 00 00 00 40
        Dim data As Byte() = {&H0, &H0, &H0, &H0, &H80, &H0, &H3F, &H40}
        Dim reader As New Encodings.ByteReader(data)
        Dim decoded As Single() = Encodings.ByteStreamSplit.ReadSingles(reader, 2)

        Call TestSupport.AssertArray(New Single() {1.0F, 2.0F}, decoded, "BYTE_STREAM_SPLIT 浮点解码正确")
    End Sub

    Private Sub RunDictionaryIndices()
        ' bit width = 1，一个 bit-packed group，索引 0, 1, 0
        Dim data As Byte() = {&H1, &H3, &H2}
        Dim reader As New Encodings.ByteReader(data)
        Dim bitWidth As Integer = CInt(reader.ReadByte())
        Dim decoded As Integer() = Encodings.BitPacking.ReadRleBitPackedHybrid(reader, 3, bitWidth)

        Call TestSupport.AssertArray(New Integer() {0, 1, 0}, decoded, "词典索引（RLE_DICTIONARY）解码正确")
    End Sub

    Private Sub RunPlainEncoding()
        Dim buffer As New MemoryStream()
        Dim doubles As Double() = {1.5R, -2.25R, Double.NaN}
        Dim ints As Integer() = {1, -2, 3}

        Call Encodings.PlainEncoding.WriteDouble(buffer, doubles)
        Call Encodings.PlainEncoding.WriteInt32(buffer, ints)
        Call Encodings.PlainEncoding.WriteBooleans(buffer, New Boolean() {True, False, True})

        Dim reader As New Encodings.ByteReader(buffer.ToArray())

        Call TestSupport.AssertArray(doubles, Encodings.PlainEncoding.ReadDouble(reader, 3), "PLAIN double 往返一致")
        Call TestSupport.AssertArray(ints, Encodings.PlainEncoding.ReadInt32(reader, 3), "PLAIN int32 往返一致")
        Call TestSupport.AssertArray(New Boolean() {True, False, True}, Encodings.PlainEncoding.ReadBooleans(reader, 3), "PLAIN boolean 往返一致")

        Dim texts As Byte()() = {System.Text.Encoding.UTF8.GetBytes("hello"), New Byte() {}}
        Dim textBuffer As New MemoryStream()

        Call Encodings.PlainEncoding.WriteByteArrays(textBuffer, texts)
        Call TestSupport.AssertEqual("hello", System.Text.Encoding.UTF8.GetString(Encodings.PlainEncoding.ReadByteArrays(New Encodings.ByteReader(textBuffer.ToArray()), 2)(0)), "PLAIN byte array 往返一致")
    End Sub

    Private Sub RunCodec()
        Dim data As Byte() = System.Text.Encoding.UTF8.GetBytes(New String("p"c, 1000))

        For Each codec As Format.CompressionCodec In {Format.CompressionCodec.UNCOMPRESSED, Format.CompressionCodec.SNAPPY, Format.CompressionCodec.GZIP}
            Dim compressed As Byte() = ParquetCodec.Compress(codec, data)
            Dim restored As Byte() = ParquetCodec.Decompress(codec, compressed, data.Length)

            Call TestSupport.AssertArray(data, restored, $"{codec} 压缩往返一致")
        Next
    End Sub

End Module
