# 纯托管 HDF5 科学数据文件读取器

## 引言

HDF5 是科学计算领域最常用的容器格式之一，但官方实现依赖原生库（`hdf5.dll` / `libhdf5.so`）。在 .NET 里引入它意味着：部署体积增大、跨平台二进制匹配问题、以及在受限环境中无法加载。

本包用**纯托管代码**解码 HDF5：

- 解析文件结构层（超级块、对象头及其各类消息、堆、符号表、B 树）；
- 支持三种数据集布局（连续、紧凑、分块）与分块流式读取；
- 支持过滤器管线（deflate / shuffle / Fletcher32）；
- 提供稀疏矩阵构建辅助，避免把整块数据集materialize到内存。

## 设计目标

- **零原生依赖**：只需 .NET 基类库；
- **可流式**：分块数据集支持逐块枚举，大数组不必一次性载入；
- **过滤器可扩展**：`IFilter` 允许补充自定义过滤器实现。

## 核心特性

- **结构层纯托管解码**：`Superblock`、`ObjectHeader` 及其消息（dataspace、datatype、data layout、fill value、filter pipeline、link、attribute、group）、`LocalHeap`、`GlobalHeap`、`SymbolTableEntry`、`DataBTree`、`GroupBTree`；
- **数据集布局**：连续（contiguous）、紧凑（compact）与分块（chunked V3）存储，并为大数组提供分块枚举；
- **过滤器管线**：deflate（GZip）、shuffle 与 Fletcher32 校验和，可通过 `IFilter` 扩展；
- **类型化读取器**：基础类型、枚举与变长类型的数据集读取器，另提供 `HDF5Sparse` 辅助在不解包整个数据集的前提下构建稀疏矩阵。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.IO.HDF5.HDF5File` —— 打开 HDF5 文件、解析超级块并按符号名解析对象；
- `Microsoft.VisualBasic.Data.IO.HDF5.HDF5Reader` —— 读取单个数据集，暴露 `data`、`attributes`、`dataSpace`、`dataType`、`layout`；
- `...HDF5.struct.Superblock` —— 文件超级块（版本、地址宽度、根组地址）；
- `...HDF5.struct.Group` —— 由 `DataObjectFacade` 条目构成的组；
- `...HDF5.dataset.ContiguousDataset` / `CompactDataset` / `ChunkedDatasetV3` —— 数据集存储布局；
- `...HDF5.dataset.filters.DeflatePipelineFilter` / `ShufflePipelineFilter` —— 过滤器管线实现；
- `...HDF5.device.BinaryFileReader` —— 基于地址的底层字节读取器；
- `...HDF5.type.FloatingPoint` / `FixedPoint` / `StringData` / `VariableLength` —— HDF5 数据类型模型。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.IO.HDF5

Using h5 As New HDF5File("sample.h5")
    Dim ds As HDF5Reader = h5("/group/dataset")
    Dim values As Object = ds.data
    Dim attrs As Dictionary(Of String, Object) = ds.attributes
End Using
```

## 实现要点

- **对象头消息是核心抽象**：HDF5 把数据集的全部属性（dataspace、datatype、layout、filters…）都编码为对象头里的消息；把这层解析清楚，其余（组、数据集、属性）就能自然推导出来。
- **分块存储与流式读取**：分块布局的意义就在于可以只解压需要的块；`ChunkedDatasetV3` 暴露分块枚举，使超大数组的按需读取成为可能。
- **过滤器顺序很重要**：HDF5 的过滤器是**逆序解码**的（写入顺序的逆序），实现中严格遵循这一约定，否则解压结果会错位。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.IO.HDF5`
- TargetFramework：`net10.0`
- Tags：`scibasic;hdf5;scientific-data;dataset;chunked-storage;filter-pipeline;sparse-matrix;reader`
- 许可：GPL-3.0-or-later
