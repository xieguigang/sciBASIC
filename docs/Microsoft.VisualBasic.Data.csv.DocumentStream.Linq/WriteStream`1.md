# WriteStream`1
_namespace: [Microsoft.VisualBasic.Data.csv.DocumentStream.Linq](./index.md)_

The stream writer for the data set, you can handling the ultra large dataset 
 serialize into a csv document by using this writer stream object.
 (文件写入流，这个一般是在遇到非常大的文件流的时候才需要使用)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Linq.WriteStream`1.#ctor(System.String,System.Boolean,System.String,System.String[],System.Collections.Generic.Dictionary{System.String,System.String})
```


|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|Explicit|Schema parsing of the object strictly?|
|metaKeys|预设的标题头部|


#### Ctype``1
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Linq.WriteStream`1.Ctype``1(System.Func{``0,`0})
```
这个是配合@``M:Microsoft.VisualBasic.Data.csv.DocumentStream.Linq.DataStream.ForEach``1(System.Action{``0})``方法使用的

|Parameter Name|Remarks|
|--------------|-------|
|_ctype|-|


#### Flush
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Linq.WriteStream`1.Flush(System.Collections.Generic.IEnumerable{`0},System.Boolean)
```
Serialize the object data source into the csv document.
 (将对象的数据源写入Csv文件之中）

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### ToArray``1
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Linq.WriteStream`1.ToArray``1(System.Func{``0,`0[]})
```
这个是配合@``M:Microsoft.VisualBasic.Data.csv.DocumentStream.Linq.DataStream.ForEachBlock``1(System.Action{``0[]},System.Int32)``方法使用的

|Parameter Name|Remarks|
|--------------|-------|
|_ctype|-|



### Properties

#### _fileIO
File system object handle for write csv row data.
#### IsMetaIndexed
Has the meta field indexed?
#### RowWriter
Schema for creates row data from the inputs object.
