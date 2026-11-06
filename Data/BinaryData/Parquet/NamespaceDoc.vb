Option Strict On
Option Explicit On
Option Infer On

''' <summary>
''' Apache Parquet 列式存储格式的纯 VB.NET 实现。
''' <para>
''' 主要功能：
''' </para>
''' <list type="bullet">
''' <item><description>Thrift compact protocol 实现的 footer / page header 元数据编解码；</description></item>
''' <item><description>PLAIN / RLE (bit-packed hybrid) / 词典 / DELTA / BYTE_STREAM_SPLIT 等编码；</description></item>
''' <item><description>UNCOMPRESSED / SNAPPY / GZIP 页面压缩；</description></item>
''' <item><description>多 row group 的写入与合并读取；</description></item>
''' <item><description>LIST / MAP / STRUCT 嵌套结构的读取（展开为 JSON 文本列）与写回。</description></item>
''' </list>
''' <para>
''' 对外入口为 <c>Microsoft.VisualBasic.DataStorage.Parquet.pq</c>：
''' </para>
''' <code>
''' Call pq.write_table(df, "data.parquet")
''' Dim df2 = pq.read_table("data.parquet")
''' </code>
''' </summary>
Friend Module NamespaceDoc

End Module
