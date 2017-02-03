# DataEnumerator
_namespace: [Microsoft.VisualBasic.Data.IO.SearchEngine](./index.md)_





### Methods

#### Execute``1
```csharp
Microsoft.VisualBasic.Data.IO.SearchEngine.DataEnumerator.Execute``1(System.Collections.Generic.IEnumerable{``0},System.String,Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens,System.Boolean,System.Boolean)
```
这个函数可以接受``LIMIT``和``TOP``参数

#### Limit``1
```csharp
Microsoft.VisualBasic.Data.IO.SearchEngine.DataEnumerator.Limit``1(System.Collections.Generic.IEnumerable{``0},System.String,System.Int32,Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens,System.Boolean,System.Boolean)
```
直接取出前n个

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|query$|-|
|n%|-|


#### Top``1
```csharp
Microsoft.VisualBasic.Data.IO.SearchEngine.DataEnumerator.Top``1(System.Collections.Generic.IEnumerable{``0},System.String,System.Int32,Microsoft.VisualBasic.Data.IO.SearchEngine.SyntaxParser.Tokens,System.Boolean,System.Boolean)
```
排序之后取得分最高的前n个

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|query$|-|
|n%|-|



