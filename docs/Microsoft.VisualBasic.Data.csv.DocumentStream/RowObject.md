# RowObject
_namespace: [Microsoft.VisualBasic.Data.csv.DocumentStream](./index.md)_

A line of data in the csv file.(Csv表格文件之中的一行)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.#ctor(System.String)
```
这个构造函数会使用@``M:Microsoft.VisualBasic.Data.csv.DocumentStream.Tokenizer.CharsParser(System.String)``解析所输入的字符串为列数据的集合

|Parameter Name|Remarks|
|--------------|-------|
|raw|A raw string line which read from the Csv text file.|


#### Contains
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.Contains(Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject)
```
查看目标行是否被包含在本行之中，即是否对应元素相等

|Parameter Name|Remarks|
|--------------|-------|
|Row|-|


#### Distinct
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.Distinct(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject})
```
去除行集合中的重复的数据行

|Parameter Name|Remarks|
|--------------|-------|
|rowList|-|


#### GetColumn
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.GetColumn(System.Int32,System.String@)
```
返回一个指示：是否为空？

|Parameter Name|Remarks|
|--------------|-------|
|Idx|-|
|retStr|-|


#### InsertAt
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.InsertAt(System.String,System.Int32)
```
insert the data into a spercific column

|Parameter Name|Remarks|
|--------------|-------|
|value|-|
|column|-|


_returns: 仅为LINQ查询使用的一个无意义的值_

#### LocateKeyWord
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.LocateKeyWord(System.String,System.Boolean)
```
查询某一个关键词在本行中的哪一个单元格，返回-1表示没有查询到本关键词

|Parameter Name|Remarks|
|--------------|-------|
|KeyWord|-|


#### op_Explicit
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.op_Explicit(Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject)~System.String
```
Write to file.

|Parameter Name|Remarks|
|--------------|-------|
|row|-|


#### op_Implicit
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.op_Implicit(System.String)~Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject
```
Row parsing into column tokens

|Parameter Name|Remarks|
|--------------|-------|
|Line|-|


#### Takes
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.Takes(System.Int32[],System.Boolean)
```
Takes the data in the specific column index collection, if the column is not exists in the row object, then a part of the returned data will be the empty string.

|Parameter Name|Remarks|
|--------------|-------|
|Cols|-|
|retNullable|(当不存在数据的时候是否返回空字符串，默认返回空字符串)|


#### ToString
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.ToString
```
Displaying in IDE


### Properties

#### _innerColumns
本行对象中的所有的单元格的数据集合
#### AsLine
Generate a line of the string data in the csv document.(将当前的行对象转换为文件中的一行字符串)
#### Column
Get the cell data in a specific column number. if the column is not exists in this row then will return a empty string.
 (获取某一列中的数据，若该列不存在则返回空字符串)
#### DirectGet
Unsafety method, @``P:Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject.Column(System.Int32)`` method is safely.
 (不做任何处理直接获取数据)
#### IsNullOrEmpty
is this row object contains any data?
#### NotNullColumns
返回本行中的非空白数据
#### Width
非空白单元格的数目
