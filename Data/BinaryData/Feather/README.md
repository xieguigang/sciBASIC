# Feather Columnar Data Frame File Format Reader and Writer

A VB.NET port of the Feather columnar file format, built on an embedded FlatBuffers runtime, for reading and writing typed and untyped data frames.

## Overview
- Memory-mapped reading via `FeatherReader` (`ReadFromFile`, `ReadFromStream`, `ReadFromBytes`, plus `TryRead*` variants), exposing `RowCount`, `ColumnCount`, row/column maps and lazily coerced `Value` cells.
- Writing via `FeatherWriter` with lazy (default) or eager scheduling (`WriteMode`), typed column blitting, null masks, and variable-sized (string) data areas.
- Typed access: `TypedDataFrame`, `TypedColumn(Of T)`, `TypedRow`, and `ProxyDataFrame`/`ProxyRow` map columns onto .NET types or POCO members.
- Column metadata for categorical, date, time and timestamp columns (`CategoryMetadata`, `DateMetadata`, `TimeMetadata`, `TimestampMetadata`) and an embedded FlatBuffers runtime (`FlatBufferBuilder`, `ByteBuffer`, `Table`, `Struct`).

## Key Types
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.FeatherReader` — creates a `DataFrame` from a Feather file, stream or byte array.
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.FeatherWriter` — writes columns into a Feather file, flushing on `Dispose`.
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.DataFrame` — the untyped in-memory data frame (rows, columns, `Value` cells).
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.Column` / `Row` — untyped column and row access with implicit coercion.
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.TypedColumn(Of T)` — eagerly type-checked, lazily coerced column.
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.Value` — lazily converted cell value inside a data frame.
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl.ColumnSpec` / `ColumnType` — on-disk column type model.
- `Microsoft.VisualBasic.DataStorage.FeatherFormat.FlatBuffers.FlatBufferBuilder` — embedded FlatBuffers writer used to emit Feather metadata.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat

Using writer As New FeatherWriter("demo.feather")
    Call writer.AddColumn("x", {1.0R, 2.0R, 3.0R})
    Call writer.AddColumn("label", {"a", "b", "c"})
End Using

Dim df As DataFrame = FeatherReader.ReadFromFile("demo.feather")
Dim nRows As Long = df.RowCount
Dim col As Column = df("x")
```

## Package
- Assembly: `Microsoft.VisualBasic.DataStorage.FeatherFormat`
- TargetFramework: `net10.0`
- Tags: `scibasic;feather;arrow;columnar;dataframe;flatbuffers`

## License
GPL-3.0-or-later
