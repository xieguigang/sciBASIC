# DataFrame and NumericTable

## R-like `DataFrame`

File: `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\DataFrame\DataFrame.vb`.

Declaration (`:78-81`):

```vb
''' R language liked dataframe object
Public Class DataFrame : Implements INumericMatrix, ILabeledMatrix, Enumeration(Of FeatureVector)
```

Representation:
- Columns: `Public Property features As New Dictionary(Of String, FeatureVector)` (`:87`). `FeatureVector` (`...\DataFrame\FeatureVector.vb:72`) holds `vector As Array` (`:78`), `type As Type` (`:84`), `name As String` (`:90`), `attributes` (`:92`), indexer `GetValue(i)` (`:119`), multi-type constructors (`:133-189`), `FromGeneral` / `NumericGetter`.
- Rows: `Public Property rownames As String()` (`:106`). There is no row object; rows are accessed via methods.
- Shape: `dims As Size` (`:116`, width=columns, height=rows), `nsamples` (`:136`), `nfeatures` (`:146`), `featureNames` (`:126`), `featureSet As FeatureVector()` (`:184`).

Key members:

| Member | Line | Notes |
|---|---|---|
| `Default Property Item(featureName As String) As FeatureVector` | 157-164 | Read/write column accessor |
| `Default ReadOnly Property Item(cols As IEnumerable(Of String)) As DataFrame` | 171-182 | Column projection |
| `row(i As Integer) As Object()` | 294 | Row values |
| `foreachRow() As IEnumerable(Of NamedCollection(Of Object))` | 302 | Named row enumeration |
| `foreachRow(Of T)(apply As Func(Of NamedCollection(Of Object), NamedCollection(Of T)))` | 316 | Closest to an Apply API |
| `slice(rownames As IEnumerable(Of String)) As DataFrame` | 330-393 | Row slice/reorder |
| `delete(featureName)` / 6 `add(...)` overloads | 226 / 230,235,248,261,273,282 | Column add/delete |
| `Union(append As DataFrame) As DataFrame` | 401-416 | cbind-like horizontal merge; requires equal sample counts |
| Constructors `New()`, `New(Dictionary(Of String, Double()))`, `New(ParamArray (name,col)())`, `New(cols, labels)` | 191,194,202,210 | |
| `Shared FromRows(Of T)(rows, colnames)` | 511 | Build from row tuples |
| `ArrayPack`, `GetLabels`, `GenericEnumerator` | 483,501,505 | Interface implementations |

Important: `DataFrame` itself has **no** `Filter`, `GroupBy`, `rbind`, `cbind`, or `Apply` methods. Similar names elsewhere are unrelated (`GroupBy(data As IEnumerable(Of DataSet), ...)` in `IO\Generic\Extensions.vb:178`; `Cbind(data As EntityObject)` in `DataSet\DataRows.vb:118`; `Apply(ByRef row As RowObject, ...)` in `Extensions\DocumentExtensions.vb:134`).

### DataFrame CSV / ARFF I/O

| Function | Location |
|---|---|
| `Public Shared Function read_csv(file As String, Optional delimiter As Char = ","c, Optional rowHeader As Boolean = True, Optional encoding As Encodings = Encodings.UTF8) As DataFrame` | `DataFrame.vb:430-437` → `FastLoader.ReadCsv` |
| `Public Shared Function read_csv(file As Stream, ..., Optional verbose As Boolean = True) As DataFrame` | `DataFrame.vb:461-468` |
| `FastLoader.ReadCsv(file As Stream, Optional delimiter As Char = ","c, Optional rowHeader As Boolean = True, Optional encoding As Encodings = Encodings.UTF8, Optional verbose As Boolean = True) As DataFrame` | `DataFrame\FastLoader.vb:78` (<Extension>, `DataFrameReader` at `:68`) |
| `FileWriter.WriteCsv(df As DataFrame, file As String, Optional blank As String = "") As Boolean` | `DataFrame\FileWriter.vb:71` |
| `FileWriter.WriteCsv(df As DataFrame, file As Stream, Optional blank As String = "")` | `DataFrame\FileWriter.vb:91` |
| `read_arff(file As String/Stream)`, `write_arff(df, Stream/TextWriter)` | `DataFrame.vb:440,447,452,457` |

