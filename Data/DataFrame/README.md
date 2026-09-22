# 表格数据框与 CSV / ARFF 存储提供者框架

## 引言

在 R 语言里，`data.frame` 是数据分析的通用货币；在 .NET 里却没有等价物。本包为 `sciBASIC#` 提供了这个缺失的构件：

- **列式内存表**（`DataFrame` / `FeatureVector`），支持行名、切片、转置、并集与数值运算；
- **文本存储**：CSV / TSV 文档模型与 ARFF（Weka）；
- **对象映射**：通过反射把 .NET 类映射为表格的行与列，实现 `Load(Of T)` / `Save(Of T)`。

## 设计目标

- **面向大文件**：CSV 读取提供缓冲式 `DataStream` / `WriteStream` / `BatchQueue`，为超大表格设计；
- **对象即表格**：任何具备列特性的类型都可以直接读写为表格；
- **多种存储后端**：文本（CSV/ARFF）、HTML、MySQL 共用同一套表格抽象。

## 核心特性

- `DataFrame` / `FeatureVector` 提供类 R 的列式矩阵：行名、切片、转置、并集与数值辅助；
- **CSV / TSV 文档模型**（`File`、`RowObject`、`RowTokenizer`、`RowIterator`），配缓冲式读写器（`DataStream`、`WriteStream`、`BatchQueue`）以应对超大文件；
- **ARFF（Weka）读写器**、MySQL 辅助、HTML 写出器与通用表抽象（`DataSet`、`EntityObject`、`Table`）；
- **反射存储提供者**：`ColumnAttribute`、`CollectionAttribute`、`MetaAttribute`、`SchemaProvider`、`RowBuilder` / `RowWriter`，以及 `Load(Of T)` / `Save(Of T)` 扩展。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.Framework`（根） | `DataFrame`、`Mappings` 与扩展方法 |
| `....Framework.IO` / `.IO.CSVFile` / `.IO.ARFFText` | 通用表格抽象、CSV 加载与分词、ARFF 读写 |
| `....Framework.IO.Linq` | 流式 LINQ 风格算子（`DataStream`、`WriteStream`、`BatchQueue`） |
| `....Framework.DataSet` / `.DataSet.Excel` | DataSet 扩展（行、并集、HTML、矩阵、MySQL、R 辅助）与 Excel 坐标 / 数值函数 |
| `....Framework.StorageProvider`（+ `.ComponentModels` / `.Reflection`） | 存储提供者抽象、组件模型与反射桥接 |

## 关键类型与 API

- `Microsoft.VisualBasic.Data.Framework.DataFrame` —— 具名特征列的类 R 数据框，支持 `read_csv` / `read_arff`；
- `...Data.Framework.DataFrame.FeatureVector` —— 单个类型化列向量（标量或数组值）；
- `...Data.Framework.IO.File` —— 内存中的 CSV / TSV 文档，提供 `Load` 与 `Save`；
- `...Data.Framework.IO.RowObject` —— 单条解析后的 CSV 行，支持按 token 访问；
- `...Data.Framework.IO.EntityObject` —— 具名属性行对象，作为通用表行；
- `...Data.Framework.StorageProvider.ComponentModels.SchemaProvider` —— .NET 类型在 CSV 布局下的反射模式；
- `...Data.Framework.StorageProvider.Reflection.Reflector` —— 对象集合的 `Load(Of T)` / `Save(Of T)` 入口；
- `...Data.Framework.IO.Linq.DataStream` —— 超大规模表格文本的缓冲读取器。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO

Dim df As DataFrame = DataFrame.read_csv("./iris.csv")
Console.WriteLine(df.dims)

Dim csv As File = File.Load("./iris.csv")
For Each row As RowObject In csv
    Console.WriteLine(row.AsLine())
Next
```

## 实现要点

- **列式 vs 行式**：列式存储让「按列统计」这一最常见的分析操作变得廉价；本包在数值列上直接以数组承载，避免逐格装箱。
- **对象映射为什么要走反射模式**：每次读写都反射属性代价高昂，`SchemaProvider` 缓存列映射（列名、序号、类型转换器），使对象集合的批量读写接近手写代码的效率。
- **流式与内存的边界**：中小表用 `File`（内存模型）即可；超大表应使用 `IO.Linq` 的流式算子，逐批处理而不整体载入。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Framework`
- TargetFramework：`net10.0`
- Tags：`scibasic;dataframe;dataset;csv;arff;storage-provider;tabular-data;reflection`
- 许可：GPL-3.0-or-later
