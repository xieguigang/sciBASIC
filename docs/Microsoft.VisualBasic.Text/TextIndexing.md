# TextIndexing
_namespace: [Microsoft.VisualBasic.Text](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Text.TextIndexing.#ctor(System.String,System.Int32,System.Int32)
```
Creates a text index instance object for the statement fuzzy match in the whole text document.

|Parameter Name|Remarks|
|--------------|-------|
|text|-|
|min|-|
|max|-|


#### Found
```csharp
Microsoft.VisualBasic.Text.TextIndexing.Found(System.String,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|keyword|-|
|cutoff|表示出现连续的m匹配的片段的长度,-1表示所搜索的关键词片段的长度一半|


#### IsMatch
```csharp
Microsoft.VisualBasic.Text.TextIndexing.IsMatch(System.String,System.Int32)
```
函数返回最长的匹配的个数，-1表示没有匹配

|Parameter Name|Remarks|
|--------------|-------|
|m|-|
|cutoff|-|



### Properties

#### PreCache
为了用于加速批量匹配计算的效率而生成的一个缓存对象
