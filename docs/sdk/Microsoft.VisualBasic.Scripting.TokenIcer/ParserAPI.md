# ParserAPI
_namespace: [Microsoft.VisualBasic.Scripting.TokenIcer](./index.md)_





### Methods

#### GetCodeComment
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.ParserAPI.GetCodeComment(System.String,System.String[])
```
假若返回来的是空字符串，则说明不是注释行

|Parameter Name|Remarks|
|--------------|-------|
|line$|-|
|prefix$|-|


#### TokenParser``1
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.ParserAPI.TokenParser``1(Microsoft.VisualBasic.Scripting.TokenIcer.TokenParser{``0},System.String,Microsoft.VisualBasic.Scripting.TokenIcer.StackTokens{``0})
```


|Parameter Name|Remarks|
|--------------|-------|
|parser|-|
|expr|表达式字符串|
|stackT|-|


#### TryCast``1
```csharp
Microsoft.VisualBasic.Scripting.TokenIcer.ParserAPI.TryCast``1(Microsoft.VisualBasic.Scripting.TokenIcer.Token{``0})
```
Try cast the token value to a .NET object based on the token type name.

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



