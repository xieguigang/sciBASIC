# DocParserAPI
_namespace: [Microsoft.VisualBasic.MIME.Markup.HTML](./index.md)_





### Methods

#### __innerTextParser
```csharp
Microsoft.VisualBasic.MIME.Markup.HTML.DocParserAPI.__innerTextParser(System.String@,System.String,System.Boolean@)
```
在得到一个标签之后前面的数据会被扔掉，开始解析标签后面的数据

|Parameter Name|Remarks|
|--------------|-------|
|innerText|-|
|parent|-|


_returns: 这个函数是一个递归函数_

#### TextParse
```csharp
Microsoft.VisualBasic.MIME.Markup.HTML.DocParserAPI.TextParse(System.String@)
```
解析标签开始和结束的位置之间的内部html文本

|Parameter Name|Remarks|
|--------------|-------|
|doc|-|

> 这个方法是最开始的解析函数，非递归的


