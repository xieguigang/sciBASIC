Option Strict On
Option Explicit On
Option Infer On

''' <summary>
''' Parquet 读写选项。
''' </summary>
Public Class ParquetOptions

    ''' <summary>
    ''' 页面压缩算法，默认 <see cref="Format.CompressionCodec.SNAPPY"/>。
    ''' </summary>
    Public Property Compression As Format.CompressionCodec = Format.CompressionCodec.SNAPPY

    ''' <summary>
    ''' 每个 row group 的最大行数，默认 1,000,000 行。
    ''' </summary>
    Public Property RowGroupSize As Integer = 1000000

    ''' <summary>
    ''' 是否把 <c>DataFrame.rownames</c> 写为 <c>__index__</c> 列（读取时会自动还原为行名）。
    ''' </summary>
    Public Property WriteIndexColumn As Boolean = True

    ''' <summary>
    ''' 是否写入列的 min / max / null_count 统计信息。
    ''' </summary>
    Public Property WriteStatistics As Boolean = True

    ''' <summary>
    ''' 嵌套列的显式类型描述（列名 → 类型描述 JSON），优先级高于 <c>FeatureVector.attributes</c> 中的声明。
    ''' </summary>
    Public Property NestedColumns As Dictionary(Of String, String)

    ''' <summary>
    ''' 默认的读写选项。
    ''' </summary>
    Public Shared ReadOnly Defaults As New ParquetOptions

    Public Function Clone() As ParquetOptions
        Return New ParquetOptions With {
            .Compression = Compression,
            .RowGroupSize = RowGroupSize,
            .WriteIndexColumn = WriteIndexColumn,
            .WriteStatistics = WriteStatistics,
            .NestedColumns = NestedColumns
        }
    End Function

End Class
