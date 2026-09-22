# 底层二进制数据流读写工具箱

## 引言

自定义二进制文件格式的解析，几乎总会撞上同一批麻烦事：

- **字节序**：同一份 spec 在大小端机器上读出来完全不同；
- **字符串长度前缀**：1 字节 / 2 字节 / 4 字节 / 零结尾，各不相同；
- **压缩**：BZip2、LZW 是常见的内嵌压缩；
- **偏移回填**：写文件时先占位、最后再回填长度或偏移；
- **异构载荷**：XDR、Python pickle、Java 风格的缓冲区语义。

`Microsoft.VisualBasic.Data.BinaryData` 就是为这些「底层琐事」准备的工具层，也是本仓库其它二进制格式读取器（HDF5、netCDF、SQLite3、HDSPack、Feather）的共同基座。

## 设计目标

- **格式可切换**：字节序、字符串格式、日期格式都是**运行时可切换的属性**，而不是编译期分支；
- **零原生依赖**：BZip2 与 LZW 均为纯托管实现；
- **可回填**：写入器支持先保留偏移、写完后再回填，这是生成「长度未知的块」的关键能力。

## 核心特性

- `BinaryDataReader` / `BinaryDataWriter` 扩展了 `BinaryReader` / `BinaryWriter`：可切换的 `ByteOrder`、`BinaryStringFormat` 字符串编码（`ByteLengthPrefix`、`WordLengthPrefix`、`DwordLengthPrefix`、`UInt32LengthPrefix`、`ZeroTerminated`）与 `BinaryDateTimeFormat` 日期编码；
- **纯托管 BZip2**：块压缩 / 解压、Huffman 阶段、MTF+RLE2、DivSufSort 与 CRC32，配合 LZW `Encoder` / `Decoder`；
- **Python pickle 编解码**：`MinimalPicklePickler` / `MinimalPickleUnpickler` 把 Python 的 tuple、set、复数与通用对象图映射到 .NET 类型；
- **Java 风格 `ByteBuffer`**：position / limit / flip / compact 语义；XDR 打包辅助；以及特性驱动的结构映射（`BindAttribute`、`FieldAttribute`、`ReaderProvider`）。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.IO.BinaryDataReader` —— 感知格式的二进制读取器（继承 `BinaryReader`）；
- `Microsoft.VisualBasic.Data.IO.BinaryDataWriter` —— 与之匹配的写入器，提供 `ReserveOffset` 与 `TemporarySeek` 用于偏移回填；
- `Microsoft.VisualBasic.Data.IO.ByteOrder` / `ByteOrderHelper` —— 字节序选择与字节序转换；
- `Microsoft.VisualBasic.Data.IO.ByteBuffer` —— 类 Java 字节缓冲，支持相对与绝对 get / put；
- `Microsoft.VisualBasic.Data.IO.Bzip2.BZip2OutputStream` / `BZip2InputStream` —— 托管 BZip2 压缩流；
- `Microsoft.VisualBasic.Data.IO.LZW.Encoder` / `Decoder` —— LZW 压缩与解压；
- `Microsoft.VisualBasic.Data.IO.Pickle.MinimalPickleUnpickler` / `MinimalPicklePickler` —— Python pickle 解码 / 编码；
- `Microsoft.VisualBasic.Data.IO.Xdr.XdrEncoding` —— XDR（RFC 4506）编码辅助。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.IO

Using bin As New BinaryDataWriter("demo.bin".Open(FileMode.OpenOrCreate, doClear:=True))
    With bin
        .ByteOrder = ByteOrder.BigEndian
        .Write("hello", BinaryStringFormat.ByteLengthPrefix)
        .Write(3.14159)
        .Flush()
    End With
End Using

Using bin As New BinaryDataReader("demo.bin".Open(FileMode.Open, doClear:=False, [readOnly]:=True))
    With bin
        .ByteOrder = ByteOrder.BigEndian
        Dim text As String = .ReadString(BinaryStringFormat.ByteLengthPrefix)
        Dim x As Double = .ReadDouble()
    End With
End Using
```

## 实现要点

- **为什么要把「格式」做成属性**：现实中的文件格式常常是「大小端混用 + 多种字符串编码」，把格式固定成方法名会导致 API 爆炸；做成属性后，一段读取代码可以适配多份 spec。
- **偏移回填的必要性**：许多格式（如 RIFF、SQLite 页）在写入时还不知道后续块的长度，必须先占位再回填；`ReserveOffset` / `TemporarySeek` 正是为此设计。
- **纯托管压缩的成本收益**：牺牲部分吞吐换取零原生依赖，使得同一个包可以在 Linux / macOS / Windows 上以相同行为解析压缩块。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.BinaryData`
- TargetFramework：`net10.0`
- Tags：`scibasic;binary-data;binary-reader;endianness;bzip2;lzw;xdr;pickle;byte-buffer`
- 许可：GPL-3.0-or-later
