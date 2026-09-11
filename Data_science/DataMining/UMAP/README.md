# UMAP Uniform Manifold Approximation and Projection

Uniform Manifold Approximation and Projection: builds a fuzzy simplicial set from k-nearest neighbours and optimises a low-dimensional layout by stochastic gradient descent.

## Overview
- Approximate nearest neighbour search by NN-descent (`NNDescent`, `NNDescentLoop`) with random-projection trees (`RPTree`) for initialisation, or by KD-tree search (`KNearestNeighbour`, `KDTreeMetric`).
- Fuzzy simplicial set construction with smooth k-nearest-neighbour distances, local connectivity and set-operation mixing.
- Parallel pipeline: a single `ParallelConfig` drives the forest build, NN-descent, heap sorting and the SGD epochs; SIMD helpers speed up vector math.
- Embedding optimisation via negative sampling over a sparse graph with configurable epochs, learning rate, spread, minimum distance and gradient clipping.

## Key Types
- `Microsoft.VisualBasic.DataMining.UMAP.Umap` — the estimator: `InitializeFit`, `Step`, `GetEmbedding`, `GetGraph`.
- `Microsoft.VisualBasic.DataMining.UMAP.KNN.NNDescent` — nearest neighbour descent that produces the approximate k-NN graph.
- `Microsoft.VisualBasic.DataMining.UMAP.KNN.KNearestNeighbour` — KD-tree based exact k-nearest neighbour search alternative.
- `Microsoft.VisualBasic.DataMining.UMAP.KNN.KNNState` — indices and distances resulting from a neighbour search.
- `Microsoft.VisualBasic.DataMining.UMAP.KNN.SmoothKNN` — smooths neighbour distances into fuzzy simplicial set weights.
- `Microsoft.VisualBasic.DataMining.UMAP.Components.ParallelConfig` — shared parallelism configuration for the whole pipeline.
- `Microsoft.VisualBasic.DataMining.UMAP.Components.SparseMatrix.SparseMatrix` — sparse graph/edge weight container with SIMD-aware map and arithmetic.
- `Microsoft.VisualBasic.DataMining.UMAP.UMAPProject` — binary file model for saving an embedding result.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.UMAP

' data is a Double()() jagged array of observations
Dim umap As New Umap(dimensions:=2, numberOfNeighbors:=15)

Call umap.InitializeFit(data)
Call umap.Step(umap.GetNEpochs())

Dim Y As Double()() = umap.GetEmbedding()
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.UMAP`
- TargetFramework: `net10.0`
- Tags: `scibasic;umap;dimensionality-reduction;manifold-learning;nearest-neighbor`

## License
GPL-3.0-or-later
