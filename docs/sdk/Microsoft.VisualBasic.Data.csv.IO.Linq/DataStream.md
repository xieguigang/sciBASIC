# DataStream
_namespace: [Microsoft.VisualBasic.Data.csv.IO.Linq](./index.md)_

Buffered large text dataset Table reader



### Methods

#### AsLinq``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.DataStream.AsLinq``1(System.Boolean)
```
Csv to LINQ

#### BufferProvider
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.DataStream.BufferProvider
```
Providers the data buffer for the @``T:Microsoft.VisualBasic.Data.csv.IO.RowObject``
> 
>  这个函数主要是为了处理第一行数据
>  因为在构造函数部分已经读取了第一行来解析schema，所以在这里需要对第一个数据块做一些额外的处理
>  

#### ForEach``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.DataStream.ForEach``1(System.Action{``0})
```
For each item in the source data fram, invoke a specific task

|Parameter Name|Remarks|
|--------------|-------|
|invoke|-|


#### ForEachBlock``1
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.DataStream.ForEachBlock``1(System.Action{``0[]},System.Int32)
```
Processing large dataset in block partitions.(以分块任务的形式来处理一个非常大的数据集)

|Parameter Name|Remarks|
|--------------|-------|
|invoke|task of this block buffer|
|blockSize|Lines of the data source.(行数)|

> 
>  2016.06.19  代码已经经过测试，没有数据遗漏的bug，请放心使用
>  

#### OpenHandle
```csharp
Microsoft.VisualBasic.Data.csv.IO.Linq.DataStream.OpenHandle(System.String,System.Text.Encoding,System.Int32)
```
Open the data frame reader for the specific csv document.

|Parameter Name|Remarks|
|--------------|-------|
|file|*.csv data file.|
|encoding|The text encoding. default is using @``F:Microsoft.VisualBasic.Text.Encodings.Default``|



### Properties

#### _schema
The columns and their index order
#### _title
The title row, which is the mapping source of the class property name.
