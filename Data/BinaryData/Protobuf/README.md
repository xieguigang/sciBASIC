# Microsoft.VisualBasic.Data.IO.Protobuf

A pure managed Protocol Buffers toolkit for sciBASIC#: wire format IO, a `.proto` schema parser, attribute-based object serialization and entity source-code generation.

## Features

- varint, zigzag, fixed32/fixed64, length-delimited fields and unknown-field preservation
- proto2 / proto3 parser for messages, enums, nested types, services and options
- reflection-based serialization driven by `ProtoContract` / `ProtoMember` attributes with a cached type model
- entity class generation in Visual Basic .NET and C# from a parsed `.proto` descriptor tree
- configurable unknown-field behavior and custom `ProtoDataFormat` encodings
