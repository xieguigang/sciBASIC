# Evaluations
_namespace: [Microsoft.VisualBasic.Text.Similarity](./index.md)_





### Methods

#### Evaluate
```csharp
Microsoft.VisualBasic.Text.Similarity.Evaluations.Evaluate(System.String,System.String,System.Boolean,System.Double,Microsoft.VisualBasic.Text.DistResult@)
```
两个字符串之间是通过单词的排布的相似度来比较相似度的

|Parameter Name|Remarks|
|--------------|-------|
|s1|-|
|s2|-|
|ignoreCase|-|
|cost#|-|
|dist|-|


#### IsOrdered
```csharp
Microsoft.VisualBasic.Text.Similarity.Evaluations.IsOrdered(System.String[],System.String[],System.Boolean,System.Boolean)
```
查看**`s2`**之中的字符串的顺序是否是在**`s1`**之中按顺序排序的

|Parameter Name|Remarks|
|--------------|-------|
|s1$|-|
|s2$|-|
|caseSensitive|-|


#### LevenshteinEvaluate
```csharp
Microsoft.VisualBasic.Text.Similarity.Evaluations.LevenshteinEvaluate(System.String,System.String,System.Boolean,System.Double,Microsoft.VisualBasic.Text.DistResult@)
```
计算字符串，这个是直接通过计算字符而非像@``M:Microsoft.VisualBasic.Text.Similarity.Evaluations.Evaluate(System.String,System.String,System.Boolean,System.Double,Microsoft.VisualBasic.Text.DistResult@)``方法之中计算单词的

|Parameter Name|Remarks|
|--------------|-------|
|s1$|-|
|s2$|-|
|ignoreCase|-|
|cost#|-|
|dist|-|


#### TokenOrders
```csharp
Microsoft.VisualBasic.Text.Similarity.Evaluations.TokenOrders(System.String,System.String,System.Boolean)
```
以s1为准则，将s2进行比较，返回s2之中的单词在s1之中的排列顺序

|Parameter Name|Remarks|
|--------------|-------|
|s1|-|
|s2|-|


_returns: 序列之中的-1表示s2之中的单词在s1之中不存在_


