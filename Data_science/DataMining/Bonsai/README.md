# Bonsai Diffusion Tree Layout for High-Dimensional Data

Reconstructs a diffusion tree from high-dimensional observations and lays it out for two-dimensional visualisation.

## Overview
- Builds a tree whose non-root edges are diffusion times and whose internal nodes are effective per-dimension coordinates, then optimises every branch length and internal-node coordinate against a continuous Felsenstein tree likelihood.
- Optimisation uses lightweight numerical primitives: a Brent 1-D root finder and a bound-constrained L-BFGS with analytic gradient; all likelihood math factorises over dimensions so no full matrices are needed.
- Topology is refined after construction with nearest-neighbour interchange (NNI), subtree pruning and regrafting (SPR), zero-time child merging and re-rooting to the minimum internal distance.
- Renders the result as a distortion-free 2D layout (`dendrogram` or `radial`), a Newick string, per-sample branch-time coordinates and the final log-likelihood.

## Key Types
- `Microsoft.VisualBasic.DataMining.Bonsai.Bonsai` — public fit/transform entry point mirroring the sibling UMAP and t-SNE reducers: `Fit`, `Transform`, `Get2DLayout`, `GetHighDimStates`, `BranchTimeCoords`, `ToNewick`, `LogLikelihood` and the static `Embed`.
- `Microsoft.VisualBasic.DataMining.Bonsai.BonsaiTree` — reconstruction engine (`Build`, `optTimes`, `mergeChildrenUB`, `PerformNNI`, `PerformSPR`, `RerootToMinInternalDist`, `calcLogLComplete`).
- `Microsoft.VisualBasic.DataMining.Bonsai.BonsaiNode` — tree node holding per-dimension effective coordinates (`ltqs`), variances (`ltqsVars`), branch time (`tParent`) and weight `W_g`.
- `Microsoft.VisualBasic.DataMining.Bonsai.PointSet` — N x D observations with per-dimension uncertainty, plus SNR filtering.
- `Microsoft.VisualBasic.DataMining.Bonsai.Likelihood` — pure numeric core: star-tree log-likelihood, its gradient and optimal branch-time solvers.
- `Microsoft.VisualBasic.DataMining.Bonsai.Optimizer` — Brent zero finder and bound-constrained L-BFGS minimiser returning `OptResult`.
- `Microsoft.VisualBasic.DataMining.Bonsai.TreeLayout` — `DendrogramLayout` and `RadialLayout` for the 2D embedding.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.Bonsai

' means: N x D matrix of observations, stds: optional N x D uncertainty
Dim model As New Bonsai() With {.maxMerges = -1, .layout = "dendrogram"}
Call model.Fit(means, stds, names)

Dim xy As Double()() = model.Transform()
Dim newick As String = model.ToNewick()

' or one shot
Dim embedding As Double()() = Bonsai.Embed(means)
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.Bonsai`
- TargetFramework: `net10.0`
- Tags: `scibasic;bonsai;dimensionality-reduction;diffusion-tree;visualization`

## License
GPL-3.0-or-later
