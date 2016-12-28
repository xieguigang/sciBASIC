# Tokenizer
_namespace: [Microsoft.VisualBasic.Data.csv.DocumentStream](./index.md)_

RowObject parsers



### Methods

#### CharsParser
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Tokenizer.CharsParser(System.String)
```
通过Chars枚举来解析域

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### IsEmptyRow
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Tokenizer.IsEmptyRow(System.String,System.Char)
```
是否等于``,,,,,,,,,``

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### RegexTokenizer
```csharp
Microsoft.VisualBasic.Data.csv.DocumentStream.Tokenizer.RegexTokenizer(System.String)
```
Parsing the row data from the input string line.(通过正则表达式来解析域)

|Parameter Name|Remarks|
|--------------|-------|
|s|-|



### Properties

#### SplitRegxExpression
A regex expression string that use for split the line text.
