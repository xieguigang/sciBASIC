# StreamPack Virtual File System in a Single Binary File

A packable virtual file system that stores many named streams, folders and attributes inside one binary file, an HDF5-like container format used for dataset archives.

## Overview
- Hierarchical container: `StreamPack` (file), `StreamGroup` (folder) and `StreamBlock` (file reference with offset, size, MIME type and attributes).
- Block allocation and buffered reads/writes: `Allocate` reserves a `BufferRegion`; `OpenBlock` returns a seekable `Stream`, or an in-memory `StreamBuffer` that is flushed to the underlying stream on dispose.
- Metadata tree serialisation (`TreeParser` / `TreeWriter`) and attribute storage (`AttributeMetadata`, `LazyAttribute`, `PackAttributeData`, encoded with MessagePack).
- Read-only mounting via `StreamPack.OpenReadOnly`, plus file listing, delete, file size and modification-time helpers.

## Key Types
- `Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem.StreamPack` — the pack file itself; create, open, list, read and delete entries.
- `Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem.StreamGroup` — a folder node inside the pack.
- `Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem.StreamBlock` — a stored file reference (offset, size, MIME type, extension).
- `Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem.StreamBuffer` — buffered write stream flushed on `Dispose`.
- `Microsoft.VisualBasic.DataStorage.HDSPack.Metadata.AttributeMetadata` / `LazyAttribute` — named attribute metadata.
- `Microsoft.VisualBasic.DataStorage.HDSPack.BinaryStream.TreeParser` / `TreeWriter` — (de)serialise the folder/file metadata tree.
- `Microsoft.VisualBasic.DataStorage.HDSPack.Extensions` — `WriteText` / `ReadText` / `ReadBinary` / `LoadStream` helpers and `ListFiles`.

## Quick Start
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

## Package
- Assembly: `Microsoft.VisualBasic.DataStorage.HDSPack`
- TargetFramework: `net10.0`
- Tags: `scibasic;hdspack;virtual-filesystem;stream-pack;archive;binary-format`

## License
GPL-3.0-or-later
