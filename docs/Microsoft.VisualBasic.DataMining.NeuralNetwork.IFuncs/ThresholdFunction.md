# ThresholdFunction
_namespace: [Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs](./index.md)_

Threshold activation function.

> The class represents threshold activation function with
>  the next expression:
>  
>  f(x) = 1, if x >= 0, otherwise 0
>  Output range of the function: [0, 1].Functions graph:


### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.ThresholdFunction.#ctor
```
Initializes a new instance of the @``T:Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.ThresholdFunction`` class.

#### Clone
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.ThresholdFunction.Clone
```
Creates a new object that is a copy of the current instance.

_returns: 
 A new object that is a copy of this instance.
 _

#### Derivative
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.ThresholdFunction.Derivative(System.Double)
```
Calculates function derivative (not supported).

|Parameter Name|Remarks|
|--------------|-------|
|x|Input value.|


_returns: Always returns 0._
> The method is not supported, because it is not possible to
>  calculate derivative of the function.

#### Derivative2
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.ThresholdFunction.Derivative2(System.Double)
```
Calculates function derivative (not supported).

|Parameter Name|Remarks|
|--------------|-------|
|y|Input value.|


_returns: Always returns 0._
> The method is not supported, because it is not possible to
>  calculate derivative of the function.

#### Function
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.ThresholdFunction.Function(System.Double)
```
Calculates function value.

|Parameter Name|Remarks|
|--------------|-------|
|x|Function input value.|


_returns: Function output value, f(x)._
> The method calculates function value at point **`x`**.


