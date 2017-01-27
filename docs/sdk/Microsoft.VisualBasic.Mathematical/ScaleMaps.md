# ScaleMaps
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_





### Methods

#### GenerateMapping
```csharp
Microsoft.VisualBasic.Mathematical.ScaleMaps.GenerateMapping(System.Collections.Generic.IEnumerable{System.Int32},System.Int32,System.Int32)
```
如果每一个数值之间都是相同的大小，则返回原始数据，因为最大值与最小值的差为0，无法进行映射的创建（会出现除0的错误）

|Parameter Name|Remarks|
|--------------|-------|
|data|-|

> 为了要保持顺序，不能够使用并行拓展

#### Scale
```csharp
Microsoft.VisualBasic.Mathematical.ScaleMaps.Scale(System.Collections.Generic.IEnumerable{System.Double},System.Boolean,System.Boolean)
```
Function centers and/or scales the columns of a numeric matrix.

|Parameter Name|Remarks|
|--------------|-------|
|data|numeric matrix|
|center|either a logical value or a numeric vector of length equal to the number of columns of x|
|isScale|either a logical value or a numeric vector of length equal to the number of columns of x|


#### TrimRanges
```csharp
Microsoft.VisualBasic.Mathematical.ScaleMaps.TrimRanges(System.Double[],System.Double,System.Double)
```
Trims the data ranges, 
 if n in **`Dbl`** vector is less than **`min`**, then set n = min;
 else if n is greater than **`max`**, then set n value to max, 
 else do nothing.

|Parameter Name|Remarks|
|--------------|-------|
|Dbl|-|
|min|-|
|max|-|



