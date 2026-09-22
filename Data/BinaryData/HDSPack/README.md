# StreamPack：单文件虚拟文件系统

## 引言

科研数据集的常见困境是「**一个数据集 = 一堆文件**」：主表、索引、附件、元数据分散在目录里，拷贝容易漏、传输容易丢、版本难以对齐。

`StreamPack` 的做法是把它们塞进**一个二进制文件**，并在文件内部维护一棵目录树：

- `StreamPack` = 文件本身；
- `StreamGroup` = 目录节点；
- `StreamBlock` = 文件引用（偏移、长度、MIME 类型、扩展名）。

每个「文件」可以有若干**命名属性**（用 MessagePack 编码），因此元数据也随包同行。

## 设计目标

- **单文件可携带**：数据集与其元数据、附件打包成一个文件；
- **块分配 + 缓冲写**：先 `Allocate` 预留 `BufferRegion`，再以可定位流写入；
- **可只读挂载**：`OpenReadOnly` 让归档不可被意外修改。

## 核心特性

- **层级容器**：`StreamPack`（文件）、`StreamGroup`（目录）、`StreamBlock`（文件引用，含偏移、长度、MIME 类型与属性）；
- **块分配与缓冲读写**：`Allocate` 预留 `BufferRegion`；`OpenBlock` 返回可定位 `Stream`，或返回内存中的 `StreamBuffer`，在 `Dispose` 时写回底层流；
- **元数据树序列化**（`TreeParser` / `TreeWriter`）与属性存储（`AttributeMetadata`、`LazyAttribute`、`PackAttributeData`，用 MessagePack 编码）；
- **只读挂载**：`StreamPack.OpenReadOnly`，并提供文件列举、删除、大小与修改时间等辅助。

## 关键类型与 API

- `...HDSPack.FileSystem.StreamPack` —— 包文件本身：创建、打开、列举、读取与删除条目；
- `...HDSPack.FileSystem.StreamGroup` —— 包内的目录节点；
- `...HDSPack.FileSystem.StreamBlock` —— 已存储的文件引用（偏移、长度、MIME 类型、扩展名）；
- `...HDSPack.FileSystem.StreamBuffer` —— 缓冲写流，`Dispose` 时刷出；
- `...HDSPack.Metadata.AttributeMetadata` / `LazyAttribute` —— 命名属性元数据；
- `...HDSPack.BinaryStream.TreeParser` / `TreeWriter` —— 目录 / 文件元数据树的（反）序列化；
- `...HDSPack.Extensions` —— `WriteText`、`ReadText`、`ReadBinary`、`LoadStream` 与 `ListFiles` 辅助方法。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem

Using pack As StreamPack = StreamPack.CreateNewStream("demo.hds")
    Call pack.WriteText("hello world", "/readme.txt")
End Using

Using pack As StreamPack = StreamPack.OpenReadOnly("demo.hds")
    Dim text As String = pack.ReadText("/readme.txt")
End Using
```

## 实现要点

- **为什么用缓冲流**：直接向底层流随机写入会导致大量小 IO；`StreamBuffer` 先在内存中累积，`Dispose` 时一次性刷出，显著减少磁盘操作。
- **元数据也用 MessagePack**：属性载荷是任意对象，需要紧凑且自描述的编码；复用 `msgpack` 包避免了自研序列化格式。
- **只读挂载的价值**：归档文件一旦生成通常不应再修改；只读模式从 API 层面阻止误写。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataStorage.HDSPack`
- TargetFramework：`net10.0`
- Tags：`scibasic;hdspack;virtual-filesystem;stream-pack;archive;binary-format;container`
- 许可：GPL-3.0-or-later
