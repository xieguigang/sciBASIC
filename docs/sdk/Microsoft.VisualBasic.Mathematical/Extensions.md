# Extensions
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_





### Methods

#### FirstDecrease
```csharp
Microsoft.VisualBasic.Mathematical.Extensions.FirstDecrease(System.Collections.Generic.IEnumerable{System.Double},System.Double)
```
返回数值序列之中的首次出现符合条件的减少的位置

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|ratio|-|


#### FirstIncrease
```csharp
Microsoft.VisualBasic.Mathematical.Extensions.FirstIncrease(System.Collections.Generic.IEnumerable{System.Double},System.Double,System.Double)
```
只对单调递增的那一部分曲线有效

|Parameter Name|Remarks|
|--------------|-------|
|data|y值|
|alpha|-|


#### Reach
```csharp
Microsoft.VisualBasic.Mathematical.Extensions.Reach(System.Collections.Generic.IEnumerable{System.Double},System.Double,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|n|-|
|offset|距离目标数据点**`n`**的正负偏移量|


#### seq2
```csharp
Microsoft.VisualBasic.Mathematical.Extensions.seq2(System.Double,System.Double,System.Double)
```
[Sequence Generation] Generate regular sequences. seq is a standard generic with a default method.

|Parameter Name|Remarks|
|--------------|-------|
|From|
 the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
 |
|To|
 the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
 |
|By|number: increment of the sequence|


#### Sim
```csharp
Microsoft.VisualBasic.Mathematical.Extensions.Sim(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
余弦相似度

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|


#### Tanimoto
```csharp
Microsoft.VisualBasic.Mathematical.Extensions.Tanimoto(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
这是x和y所共有的属性个数与x或y所具有的属性个数之间的比率。这个函数被称为Tanimoto系数或Tanimoto距离，
 它经常用在信息检索和生物学分类中。(余弦度量的一个简单的变种)
 当属性是二值属性时，余弦相似性函数可以用共享特征或属性解释。假设如果xi=1，则对象x具有第i个属性。于是，
 x·y是x和y共同具有的属性数，而xy是x具有的属性数与y具有的属性数的几何均值。于是，sim(x,y)是公共属性相
 对拥有的一种度量。

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|

> 
>  http://xiao5461.blog.163.com/blog/static/22754562201211237567238/
>  


