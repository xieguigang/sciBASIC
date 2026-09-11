# Restricted Boltzmann Machines with Contrastive Divergence Training

Restricted Boltzmann machine library for sciBASIC#, offering single, deep and recurrent RBM models trained by contrastive divergence or back-error propagation, plus dense matrix math and word-encoding helpers for text input.

## Overview
- `RBM` holds the visible-to-hidden weight matrix; `DeepRBM` stacks `RBMLayer`s described by `LayerParameters`, which allows convolution-like layouts inside a deep network.
- Training algorithms: `ContrastiveDivergence`, `DeepContrastiveDivergence`, `MultiThreadedDeepContrastiveDivergence`, `RecurrentContrastiveDivergence` and `BackErrorPropagation`, all configured through `LearningParameters` (epochs, learning rate, logging).
- `DenseMatrix` provides immutable-style linear algebra on top of Parallel Colt — `make`, `random`, `randomGaussian`, `dot`, `transpose`, `add/subtract/multiply/divide`, `pow`, `sum`, split/concat columns — with pluggable `DoubleFunction` / `DoubleDoubleFunction` operators (sigmoid, Gaussian noise, power, round, ...).
- Text helpers: `WordDictionary`, `NGramGenerator`, `RandomWordEncoder` and `DiscreteRandomWordEncoder` turn words into vectors for RBM input; `EuclideanDistanceFunction` / `DiscreteDistanceFunction` score matrix similarity.
- `RBMPersister` and `DeepRBMPersister` are present but their save/load bodies are not implemented yet (`load` throws `NotImplementedException`).

## Key Types
- `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.RBM` — a single restricted Boltzmann machine with its weight matrix.
- `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.learn.ContrastiveDivergence` — CD-1 trainer; `learn`, `runVisible`, `runHidden`, `dayDream`.
- `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep.DeepRBM` — multi-layer RBM stack built from `RBMLayer` / `LayerParameters`.
- `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.factory.RandomRBMFactory` — creates randomly initialised RBMs for a layer configuration.
- `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.DenseMatrix` — dense matrix with functional element-wise and matrix operators.
- `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nlp.WordDictionary` — vocabulary/index mapping used by the word encoders.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.learn

Dim rbm As New RBM(visibleSize:=6, hiddenSize:=3)
Dim params As New LearningParameters With {
    .Epochs = 1000,
    .LearningRate = 0.1,
    .Log = True
}
Dim trainer As New ContrastiveDivergence(params)
Dim data = DenseMatrix.make({
    New Double() {1, 1, 1, 0, 0, 0},
    New Double() {0, 0, 0, 1, 1, 1}
})

' each row is one training sample
Call trainer.learn(rbm, data)

Dim hidden As DenseMatrix = trainer.runVisible(rbm, data)
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine`
- TargetFramework: `net10.0`
- Tags: `scibasic;rbm;contrastive-divergence;deep-learning;unsupervised`

## License
GPL-3.0-or-later
