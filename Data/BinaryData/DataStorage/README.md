# 表格数据二进制序列化（含 SAS XPORT 与 ASN.1 读取）

## 引言

把 `DataFrame` 存成 CSV 的问题是：**列类型丢失、体积大、无法随机访问列**。而存成数据库又太重。

本包给出的方案是：**单个可定位的二进制文件 + JSON 模式尾部**。

- 文件头部是魔数与元信息；
- 中间是每一列的数据块；
- 尾部是一段 JSON 模式描述，记录**每一列的偏移量**。

于是读取时可以直接 `Seek` 到某一列的偏移，只读取需要的列——这对「宽表取少数列」的场景极其重要。

## 设计目标

- **列级随机访问**：模式中记录偏移，读取器按需定位；
- **类型不丢失**：模式记录 `TypeCode`、元素个数、是否标量等；
- **外部格式可导入**：内置 SAS XPORT（XPT）与 ASN.1 两类常见科研数据格式的读取。

## 核心特性

- **二进制表格帧格式**：`FrameWriter.WriteFrame` 写入魔数头、逐列数据块与 JSON 模式尾部；`FrameReader.ReadFrame` 依据模式中的列偏移直接定位并还原 `DataFrame`；
- **类型化列模式**：`Schema` / `VectorSchema` 记录列名、序号、`TypeCode`、元素数量、标量标记、流偏移与属性；
- **SAS XPORT（XPT）导入**：`SASXportConverter`、`SASXportFileIterator`、`XPTReaderUtils` 完成 IBM / IEEE 浮点转换、缺失值与头部元数据解码；
- **ASN.1 解码辅助**：`ASN1.StreamReader`、`ASN1.Index` 用于检查 tag-length-value 编码的字节流。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.IO.FrameReader` —— 把二进制数据帧文件或 SAS XPT 文件读成 `DataFrame`；
- `Microsoft.VisualBasic.Data.IO.FrameWriter` —— 把 `DataFrame` 及其列模式写入可定位流；
- `Microsoft.VisualBasic.Data.IO.Schema` / `VectorSchema` —— 帧级与列级的二进制布局元数据；
- `Microsoft.VisualBasic.Data.IO.Xpt.SASXportConverter` —— 把 XPT 传输文件转换为表格数据；
- `Microsoft.VisualBasic.Data.IO.Xpt.SASXportFileIterator` —— 流式遍历 XPT 文件中的数据集；
- `Microsoft.VisualBasic.Data.IO.Xpt.XPTReaderUtils` —— XPT 底层原语（IBM 浮点转换、日期解析、缺失值处理）；
- `Microsoft.VisualBasic.Data.IO.Xpt.Types.ReadstatType` —— XPT / ReadStat 列类型枚举；
- `Microsoft.VisualBasic.Data.IO.ASN1.StreamReader` —— ASN.1 tag / length / value 读取器。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.IO

Dim df As DataFrame = FrameReader.ReadFrame("data.frame")
Dim sas As DataFrame = FrameReader.ReadSasXPT("demo.xpt")

Using file As Stream = "copy.frame".Open(FileMode.OpenOrCreate, doClear:=True)
    Call df.WriteFrame(file)
End Using
```

## 实现要点

- **为什么把模式放在文件尾部**：写入时列的偏移在写完之前无法确定；先写数据、最后写模式（模式本身含各列偏移），就可以用一次顺序写入完成「带索引」的存储。
- **IBM 浮点转换**：SAS XPT 使用的是 IBM 360 风格的 64 位浮点，与 IEEE 754 不同，必须显式转换，这是 XPT 读取最容易出错的地方。
- **ASN.1 只做读取**：本包对 ASN.1 提供 tag-length-value 遍历能力，用于检查而非生成。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Storage`
- TargetFramework：`net10.0`
- Tags：`scibasic;data-storage;sas-xport;xpt;asn1;dataframe;binary-serialization;frame`
- 许可：GPL-3.0-or-later
