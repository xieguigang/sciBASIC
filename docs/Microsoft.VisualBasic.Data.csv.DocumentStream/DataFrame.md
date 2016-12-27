# DataFrame
_namespace: [Microsoft.VisualBasic.Data.csv.DocumentStream](./index.md)_

The dynamics data frame object which its first line is not contains the but using for the title property.
 (第一行总是没有的，即本对象类型适用于第一行为列标题行的数据)



### Methods

#### __createSchemaOridinal
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.__createSchemaOridinal(Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame)
```
Indexing the column headers

|Parameter Name|Remarks|
|--------------|-------|
|df|-|


#### __reviewColumnHeader
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.__reviewColumnHeader(System.String)
```
这里不能够使用Trim函数，因为Column也可能是故意定义了空格在其实或者结束的位置的，使用Trim函数之后，反而会导致GetOrder函数执行失败。故而在这里只给出警告信息即可

|Parameter Name|Remarks|
|--------------|-------|
|strValue|-|


#### ChangeMapping
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.ChangeMapping(System.Collections.Generic.Dictionary{System.String,System.String})
```
``Csv.Field -> @``P:System.Reflection.MemberInfo.Name````

|Parameter Name|Remarks|
|--------------|-------|
|MappingData|{oldFieldName, newFieldName}|


#### Close
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.Close
```
Closes the @``T:System.Data.IDataReader``:@``T:Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame`` Object.

#### CopyFrom
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.CopyFrom(Microsoft.VisualBasic.Data.csv.DocumentStream.File)
```
这个方法会清除当前对象之中的原有数据

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### CreateDataSource
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.CreateDataSource
```
Get the lines data for the convinent data operation.(为了保持一致的顺序，这个函数是非并行化的)

#### CreateObject
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.CreateObject(Microsoft.VisualBasic.Data.csv.DocumentStream.File)
```
Creates the data frame object from the csv docs.

|Parameter Name|Remarks|
|--------------|-------|
|file|-|


#### csv
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.csv
```
Convert this dataframe object as a csv document object

#### GetOrdinal
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.GetOrdinal(System.String)
```
Function return -1 when column not found.

|Parameter Name|Remarks|
|--------------|-------|
|Column|-|


#### GetOrdinalSchema
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.GetOrdinalSchema(System.String[])
```
Gets the order list of the specific column list, -1 value will be returned when it is not exists in the table.
 (获取列集合的位置列表，不存在的列则返回-1)

|Parameter Name|Remarks|
|--------------|-------|
|columns|-|

> 由于存在一一对应关系，这里不会再使用并行拓展

#### GetSchemaTable
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.GetSchemaTable
```
Returns a System.Data.DataTable that describes the column metadata of the System.Data.IDataReader.

_returns: A System.Data.DataTable that describes the column metadata._

#### Load
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.Load(System.String,System.Text.Encoding,System.Boolean)
```
Try loading a excel csv data file as a dynamics data frame object.(尝试加载一个Csv文件为数据框对象，请注意，第一行必须要为标题行)

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|encoding|-|


#### Read
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.Read
```
The data frame object start to reading the data in this table, if the current pointer is reach 
 the top of the lines then this function will returns FALSE to stop the reading loop.

#### Reset
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.Reset
```
Reset the reading position in the data frame object.


### Properties

#### __columnList
Using the first line of the csv row as the column headers in this csv file.
#### __current
@``F:Microsoft.VisualBasic.Data.csv.DocumentStream.DataFrame.__currentLine``在@``F:Microsoft.VisualBasic.Data.csv.DocumentStream.File._innerTable``之中的位置
#### DuplicatedKeys
There is an duplicated key exists in your csv table, please delete the duplicated key and try load again!
#### EmptyWarning
``[CSV::Reflector::Warnning] There are empty column header in your data!``
#### Headers
The column headers in the csv file first row.
#### HeadTitles
The column headers in the csv file first row.
