Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' 文件格式规范自检与错误处理测试（magic / footer / 列块偏移 / 页面头 / 异常路径）。
''' </summary>
Friend Module InteropTests

    Friend Sub Run()
        Call RunFormatSelfCheck()
        Call RunChunkLayout()
        Call RunStreamRoundtrip()
        Call RunInvalidFile()
        Call RunTruncatedFile()
    End Sub

    Private Sub RunFormatSelfCheck()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "selfcheck.parquet")

        Call pq.write_table(Frames.MakeFlat(64), file, New ParquetOptions With {
            .Compression = Format.CompressionCodec.SNAPPY,
            .RowGroupSize = 32
        })

        Dim meta As Format.FileMetaData = pq.read_metadata(file)

        Call TestSupport.AssertEqual(2, meta.RowGroups.Count, "row group 数量为 2")
        Call TestSupport.AssertEqual(64L, meta.NumRows, "footer 记录的总行数正确")
        Call TestSupport.AssertTrue(meta.CreatedBy IsNot Nothing AndAlso meta.CreatedBy.Contains("Parquet"), "footer 写入 created_by")
        Call TestSupport.AssertEqual(2, meta.Version, "footer 版本号为 2")
        Call TestSupport.AssertEqual(9, meta.Schema.Count, "schema 元素数量（根 + 7 列 + 行名）")

        For Each group As Format.RowGroup In meta.RowGroups
            Call TestSupport.AssertEqual(32L, group.NumRows, "row group 行数正确")
            Call TestSupport.AssertEqual(8, group.Columns.Count, "row group 列块数量正确")
            Call TestSupport.AssertTrue(group.TotalByteSize > 0, "row group 未压缩字节数有效")

            For Each chunk As Format.ColumnChunk In group.Columns
                Call TestSupport.AssertTrue(chunk.MetaData.Encodings.Contains(Format.Encoding.RLE), "列块包含 RLE 级别编码")
                Call TestSupport.AssertTrue(chunk.MetaData.Encodings.Contains(Format.Encoding.PLAIN), "列块包含 PLAIN 数据编码")
                Call TestSupport.AssertTrue(chunk.FileOffset = chunk.MetaData.DataPageOffset, "ColumnChunk.file_offset 指向数据页")
                Call TestSupport.AssertTrue(chunk.MetaData.TotalCompressedSize > 0, "列块压缩字节数有效")
            Next
        Next

        ' 直接在列块偏移处解析页面头，验证偏移量的正确性
        Using input As New FileStream(file, FileMode.Open, FileAccess.Read)
            Dim chunk As Format.ColumnChunk = meta.RowGroups(0).Columns(1)

            input.Seek(chunk.FileOffset, SeekOrigin.Begin)

            Dim header As Format.PageHeader = Format.PageHeader.ReadFrom(New Thrift.ThriftCompactReader(input))

            Call TestSupport.AssertEqual(Format.PageType.DATA_PAGE, header.Type, "列块起始位置是一个数据页")
            Call TestSupport.AssertEqual(64 \ 2, header.DataPage.NumValues, "数据页数值槽位数量等于 row group 行数")
            Call TestSupport.AssertEqual(Format.Encoding.PLAIN, header.DataPage.ValueEncoding, "数据页使用 PLAIN 编码")
            Call TestSupport.AssertEqual(Format.Encoding.RLE, header.DataPage.DefinitionLevelEncoding, "定义级别使用 RLE 编码")
        End Using
    End Sub

    ''' <summary>
    ''' 校验列块与页面在文件中的实际布局：列块首尾相接，最后一个列块结束于 footer 起始位置。
    ''' </summary>
    Private Sub RunChunkLayout()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "layout.parquet")

        Call pq.write_table(Frames.MakeFlat(40), file, New ParquetOptions With {
            .Compression = Format.CompressionCodec.SNAPPY,
            .RowGroupSize = 20
        })

        Dim bytes As Byte() = System.IO.File.ReadAllBytes(file)
        Dim footerLength As Integer = BitConverter.ToInt32(bytes, bytes.Length - 8)
        Dim footerOffset As Long = bytes.Length - 8 - footerLength
        Dim meta As Format.FileMetaData = pq.read_metadata(file)
        Dim position As Long = 4 ' 跳过文件头 magic
        Dim totalChunks As Integer = 0

        For Each group As Format.RowGroup In meta.RowGroups
            For Each chunk As Format.ColumnChunk In group.Columns
                Call TestSupport.AssertEqual(position, chunk.MetaData.DataPageOffset, $"列块偏移量与上一列块首尾相接（{String.Join(".", chunk.MetaData.PathInSchema)}）")

                position = chunk.MetaData.DataPageOffset + chunk.MetaData.TotalCompressedSize
                totalChunks += 1
            Next
        Next

        Call TestSupport.AssertEqual(footerOffset, position, "最后一个列块结束位置等于 footer 起始位置")
        Call TestSupport.AssertEqual(16, totalChunks, "两个 row group 共 16 个列块")
    End Sub

    Private Sub RunStreamRoundtrip()
        Dim df As DataFrame = Frames.MakeFlat(20)
        Dim memory As New MemoryStream()

        Call pq.write_table(df, memory)

        Call TestSupport.AssertTrue(memory.Length > 0, "写入 MemoryStream 成功")

        memory.Seek(0, SeekOrigin.Begin)

        Dim df2 As DataFrame = pq.read_table(memory)

        Call TestSupport.AssertEqual(20, df2.nsamples, "内存流往返行数一致")
        Call TestSupport.AssertTrue(memory.CanRead, "读取后调用方数据流没有被关闭")

        Dim table As ParquetTable = Nothing

        memory.Seek(0, SeekOrigin.Begin)

        table = ParquetReader.ReadTable(memory)

        Call TestSupport.AssertEqual(8, table.Columns.Count, "列式数据表的叶子列数量正确")
        Call TestSupport.AssertTrue(table.GetColumn("score") IsNot Nothing, "可以按 path_in_schema 获取列")
    End Sub

    Private Sub RunInvalidFile()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "invalid.parquet")

        Call System.IO.File.WriteAllBytes(file, System.Text.Encoding.ASCII.GetBytes("THIS-IS-NOT-A-PARQUET-FILE"))

        Call TestSupport.AssertThrows(Of ParquetException)(
            Sub()
                Call pq.read_table(file)
            End Sub,
            "读取非法文件抛出 ParquetException")
    End Sub

    Private Sub RunTruncatedFile()
        Dim dir As String = TestSupport.TempDir()
        Dim good As String = System.IO.Path.Combine(dir, "good.parquet")
        Dim broken As String = System.IO.Path.Combine(dir, "broken.parquet")

        Call pq.write_table(Frames.MakeFlat(30), good)

        Dim bytes As Byte() = System.IO.File.ReadAllBytes(good)
        Dim truncated As Byte() = New Byte(bytes.Length \ 3 - 1) {}

        System.Buffer.BlockCopy(bytes, 0, truncated, 0, truncated.Length)
        Call System.IO.File.WriteAllBytes(broken, truncated)

        Call TestSupport.AssertThrows(Of ParquetException)(
            Sub()
                Call pq.read_table(broken)
            End Sub,
            "读取被截断的文件抛出 ParquetException")
    End Sub

End Module
