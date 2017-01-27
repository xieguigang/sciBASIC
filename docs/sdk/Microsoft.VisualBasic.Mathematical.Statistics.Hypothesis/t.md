# t
_namespace: [Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis](./index.md)_

Performs one and two sample t-tests on vectors of data.



### Methods

#### Pvalue
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis.t.Pvalue(System.Double,System.Double,Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis.Hypothesis)
```


|Parameter Name|Remarks|
|--------------|-------|
|t#|The t test value|
|v|v is the degrees of freedom|


#### Tcdf
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis.t.Tcdf(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|t|-|
|v#|-|

> 
>  ```
>  Tcdf({0, 2, 4}, 5) = {0.5, 0.949, 0.995}
>  ```
>  

#### Test
```csharp
Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis.t.Test(System.Collections.Generic.IEnumerable{System.Double},System.Collections.Generic.IEnumerable{System.Double},Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis.Hypothesis,System.Double,System.Double,System.Boolean)
```
Performs two sample t-tests on vectors of data.

|Parameter Name|Remarks|
|--------------|-------|
|a|a (non-empty) numeric vector of data values.|
|b|a (non-empty) numeric vector of data values.|
|mu#|a number indicating the True value Of the mean (Or difference In means If you are performing a two sample test).|
|alpha#|-|
|alternative|specifying the alternative hypothesis|
|varEqual|Default using **student's t-test**, set this parameter to False using **Welch's t-test**|

> 
>  ``ttest({0,1,1,1}, {1,2,2,2}, mu:= -1).valid() = True``
>  


