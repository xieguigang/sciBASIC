# ODEs
_namespace: [Microsoft.VisualBasic.Mathematical.Calculus](./index.md)_

Solving ODEs in R language, as example for test this class:
 
 ```R
 func <- function(t, x, parms) {
 with(as.list(c(parms, x)), {
 
 dP <- a * P - b * C * P
 dC <- b * P * C - c * C
 
 list(c(dP, dC))
 })
 }

 y0 <- c(P = 2, C = 1)
 parms <- c(a = 0.1, b = 0.1, c = 0.1)
 out <- ode(y = y0, times = 0:100, func, parms = parms)
 
 head(out)
 plot(out)
 ```



### Methods

#### __getY0
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.__getY0(System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|incept|是否是为蒙特卡洛实验设计的？|


#### __rungeKutta
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.__rungeKutta(System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector@,System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector@)
```
RK4 ODEs solver

|Parameter Name|Remarks|
|--------------|-------|
|dxn|The x initial value.(x初值)|
|dyn|The y initial value.(初值y(n))|
|dh|Steps delta.(步长)|
|dynext|
 Returns the y(n+1) result from this parameter.(下一步的值y(n+1))
 |


#### func
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.func(System.Double,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector@)
```
在这里计算具体的微分方程组

|Parameter Name|Remarks|
|--------------|-------|
|dx|-|
|dy|-|


#### GetParameters
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.GetParameters(System.Type)
```
Get function parameters name collection

#### GetVariables
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.GetVariables(System.Type)
```
Get Y names

#### Solve
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.Solve(System.Int32,System.Double,System.Double,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|n|A larger value of this parameter, will makes your result more precise.|
|a|-|
|b|-|


#### y0
```csharp
Microsoft.VisualBasic.Mathematical.Calculus.ODEs.y0
```
初值


### Properties

#### Parameters
返回的值包括@``T:System.Double``类型的Field或者@``T:Microsoft.VisualBasic.Language.float``类型的field
