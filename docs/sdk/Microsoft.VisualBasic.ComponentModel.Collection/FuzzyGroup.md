# FuzzyGroup
_namespace: [Microsoft.VisualBasic.ComponentModel.Collection](./index.md)_

对数据进行分组，通过标签数据的相似度



### Methods

#### FuzzyGroups``1
```csharp
Microsoft.VisualBasic.ComponentModel.Collection.FuzzyGroup.FuzzyGroups``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0,System.String},System.Double,System.Boolean)
```
Grouping objects in a collection based on their unique key string Fuzzy equals to others'.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|getKey|The unique key provider|
|cut|字符串相似度的阈值|

> 
>  由于list在查找方面的速度非常的慢，而字典可能在生成的时候会慢一些，但是查找很快，所以在这里函数里面使用字典来替代列表
>  


