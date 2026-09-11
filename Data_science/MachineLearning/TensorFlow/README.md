# Pure VB.NET Tensor Library with NumPy API and Autodiff

Self-contained tensor engine for sciBASIC#, not a Google TensorFlow binding: a managed multi-dimensional `Tensor`, a NumPy-style API, neural network activations and losses, and reverse-mode automatic differentiation.

## Overview
- `Tensor` is an N-dimensional double array with shape/indexers, `MatMul`, `Transpose`, `Reshape`, `Apply`, reductions and the initialisers `Zeros`, `Ones`, `Random`, `RandomNormal`, `HeInit`, `XavierInit`, plus a `Gradient` slot for training.
- `NumPy.NumPyModule` offers a NumPy-like static surface: `array`, `zeros`, `ones`, `arange`, `linspace`, `eye`, `reshape`, `transpose`, `dot`/`matmul`, `concatenate`/`stack`/`vstack`/`hstack`, `sum`/`mean`/`std`/`max`/`min`/`argmax`, and `NumPyModule.RandomState` for `rand`, `randn`, `randint`.
- `Math` implements TensorFlow-style elementwise math and clipping (`exp`, `log`, `sqrt`, `square`, `abs`, `sin`, `cos`, `tanh`, `sigmoid`, `pow`, `negative`); `nn` implements activations and losses (`relu`, `sigmoid`, `softmax`, `sigmoid_cross_entropy_with_logits`, `softmax_cross_entropy_with_logits`, `sparse_softmax_cross_entropy_with_logits`, `mse_loss`).
- `AutomaticDifferentiation` supplies reverse-mode autodiff: the `Rev` value type with operator-overloaded derivatives, `Checkpoints` for gradient checkpointing, and parallel `SIMDMatMul` / `MultiplyScale` kernels.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.TensorFlow.Tensor` — the multi-dimensional numeric container all computation is built on.
- `Microsoft.VisualBasic.MachineLearning.TensorFlow.NumPy.NumPyModule` — NumPy-compatible static API over `Tensor`.
- `Microsoft.VisualBasic.MachineLearning.TensorFlow.NumPy.NumPyModule.RandomState` — random tensor generation (`rand`, `randn`, `randint`).
- `Microsoft.VisualBasic.MachineLearning.TensorFlow.Math` — elementwise math, reductions and clipping.
- `Microsoft.VisualBasic.MachineLearning.TensorFlow.nn` — activations, losses and regularisation.
- `Microsoft.VisualBasic.MachineLearning.TensorFlow.AutomaticDifferentiation.Rev` — reverse-mode automatic differentiation scalar with gradient accumulation.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.NumPy

Dim a As Tensor = NumPyModule.zeros(3, 4)
Dim b As Tensor = NumPyModule.RandomState.randn(4, 2)
Dim y As Tensor = NumPyModule.dot(a, b)      ' shape (3, 2)
Dim p As Tensor = nn.softmax(y)
Dim loss As Tensor = Math.sum(Math.square(y))

Console.WriteLine(p.Shape(0) & " x " & p.Shape(1))
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.TensorFlow`
- TargetFramework: `net10.0`
- Tags: `scibasic;tensor;numpy;automatic-differentiation;numerical`

## License
GPL-3.0-or-later
