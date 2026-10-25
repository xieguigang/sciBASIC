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
- **ASN.1 解码辅助**：`ASN1.StreamReader`、`ASN1.Index` 用于检查 tag-length-value 编码的字节流；
- **NumPy 数据文件**：`Numpy.NpyFile` / `Numpy.NpzFile` 读写 `.npy` 单数组与 `.npz` 归档，支持小端/大端、C 序/Fortran 序、数值/布尔/字符串 dtype，并可通过 pickle 还原 `dtype='O'` 的对象数组（数组里套矩阵）。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.IO.FrameReader` —— 把二进制数据帧文件或 SAS XPT 文件读成 `DataFrame`；
- `Microsoft.VisualBasic.Data.IO.FrameWriter` —— 把 `DataFrame` 及其列模式写入可定位流；
- `Microsoft.VisualBasic.Data.IO.Schema` / `VectorSchema` —— 帧级与列级的二进制布局元数据；
- `Microsoft.VisualBasic.Data.IO.Xpt.SASXportConverter` —— 把 XPT 传输文件转换为表格数据；
- `Microsoft.VisualBasic.Data.IO.Xpt.SASXportFileIterator` —— 流式遍历 XPT 文件中的数据集；
- `Microsoft.VisualBasic.Data.IO.Xpt.XPTReaderUtils` —— XPT 底层原语（IBM 浮点转换、日期解析、缺失值处理）；
- `Microsoft.VisualBasic.Data.IO.Xpt.Types.ReadstatType` —— XPT / ReadStat 列类型枚举；
- `Microsoft.VisualBasic.Data.IO.ASN1.StreamReader` —— ASN.1 tag / length / value 读取器；
- `Microsoft.VisualBasic.Data.IO.Numpy.NpyFile` —— `.npy` 读写门面（`Read` / `ReadHeader` / `Write` / `Save`）；
- `Microsoft.VisualBasic.Data.IO.Numpy.NpzFile` —— `.npz` 归档读写（`Read` / `Add` / `Save`）；
- `Microsoft.VisualBasic.Data.IO.Numpy.NpyArray` —— numpy 数组模型（形状、dtype、C 序展平、矩阵转换）；
- `Microsoft.VisualBasic.Data.IO.Numpy.NpyHeader` / `NpyDType` —— 头部字典与 dtype 描述解析；
- `Microsoft.VisualBasic.Data.IO.Numpy.NumpyPickleFactory` —— 把 pickle 流里的 numpy 对象还原为 `NpyArray`。

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

## NumPy 数据文件（.npy / .npz）

`Numpy` 子命名空间提供 numpy 存储格式的完整读写能力，便于与 Python 侧的科研数据交换。

### 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.IO.Numpy

' 只读头部（大文件探测）
Dim header = NpyFile.ReadHeader("./expr.npy")
' npy 1.0 dtype='<f8' shape=(14464, 160) fortran=False offset=128
Console.WriteLine(header.ToString())

' 读取数组
Dim array As NpyArray = NpyFile.Read("./expr.npy")
Dim matrix = array.AsMatrix()          ' 二维数值数组 -> NumericMatrix
Dim values As Double() = array.AsDouble()   ' C 序线性展开

' 写出（默认小端 + C 序，头部按 numpy 约定补齐到 64 字节）
Call NpyFile.Write("./copy.npy", values, array.Shape)
Call NpyFile.Write("./fortran.npy", values, array.Shape, fortranOrder:=True)

' npz 归档
Dim npz As New NpzFile()
Call npz.Add("expression", array)
Call npz.Add("labels", New NpyArray With {.DType = NpyDType.Parse("<U8"), .Shape = {3}, .RawData = utf32Bytes})
Call npz.Save("./demo.npz")

Dim archive = NpzFile.Read("./demo.npz")
Dim restored = archive("expression")
```

### 支持范围

- **dtype**：`b1`（布尔）、`i1/i2/i4/i8`、`u1/u2/u4/u8`、`f2/f4/f8`、`c8/c16`、`S`（定长字节串）、`U`（定长 Unicode，UTF-32 码元）、`V`（原始字节）、`M`/`m`（datetime64 / timedelta64，按 int64 读取）、`O`（Python 对象数组）；结构化记录只解析字段布局与字节宽度，元素值按原始字节使用。
- **字节序**：`<`（小端）、`>`（大端）、`|`（与字节序无关）、`=`（本机）；读取时统一归一化到本机字节序，`AsDouble()` 返回 `Double` 数组时可直接走 `Buffer.BlockCopy` 快路径。
- **内存布局**：`fortran_order=True` 的数组按 numpy `ravel(order='C')` 语义重排后缓存；写出时 `fortranOrder:=True` 会先做 C 序到 F 序的重排，已用真实 numpy 验证 `flags.f_contiguous` 为 `True` 且数值正确。
- **对象数组**：`dtype='O'` 的数据区是 Python pickle（协议 0～5），借助 `Pickle.MinimalPickleUnpickler` 与 `NumpyPickleFactory` 还原内嵌的 `ndarray`、字符串与标量；`NpyArray.AsObjectArray()` 返回的元素中，嵌套数组仍是 `NpyArray`。
- **写入限制**：对象数组写出需要自行生成 numpy 兼容的 pickle 流，目前会抛出 `NotSupportedException` 并给出替代方案。

### 实现要点

- **头部**：`\x93NUMPY` 魔数 + 版本（1.0 / 2.0 / 3.0）+ 长度字段（v1 为 2 字节，v2/v3 为 4 字节）+ Python 字典文本；字典用自写的 Python 字面量解析器（`PyLiteral`）解析，避免正则处理结构化 dtype 的转义与嵌套。
- **`|b1` 是布尔不是 int8**：numpy 的 `dtype.str` 用 `b` 表示布尔、用 `i` 表示有符号整数，读取 `|b1` 必须落到布尔分支，否则整数与逻辑值会互相错位。
- **对象数组的占位构造**：numpy 先 `_reconstruct(ndarray, (0,), b'b')` 造空数组，再用 `__setstate__((version, shape, dtype, is_fortran, data))` 填数据；因此 `Pickle.IPickleObjectFactory.ApplyState` 的 `obj` 参数是 `ByRef`，工厂用它把占位对象整体替换为 `NpyArray`。
- **头部对齐**：`魔数 + 版本 + 长度字段 + 头部文本` 总长度补齐到 64 的整数倍（以空格填充、以换行结尾），这样数据区起始位置满足 numpy 的 mmap 对齐约定。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Storage`
- TargetFramework：`net10.0`
- Tags：`scibasic;data-storage;sas-xport;xpt;asn1;dataframe;binary-serialization;frame`
- 许可：GPL-3.0-or-later
