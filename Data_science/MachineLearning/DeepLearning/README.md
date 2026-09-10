# Deep Learning Framework: CNN, RNN, Transformer and ANN Models

Deep learning toolkit for sciBASIC#, spanning a CeNiN CNN reader, a trainable convolutional network, recurrent and Transformer models, and a general ANN facade.

## Overview
- CeNiN (`Convolutional.CeNiN`): loads the "CeNiN NEURAL NETWORK FILE" binary format (conv / relu / pool / softmax layers) and runs the feed-forward phase to classify an image.
- Trainable CNN (`CNN.ConvolutionalNN`): layers are composed fluently with `LayerBuilder` (conv, pool, ReLU/LeakyReLU, sigmoid, tanh, maxout, dropout, LRN, fully connected, softmax/regression/SVM loss) and fitted with SGD, AdaGrad, AdaDelta, Adam, Nesterov or window-grad trainers.
- Character-level RNNs (`RNN`): single- and multi-layer networks over an `Alphabet` with `RNNTrainer`, plus a `CharRNN` port.
- Transformer (`Transformer`): embedding, multi-head attention, encoder/decoder stacks and an Adam optimizer, following "Attention is all you need".
- Artificial neural networks (`NeuralNetwork.Netz` / `NeuralNetwork.Network`): classic MLP API implemented on top of the CNN fully-connected kernel, with save/load of `.cnn` models.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.Convolutional.CeNiN` — `LoadFile(stream)` reader; `Solver.DetectObject` extension classifies a bitmap.
- `Microsoft.VisualBasic.MachineLearning.CNN.ConvolutionalNN` — `New(layers As LayerBuilder)`, `forward`, `backward`, `predict(v As Double())`.
- `Microsoft.VisualBasic.MachineLearning.CNN.LayerBuilder` — fluent builder (`buildInputLayer`, `buildConvLayer`, `buildPoolLayer`, `buildFullyConnectedLayer`, ...).
- `Microsoft.VisualBasic.MachineLearning.CNN.Trainer` and `CNN.trainers.*Trainer` — training loop and the AdaGrad/AdaDelta/Adam/SGD/Nesterov/window-grad update rules.
- `Microsoft.VisualBasic.MachineLearning.RNN.net.MultiLayerCharLevelRNN` — stacked character-level RNN with sampling interfaces.
- `Microsoft.VisualBasic.MachineLearning.Transformer.TransformerModel` — encoder/decoder stack, `MultiHeadAttention`, `Embedding`, `Optimizer` (Adam).
- `Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Netz` — MLP facade: `train`, `predict`, `TotalError`, `Save`/`Load`.
- `Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Network` — MLP with `ForwardPropagate`, `BackPropagate`, `TrainBatch`, `Compute`, `Save`/`Load`.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork

Dim net As New Netz(inputNeurons:=2, hiddenNeurons:=4, hiddenLayers:=1, outputNeurons:=1,
                    activate:=Function(x) 1.0 / (1.0 + Math.Exp(-x)))
net.LERNRATE = 0.05

For i As Integer = 1 To 8000
    Call net.train({rnd.NextDouble(), rnd.NextDouble()}, {0.8})
Next

Dim y As Double() = net.predict({0.3, 0.5})
Call net.Save("netz_model.cnn")
```

## Package
- Assembly: `Microsoft.VisualBasic.DeepLearning` (RootNamespace `Microsoft.VisualBasic.MachineLearning`)
- TargetFramework: `net10.0`
- Tags: `scibasic;deep-learning;cnn;rnn;transformer`

## License
GPL-3.0-or-later
