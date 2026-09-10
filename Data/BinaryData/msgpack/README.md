# MessagePack Binary Object Serialization Implementation

Serializes and deserializes .NET objects to and from the MessagePack binary format, with reflection based member schemas, `MessagePackMember` attributes, nil implication handling and typed collection support.

## Overview
- Object graph (de)serialization through `MsgPackSerializer` (`SerializeObject`, `Deserialize(Of T)`, `DeserializeObject`), backed by a cached reflection schema (`TypeInfo`, `SerializableProperty`).
- Two wire layouts selected by `SerializationContext.SerializationMethod`: compact array layout driven by `MessagePackMemberAttribute` ids, or named map layout over all public read/write properties.
- Full MessagePack format constants and magic-byte helpers (`MsgPackConstants`, `MsgPackFormats`, `FixedArray`, `FixedMap`, `FixedInteger`, `FixedString`, `Bool`), including str8/16/32, array16/32 and map16/32.
- Nil handling per member via `NilImplication`, plus typed list and dictionary collection support.

## Key Types
- `Microsoft.VisualBasic.Data.IO.MessagePack.MsgPackSerializer` — main serializer: `SerializeObject`, `Deserialize(Of T)`, `DeserializeObject`.
- `Microsoft.VisualBasic.Data.IO.MessagePack.SerializationContext` — global context holding the serialization method and per-type serializers.
- `Microsoft.VisualBasic.Data.IO.MessagePack.Serialization.MessagePackMemberAttribute` — marks a member for array-mode serialization with an explicit id.
- `Microsoft.VisualBasic.Data.IO.MessagePack.Serialization.Reflection.TypeInfo` / `SerializableProperty` — cached reflection schema of a serializable type.
- `Microsoft.VisualBasic.Data.IO.MessagePack.Serialization.NilImplication` — controls how nil values map onto members.
- `Microsoft.VisualBasic.Data.IO.MessagePack.Constants.MsgPackFormats` — MessagePack format byte constants.
- `Microsoft.VisualBasic.Data.IO.MessagePack.ObjectExtensions` — `ToMsgPack()` extension method.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization

' map layout: every public read/write property is written by name
MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Map

Dim bin As Byte() = MsgPackSerializer.SerializeObject(New Sample With {.Name = "a", .Value = 42})
Dim obj As Sample = MsgPackSerializer.Deserialize(Of Sample)(bin)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.IO.MessagePack`
- TargetFramework: `net10.0`
- Tags: `scibasic;msgpack;messagepack;serialization;binary-format;deserialization`

## License
GPL-3.0-or-later
