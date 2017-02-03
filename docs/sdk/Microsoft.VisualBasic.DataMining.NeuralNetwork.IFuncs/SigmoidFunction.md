# SigmoidFunction
_namespace: [Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs](./index.md)_

Sigmoid activation function.

> The class represents sigmoid activation function with
>  the next expression:
>  
>                 1
>  f(x) = ------------------
>         1 + exp(-alpha * x)
> 
>            alpha * exp(-alpha * x )
>  f'(x) = ---------------------------- = alpha * f(x) * (1 - f(x))
>            (1 + exp(-alpha * x))^2
>  Output range of the function: [0, 1].Functions graph:


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction.#ctor(System.Double)
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction`` class.

|Parameter Name|Remarks|
|--------------|-------|
|alpha|Sigmoid's alpha value.|


#### Clone
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction.Clone
```
Creates a new object that is a copy of the current instance.

_returns: 
 A new object that is a copy of this instance.
 _

#### Derivative
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction.Derivative(System.Double)
```
Calculates function derivative.

|Parameter Name|Remarks|
|--------------|-------|
|x|Function input value.|


_returns: Function derivative, f'(x)._
> The method calculates function derivative at point **`x`**.

#### Derivative2
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction.Derivative2(System.Double)
```
Calculates function derivative.

|Parameter Name|Remarks|
|--------------|-------|
|y|Function output value - the value, which was obtained
 with the help of "Function" method.|


_returns: Function derivative, f'(x)._
> The method calculates the same derivative value as the
>  @``M:Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction.Derivative(System.Double)`` method, but it takes not the input x value
>  itself, but the function value, which was calculated previously with
>  the help of "Function" method.Some applications require as function value, as derivative value,
>  so they can save the amount of calculations using this method to calculate derivative.

#### Function
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.SigmoidFunction.Function(System.Double)
```
Calculates function value.

|Parameter Name|Remarks|
|--------------|-------|
|x|Function input value.|


_returns: Function output value, f(x)._
> The method calculates function value at point **`x`**.


### Properties

#### Alpha
Sigmoid's alpha value.
