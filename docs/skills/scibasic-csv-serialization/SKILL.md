---
name: scibasic-csv-serialization
description: Serialize and deserialize CSV/TSV tabular data in the sciBASIC# / GCModeller VB.NET ecosystem using the dataframework-netcore5.vbproj (Microsoft.VisualBasic.Data.Framework) library. This skill should be used when a task involves reading a csv/tsv file into CLR objects with LoadCsv(Of T), writing object collections to csv/tsv with SaveTo, mapping headers to properties with ColumnAttribute or NameMapping, flattening dictionary properties into extra columns, or working with the R-like DataFrame object and the unified NumericTable data carrier and their ReadCsv/WriteCsv/ReadTsv/WriteTsv APIs. Trigger on .vb files calling LoadCsv, SaveTo, DataFrame.read_csv, FileWriter.WriteCsv, NumericTableIO.ReadCsv/WriteCsv, or referencing Data.Framework.
---

# sciBASIC# CSV Serialization

## Overview

The `dataframework-netcore5.vbproj` project (`g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\`, root namespace `Microsoft.VisualBasic.Data.Framework`) provides CSV/TSV read/write capabilities for building data-table-driven analysis pipelines. Despite the "netcore5" filename, it targets `net10.0` and references only `Core.vbproj`. It offers three layers:

1. **Object serialization** — `LoadCsv(Of T)` / `SaveTo(Of T)` map CSV rows to/from CLR object collections via reflection (`Extensions\Extensions.vb`, `Extensions\Write_csv.vb`).
2. **R-like DataFrame** — an in-memory data frame (`DataFrame`) with `read_csv` / `FileWriter.WriteCsv` for arbitrary typed columns.
3. **Unified NumericTable** — a numeric-only matrix (`NumericTable` in Core) with `NumericTableIO.ReadCsv/WriteCsv/ReadTsv/WriteTsv`, designed as the shared algorithm-layer carrier.

Choose the layer that matches the task; all three can interoperate (`NumericTable` ↔ `DataFrame` conversion is lossless).

## Choosing an API (Decision Tree)

1. **Persist a collection of known CLR objects to/from a table file?** → Use object serialization: `path.LoadCsv(Of T)` and `objects.SaveTo(path)`. See `references/object-serialization.md`.
2. **Need header renaming, column selection, or non-default column names?** → Add `<Column>` attributes, `maps As NameMapping`, or `layout`. See `references/mapping-and-attributes.md`.
3. **One CLR property is a `Dictionary(Of String, T)` whose keys should become columns** (e.g. `bmi`, `weight` alongside `name`, `age`)? → Dictionary flattening via `<Meta>` / auto-detection. See `references/dictionary-flattening.md`.
4. **Need R-style column operations on mixed-type data** (column add/delete, row slice, cbind-like `Union`)? → Use `DataFrame`. See `references/dataframe-and-numerictable.md`.
5. **Feeding a numeric algorithm (regression, clustering, embedding)?** → Use `NumericTable` + `NumericTableIO` and `SetLabel` for outputs. See `references/dataframe-and-numerictable.md`.

## Quick Start: Object Serialization

Given this CLR type:

```vbnet
Public Class Person
    Public Property name As String
    Public Property age As Double
    Public Property data As Dictionary(Of String, Double)
End Class
```

Write and read (as in the user example):

```vbnet
Imports Microsoft.VisualBasic.Data.Framework.Extensions   ' LoadCsv / SaveTo extension methods

Dim a As New Person With {
    .name = "a",
    .age = 22,
    .data = New Dictionary(Of String, Double) From {{"bmi", 11}, {"weight", 33}}
}

Call {a}.SaveTo("data.csv")
' Produces:
'   name,age,bmi,weight
'   a,22,11,33

Dim loaded As List(Of Person) = "data.csv".LoadCsv(Of Person)()
```

The dictionary keys `bmi`/`weight` are automatically expanded into extra columns by the writer and recollected into the dictionary by the reader. Reader is safe on missing/empty files: it returns an empty `List(Of T)`, never `Nothing`.

For TSV, pass `tsv:=True` to both `SaveTo` and `LoadCsv`:

```vbnet
Call people.SaveTo("data.tsv", tsv:=True)
Dim people2 = "data.tsv".LoadCsv(Of Person)(tsv:=True)
```

## Quick Start: NumericTable CSV I/O

```vbnet
Imports Microsoft.VisualBasic.Data.Framework.DataFrame   ' NumericTableIO extension methods
Imports Microsoft.VisualBasic.Data

Dim table As New NumericTable(
    features:={ {1.0, 2.0}, {3.0, 4.0} },
    rowNames:={"s1", "s2"},
    featureNames:={"f1", "f2"})

Call table.SetLabel("score", {0.5, 0.8})

Call table.WriteCsv("data.csv")                       ' row names + features + label:score
Dim restored = NumericTableIO.ReadCsv("data.csv")     ' labels restored from label: prefix
```

## Development Workflow

When asked to read/write tabular data with this framework:

1. Identify the target layer using the decision tree; default to object serialization for application data and `NumericTable` for algorithm input.
2. Define the CLR model with public properties; annotate with `<Column>` only when the CSV header differs from the property name or `strict:=True` writing is used.
3. Choose `LoadCsv` options deliberately: `explicit`, `maps`, `tsv`, `encoding`, `skipWhile` for large files. See `references/object-serialization.md`.
4. Choose `SaveTo` options: `strict`, `layout`, `maps`, `metaBlank`, `tsv`, `transpose`, `silent`. See `references/object-serialization.md`.
5. For dictionary/metadata properties, verify `<Meta>` or auto-detection collects the expected keys; use `reorderKeys` / `layout` to control column order. See `references/dictionary-flattening.md`.
6. Build and verify round-trip: save then load and compare values; check the generated header column order.

When debugging CSV issues, check these common failure points:

- **Column names do not match properties**: by default (`explicit:=False`) all simple properties are mapped by name; when `explicit:=True` only `<Column>`-marked properties are read, and `strict:=True` writing only writes marked ones.
- **`maps` direction differs on read vs write**: read `maps` is `Csv.Field -> PropertyInfo.Name` (`NameMapping`); write `maps` is `{meta_define -> custom}` (`Dictionary(Of String, String)`).
- **Dictionary columns missing**: the writer's `RowWriter.CacheIndex` collects dictionary keys across all rows — heterogeneous keys union; missing values filled with `metaBlank`.
- **Wrong column order**: use `layout As Dictionary(Of String, Integer)` (smaller value = earlier) or `reorderKeys` when `EntityObject.ID` must stay first.
- **`fast:=True` parser is strict about quoting** — use it only for clean, comma/tab-split tables.
- **Large datasets**: `SaveTo` (path overload) is documented for small datasets / materialized collections, not LINQ expressions; use `simpleRowIterators:=False` / `skipWhile` or the streaming APIs for big files.
- **Labels lost in NumericTable round-trip**: label columns must carry the `label:` prefix (`DefaultLabelPrefix`) or be listed in `labels:=`.

## Resources

### references/

| File | Contents |
|---|---|
| `object-serialization.md` | Project details, all `LoadCsv` / `SaveTo` overloads and every option (`explicit`, `strict`, `fast`, `encoding`, `maps`, `metaBlank`, `layout`, `tsv`, `transpose`, `skipWhile`, `simpleRowIterators`), usage examples. |
| `mapping-and-attributes.md` | `ColumnAttribute`, `DataFrameColumnAttribute`, `NameMapping`, `MapsHelper`, schema mapping internals, header renaming, custom `IParser`. |
| `dictionary-flattening.md` | How dictionary properties become extra CSV columns on write and are recollected on read (`<Meta>`, `RowWriter.CacheIndex`, `RowBuilder.NonIndexed`, `reorderKeys`, `metaBlank`). |
| `dataframe-and-numerictable.md` | R-like `DataFrame` (`read_csv`, `WriteCsv`, column/row APIs), `NumericTable` (properties, `SetLabel`), `NumericTableIO` read/write functions, `AsDataFrame`/`AsNumericTable` conversions, binary IO. |

### assets/

| Path | Contents |
|---|---|
| `person-model.vb` | Example `Person` class with a `Dictionary(Of String, Double)` property, matching the quick start. |
| `csv-roundtrip.vb` | Runnable-style module demonstrating save/load round-trip, TSV, and `NumericTable` CSV I/O. |

Load a reference file with a search/read tool when its topic is needed; do not load all references up front.
