# Poisson
_namespace: [Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension](./index.md)_

Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.



### Methods

#### Dpois
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.Poisson.Dpois(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Boolean)
```
Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.

|Parameter Name|Remarks|
|--------------|-------|
|x|vector of (non-negative integer) quantiles.|
|lambda|vector of (non-negative) means.|
|log|logical; if TRUE, probabilities p are given as log(p).|


#### rPois
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.Poisson.rPois(System.Int32,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
Density, distribution function, quantile function and random generation for the Poisson distribution with parameter lambda.

|Parameter Name|Remarks|
|--------------|-------|
|n|number of random values to return.|
|lambda|vector of (non-negative) means.|



