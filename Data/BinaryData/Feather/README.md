# Feather 列式数据框文件格式读写器

## 引言

CSV 的问题在于**类型信息丢失**与**解析开销**；Parquet 性能优秀但实现复杂。**Feather** 恰好落在中间：

- 列式存储，按列连续排列，读取单列无需扫描其他列；
- 元数据用 **FlatBuffers** 描述，解析极快；
- 支持分类、日期与时间戳等语义类型。

本包是 Feather 的 VB.NET 实现，并且**自带一个 FlatBuffers 运行时**，因此不引入任何原生依赖。

## 设计目标

- **无原生依赖**：FlatBuffers 运行时内嵌实现；
- **类型化与无类型视图共存**：既可用 `TypedDataFrame(Of ...)` 获得强类型访问，也可用 `DataFrame` 动态访问；
- **语义元数据可往返**：分类、日期、时间戳列的类型信息在读写过程中不丢失。

## 核心特性

- **数据模型**：`Column`、`DataFrame`、`Row` 及其 `Typed*` 变体，加上用于惰性访问的 `ProxyDataFrame` / `ProxyRowEnumerable`，以及描述存储值的 `Value` / `BasisType`；
- **实现层**：列元数据与规格（`ColumnMetadata`、`ColumnSpec`）、类型与值的加宽与转换（`DataWidener`、`ValueCaster`）、枚举映射（`EnumMapper`、`SyntheticEnum`）；
- **FlatBuffers 运行时**：`Impl.Flatbuffers` 提供 `ByteBuffer`、`FlatBufferBuilder`、`Table`、`Struct`、`Offset` 等基础构件；
- **语义元数据**：`Impl.FbsMetadata` 提供 `Column`、`Type`、`Encoding`、`CategoryMetadata`、`DateMetadata`、`TimeMetadata`、`TimestampMetadata`、`TimeUnit`。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.DataStorage.FeatherFormat`（根） | 数据框 / 列 / 行模型与 `FeatherReader` / `FeatherWriter` |
| `....FeatherFormat.Impl` | 列元数据与规格、类型转换、枚举映射、读写适配器 |
| `....FeatherFormat.Impl.Flatbuffers` | 内嵌 FlatBuffers 运行时 |
| `....FeatherFormat.Impl.FbsMetadata` | FlatBuffers 生成的元数据类型 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat

' 读取
Using reader As New FeatherReader("data.feather")
    Dim df As DataFrame = reader.ReadDataFrame()
    Console.WriteLine(df.ColumnNames.JoinBy(", "))
End Using

' 写出
Using writer As New FeatherWriter("out.feather")
    Call writer.WriteDataFrame(df)
End Using
```

## 实现要点

- **为什么 Feather 需要 FlatBuffers**：FlatBuffers 允许**零拷贝读取**——元数据可以直接在字节缓冲上按偏移访问，无需反序列化为对象，这正是 Feather 元数据解析极快的原因。
- **类型化与无类型视图的桥梁**：`ValueCaster` 与 `DataWidener` 负责在「按列宽化的连续数组」与「装箱值」之间转换，使得同一份数据既能以数组形式高效计算，也能以对象形式逐格访问。
- **语义类型必须显式建模**：分类列在物理上只是一个整数索引，语义保存在 `CategoryMetadata` 中；若忽略这层，读回来的就只是数字。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataStorage.FeatherFormat`
- TargetFramework：`net10.0`
- Tags：`scibasic;feather;arrow;columnar;dataframe;flatbuffers;interop`
- 许可：GPL-3.0-or-later
