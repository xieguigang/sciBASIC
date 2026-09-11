# Data Framework Extension Methods for Object Serialization

Extension helpers for the sciBASIC# data framework: they turn CLR objects into tabular representations (DataTable, linked CSV files, outlined CSV documents) and read them back.

## Overview
- Streams an `IEnumerable(Of T)` into a `System.Data.DataTable`, creating columns from the reflection storage-provider schema and filling rows (with optional column maps, layout and transpose).
- Dumps a complex object graph into a directory of linked CSV tables plus a JSON schema file, and flattens the same graph into a single `EntityObject()` summary table.
- Loads hierarchical "outlined" CSV documents (indent level encoded by leading blank cells) back into typed objects, including sub-table rows.
- Provides a Java-style `Properties` store (text and XML) with comments, defaults, ordering and reflection-based object fill/dump, plus CSV template attributes and helpers.

## Key Types
- `Microsoft.VisualBasic.Data.Framework.DataTableStream` — `StreamTo(Of T)` extension that fills a `DataTable` from an object collection.
- `Microsoft.VisualBasic.Data.Framework.SchemasAPI` — `SaveData(Of T)` and `Summary(Of T)` extensions for complex object persistence.
- `Microsoft.VisualBasic.Data.Framework.Serialize.Writer` — incremental writer that flushes the linked CSV tables on dispose.
- `Microsoft.VisualBasic.Data.Framework.Serialize.ObjectSchema.Schema` — the project JSON schema describing the split tables.
- `Microsoft.VisualBasic.Data.Framework.Serialize.ObjectSchema.Class` / `.Field` — per-type schema model and field/primary-key bindings.
- `Microsoft.VisualBasic.Data.Framework.Outlining.OutliningDataLoader` / `.Builder` — typed loading of indent-based outlined CSV files.
- `Microsoft.VisualBasic.Data.Framework.IO.Properties.Properties` / `.Reflector` — persistent key/value property lists and object mapping.
- `Microsoft.VisualBasic.Data.Framework.TemplateAttribute` / `TemplateHelper` — CSV template metadata and helpers.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.Serialize.ObjectSchema

' dump a complex object graph into a folder of linked csv tables + schema json
Call compounds.SaveData("./export/")

' or fill a DataTable from a plain object collection
Dim table As New System.Data.DataTable()
Call compounds.StreamTo(table)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.Framework.Extensions`
- TargetFramework: `net10.0`
- Tags: `scibasic;dataframe;extensions;serialization;datatable;outlining`

## License
GPL-3.0-or-later
