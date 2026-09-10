# Tabular Data Frame and CSV/ARFF Storage Provider Framework

Core tabular data library of sciBASIC#: R-like `DataFrame`, CSV/TSV and ARFF documents, and reflection based storage providers that map .NET classes to table rows and columns.

## Overview
- `DataFrame` / `FeatureVector` give an R-like column-oriented matrix with row names, slicing, transposing, union and numeric helpers.
- CSV/TSV document model (`File`, `RowObject`, `RowTokenizer`, `RowIterator`) with buffered readers/writers (`DataStream`, `WriteStream`, `BatchQueue`) for very large files.
- ARFF (Weka) reader/writer, MySQL helpers, HTML writer and generic table abstractions (`DataSet`, `EntityObject`, `Table`).
- Reflection storage providers: `ColumnAttribute`, `CollectionAttribute`, `MetaAttribute`, `SchemaProvider`, `RowBuilder`/`RowWriter` and `Load(Of T)` / `Save(Of T)` extensions.

## Key Types
- `Microsoft.VisualBasic.Data.Framework.DataFrame` — R-like data frame of named feature columns with `read_csv` / `read_arff`.
- `Microsoft.VisualBasic.Data.Framework.DataFrame.FeatureVector` — one typed column vector (scalar or array valued).
- `Microsoft.VisualBasic.Data.Framework.IO.File` — in-memory CSV/TSV document with `Load` and `Save`.
- `Microsoft.VisualBasic.Data.Framework.IO.RowObject` — a single parsed CSV row with token access.
- `Microsoft.VisualBasic.Data.Framework.IO.EntityObject` — named-property row object used as the generic table row.
- `Microsoft.VisualBasic.Data.Framework.StorageProvider.ComponentModels.SchemaProvider` — reflection schema of a .NET type in CSV layout.
- `Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection.Reflector` — `Load(Of T)` / `Save(Of T)` entry points for object collections.
- `Microsoft.VisualBasic.Data.Framework.IO.Linq.DataStream` — buffered reader for ultra large tabular text files.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO

Dim df As DataFrame = DataFrame.read_csv("./iris.csv")
Console.WriteLine(df.dims)

Dim csv As File = File.Load("./iris.csv")
For Each row As RowObject In csv
    Console.WriteLine(row.AsLine())
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.Framework`
- TargetFramework: `net10.0`
- Tags: `scibasic;dataframe;csv;arff;storage-provider;tabular-data`

## License
GPL-3.0-or-later