There are no `DataFrame.Load` / `SaveAs` / `ToCsv` / `FromCsv` members.

## Unified `NumericTable`

File: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\Data\NumericTable.vb` (namespace `Data`). Docs `:80-95`: a preprocessed, numeric-only 2-D table; logical values, string labels, missing values must already be converted upstream; algorithm layer sees a pure numeric matrix. Results (clustering/regression) are written into `labels` via `SetLabel`.

Declaration (`:96-99`):

```vb
Public Class NumericTable
    Implements INumericMatrix
    Implements ILabeledMatrix
    Implements IEnumerable(Of Double())
```

Properties:

| Property | Line | Meaning |
|---|---|---|
| `name` / `description` | 105 / 110 | Table identity |
| `rowNames As String()` | 117 | Sample IDs; defaults to `1..n` |
| `featureNames As String()` | 123 | Feature columns |
| `features As Double()()` | 129 | Row-major `features(sample)(feature)` |
| `labelNames As String()` | 135 | Label columns |
| `labels As Double()()` | 141 | Row-major `labels(sample)(label)` |
| `nsamples` / `nfeatures` / `nlabels` / `isEmpty` | 147 / 157 / 167 / 177 | Dimensions |
| `Default ReadOnly Property Item(i As Integer) As Double()` | 188 | i-th feature vector |

Constructors: `New()` (`:198`), `New(features As Double()(), Optional rowNames, Optional featureNames)` (`:207`), `New(x As IEnumerable(Of Double()), y As IEnumerable(Of Double))` regression (`:218`), `New(x, y As IEnumerable(Of Integer))` classification (`:228`), `New(featureNames As String(), columns As IEnumerable(Of Double()), Optional rowNames)` column-transpose (`:239`), `New(samples As IEnumerable(Of NamedCollection(Of Double)), featureNames)` (`:245`).

Methods: `RowNamesOrDefault()` (`:314`), `GetRowName(i)` (`:328`), `Row(i)` (`:336`), `Feature(name) As Double()` (`:345`), `HasFeature`/`HasLabel` (`:360,368`), `GetLabel(name)` (`:421`), `SetLabel(name As String, values As Double()) As NumericTable` (`:466`, overwrite or append, returns `Me` for chaining), `SetLabel(name, values As Integer())` (`:493`), `RemoveLabel(name)` (`:530`), `Slice(rows)` (`:574`), `[Select](features)` (`:619`), `Clone()` (`:642`), `Validate()` (`:676`), static `FromRows` (`:719`) / `FromColumns` (`:727`), `LoadBinary` (`:743,753`), `ArrayPack` (`:761`), `GetLabels()` (`:772`), `EnumerateAll()` (`:787`).

Binary IO: `WriteBinary(table, file, Optional gzip)` in `g:\...\Microsoft.VisualBasic.Core\src\Data\NumericTableBinary.vb:139`, stream overload `:159`; format documented `:68-92` (magic `NVTABLE`, big-endian, optional gzip).

### `NumericTableIO` Text I/O

Module in `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\DataFrame\NumericTableIO.vb` (`:84`). File layout (`:66-83`): first column = row names, then feature columns, then label columns with `label:` prefix; on read labels are restored into the label matrix with the prefix stripped. `DefaultLabelPrefix = "label:"` in `g:\...\Microsoft.VisualBasic.Core\src\Data\NumericTableExtensions.vb:94`.

```vbnet
' :101
Public Function WriteDelimited(table As NumericTable, file As String, delimiter As Char,
                               Optional labelPrefix As String = DefaultLabelPrefix,
                               Optional naValue As String = "") As Boolean
