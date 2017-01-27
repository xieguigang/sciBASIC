# StreamExtension
_namespace: [Microsoft.VisualBasic.Mathematical.Calculus](./index.md)_





### Methods

#### DataFrame
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.StreamExtension.DataFrame(Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut,System.String,System.Int32)
```
Generates datafram and then can makes the result save data into a csv file.

|Parameter Name|Remarks|
|--------------|-------|
|xDisp|-|
|fix%|Formats output by using @``M:System.Math.Round(System.Decimal)``|


#### LoadFromDataFrame
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.StreamExtension.LoadFromDataFrame(System.String,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|csv$|-|
|noVars|ODEs Parameter value is not exists in the data file?|


#### Merge
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.StreamExtension.Merge(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut},System.Func{System.Collections.Generic.IEnumerable{System.Double},System.Double})
```


|Parameter Name|Remarks|
|--------------|-------|
|source|这些对象的X的尺度和范围必须都是一致的|
|method|默认是平均值|



