# Network
_namespace: [Microsoft.VisualBasic.DataMining.NeuralNetwork](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.Network.#ctor(System.Int32,System.Int32,System.Int32,System.Double,System.Double,Microsoft.VisualBasic.DataMining.NeuralNetwork.IFuncs.IActivationFunction)
```


|Parameter Name|Remarks|
|--------------|-------|
|inputSize|>=2|
|hiddenSize|>=2|
|outputSize|>=1|
|learnRate__1|-|
|momentum__2|-|


#### Compute
```csharp
Microsoft.VisualBasic.DataMining.NeuralNetwork.Network.Compute(System.Double[])
```
Compute result output for the neuron network **`inputs`**.
 (请注意ANN的输出值是在0-1之间的，所以还需要进行额外的编码和解码)

|Parameter Name|Remarks|
|--------------|-------|
|inputs|-|



