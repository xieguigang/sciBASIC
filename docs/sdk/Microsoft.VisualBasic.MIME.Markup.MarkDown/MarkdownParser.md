# MarkdownParser
_namespace: [Microsoft.VisualBasic.MIME.Markup.MarkDown](./index.md)_

在markdown里面有两张类型的标记语法：
 
 + 一种是和普通的文本混合在一起的
 + 一种是自己占有一整行文本或者一整个文本块的



### Methods

#### IsHeader
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownParser.IsHeader(System.String,System.String[],System.Int32@)
```


|Parameter Name|Remarks|
|--------------|-------|
|lines|-|
|i|-|


#### MarkdownParser
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownParser.MarkdownParser(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|path|The file path to the markdown text document.|


#### SyntaxParser
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownParser.SyntaxParser(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|md|The markdown file text content, not file path|

> 在这个函数之中只是解析区块的文本数据，段落型的格式则是在另外的一个模块之中解析的


