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
- **Python pickle 编解码**：`MinimalPicklePickler` / `MinimalPickleUnpickler` 把 Python 的 tuple、set、复数与通用对象图映射到 .NET 类型，并且支持 **.NET CLR 对象的往返**（`Pickle` 保存后再 `Unpickle` 得到原始类型而不是 `PythonObject` 包装器）；
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

## .NET CLR 对象的 pickle 往返

`MinimalPicklePickler` 可以把任意 CLR 对象写成 pickle 字节流，`MinimalPickleUnpickler` 再把它还原成**原始类型**：

```vbnet
Imports Microsoft.VisualBasic.Data.IO.Pickle

Dim bytes As Byte() = MinimalPicklePickler.Pickle(myObject)         ' 也可以 PickleToFile / PickleToBase64
Dim copy As MyClass = DirectCast(MinimalPickleUnpickler.UnpickleFromBytes(bytes), MyClass)

' 或者直接从流/文件读回
Using stream As Stream = New FileStream("object.pickle", FileMode.Open, FileAccess.Read)
    copy = DirectCast(MinimalPickleUnpickler.UnpickleFromStream(stream), MyClass)
End Using
```

### 类型标识

CLR 对象以 `GLOBAL` 操作码携带 `clr:` 前缀的类型标识：

```
GLOBAL 'clr:MyAssembly'
My.Name.Space.MyClass
EMPTY_TUPLE / REDUCE          # 构造一个"未填充"的实例
{ '字段名': 值, ... } / BUILD  # 回填全部实例字段
```

反序列化时按「程序集 + 类型全名」解析；解析失败后再扫描当前已加载的全部程序集（按类型全名匹配），因此程序集改名/升级后只要类型全名不变仍可还原。旧版本写出的「命名空间 + 类名」形式（没有 `clr:` 前缀）也会在默认的 Python 全局符号映射全部落空后做一次兼容解析。

### 状态采集规则

| 项目 | 行为 |
| --- | --- |
| 实例字段 | 全部采集（`Public` 与 `Private`，含基类继承字段），因此 VB 自动属性（编译器生成的 `_Name` 字段）与只读字段都能还原 |
| `<NonSerialized>` 字段 | 跳过 |
| 委托字段 / 委托对象 | 跳过（绑定的是方法与宿主实例，无法跨进程重建） |
| 属性 | 只在字段匹配不到同名键时作为回退（要求可写且非索引器） |
| 构造 | 优先调用无参构造函数；没有时用 `RuntimeHelpers.GetUninitializedObject` 创建空实例，因此**只带参数化构造函数的类型也能还原** |
| 枚举 | 以底层整数值保存，还原时按目标成员的枚举类型转换 |
| Guid / Uri / TimeSpan / DateTimeOffset / DateOnly / TimeOnly / Type | 以字符串保存，还原时按目标成员类型解析回原类型 |
| IntPtr / UIntPtr | 以 64 位整数保存 |
| 集合 | `List(Of T)`、数组、`Dictionary(Of K, V)`、`HashSet(Of T)`、`ObservableCollection(Of T)` 等按目标成员的元素类型逐项转换 |
| 共享引用 / 循环引用 | 由 memo 机制保持（同一实例在还原后仍是同一实例，环形结构不会被无限展开） |

### 实现要点

- **memo 必须用引用相等**：覆盖了 `Equals` 的类如果使用默认比较器，两个不同实例会被误判为同一个 memo 条目，导致还原后的引用关系错乱；`ReferenceComparer` 用 `RuntimeHelpers.GetHashCode` + `ReferenceEquals` 解决。
- **`LONG1`/`LONG4` 是任意精度补码**：例如 `-1` 会被编码成 9 个 `0xFF` 字节。用 64 位整数做 `<<` 位移时移位量会被掩码到 0..63，从而把负数整体算错；编码与解码两端都按补码规则处理最小字节数与符号扩展字节。
- **状态字典而非 pickle 类实例**：`BUILD` 携带的是「字段名 → 值」字典，`ClrObjectCodec.ApplyState` 再按目标成员类型做一次宽松转换（`Long` → `Integer`/枚举/`IntPtr`、`List(Of Object)` → `List(Of T)`/数组等），这也是 Python 侧不可读但 .NET 侧可以无损往返的原因。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.BinaryData`
- TargetFramework：`net10.0`
- Tags：`scibasic;binary-data;binary-reader;endianness;bzip2;lzw;xdr;pickle;byte-buffer`
- 许可：GPL-3.0-or-later
