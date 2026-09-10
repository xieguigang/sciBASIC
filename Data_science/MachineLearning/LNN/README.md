# Liquid Neural Networks with ODE Solvers for Time Series

Continuous-time liquid neural networks for sciBASIC#, where neuron dynamics follow learnable ODEs integrated by explicit or adaptive solvers.

## Overview
- `LiquidCell` implements the liquid state equation `dh/dt = (1/tau + f) * (A - h)` with learnable time constant, input/recurrent weights, optional gating and bounded `tau`; selectable dynamics modes (CT-RNN, LTC, CfC).
- ODE integration through `ODESolver`: `EulerStep`, `HeunStep`, `RK4Step` and an adaptive `AdaptiveRK45Step` (Dormand-Prince style step doubling).
- Backpropagation through time with constant-memory replay: only evaluated state points are recorded (`StepRecord`, `ODEStages`) and activations are recomputed on the backward pass.
- `LNNTrainer` provides MSE/MAE losses, exact reverse-mode gradients, global-L2 gradient clipping and SGD or Adam updates, with `TimeSeriesUtils` for sliding windows, normalisation and metrics.

## Key Types
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.LiquidNeuralNetwork` — full model: `Forward`, `ProcessSequence`, `ForwardSequence` (irregular time grids), `BackwardOutput`, `BackwardLiquid`.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.LiquidCell` — single-step dynamics: `ComputeDerivative`, `GetSystemTau`, `Forward(input, dt, solverType)`, `Backward`.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.LiquidLayer` — stack of liquid cells with state reset and record management.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.ODESolver` — Euler / Heun / RK4 / adaptive RK45 integrators.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.LNNTrainer` — `TrainStep`, `TrainSequence`, `Fit`, `Backward`, `Step`, `MSE`, `MAE`.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.ParameterPair` — parameter/gradient pairing used by the optimizers and gradient clipping.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.TimeSeriesUtils` — `CreateSlidingWindowDataset`, `Normalize`, `Standardize`, `CalculateMetrics`.
- `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork.ActivationFunctions` — tensor sigmoid/tanh/ReLU/leaky-ReLU/softmax and derivatives.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork

Dim net As New LiquidNeuralNetwork(inputSize:=2, hiddenSize:=16, outputSize:=1,
                                   numLiquidLayers:=1,
                                   mode:=LiquidMode.CT_RNN) With {
    .SolverType = "rk4",
    .DefaultDt = 0.1
}

Dim trainer As New LNNTrainer(net, learningRate:=0.01) With {.OptimizerType = "adam"}
Dim losses As List(Of Double) = trainer.Fit(inputSequences, targetSequences, epochs:=200)
```

## Package
- Assembly: `Microsoft.VisualBasic.DeepLearning.LNN` (RootNamespace `Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork`)
- TargetFramework: `net10.0`
- Tags: `scibasic;liquid-neural-network;ode;time-series;continuous-time`

## License
GPL-3.0-or-later
