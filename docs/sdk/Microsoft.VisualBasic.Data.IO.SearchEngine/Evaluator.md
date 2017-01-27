# Evaluator
_namespace: [Microsoft.VisualBasic.Data.IO.SearchEngine](./index.md)_

进行字符串计算的具体过程



### Methods

#### ContainsAny
```csharp
Microsoft.VisualBasic.Data.IO.SearchEngine.Evaluator.ContainsAny(System.String,System.String,System.Boolean,System.Boolean)
```
大小写敏感，在使用之前要先用tolower或者toupper

|Parameter Name|Remarks|
|--------------|-------|
|term$|-|
|searchIn$|-|


#### MustxContains
```csharp
Microsoft.VisualBasic.Data.IO.SearchEngine.Evaluator.MustxContains(System.String,System.String,System.Boolean)
```
假若是一个单词，则要整个单词都相等才行，假若为组合词，则直接匹配

|Parameter Name|Remarks|
|--------------|-------|
|term$|-|
|searchIn$|-|



