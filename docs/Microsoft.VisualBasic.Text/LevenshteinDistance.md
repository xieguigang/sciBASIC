# LevenshteinDistance
_namespace: [Microsoft.VisualBasic.Text](./index.md)_

Levenshtein Edit Distance Algorithm for measure string distance

> 
>  http://www.codeproject.com/Tips/697588/Levenshtein-Edit-Distance-Algorithm
>  


### Methods

#### __computeRoute
```csharp
])
```
计算lev编辑的变化路径

|Parameter Name|Remarks|
|--------------|-------|
|hypotheses|-|
|result|-|
|i|-|
|j|-|
|distTable|-|


#### __createTable
```csharp
Microsoft.VisualBasic.Text.LevenshteinDistance.__createTable(System.Int32[],System.Int32[],System.Double)
```
Creates distance table for data visualization

|Parameter Name|Remarks|
|--------------|-------|
|reference|-|
|hypotheses|-|
|cost|-|


#### ComputeDistance
```csharp
Microsoft.VisualBasic.Text.LevenshteinDistance.ComputeDistance(System.String,System.String,System.Double)
```
The edit distance between two strings is defined as the minimum number of
 edit operations required to transform one string into another.

|Parameter Name|Remarks|
|--------------|-------|
|reference|-|
|hypotheses|-|
|cost|-|


#### ComputeDistance``1
```csharp
Microsoft.VisualBasic.Text.LevenshteinDistance.ComputeDistance``1(``0[],``0[],Microsoft.VisualBasic.Text.LevenshteinDistance.Equals{``0},Microsoft.VisualBasic.Text.LevenshteinDistance.ToChar{``0},System.Double)
```
泛型序列的相似度的比较计算方法，这个会返回所有的数据

|Parameter Name|Remarks|
|--------------|-------|
|reference|-|
|hypotheses|-|
|equals|-|
|asChar|这个只是用于进行显示输出的|
|cost|-|


#### CreateTable``1
```csharp
Microsoft.VisualBasic.Text.LevenshteinDistance.CreateTable``1(``0[],``0[],System.Double,Microsoft.VisualBasic.Text.LevenshteinDistance.Equals{``0})
```
用于泛型的序列相似度比较

|Parameter Name|Remarks|
|--------------|-------|
|reference|-|
|hypotheses|-|
|cost|-|
|equals|泛型化的元素等价性的比较方法|



