# Writer
_namespace: [Microsoft.VisualBasic.Data.csv](./index.md)_

@``M:Microsoft.VisualBasic.Data.csv.Writer.Dispose``的时候会自动保存Csv文件的数据



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.csv.Writer.#ctor(Microsoft.VisualBasic.Data.csv.Class,System.String,Microsoft.VisualBasic.Text.Encodings)
```


|Parameter Name|Remarks|
|--------------|-------|
|cls|Schema maps|
|DIR|
 Dump data to this directory. The index file will using ``#.Csv`` as its default name.
 |
|encoding|Text document encoding of the csv file.|


#### WriteRow
```csharp
Microsoft.VisualBasic.Data.csv.Writer.WriteRow(System.Object,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|obj|.NET object for maps to csv data row.|
|i|Uid reference for the external table.|



### Properties

#### __file
File handle for the csv data file.
