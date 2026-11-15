Option Strict On
Option Explicit On
Option Infer On

''' <summary>
''' Apache Parquet 列式存储格式的纯 VB.NET 实现（不依赖 parquet-mr / arrow 等原生库）。
''' <para>
''' 已实现的能力：
''' </para>
''' <list type="bullet">
''' <item><description>Thrift compact protocol 实现的 footer / page header 元数据编解码；</description></item>
''' <item><description>写入：PLAIN 数值编码 + RLE bit-packed hybrid 级别编码 + 可选的列统计信息（null_count / min / max）；</description></item>
''' <item><description>读取：PLAIN、PLAIN_DICTIONARY / RLE_DICTIONARY、DELTA_BINARY_PACKED、DELTA_LENGTH_BYTE_ARRAY、
''' DELTA_BYTE_ARRAY、BYTE_STREAM_SPLIT、RLE（布尔值）以及旧式 BIT_PACKED 级别编码；</description></item>
''' <item><description>UNCOMPRESSED / SNAPPY / GZIP 页面压缩；</description></item>
''' <item><description>DataPageV1 写入，DataPageV1 / DataPageV2 读取，多 row group 的写入与合并读取；</description></item>
''' <item><description>LIST / MAP / STRUCT 嵌套结构的读取（按行展开为 JSON 文本列）与写回（JSON 文本按 schema shredding）。</description></item>
''' </list>
''' <para>
''' 数据类型映射约定：
''' </para>
''' <list type="bullet">
''' <item><description><c>Integer</c>/<c>Short</c>/<c>Byte</c> → <c>INT32</c>，<c>Long</c> → <c>INT64</c>，
''' <c>Single</c> → <c>FLOAT</c>，<c>Double</c> → <c>DOUBLE</c>，<c>Boolean</c> → <c>BOOLEAN</c>；</description></item>
''' <item><description><c>String</c> → <c>BYTE_ARRAY</c> + <c>STRING</c> 逻辑类型；<c>Date</c> → <c>INT64</c> + <c>TIMESTAMP_MILLIS</c>；</description></item>
''' <item><description>行名以 <c>__index__</c> 列存储，读取时自动还原为 <c>DataFrame.rownames</c>；</description></item>
''' <item><description>没有 <c>STRING</c> 逻辑类型的 <c>BYTE_ARRAY</c> 列按 base64 文本承载（二进制安全，可回写）。</description></item>
''' </list>
''' <para>
''' 尚未支持：ZSTD / LZ4 / LZO / BROTLI 压缩、页级索引（page index）、Bloom filter、
''' 列块外置（<c>file_path</c>）、以及无符号整数之外的自定义逻辑类型写入。
''' </para>
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
