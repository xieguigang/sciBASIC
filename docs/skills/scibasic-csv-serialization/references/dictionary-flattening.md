# Dictionary Flattening (Metadata Columns)

A CLR property typed as `Dictionary(Of String, T)` can be expanded so each dictionary key becomes an extra CSV column. This is why the example `Person` with `data As Dictionary(Of String, Double)` writes `name,age,bmi,weight`.

## Example

```vbnet
Public Class Person
    Public Property name As String
    Public Property age As Double
    Public Property data As Dictionary(Of String, Double)
End Class

Dim a As New Person With {
    .name = "a",
    .age = 22,
    .data = New Dictionary(Of String, Double) From {{"bmi", 11}, {"weight", 33}}
}
Call {a}.SaveTo("data.csv")
' name,age,bmi,weight
' a,22,11,33

Dim loaded = "data.csv".LoadCsv(Of Person)()
' loaded(0).data("bmi") = 11, loaded(0).data("weight") = 33
```

## Detecting Dictionary Properties

- `<Meta>` attribute: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\DataSource\Attributes\MetaAttributeProvider.vb:67-100`. XML docs `:63-65`: "all un-serialized properties are treated as dictionary elements". `TypeId` is the dictionary value type, default `String` (`:74,83,92-94`).
- Bridge class: `g:\...\Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\Dictionary.vb:64-101` — the dictionary property is `Dictionary(Of String, attr.TypeId)` (`:78,86-92`), `CreateDictionary()` (`:94-96`), `ProviderId=MetaAttribute` (`:80-84`).
- Discovery: `TypeSchemaProvider.vb:130-135` (explicit `<Meta>`) and `:231-233` (unattributed property whose type is a dictionary is auto-detected). `SchemaProvider.vb:549-573` auto-binds `Properties` for `DynamicPropertyBase(Of T)` subclasses (`:565-569`). Example: `IO\Generic\DataSet.vb:78` — `DataSet : Inherits DynamicPropertyBase(Of Double)`, base `Properties As Dictionary(Of String,T)` in `...\ComponentModel\DataSource\Property\DynamicProperty.vb:85-87`.

## Write Path — Columns from Dictionary Keys

- `StorageProvider\Reflection\StorageProviders\Reflection.vb:284-286,295`: `New RowWriter(schema, metaBlank, layout).CacheIndex(source, reorderKeys)`; header = `GetRowNames(maps).Join(GetMetaTitles)` — dictionary keys become extra columns (`:295`); `:299-316` writes key types into the schema.
- `StorageProvider\ComponntModels\RowWriter.vb`:
  - `CacheIndex` (`:308-333`) uses `getAllMetadata` (`:275-284`, casts property value to `IDictionary`) and `extractAllKeys` (`:286-294`) to collect every dictionary key across all rows; keys are `Distinct` and sorted ascending/descending by `reorderKeys` (`:318-330`).
  - `GetMetaTitles` returns the cached index (`:352-358`).
  - `__buildRowMeta` (`:340-349`) appends `__meta(obj)` after the fixed columns.
  - `__meta` (`:360-387`) reads values by cached key (`:375-384`); missing key or `Nothing` dictionary fills `_metaBlank` (`:367-369,381-383`).

Key consequences:

- Union of all dictionary keys across all rows forms the metadata columns. A row missing a key gets `metaBlank`.
- `layout` can reorder all columns including metadata; `reorderKeys` controls alphabetical order of the metadata keys (`0` = natural/insertion order as written).
- Because keys are gathered by scanning the source, the source should be a materialized collection (List/array), not a lazy LINQ query.

## Read Path — Recollecting Dictionary Columns

- `Reflection.vb:156-164` → `RowBuilder.vb:159-167`: columns whose names match no fixed property are placed in `NonIndexed` (the "could all be dictionary data" comment at `:158`).
- `SolveReadOnlyMetaConflicts` (`RowBuilder.vb:185-203`) removes names that are actually read-only properties to avoid duplicates.
- `FillData` (`RowBuilder.vb:205-229`): for each `NonIndexed` column, reads `s=row(field.Value)` (`:211`), substitutes `metaBlank` for empty (`:211-212`), converts via `SchemaProvider.MetaAttributes.LoadMethod(str)` to the dictionary value type (`:213`), adds to `CreateDictionary()` (`:216-220`), then `BindProperty.SetValue(obj, meta, Nothing)` (`:222-225`). A second overload for `IEnumerable(Of NamedValue(Of Object))` is at `:237-278` (`:264-275`).

## Controlling Metadata Columns

```vbnet
' Preserve a fixed ID column first, deterministic metadata key order, blank fill:
Call records.SaveTo("out.csv",
                    layout:=New Dictionary(Of String, Integer) From {{"name", 0}, {"age", 1}},
                    reorderKeys:=1,
                    metaBlank:="NA")

' On load, empty cells become "NA" instead of "":
Dim loaded = "out.csv".LoadCsv(Of Person)(metaBlank:="NA")
```

- `reorderKeys`: nonzero sorts metadata keys; sign selects ascending/descending (`RowWriter.vb:318-330`).
- `metaBlank`: must match between write and read if blank placeholders are used; otherwise conversion of the placeholder to `Double` may fail.
- `<Meta>` can force a property to be treated as the dictionary when auto-detection is not desired, and its `TypeId` sets the value type.
