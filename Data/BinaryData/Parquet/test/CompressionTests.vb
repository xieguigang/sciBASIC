Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' 三种压缩编码（UNCOMPRESSED / SNAPPY / GZIP）的写入与读回一致性测试。
''' </summary>
Friend Module CompressionTests

    Friend Sub Run()
        Dim codecs As Format.CompressionCodec() = {
            Format.CompressionCodec.UNCOMPRESSED,
            Format.CompressionCodec.SNAPPY,
            Format.CompressionCodec.GZIP
        }
        Dim df As DataFrame = Frames.MakeFlat(500)
        Dim dir As String = TestSupport.TempDir()

        For Each codec As Format.CompressionCodec In codecs
            Dim file As String = System.IO.Path.Combine(dir, $"flat-{codec.ToString.ToLower}.parquet")
            Dim options As New ParquetOptions With {
                .Compression = codec,
                .RowGroupSize = 1000
            }

            Call pq.write_table(df, file, options)

            Dim bytes As Byte() = System.IO.File.ReadAllBytes(file)
            Dim magic As String = System.Text.Encoding.ASCII.GetString(bytes, 0, 4)
            Dim tail As String = System.Text.Encoding.ASCII.GetString(bytes, bytes.Length - 4, 4)

            Call TestSupport.AssertEqual("PAR1", magic, $"{codec}：文件头 magic 正确")
            Call TestSupport.AssertEqual("PAR1", tail, $"{codec}：文件尾 magic 正确")

            Dim meta As Format.FileMetaData = pq.read_metadata(file)
            Dim chunkCodec As Format.CompressionCodec = meta.RowGroups(0).Columns(0).MetaData.Codec

            Call TestSupport.AssertEqual(codec, chunkCodec, $"{codec}：列块压缩算法写入正确")

            Dim df2 As DataFrame = pq.read_table(file)

            Call TestSupport.AssertEqual(df.nsamples, df2.nsamples, $"{codec}：行数一致")
            Call TestSupport.AssertArray(TestSupport.ColumnArray(df, "score"), TestSupport.ColumnArray(df2, "score"), $"{codec}：double 列一致")
            Call TestSupport.AssertArray(TestSupport.ColumnArray(df, "label"), TestSupport.ColumnArray(df2, "label"), $"{codec}：string 列一致")
            Call TestSupport.AssertArray(TestSupport.ColumnArray(df, "id"), TestSupport.ColumnArray(df2, "id"), $"{codec}：int 列一致")
        Next
    End Sub

    Friend Sub RunCompressionRatio()
        Dim df As DataFrame = Frames.MakeFlat(2000)
        Dim dir As String = TestSupport.TempDir()
        Dim rawFile As String = System.IO.Path.Combine(dir, "raw.parquet")
        Dim snappyFile As String = System.IO.Path.Combine(dir, "snappy.parquet")

        Call pq.write_table(df, rawFile, New ParquetOptions With {.Compression = Format.CompressionCodec.UNCOMPRESSED})
        Call pq.write_table(df, snappyFile, New ParquetOptions With {.Compression = Format.CompressionCodec.SNAPPY})

        Dim rawLength As Long = New System.IO.FileInfo(rawFile).Length
        Dim snappyLength As Long = New System.IO.FileInfo(snappyFile).Length

        Call DebugAuditMark(rawLength, snappyLength)
        Call TestSupport.AssertTrue(snappyLength > 0 AndAlso rawLength > 0, "压缩后的文件大小有效")
    End Sub

    Private Sub DebugAuditMark(rawLength As Long, snappyLength As Long)
        Call Microsoft.VisualBasic.ApplicationServices.Debugging.DebugAudit.Mark("parquet:size:uncompressed", rawLength)
        Call Microsoft.VisualBasic.ApplicationServices.Debugging.DebugAudit.Mark("parquet:size:snappy", snappyLength)
    End Sub

End Module
