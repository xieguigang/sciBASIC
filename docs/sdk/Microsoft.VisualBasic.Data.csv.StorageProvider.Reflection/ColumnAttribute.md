# ColumnAttribute
_namespace: [Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection](./index.md)_

This is a column in the csv document.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.ColumnAttribute.#ctor(System.String,System.Type)
```


|Parameter Name|Remarks|
|--------------|-------|
|Name|-|
|customParser|The type should implements the interface @``T:Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.IParser``.
 (对于基本类型，这个参数是可以被忽略掉的，但是对于复杂类型，这个参数是不能够被忽略的，否则会报错)
 |


#### ToString
```csharp
Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.ColumnAttribute.ToString
```
Display name


### Properties

#### CustomParser
The type should implements the interface @``T:Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.IParser``
#### TypeInfo
Reflector
