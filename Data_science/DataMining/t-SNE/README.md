# Barnes-Hut t-SNE Dimensionality Reduction Embedding

t-Distributed Stochastic Neighbor Embedding that converts pairwise affinities into joint probabilities and minimises their KL divergence against the low-dimensional embedding.

## Overview
- Builds the joint probability matrix either densely (from raw data or a supplied distance matrix) or sparsely with a CSR-style matrix for large data.
- Optional Barnes-Hut approximation: an `SPTree` space-partition tree plus a dedicated gradient routine reduce the repulsive term from O(N^2) to roughly O(N log N).
- Perplexity-driven affinity search with binary search on the Gaussian bandwidth; neighbour count per row is tunable or derived as 3 x perplexity.
- Multi-threaded: the embedding is stored as a flat array and the cost/gradient evaluation is parallelised over configurable thread blocks.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.tSNE.tSNE` — the estimator: initialises data, steps the gradient and returns the embedding.
- `Microsoft.VisualBasic.MachineLearning.tSNE.SparseP` — CSR compressed sparse joint probability matrix with row pointers, column indices and values.
- `Microsoft.VisualBasic.MachineLearning.tSNE.SparseProbability` — assembly helpers that turn distances into the sparse P matrix and suggest k from perplexity.
- `Microsoft.VisualBasic.MachineLearning.tSNE.SPTree` / `SPNode` — Barnes-Hut space-partition tree and its nodes, with non-edge force computation.
- `Microsoft.VisualBasic.MachineLearning.tSNE.BarnesHutGradient` — attractive/repulsive gradient evaluation over the sparse P matrix and the SP tree.
- `Microsoft.VisualBasic.MachineLearning.tSNE.CostFunction` — cost and gradient of the KL divergence for a given embedding.
- `Microsoft.VisualBasic.MachineLearning.tSNE.RandomHelper` — reproducible random initialisation of the embedding.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.tSNE

Dim model As New tSNE(perplexity:=30, [dim]:=2, epsilon:=200, useBarnesHut:=True)

Call model.InitDataRaw(X)      ' X is an IEnumerable(Of Double())

For i As Integer = 1 To 1000
    Call model.Step()
Next

Dim Y As Double()() = model.GetEmbedding()
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.t-SNE`
- TargetFramework: `net10.0`
- Tags: `scibasic;t-sne;dimensionality-reduction;barnes-hut;visualization`

## License
GPL-3.0-or-later
