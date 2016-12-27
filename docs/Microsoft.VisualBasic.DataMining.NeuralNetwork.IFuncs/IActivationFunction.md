# IActivationFunction
_namespace: [Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs](./index.md)_

Activation function interface.

> All activation functions, which are supposed to be used with
>  neurons, which calculate their output as a function of weighted sum of
>  their inputs, should implement this interfaces.
>  


### Methods

#### Derivative
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.IActivationFunction.Derivative(System.Double)
```
Calculates function derivative.

|Parameter Name|Remarks|
|--------------|-------|
|x|Function input value.|


_returns: Function derivative, f'(x)._
> The method calculates function derivative at point **`x`**.

#### Derivative2
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.IActivationFunction.Derivative2(System.Double)
```
Calculates function derivative.

|Parameter Name|Remarks|
|--------------|-------|
|y|Function output value - the value, which was obtained
 with the help of "Function" method.|


_returns: Function derivative, f'(x)._
> The method calculates the same derivative value as the
>  @``M:Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.IActivationFunction.Derivative(System.Double)`` method, but it takes not the input x value
>  itself, but the function value, which was calculated previously with
>  the help of "Function" method.Some applications require as function value, as derivative value,
>  so they can save the amount of calculations using this method to calculate derivative.

#### Function
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.IActivationFunction.Function(System.Double)
```
Calculates function value.

|Parameter Name|Remarks|
|--------------|-------|
|x|Function input value.|


_returns: Function output value, f(x)._
> The method calculates function value at point **`x`**.


