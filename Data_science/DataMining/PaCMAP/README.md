# PaCMAP Pairwise Controlled Manifold Approximation Embedding

PaCMAP dimensionality reduction that learns a low-dimensional layout by balancing neighbour, mid-near and further point pairs.

## Overview
- Preserves local and global structure by optimising three pair families (nearest neighbours, mid-near pairs and further pairs) with a three-phase, gradually re-weighted loss.
- Works over tensor data: the embedding and all pair sampling are expressed with `Tensor` operations and gradients are applied by an Adagrad optimiser.
- Configurable projection dimension, pair counts/ratios, learning rate and iteration count; initialisation can be random or PCA-based.
- Helper modules supply Euclidean distance, normalised distance and chainable tensor math (top-k, gather, matmul, broadcasting).

## Key Types
- `Microsoft.VisualBasic.DataMining.PaCMAP.PaCMAP` — the estimator: finds the three pair sets, runs the phased loss and exposes `Fit`.
- `Microsoft.VisualBasic.DataMining.PaCMAP.AdagradOptimizer` — Adagrad parameter update used to drive the embedding.
- `Microsoft.VisualBasic.DataMining.PaCMAP.EuclideanDistance` — Euclidean distance between tensors (pairwise and broadcast forms).
- `Microsoft.VisualBasic.DataMining.PaCMAP.EuclideanDistanceExtensions` — extension methods for distance computation and chainable use.
- `Microsoft.VisualBasic.DataMining.PaCMAP.NormalizedDistanceExtensions` — scales distances using the k-th nearest neighbour statistics.
- `Microsoft.VisualBasic.DataMining.PaCMAP.TensorExtensions` — chainable tensor helpers (neg, square, sqrt, tile, slice, concat, matmul, top-k, gather).
- `Microsoft.VisualBasic.DataMining.PaCMAP.TopKResult` — values/indices pair returned by the top-k selection.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.PaCMAP

' X is a 2D array: rows are observations, columns are features
Using model As New PaCMAP(nDimensions:=2, numIterations:=450)
    Dim embedding As Double(,) = model.Fit(X, init:="random")
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.PaCMAP`
- TargetFramework: `net10.0`
- Tags: `scibasic;pacmap;dimensionality-reduction;manifold-learning;visualization`

## License
GPL-3.0-or-later
