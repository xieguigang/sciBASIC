Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' 扁平表的写入 / 读取往返测试：类型矩阵、空值、行名、多 row group。
''' </summary>
Friend Module FlatTableTests

    Friend Sub Run()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "flat.parquet")
        Dim df As DataFrame = Frames.MakeFlat(100)

        Call pq.write_table(df, file)

        Call TestSupport.AssertTrue(System.IO.File.Exists(file), "写入后 parquet 文件存在")
        Call TestSupport.AssertTrue(New System.IO.FileInfo(file).Length > 100, "parquet 文件非空")

        Dim df2 As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(100, df2.nsamples, "行数一致")
        Call TestSupport.AssertEqual(7, df2.nfeatures, "列数一致")
        Call TestSupport.AssertEqual(100, df2.rownames.Length, "行名数量一致")
        Call TestSupport.AssertEqual("r0", df2.rownames(0), "行名首项一致")
        Call TestSupport.AssertEqual("r99", df2.rownames(99), "行名末项一致")

        Call TestSupport.AssertEqual(GetType(Integer), df2("id").type, "id 列类型")
        Call TestSupport.AssertEqual(GetType(Long), df2("count").type, "count 列类型")
        Call TestSupport.AssertEqual(GetType(Double), df2("score").type, "score 列类型")
        Call TestSupport.AssertEqual(GetType(Single), df2("weight").type, "weight 列类型")
        Call TestSupport.AssertEqual(GetType(Boolean), df2("flag").type, "flag 列类型")
        Call TestSupport.AssertEqual(GetType(String), df2("label").type, "label 列类型")
        Call TestSupport.AssertEqual(GetType(Date), df2("time").type, "time 列类型")

        For Each name As String In df.featureNames
            Call TestSupport.AssertArray(TestSupport.ColumnArray(df, name), TestSupport.ColumnArray(df2, name), $"{name} 列数值一致")
        Next

        Dim labels As String() = DirectCast(df2("label").vector, String())

        Call TestSupport.AssertTrue(labels(0) Is Nothing, "字符串空值保留为 Nothing")
        Call TestSupport.AssertEqual("row_1", labels(1), "字符串非空值一致")

        ' 时间列（毫秒精度）
        Dim times As Date() = DirectCast(df2("time").vector, Date())

        Call TestSupport.AssertEqual(DateTime.UnixEpoch.AddSeconds(42), times(42), "time 列时间值一致")
    End Sub

    Friend Sub RunMultipleRowGroups()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "flat-groups.parquet")
        Dim df As DataFrame = Frames.MakeFlat(100)
        Dim options As New ParquetOptions With {
            .RowGroupSize = 25,
            .Compression = Format.CompressionCodec.GZIP
        }

        Call pq.write_table(df, file, options)

        Dim meta As Format.FileMetaData = pq.read_metadata(file)

        Call TestSupport.AssertEqual(4, meta.RowGroups.Count, "按行数阈值切分出 4 个 row group")
        Call TestSupport.AssertEqual(100L, meta.NumRows, "footer 中的总行数正确")
        Call TestSupport.AssertEqual(Format.CompressionCodec.GZIP, meta.RowGroups(0).Columns(0).MetaData.Codec, "列块压缩算法为 GZIP")

        Dim df2 As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(100, df2.nsamples, "多 row group 合并后行数一致")
        Call TestSupport.AssertArray(TestSupport.ColumnArray(df, "score"), TestSupport.ColumnArray(df2, "score"), "多 row group 合并后数值一致")
        Call TestSupport.AssertArray(TestSupport.ColumnArray(df, "label"), TestSupport.ColumnArray(df2, "label"), "多 row group 合并后字符串列一致")
    End Sub

    Friend Sub RunRowGroupBoundary()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "flat-boundary.parquet")
        Dim df As DataFrame = Frames.MakeFlat(10)
        Dim options As New ParquetOptions With {
            .RowGroupSize = 10,
            .Compression = Format.CompressionCodec.UNCOMPRESSED
        }

        Call pq.write_table(df, file, options)

        Dim meta As Format.FileMetaData = pq.read_metadata(file)

        Call TestSupport.AssertEqual(1, meta.RowGroups.Count, "恰好整除时只有一个 row group")
        Call TestSupport.AssertEqual(Format.CompressionCodec.UNCOMPRESSED, meta.RowGroups(0).Columns(0).MetaData.Codec, "UNCOMPRESSED 压缩标记正确")

        Dim df2 As DataFrame = pq.read_table(file)

        Call TestSupport.AssertArray(TestSupport.ColumnArray(df, "id"), TestSupport.ColumnArray(df2, "id"), "边界行数往返一致")
    End Sub

    ''' <summary>
    ''' 边界情况：空表、无行名、标量列、单行表。
    ''' </summary>
    Friend Sub RunEdgeCases()
        Dim dir As String = TestSupport.TempDir()

        ' ---- 空表（0 行，保留 schema）----
        Dim empty As New DataFrame

        Call empty.add(New FeatureVector("value", New Double() {}))
        Call empty.add(New FeatureVector("tag", New String() {}))

        Dim emptyFile As String = System.IO.Path.Combine(dir, "empty.parquet")

        Call pq.write_table(empty, emptyFile)

        Dim empty2 As DataFrame = pq.read_table(emptyFile)

        Call TestSupport.AssertEqual(0, empty2.nsamples, "空表读取后行数为 0")
        Call TestSupport.AssertEqual(2, empty2.nfeatures, "空表读取后列结构保留")
        Call TestSupport.AssertEqual(0, DirectCast(empty2("value").vector, Double()).Length, "空表列长度为 0")

        ' ---- 没有行名 ----
        Dim noNames As New DataFrame

        Call noNames.add(New FeatureVector("x", New Double() {1.0R, 2.0R, 3.0R}))

        Dim noNamesFile As String = System.IO.Path.Combine(dir, "nonames.parquet")

        Call pq.write_table(noNames, noNamesFile)

        Dim noNames2 As DataFrame = pq.read_table(noNamesFile)

        Call TestSupport.AssertEqual(3, noNames2.nsamples, "无行名表行数一致")
        Call TestSupport.AssertEqual("0", noNames2.rownames(0), "无行名表读取时生成顺序行名")
        Call TestSupport.AssertEqual("2", noNames2.rownames(2), "无行名表读取时生成顺序行名（末项）")

        Dim meta As Format.FileMetaData = pq.read_metadata(noNamesFile)

        Call TestSupport.AssertEqual(1, meta.Schema.Count - 1, "无行名表不写入 __index__ 列")

        ' ---- 标量列自动扩展 ----
        Dim scalar As New DataFrame

        Call scalar.add(New FeatureVector("id", New Integer() {1, 2, 3, 4}))
        Call scalar.add(New FeatureVector("constant", New Double() {9.5R}))

        Dim scalarFile As String = System.IO.Path.Combine(dir, "scalar.parquet")

        Call pq.write_table(scalar, scalarFile)

        Dim scalar2 As DataFrame = pq.read_table(scalarFile)

        Call TestSupport.AssertEqual(4, scalar2.nsamples, "标量列扩展后行数一致")
        Call TestSupport.AssertArray(New Double() {9.5R, 9.5R, 9.5R, 9.5R}, DirectCast(scalar2("constant").vector, Double()), "标量列扩展为常量列")

        ' ---- 单行表 ----
        Dim oneRow As New DataFrame

        Call oneRow.add(New FeatureVector("flag", New Boolean() {True}))
        Call oneRow.add(New FeatureVector("name", New String() {"only"}))

        oneRow.rownames = New String() {"only_row"}

        Dim singleFile As String = System.IO.Path.Combine(dir, "single.parquet")

        Call pq.write_table(oneRow, singleFile)

        Dim single2 As DataFrame = pq.read_table(singleFile)

        Call TestSupport.AssertEqual(1, single2.nsamples, "单行表行数一致")
        Call TestSupport.AssertEqual("only_row", single2.rownames(0), "单行表行名一致")
        Call TestSupport.AssertEqual(True, DirectCast(single2("flag").vector, Boolean())(0), "单行表布尔值一致")
    End Sub

    Friend Sub RunSchemaAndStatistics()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "flat-schema.parquet")
        Dim df As DataFrame = Frames.MakeFlat(50)

        Call pq.write_table(df, file)

        Dim schema As Schema.ParquetSchema = pq.read_schema(file)
        Dim names As New List(Of String)

        For Each child As Schema.ParquetSchemaNode In schema.Root.Children
            Call names.Add(child.Name)
        Next

        Call TestSupport.AssertTrue(names.Contains("__index__"), "schema 中包含 __index__ 列")
        Call TestSupport.AssertTrue(names.Contains("score"), "schema 中包含 score 列")
        Call TestSupport.AssertEqual(8, schema.Leaves.Count, "schema 叶子数量（7 列 + 行名）")

        Dim score As Schema.ParquetSchemaNode = schema.Root.Select("score")

        Call TestSupport.AssertEqual(Format.ParquetType.[DOUBLE], score.PhysicalType.Value, "score 的物理类型为 DOUBLE")
        Call TestSupport.AssertEqual(Format.FieldRepetitionType.REQUIRED, score.Repetition, "score 列为 REQUIRED（没有空值）")

        Dim label As Schema.ParquetSchemaNode = schema.Root.Select("label")

        Call TestSupport.AssertEqual(Format.FieldRepetitionType.[OPTIONAL], label.Repetition, "label 列为 OPTIONAL（含空值）")
        Call TestSupport.AssertEqual(Format.LogicalType.KindString, label.Logical.Kind, "label 列带 STRING 逻辑类型")

        Dim stats As Format.Statistics = Nothing

        For Each item As Format.ColumnChunk In pq.read_metadata(file).RowGroups(0).Columns
            If item.MetaData.PathInSchema(0) = "score" Then
                stats = item.MetaData.Stats
            End If
        Next

        Call TestSupport.AssertTrue(stats IsNot Nothing, "score 列写入统计信息")
        Call TestSupport.AssertEqual(0L, stats.NullCount.Value, "score 列空值数量为 0")
        Call TestSupport.AssertTrue(stats.MinValue IsNot Nothing AndAlso stats.MaxValue IsNot Nothing, "score 列写入 min/max")
    End Sub

End Module
