# DataImports
_namespace: [Microsoft.VisualBasic.Data.csv](./index.md)_

Module provides the csv data imports operation of the csv document creates from a text file.
 (模块提供了从文本文档之中导入数据的方法)



### Methods

#### FixLengthImports
```csharp
Microsoft.VisualBasic.Data.csv.DataImports.FixLengthImports(System.String,System.Int32,System.Text.Encoding)
```
Imports the data in a well formatted text file using the fix length as the data separate method.

|Parameter Name|Remarks|
|--------------|-------|
|txtPath|-|
|length|The string length width of the data row.(固定的列字符数的宽度)|


#### Imports
```csharp
Microsoft.VisualBasic.Data.csv.DataImports.Imports(System.String,System.String,System.Text.Encoding)
```
Imports the data in a well formatted text file using a specific delimiter, default delimiter is comma character.

|Parameter Name|Remarks|
|--------------|-------|
|txtPath|The file path for the data imports text file.(将要进行数据导入的文本文件)|


#### Imports``1
```csharp
Microsoft.VisualBasic.Data.csv.DataImports.Imports``1(System.String,System.String,System.Text.Encoding)
```
Imports data source by using specific delimiter

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|delimiter|-|
|encoding|-|


#### RowParsing
```csharp
Microsoft.VisualBasic.Data.csv.DataImports.RowParsing(System.String,System.String)
```
Row parsing its column tokens

|Parameter Name|Remarks|
|--------------|-------|
|Line|-|


#### SampleForType
```csharp
Microsoft.VisualBasic.Data.csv.DataImports.SampleForType(System.Collections.Generic.IEnumerable{System.String})
```
从字符串集合之中推测可能的数据类型

|Parameter Name|Remarks|
|--------------|-------|
|column|-|

> 
>  推测规则：
>  会对数据进行采样
>  类型的优先级别为：
>  


### Properties

#### SplitRegxExpression
A regex expression string that use for split the line text.
