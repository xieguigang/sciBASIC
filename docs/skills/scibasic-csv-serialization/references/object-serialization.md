# Object Serialization — `LoadCsv` / `SaveTo`

Library: `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\dataframework-netcore5.vbproj` (root namespace / assembly `Microsoft.VisualBasic.Data.Framework`, package version `10.8.9818.14504`, `OutputType=Library`, `TargetFrameworks=net10.0`, conditional constant `netcore5=1`). Only `ProjectReference` is `..\..\Microsoft.VisualBasic.Core\src\Core.vbproj` (line 198); no NuGet references. Global imports `System.Data` / `System.Data.Common` (202-204).

## `LoadCsv(Of T As Class)` Overloads

Primary overload — `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\Extensions\Extensions.vb:550-587`:

```vbnet
<Extension>
Public Function LoadCsv(Of T As Class)(path$,
    Optional explicit As Boolean = False,
    Optional encoding As Encoding = Nothing,
    Optional fast As Boolean = False,
    Optional maps As NameMapping = Nothing,
    Optional mute As Boolean = False,
    Optional metaBlank As String = "",
    Optional simpleRowIterators As Boolean = True,
    Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing,
    Optional tsv As Boolean = False) As List(Of T)
```

Internals (`:567-576`): times the call and invokes
`Reflector.Load(Of T)(path, explicit, encoding, fast:=fast, maps:=maps, mute:=mute, metaBlank:=metaBlank, skipWhile:=skipWhile, simpleRowIterators:=simpleRowIterators, tsv:=tsv).AsList`.
`maps` is `NameMapping`, converted by a narrowing operator to `Dictionary(Of String, String)` (`MapsHelper.vb:179-185`).

Other definitions:

| File:Line | Signature |
|---|---|
| `Extensions\Extensions.vb:102-106` | `LoadCsv(Of T As Class)(path As DefaultString) As List(Of T)` |
| `Extensions\Extensions.vb:153-156` | `LoadCsv(Of T As Class)(path$, template As T, Optional encoding As Encodings = Encodings.UTF8) As T()` — uses an anonymous template object to infer columns |
| `Extensions\DocumentExtensions.vb:449-453` | `LoadCsv(path$, Optional encoding As Encodings = Encodings.ASCII) As IO.File` — non-generic, returns the raw table object |

Core loader: `StorageProvider\Reflection\StorageProviders\Reflection.vb:199-233`, `Reflector.Load(Of T As Class)(path$, Optional Explicit=True, encoding, fast=False, maps As Dictionary(Of String,String), mute, metaBlank, skipWhile, simpleRowIterators=True, tsv=False) As IEnumerable(Of T)`. File-not-found returns an empty sequence (`:209-215`).

### Option Semantics (with evidence)

- `explicit=False` (default): parse all simple-type properties without requiring `<Column>`; `True` parses only attributed members (`Reflection.vb:192-195`; `TypeSchemaProvider.vb:197-201`).
- `encoding=Nothing`: falls back to `TextEncodings.DefaultEncoding` (`IO\CSVText\File.vb:740`).
- `fast=False`: `True` → `FileLoader.FastLoad(path, True, encoding, skipWhile, tsv)` (`DataFrameResolver.vb:433-434`), simple comma/tab split that is quick but strict about quoting (`FileLoader.vb:92-105`); `False` → tokenizer-based `IO.File.Load`.
- `maps`: direction `Csv.Field -> PropertyInfo.Name` (`Extensions.vb:542`). Applied via `reader.ChangeMapping(maps)` (`Reflection.vb:223-226`), implemented in `HeaderSchema.vb:163-186` (keeps ordinal, renames `SchemaOridinal` key; duplicate target name throws).
- `mute=False`: only suppresses debug logs (`Extensions.vb:564,584`).
- `metaBlank=""`: fills empty cells (`rowBuilder.FillData(row, obj, metaBlank)`, `Reflection.vb:164`; `RowBuilder.vb:209-213`).
- `simpleRowIterators=True`: `ReadAllLines` + `FileLoader.Load` (`File.vb:790-796`); `False` uses streaming `RowIterator` (`:797-801`; docs `DataFrameResolver.vb:420-422`).
- `skipWhile`: a `NamedValue` whose `Name` is a column name and `Value` is a predicate. In `FastLoad`, `headers.IndexOf(skipWhile.Name)` locates the column and rows are filtered (`FileLoader.vb:104,116-117`). Use for large files to drop rows early (`Extensions.vb:543-545`).
- `tsv=False`: `True` switches delimiter to Tab (`FileLoader.vb:105`).

## `SaveTo(Of T)` Overloads

Primary overload — `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\Extensions\Write_csv.vb:135-170`:

```vbnet
<Extension>
Public Function SaveTo(Of T)(source As IEnumerable(Of T), path As String,
    Optional strict As Boolean = False,
    Optional encoding As Encoding = Nothing,
    Optional metaBlank As String = "",
    Optional nonParallel As Boolean = False,
    Optional maps As Dictionary(Of String, String) = Nothing,
    Optional reorderKeys As Integer = 0,
    Optional layout As Dictionary(Of String, Integer) = Nothing,
    Optional tsv As Boolean = False,
    Optional transpose As Boolean = False,
    Optional silent As Boolean = False) As Boolean
```

