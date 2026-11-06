Option Strict On
Option Explicit On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Snappy raw block tests
'  Data\BinaryData\BinaryData\test\SnappyTests\SnappyRawTests.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Snappy

Friend Module SnappyRawTests

    ''' <summary>已知向量、CRC32C 标准向量与编码器的规范输出。</summary>
    Friend Sub Vectors()
        Dim crc As UInteger = Crc32C.ComputeHash(Encoding.ASCII.GetBytes("123456789"))
        TestSupport.AssertEqual(&HE3069283UI, crc, "CRC32C 标准向量")
        TestSupport.AssertEqual(&HC78AB0E5UI, Crc32C.Mask(crc), "CRC32C 掩码标准向量")
        TestSupport.AssertEqual(crc, Crc32C.Unmask(Crc32C.Mask(crc)), "CRC32C 掩码可逆")

        For Each vector As RawVector In SnappyVectors.RawVectors()
            Dim actual As Byte() = Snappy.Uncompress(vector.Compressed)

            TestSupport.AssertBytesEqual(vector.Expected, actual, $"原始块向量 {vector.Name}")
            TestSupport.AssertEqual(vector.Expected.Length, Snappy.UncompressedLength(vector.Compressed), $"原始块长度 {vector.Name}")
            TestSupport.AssertTrue(Snappy.IsValidCompressedBuffer(vector.Compressed), $"原始块有效性 {vector.Name}")
        Next

        ' 编码器对简单行程数据应产生与标准实现一致的规范形态
        TestSupport.AssertBytesEqual(TestSupport.HexBytes("05 00 61 01 01"), Snappy.Compress(TestSupport.Repeat(&H61, 5)), "规范编码 run-5")
        TestSupport.AssertBytesEqual(TestSupport.HexBytes("0A 00 61 15 01"), Snappy.Compress(TestSupport.Repeat(&H61, 10)), "规范编码 run-10")
        TestSupport.AssertBytesEqual(TestSupport.HexBytes("0C 00 61 1D 01"), Snappy.Compress(TestSupport.Repeat(&H61, 12)), "规范编码 run-12")
    End Sub

    ''' <summary>逐项校验随机数据、文本数据与二进制数据的往返一致性。</summary>
    Friend Sub RoundTrip()
        Dim sizes As Integer() = {0, 1, 2, 3, 4, 5, 10, 59, 60, 61, 255, 256, 1000, 65535, 65536, 65537, 200000}

        For Each size As Integer In sizes
            Dim data As Byte() = TestSupport.RandomBytes(1000 + size, size)
            Dim compressed As Byte() = Snappy.Compress(data)

            TestSupport.AssertTrue(compressed.Length <= Snappy.MaxCompressedLength(size), $"随机 {size} 字节：压缩长度不超过上界")
            TestSupport.AssertEqual(size, Snappy.UncompressedLength(compressed), $"随机 {size} 字节：长度前缀")
            TestSupport.AssertBytesEqual(data, Snappy.Uncompress(compressed), $"随机 {size} 字节：往返")
            TestSupport.AssertTrue(Snappy.IsValidCompressedBuffer(compressed), $"随机 {size} 字节：缓冲区有效")
        Next

        Dim mixed As Byte() = TestSupport.Concat(
            TestSupport.Repeat(&H41, 70000),
            TestSupport.TextBytes(130000),
            TestSupport.Repeat(&H0, 3000))
        Dim packed As Byte() = Snappy.Compress(mixed)

        TestSupport.AssertTrue(packed.Length < mixed.Length, "可压缩数据应显著变小")
        TestSupport.AssertBytesEqual(mixed, Snappy.Uncompress(packed), "混合数据往返")

        Using input As New MemoryStream(mixed)
            Dim fromStream As Byte() = Snappy.Compress(input)

            TestSupport.AssertBytesEqual(mixed, Snappy.Uncompress(New MemoryStream(fromStream)), "流重载往返")
        End Using

        TestSupport.AssertTrue(Snappy.MaxCompressedLength(100) > 100, "压缩长度上界大于输入长度")
    End Sub

    ''' <summary>非法输入必须被拒绝，而不是产生错误结果。</summary>
    Friend Sub Errors()
        TestSupport.AssertThrows(Of ArgumentNullException)(Sub() Snappy.Compress(CType(Nothing, Byte())), "压缩 Nothing")
        TestSupport.AssertThrows(Of ArgumentNullException)(Sub() Snappy.Uncompress(CType(Nothing, Byte())), "解压 Nothing")
        TestSupport.AssertThrows(Of ArgumentOutOfRangeException)(Sub() Snappy.MaxCompressedLength(-1), "负数长度上界")

        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(New Byte() {}), "空压缩缓冲区")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("80")), "截断的 varint")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("80 80 80 80 80")), "超长 varint")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("05 00 61 01")), "截断的一字节偏移复制")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("05 00 61 01 00")), "偏移为零")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("05 00 61 01 02")), "偏移超出已输出区域")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("05 00 61")), "截断的指令流")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("01 00 61 00 62")), "块末尾存在多余数据")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("01 EC 61")), "字面量超出声明长度")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("01 FC FF FF FF FF")), "超大四字节字面量长度")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.Uncompress(TestSupport.HexBytes("02 00 61 03 FF FF FF FF")), "超大四字节复制距离")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.UncompressedLength(New Byte() {}), "空缓冲区的长度前缀")
        TestSupport.AssertThrows(Of InvalidDataException)(Sub() Snappy.UncompressedLength(TestSupport.HexBytes("80")), "截断的长度前缀")

        TestSupport.AssertFalse(Snappy.IsValidCompressedBuffer(TestSupport.HexBytes("FF")), "非法缓冲区判定")
        TestSupport.AssertFalse(Snappy.IsValidCompressedBuffer(Nothing), "Nothing 缓冲区判定")
    End Sub

End Module
