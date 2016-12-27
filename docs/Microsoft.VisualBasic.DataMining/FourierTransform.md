# FourierTransform
_namespace: [Microsoft.VisualBasic.DataMining](./index.md)_

Fourier transformation.

> The class implements one dimensional and two dimensional
>  Discrete and Fast Fourier Transformation.


### Methods

#### DFT
```csharp
Microsoft.VisualBasic.DataMining.FourierTransform.DFT(System.Numerics.Complex[],Microsoft.VisualBasic.DataMining.FourierTransform.Direction)
```
One dimensional Discrete Fourier Transform.

|Parameter Name|Remarks|
|--------------|-------|
|data|Data to transform.|
|direction__1|Transformation direction.|


#### DFT2
```csharp
],Microsoft.VisualBasic.DataMining.FourierTransform.Direction)
```
Two dimensional Discrete Fourier Transform.

|Parameter Name|Remarks|
|--------------|-------|
|data|Data to transform.|
|direction__1|Transformation direction.|


#### FFT
```csharp
Microsoft.VisualBasic.DataMining.FourierTransform.FFT(System.Numerics.Complex[],Microsoft.VisualBasic.DataMining.FourierTransform.Direction)
```
One dimensional Fast Fourier Transform.

|Parameter Name|Remarks|
|--------------|-------|
|data|Data to transform.|
|direction__1|Transformation direction.|

> The method accepts **`data`** array of 2n size
>  only, where n may vary in the [1, 14] range.

#### FFT2
```csharp
],Microsoft.VisualBasic.DataMining.FourierTransform.Direction)
```
Two dimensional Fast Fourier Transform.

|Parameter Name|Remarks|
|--------------|-------|
|data|Data to transform.|
|direction|Transformation direction.|

> The method accepts **`data`** array of 2n size
>  only in each dimension, where n may vary in the [1, 14] range. For example, 16x16 array
>  is valid, but 15x15 is not.