' :128 stream overload
Public Sub WriteDelimited(table As NumericTable, stream As Stream, delimiter As Char, ...)

' :181 / :193
Public Function WriteCsv(table As NumericTable, file As String, ...) As Boolean
Public Sub WriteCsv(table As NumericTable, stream As Stream, ...)
' :205 / :217
Public Function WriteTsv(table As NumericTable, file As String, ...) As Boolean
Public Sub WriteTsv(table As NumericTable, stream As Stream, ...)

' :241 / :257
Public Function ReadCsv(file As String, Optional labels As String() = Nothing,
                        Optional labelPrefix As String = DefaultLabelPrefix,
                        Optional rowHeader As Boolean = True,
                        Optional encoding As Encodings = Encodings.UTF8,
                        Optional strict As Boolean = True,
                        Optional columns As String() = Nothing) As NumericTable
Public Function ReadCsv(stream As Stream, ...) As NumericTable
' :271 / :287
Public Function ReadTsv(file As String, ...) As NumericTable
Public Function ReadTsv(stream As Stream, ...) As NumericTable
' :326
Public Function ReadDelimited(stream As Stream, delimiter As Char, ...) As NumericTable
```

`ReadCsv` options: `labels` names columns to treat as labels even without prefix; `strict=False` turns non-numeric cells into `Double.NaN`; `columns:=` selects a subset of columns (e.g. `{"f2","label:score"}`).

Representative usage: `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\_smoketest\Program.vb:83,89,97,105,111,117,123,136,152` — `table.WriteCsv(csv)`, `NumericTableIO.ReadCsv(csv)`, `table.WriteTsv(ms)`, `ReadTsv`, `table.AsDataFrame()`, `df.AsNumericTable()`, `ReadCsv(csv, labels:={"f1"})`, `ReadCsv(csv, columns:={"f2","label:score"})`, `strict:=False`. Also `vs_solutions\libs\stdio\read.vb:28-43` (`csv`/`tsv` → `NumericTable`), `write.vb:19-50` (write `csv`/`tsv`/`table`), `stdlib\table.vb:20-95`, and `Data_science\Mathematica\Math\DataFittings\NumericTableRegressions.vb:270` onward (regression APIs taking `NumericTable`).

## Interfaces and Conversion

- `INumericMatrix` — `g:\...\Microsoft.VisualBasic.Core\src\Math\Matrix.vb:71-78`: sole member `Function ArrayPack(Optional deepcopy As Boolean = False) As Double()()`.
- `ILabeledMatrix` — `g:\...\Microsoft.VisualBasic.Core\src\Data\ILabeledMatrix.vb:60-68`: sole member `Function GetLabels() As IEnumerable(Of String)`.
Both implemented by `DataFrame` and `NumericTable`; algorithms use `ArrayPack()` to get a row-major `Double()()` regardless of carrier. `NumericTableExtensions.NumericRows` (`NumericTableExtensions.vb:215`) wraps `ArrayPack`.

Conversion module: `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\DataFrame\NumericTableConversion.vb` (`:63`, docs `:56-62` note lossless `table -> dataframe -> table` round-trip):

```vbnet
' :76
<Extension>
Public Function AsDataFrame(table As NumericTable,
                            Optional labelPrefix As String = DefaultLabelPrefix) As DataFrame
' :125
<Extension>
Public Function AsNumericTable(df As DataFrame,
                               Optional labels As String() = Nothing,
                               Optional labelPrefix As String = DefaultLabelPrefix,
                               Optional strict As Boolean = True) As NumericTable
```

`AsDataFrame` (`:76-107`): feature columns from `featureNames`, label columns written as `{prefix}{labelName}`, row names from `rowNames`, copies `name`/`description`.

`AsNumericTable` (`:125-146`): columns starting with `labelPrefix` or listed in `labels` become labels (prefix stripped), others become features; `strict=False` maps non-numeric cells to `Double.NaN`; implemented via `NumericTableIO.SplitColumns` (`:459`) and `ToMatrix` (`:522`).
