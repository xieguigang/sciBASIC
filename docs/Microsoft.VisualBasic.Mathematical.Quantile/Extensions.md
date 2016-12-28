# Extensions
_namespace: [Microsoft.VisualBasic.Mathematical.Quantile](./index.md)_





### Methods

#### GKQuantile
```csharp
Microsoft.VisualBasic.Mathematical.Quantile.Extensions.GKQuantile(System.Collections.Generic.IEnumerable{System.Int64},System.Double,System.Int32)
```
Example Usage:
 
 ```vbnet
 Dim shuffle As Long() = New Long(window_size - 1) {}

 For i As Integer = 0 To shuffle.Length - 1
 shuffle(i) = i
 Next

 shuffle = shuffle.Shuffles

 Dim estimator As QuantileEstimationGK = Shuffle.GKQuantile
 Dim quantiles As Double() = {0.5, 0.9, 0.95, 0.99, 1.0}

 For Each q As Double In quantiles
 Dim estimate As Long = estimator.query(q)
 Dim actual As Long = Shuffle.actually(q)
 Dim out As String = String.Format("Estimated {0:F2} quantile as {1:D} (actually {2:D})", q, estimate, actual)

 Call out.__DEBUG_ECHO
 Next
 ```

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|epsilon|-|
|compact_size|-|


#### SelectByQuantile``1
```csharp
Microsoft.VisualBasic.Mathematical.Quantile.Extensions.SelectByQuantile``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0,System.Int64},System.Double[],System.Double,System.Int32)
```
Selector for object sequence that by using quantile calculation.

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|getValue|
 Object in the input sequence that can be measuring as a numeric value by using this function pointer.
 (通过这个函数指针可以将序列之中的对象转换为可计算quantile的数值)
 |
|quantiles#|-|
|epsilon#|-|
|compact_size%|-|


#### Test
```csharp
Microsoft.VisualBasic.Mathematical.Quantile.Extensions.Test
```
使用示例


