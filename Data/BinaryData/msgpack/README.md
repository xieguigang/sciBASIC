# MessagePack 二进制对象序列化实现

## 引言

消息传输与持久化里，JSON 可读但体积大，Protobuf 需要预编译 schema，而 **MessagePack** 处在两者之间：二进制紧凑、无需额外编译步骤、且格式足够简单。

本包提供完整的 MessagePack 序列化实现，并给使用者一个关键选择：

- **数组布局（compact array）**：按成员注解的 id 顺序紧凑排列，体积最小、但不可自描述；
- **命名映射布局（named map）**：所有公开读写属性以名字为键，自描述、代价是体积。

## 设计目标

- **反射 schema 只付一次代价**：类型信息在首次序列化时缓存（`TypeInfo`、`SerializableProperty`）；
- **布局可切换**：通过全局 `SerializationContext.SerializationMethod` 选择紧凑或自描述；
- **nil 语义可控**：成员级别的 `NilImplication` 决定 nil 如何映射到 .NET 成员。

## 核心特性

- **对象图（反）序列化**：`MsgPackSerializer`（`SerializeObject`、`Deserialize(Of T)`、`DeserializeObject`），底层是缓存的反射 schema（`TypeInfo`、`SerializableProperty`）；
- **两种线格式**：由 `SerializationContext.SerializationMethod` 选择 —— 由 `MessagePackMemberAttribute` id 驱动的紧凑数组布局，或覆盖全部公开读写属性的命名映射布局；
- **完整格式常量与魔数辅助**：`MsgPackConstants`、`MsgPackFormats`、`FixedArray`、`FixedMap`、`FixedInteger`、`FixedString`、`Bool`，覆盖 str8/16/32、array16/32、map16/32；
- **nil 处理与集合支持**：成员级 `NilImplication`，以及类型化的列表与字典集合支持。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.IO.MessagePack.MsgPackSerializer` —— 主序列化器：`SerializeObject`、`Deserialize(Of T)`、`DeserializeObject`；
- `Microsoft.VisualBasic.Data.IO.MessagePack.SerializationContext` —— 全局上下文：序列化方法与逐类型序列化器；
- `...MessagePack.Serialization.MessagePackMemberAttribute` —— 标记成员参与数组模式序列化并指定 id；
- `...MessagePack.Serialization.Reflection.TypeInfo` / `SerializableProperty` —— 可序列化类型的缓存反射 schema；
- `...MessagePack.Serialization.NilImplication` —— 控制 nil 值如何映射到成员；
- `...MessagePack.Constants.MsgPackFormats` —— MessagePack 格式字节常量；
- `...MessagePack.ObjectExtensions` —— `ToMsgPack()` 扩展方法。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.IO.MessagePack

' 紧凑数组布局：成员需标注 MessagePackMember(id)
Dim bytes As Byte() = MsgPackSerializer.SerializeObject(model)
Dim copy As MyModel = MsgPackSerializer.Deserialize(Of MyModel)(bytes)

' 便捷扩展
Dim payload As Byte() = model.ToMsgPack()
```

## 实现要点

- **两种布局的取舍**：数组布局依赖成员 id 稳定（改动顺序会破坏兼容性），换来最小体积；命名映射布局牺牲体积换取向前兼容，适合长期存储或跨版本通信。
- **反射缓存的重要性**：MessagePack 常被用于高频 RPC，如果每次序列化都反射一遍成员，开销会压过编码本身；缓存 schema 是性能的关键。
- **格式常量独立成命名空间**：把字节常量集中管理，便于与官方 MessagePack 规范逐条对照。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.IO.MessagePack`
- TargetFramework：`net10.0`
- Tags：`scibasic;msgpack;messagepack;serialization;binary-format;deserialization;reflection`
- 许可：GPL-3.0-or-later
