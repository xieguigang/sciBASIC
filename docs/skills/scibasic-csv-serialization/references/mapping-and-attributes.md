# Mapping and Attributes

Covers how CSV headers map to CLR properties and how to control the mapping.

## `ColumnAttribute`

Definition: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\DataSource\Attributes\Column.vb:69` — namespace `Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps`, `<AttributeUsage(Property, AllowMultiple:=False, Inherited:=False)>` (`:68`), inherits `DataAttribute`.

- `Name` inherited from `DataAttribute`: `...\Attributes\DataAttribute.vb:59-65` (`Public Property Name As String`).
- Own members: `ProviderId = ProviderIds.Column` (`:72-77`), `CustomParser As Type` (`:83`), `alias As String()` (`:85`).
- Constructors: parameterless (`:87`), `(name, ParamArray alias)` (`:90-93`), `(name, Optional customParser As Type)` (`:102-109`; empty name throws `DataException`).
- `GetParser()` (`:119-130`) instantiates an `IParser` for the column; `TypeInfo` (`:144`); String↔`ColumnAttribute` conversion operators (`:147-154`).
- Crucially, `ColumnAttribute` has **no** `Index`/`ordinal`/`IsNullable` properties. Those belong to the sibling `DataFrameColumnAttribute`.

Usage: `TypeSchemaProvider.vb:137-152` uses `attr.Name` as the column alias and allows non-basic types; entity bridge `...\ReflectionBridges\Field.vb:64-106` holds `define As ColumnAttribute` (`:70`); on write, `strict=False` only saves `<Column>`-marked properties (`Write_csv.vb:122-125`). `strict` flows through `SchemaProvider.CreateObjectInternal(type, strict)` → `CreateObject` → `GetProperties(type, strict)` (`SchemaProvider.vb:492-514`).

Custom parser example (`g:\GCModeller\src\runtime\sciBASIC#\Data\test\IOExample.vb:112`):

```vbnet
<Column("POST -> data", GetType(CustomParserExample))>
Public Property data As KeyValuePair(Of String, Integer)
```

## `DataFrameColumnAttribute`

Definition: `g:\...\Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\DataSource\DataFrameColumnAttribute.vb` — declaration at `:83`, `Name :88`, `Description :90`, `GetIndex :150-152`, `LoadMapping :162/:181`.

- XML docs `:69-71,200-212`: an empty name falls back to the property name; renaming a property requires maintaining the mapping.
- `mapsAll` controls whether unmarked properties are indexed (`:219-223`).

## `NameMapping`

Definition: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\MapsHelper.vb:145-186` (namespace `ComponentModel`), `Public Class NameMapping : Inherits MapsHelper(Of String)`.

- Base `MapsHelper(Of String)` (`:95-140`): internal `IDictionary` + default value, default indexer (`:100-105`).
- Constructor `(Optional dictionary As Dictionary(Of String,String), Optional default$)` (`:158-161`; default empty dictionary factory `:154-156`).
- `Add(key$, map$)` (`:164-166`).
- Widening operator from 2-D string array (`:168-176`): each row `[0] -> [1]`, so `{{csvHeader, propertyName}, ...}`.
- **Narrowing operator** `CType(maps As NameMapping) As Dictionary(Of String,String)` (`:179-185`) lets `LoadCsv` pass it to `Reflector.Load`.
- Important comment (`:150-153`): `Add` mutates the internal dictionary, so construct a new object each time; reusing one can cause reference bugs.

Read direction: `Csv.Field -> PropertyInfo.Name` (`Extensions.vb:542`; `DataFrameResolver.vb:166-175`; `HeaderSchema.vb:158-186`). Real usage (`g:\...\Data\DataFrame\IO\Generic\EntityObject.vb:229-245`):

```vbnet
With New NameMapping(fieldNameMaps)
    Call .Add(uidMap, NameOf(EntityObject.ID))
    If tsv Then
        Return path.LoadTsv(Of T)(nameMaps:=.ByRef, encoding:=encoding)
    Else
        Return path.LoadCsv(Of T)(explicit:=False, maps:=.ByRef, ...)
    End If
End With
```

Other entry points: `LoadTsv` `nameMaps` (`DocumentExtensions.vb:264-267,286-290,310`), `ImportsTsv` (`DataImports.vb:199-201`).

Note the write side takes a plain `Dictionary(Of String, String)` (direction `{meta_define -> custom}`), not `NameMapping`.

## Schema Mapping Internals

- `g:\...\Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\Mappings.vb:99-127` scans fields/properties for attributes; `explict` controls whether unmarked members are included. `:138-140` uses `ColumnAttribute.Name` as the column name. `:155-160,231-263` `FieldNameMappings(Of T)` builds member↔column dictionaries (`reversed:=True` gives header→property). `:271-277` `GetColumnName`; `:192-220` `GetAliasName` recognizes `ColumnAttribute`, `Field`, `DataFrameColumnAttribute`, and (under NETCOREAPP) `DataAnnotations.Schema.ColumnAttribute`.

- `g:\...\Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\Field.vb:70-74`: `define As ColumnAttribute` → `Name`.

- `g:\...\Data\DataFrame\StorageProvider\Reflection\StorageProviders\Reflection.vb:192-195`: with `explicit=False`, ignores `ColumnAttribute` and resolves all simple properties; `:223-226` applies `maps` via `reader.ChangeMapping`.

- `g:\...\Data\DataFrame\StorageProvider\Reflection\StorageProviders\TypeSchemaProvider.vb:222-225`: constructs a default `ColumnAttribute(_getName)` when no `ColumnMaps` exist; nullable primitive types also get a default mapping.

- `HeaderSchema.vb:163-186` applies `ChangeMapping`: preserves column ordinals and replaces the `SchemaOridinal` key name; throws if the target name already exists.

## Property Discovery Order

`TypeSchemaProvider.vb:118-202`:

1. `<Collection>` (`:120-128`)
2. `<Meta>` (`:130-135`)
3. `<Column>` (`:137-152`)
4. Other `Column` / `DataFrameColumn` / `Field` / `DataAnnotations.Column` (`:165-195`)
5. Unattributed properties: by name when `Explicit=False` (`:197-198`); skipped when `True` (`:200`)

`__generateMask` (`:210-247`) distinguishes primitives / dictionaries / collections / `KeyValuePair` / enums.

## Header-to-Property Mapping Example

```vbnet
' CSV header:  id,name,score
' CLR type:
Public Class Record
    <Column("id")> Public Property Tag As String
    Public Property name As String
    Public Property score As Double
End Class

' Or without attributes, map header -> property:
Dim data = "data.csv".LoadCsv(Of Record)(maps:={{"id", NameOf(Record.Tag)}})
```

`maps` uses a 2-D string array widened to `NameMapping`; the first element is the CSV header, the second is the property name.
