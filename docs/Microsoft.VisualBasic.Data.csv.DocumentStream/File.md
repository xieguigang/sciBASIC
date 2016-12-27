# File
_namespace: [Microsoft.VisualBasic.Data.csv.DocumentStream](./index.md)_

A comma character seperate table file that can be read and write in the EXCEL.(一个能够被Excel程序所读取的表格文件)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.#ctor(System.String,Microsoft.VisualBasic.Text.Encodings,System.Boolean)
```
Load document from path

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### __createTableVector
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.__createTableVector
```
这个方法是保存@``M:Microsoft.VisualBasic.Data.csv.Extensions.DataFrame(Microsoft.VisualBasic.Data.csv.DocumentStream.File)``对象之中的数据所需要的

#### __LINQ_LOAD
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.__LINQ_LOAD(System.String[])
```


|Parameter Name|Remarks|
|--------------|-------|
|data|-|

> 为了提高数据的加载效率，先使用LINQ预加载数据，之后使用Parallel LINQ进行数据的解析操作

#### __loads
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.__loads(System.String,System.Text.Encoding,System.Boolean)
```
同时兼容本地文件和网络文件的

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|encoding|-|


#### AppendRange
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.AppendRange(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject})
```
Add a data row collection into this Csv file object instance and then return the total row number after the add operation.
 (向CSV文件之中批量添加行记录，之后返回当前所打开的文件在添加纪录之后的总行数)

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### DeleteCell
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.DeleteCell(System.Func{System.String,System.Boolean},System.Int32)
```
删除目标列中的单元格中符合条件的内容

|Parameter Name|Remarks|
|--------------|-------|
|Condition|条件测试|
|index|列标号|


#### Distinct
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Distinct(Microsoft.VisualBasic.Data.csv.DocumentStream.File,System.Int32,System.Boolean)
```
去除Csv文件之中的重复记录

|Parameter Name|Remarks|
|--------------|-------|
|OrderBy|当为本参数指定一个非负数值的时候，程序会按照指定的列值进行排序|
|Asc|当进行排序操作的时候，是否按照升序进行排序，否则按照降序排序|


#### FastLoad
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.FastLoad(System.String,System.Boolean,System.Text.Encoding)
```
If you are sure about your csv data document have no character such like " or, in a cell, then you can try using this fast load method to load your csv data.
 if not, please using the @``M:Microsoft.VisualBasic.Data.csv.DocumentStream.File.Load(System.String,System.Text.Encoding,System.Boolean)`` method to avoid of the data damages.
 (假若你确信你的数据文件之中仅含有数字之类的数据，则可以尝试使用本方法进行快速加载，假若文件之中每一个单元格还含有引起歧义的例如双引号或者逗号，则请不要使用本方法进行加载)

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|encoding|-|


#### FindAll
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.FindAll(System.String)
```
使用迭代器返回包含有目标关键词的行

|Parameter Name|Remarks|
|--------------|-------|
|KeyWord|-|


#### FindAtColumn
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.FindAtColumn(System.String,System.Int32)
```
Using the content in a specific column as the target for search using a specific keyword, and then return all of the rows that have the query keyword.
 (以指定的列中的内容搜索关键词，并返回检索成功的行的集合)

|Parameter Name|Remarks|
|--------------|-------|
|KeyWord|-|
|Column|-|


#### Generate
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Generate
```
Generate the csv data file document using the table data.(将表格对象转换为文本文件以进行保存)

#### GenerateDocument
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.GenerateDocument(System.Boolean)
```
将表对象转换为文本文件之中的文本内容

|Parameter Name|Remarks|
|--------------|-------|
|Parallel|假若是不需要顺序的大文件，请设置为True|


#### GetByLine
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.GetByLine(System.Int32)
```
Get a data row in the specific row number, when a row is not exists in current csv file then the function will return a empty row.
 (当目标对象不存在的时候，会返回一个空行对象)

|Parameter Name|Remarks|
|--------------|-------|
|line|-|


#### Insert
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Insert(System.Int32,Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject)
```
在所指定的行号下面插入一行数据

|Parameter Name|Remarks|
|--------------|-------|
|rowId|-|
|Row|-|


#### InsertAt
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.InsertAt(System.Int32,Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject)
```
使用IList接口本身的Insert方法来执行插入

|Parameter Name|Remarks|
|--------------|-------|
|index|-|
|item|-|


#### InsertEmptyColumnBefore
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.InsertEmptyColumnBefore(System.Int32)
```
Insert a new empty line of row data before the specific row number.(在指定列标号的列之前插入一行空列)

|Parameter Name|Remarks|
|--------------|-------|
|column|-|


#### IsNullOrEmpty
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.IsNullOrEmpty(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject})
```
判断目标数据文件是否为空

|Parameter Name|Remarks|
|--------------|-------|
|df|-|


#### Load
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Load(System.String[],System.Boolean)
```
排序操作在这里会不会大幅度的影响性能？

|Parameter Name|Remarks|
|--------------|-------|
|buf|-|
|trimBlanks|如果这个选项为真，则会移除所有全部都是逗号分隔符``,,,,,,,,,``的空白行|


#### Normalization
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Normalization(System.String,System.String)
```
将一些奇怪的符号去除

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### op_Addition
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.op_Addition(Microsoft.VisualBasic.Data.csv.DocumentStream.File,System.Collections.Generic.IEnumerable{System.String})
```
Add a row

|Parameter Name|Remarks|
|--------------|-------|
|file|-|
|row|-|


#### op_Implicit
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.op_Implicit(System.String)~Microsoft.VisualBasic.Data.csv.DocumentStream.File
```
Read a Csv file, default encoding is utf8

|Parameter Name|Remarks|
|--------------|-------|
|Path|-|


#### Remove
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Remove(System.Func{Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject,System.Boolean})
```
Delete all of the row that meet the delete condition.(将所有满足条件的行进行删除)

|Parameter Name|Remarks|
|--------------|-------|
|condition|-|


#### RemoveRange
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.RemoveRange(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.DocumentStream.RowObject})
```
Remove the item in a specific row collection.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|


#### Save
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Save(System.String,System.Text.Encoding)
```
Save this csv document into a specific file location **`path`**.

|Parameter Name|Remarks|
|--------------|-------|
|Path|-|

> 当目标保存路径不存在的时候，会自动创建文件夹

#### TokenCounts
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.TokenCounts(System.Int32,System.Boolean,System.Boolean,System.Boolean)
```
统计某一个指定的列之中的各个项目的出现次数

|Parameter Name|Remarks|
|--------------|-------|
|ColumnIndex|-|
|FirstLineTitle|-|
|IgnoreBlanks|-|


#### Transpose
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.File.Transpose
```
对当前的csv矩阵进行转置之后返回新的文件


### Properties

#### _innerTable
First line in the table is the column name definition line.
#### Cell
Get and set the string content in a specific table cell.(设置或者获取某一个指定的单元格中的字符串内容)
#### Column
Get all data of a column of a specific column number.(获取文件中的某一列中的所有数据)
#### Columns
将本文件之中的所有列取出来，假若有任意一个列的元素的数目不够的话，则会在相应的位置之上使用空白来替换
#### Headers
The first row in the table was using as the headers
#### Item
Gets a row in the document stream object.
#### RowNumbers
Row Counts
#### Width
Get the max width number of the rows in the table.(返回表中的元素最多的一列的列数目)
