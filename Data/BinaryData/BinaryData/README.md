# Low-Level Binary Data Stream Reader and Writer Toolkit

A low-level toolkit for reading and writing custom binary file formats: endianness-aware primitives, length-prefixed strings, BZip2/LZW compression, XDR encoding and a minimal Python pickle codec.

## Overview
- `BinaryDataReader` / `BinaryDataWriter` extend `BinaryReader` / `BinaryWriter` with a switchable `ByteOrder`, `BinaryStringFormat` string encodings (`ByteLengthPrefix`, `WordLengthPrefix`, `DwordLengthPrefix`, `UInt32LengthPrefix`, `ZeroTerminated`) and `BinaryDateTimeFormat` date encodings.
- A fully managed BZip2 implementation (block compressor/decompressor, Huffman stages, MTF+RLE2, DivSufSort, CRC32) plus LZW `Encoder` / `Decoder`.
- A minimal Python pickle codec (`MinimalPicklePickler` / `MinimalPickleUnpickler`) that maps Python tuple, set, complex and generic object graphs onto .NET types.
- Java-style `ByteBuffer` with position/limit/flip/compact semantics, XDR packing helpers, and attribute-driven struct mapping helpers (`BindAttribute`, `FieldAttribute`, `ReaderProvider`).

## Key Types
- `Microsoft.VisualBasic.Data.IO.BinaryDataReader` — format-aware binary reader over a `Stream` (inherits `BinaryReader`).
- `Microsoft.VisualBasic.Data.IO.BinaryDataWriter` — matching writer, with `ReserveOffset` and `TemporarySeek` for back-patching offsets.
- `Microsoft.VisualBasic.Data.IO.ByteOrder` / `ByteOrderHelper` — endianness selection and byte-order conversion.
- `Microsoft.VisualBasic.Data.IO.ByteBuffer` — Java-like byte buffer with relative and absolute get/put.
- `Microsoft.VisualBasic.Data.IO.Bzip2.BZip2OutputStream` / `BZip2InputStream` — managed BZip2 compression streams.
- `Microsoft.VisualBasic.Data.IO.LZW.Encoder` / `Decoder` — LZW compression and decompression.
- `Microsoft.VisualBasic.Data.IO.Pickle.MinimalPickleUnpickler` / `MinimalPicklePickler` — Python pickle decode/encode.
- `Microsoft.VisualBasic.Data.IO.Xdr.XdrEncoding` — XDR (RFC 4506) encoding helpers.

## Quick Start
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

## Package
- Assembly: `Microsoft.VisualBasic.Data.BinaryData`
- TargetFramework: `net10.0`
- Tags: `scibasic;binary-data;binary-reader;endianness;compression;pickle`

## License
GPL-3.0-or-later
