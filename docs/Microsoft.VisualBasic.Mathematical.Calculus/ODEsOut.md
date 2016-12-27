# ODEsOut
_namespace: [Microsoft.VisualBasic.Mathematical.Calculus](./index.md)_

ODEs output, this object can populates the @``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.y`` 
 variables values through its enumerator interface.



### Methods

#### GetY0
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.GetY0
```
Using the first value of @``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.y`` as ``y0``

#### Join
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.Join
```
Merge @``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.y0`` into @``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.params``

#### LoadFromDataFrame
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.LoadFromDataFrame(System.String,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|csv$|-|
|noVars|ODEs Parameter value is not exists in the data file?|



### Properties

#### HaveNaN
Is there NAN value in the function value @``P:Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut.y`` ???