Builds an `Arguments` object (`:148-159`), normalizes the path (`:162`), opens with `FileMode.OpenOrCreate, doClear:=True` (`:167`), then calls `source.SaveTo(file, args:=argv)` (`:168`).

`Arguments` class — `Write_csv.vb:78-114`: properties `strict`(:80), `encoding`(:81), `metaBlank`(:82), `nonParallel`(:83), `maps`(:84), `reorderKeys`(:85), `layout`(:86), `tsv`(:87), `transpose`(:88), `silent`(:89). `ToCsv(Of T)` (`:91-112`) calls `Reflector.GetsRowData(source, GetType(T), strict, maps, parallel:=Not nonParallel, metaBlank, reorderKeys, layout)` and applies `MatrixTranspose` when `transpose` is set (`:103-111`).

Stream overload — `Write_csv.vb:182-206`: `SaveTo(Of T)(source As IEnumerable(Of T), file As Stream, args As Arguments) As Boolean` → `args.ToCsv(Of T)(objSeq).SaveDataFrame(file, encoding:=args.encoding, tsv:=args.tsv, silent:=args.silent)`.

Other `SaveTo` overloads in `Extensions\Extensions.vb`:

| Line | Signature |
|---|---|
| 167 | `SaveTo(Of T As INamedValue)(table As Dictionary(Of T), path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean` |
| 180 | `SaveTo(samples As IEnumerable(Of NamedValue(Of Double())), path$, Optional encoding As Encodings = Encodings.ASCII, Optional xlabels#() = Nothing) As Boolean` |
| 395 | `SaveTo(data As IEnumerable(Of DynamicObjectLoader), path$, Optional encoding As Encoding = Nothing) As Boolean` |
| 414 | `SaveTo(dat As IEnumerable(Of RowObject), path$, Optional encoding As Encoding = Nothing) As Boolean` |
| 654 | `SaveTo(Of T)(source As IEnumerable(Of T), path As String, encoding As Encodings, Optional explicit As Boolean = False) As Boolean` |
| 698 | `SaveTo(data As IEnumerable(Of Double), path$, Optional encoding As Encodings = Encodings.ASCII) As Boolean` |

No `DataTable`-specific `SaveTo` exists. `SaveDataSet(Of T As EntityObject)` (`Extensions.vb:628-651`) internally delegates to `SaveTo` (`:650`).

### Option Semantics (with evidence)

- `strict=True` writes all simple-type property values; `False` (default) writes only properties carrying `<Column>` (`Write_csv.vb:122-125`).
- `source` should be a materialized `List`, array, or collection — not a LINQ expression (`:120`). Only simple basic-type properties are saved; intended for small datasets; use LINQ/streaming variants for large data (`:131-133`).
- `layout`: reorder columns; smaller value sorts earlier (`:128`). `SaveDataSet` exists because direct `SaveTo` may shuffle columns so `EntityObject.ID` is not first (`:611-615`).
- `maps` write direction: ``{meta_define -> custom}`` (`:127`) — a `Dictionary(Of String, String)`, unlike the read-side `NameMapping`.
- `transpose`: transposes the row matrix before writing (`:103-111,145`).
- `metaBlank`: empty metadata cells are filled with this value (`FileLoader.vb:155` removes all-blank rows; `Reflection.vb:164`).
- `nonParallel=True`: disables parallel row serialization (`parallel:=Not nonParallel`, `:83,97`); real usage e.g. `RunModel.vb:158`.
- `silent=True`: suppresses console progress output (`:188-192,201-203`).

## Round-Trip Safety Notes

- Empty/missing file on read returns an empty collection, never `Nothing` (`Extensions.vb:547-549`; `Reflection.vb:209-212`, warning only in DEBUG).
- `Column` on a non-basic type requires a custom `IParser` implementation (`Column.vb:96-101`; `GetParser()` `:119-130`), otherwise an error is raised.
- `DataFrameColumnAttribute` lets an empty name fall back to the property name; changing a property name requires maintaining the mapping (`DataFrameColumnAttribute.vb:69-71,200-212`).

## Real Call Sites

- `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\test\Reflection.vb:69-73` — basic generic round-trip:
  ```vbnet
  Dim visitors As Visitor() = "../../../../Example/visitors.csv".LoadCsv(Of Visitor)
  Call visitors.SaveTo("./test.csv")
  ```
- `g:\GCModeller\src\runtime\sciBASIC#\Data\test\IOExample.vb:148,163-168` — template-based `LoadCsv` + `<Column("POST -> data", GetType(CustomParserExample))>` custom parser (`:112`) + `SaveTo`.
- `g:\GCModeller\src\GCModeller\analysis\metaTraits\TraitAnnotation.vb:96-97,122-125` — `file.LoadCsv(Of TraitAnnotation)(mute:=True, tsv:=True)` with `<Collection("discrete_values", ";")>`.
- `g:\GCModeller\src\GCModeller\analysis\Metagenome\Metagenome\Kmers\Kraken2\Bracken.vb:88-91` — `Public Shared Function LoadTable(tsvfile As String) As Bracken()` returning `tsvfile.LoadCsv(Of Bracken)(mute:=True, tsv:=True)`.
- `g:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\_smoketest\Program.vb:63-65` — NumericTable CSV/TSV round-trip and DataFrame conversion self-test.
