# Classical Machine Learning: Random Forest, SVM, GA and Q-Learning

Classical machine learning library for sciBASIC#: ensembles, kernel machines, evolutionary optimisation and reinforcement learning, plus the shared sample/activation plumbing.

## Overview
- Random forests (`RandomForests.RanFog`) for classification and regression with out-of-bag MSE, variable importance and GEBV prediction.
- A LibSVM-compatible port (`SVM`): problems over sparse `Node` vectors, RBF/linear/polynomial/sigmoid/precomputed kernels, C-SVC / nu-SVC / one-class / epsilon-SVR / nu-SVR, scaling and range/Gaussian transforms, grid parameter selection, cross-validation, prediction with optional probabilities, and JSON/XML model storage.
- Evolutionary algorithms (`Darwinism`): genetic algorithm engine with population models, elite/simple replacement strategies, parallel fitness evaluation, and differential evolution.
- Q-learning (`QLearning`): Q-table with action selection, `QModel` persistence and a maze demo.
- Shared data layer (`ComponentModel`): `DataSet`/`SampleList`/`NormalizeMatrix` and XML-serialisable activation functions (sigmoid, bipolar sigmoid, tanh, ReLU, softplus, sinc, threshold, linear).

## Key Types
- `Microsoft.VisualBasic.MachineLearning.SVM.Training` — `Train(problem, parameters)` and `PerformCrossValidation`.
- `Microsoft.VisualBasic.MachineLearning.SVM.Prediction` — `Predict(model, x)`, `PredictLabels`, `PredictProbability`.
- `Microsoft.VisualBasic.MachineLearning.SVM.Parameter` / `KernelType` / `SvmType` — kernel and solver configuration.
- `Microsoft.VisualBasic.MachineLearning.RandomForests.RanFog` — forest trainer; `Run(train As Data) As Result`.
- `Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Population.Population` and `Darwinism.GeneticAlgorithm.GeneticAlgorithm` — evolutionary engines.
- `Microsoft.VisualBasic.MachineLearning.QLearning.QTable` / `Model.QModel` — Q-value table and its serialisable model.
- `Microsoft.VisualBasic.MachineLearning.ComponentModel.DataSet` / `SampleList` / `NormalizeMatrix` — XML training datasets and normalisation.
- `Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations` — activation functions with parseable `ActiveFunction` XML storage.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.SVM

' labels() are class names, samples()() are sparse Node vectors
Dim problem As New Problem(labels, samples, maxIndex)
Dim model As Model = Training.Train(problem, New Parameter() With {.kernelType = KernelType.RBF})
Dim prediction As SVMPrediction = Prediction.Predict(model, samples(0))
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning`
- TargetFramework: `net10.0`
- Tags: `scibasic;machine-learning;random-forest;svm;genetic-algorithm`

## License
GPL-3.0-or-later
