# Graph Neural Networks: GCN, GAT and Recurrent Temporal Graph Layers

A graph neural network library for sciBASIC# built on the `Tensor` type, covering graph convolutions, attention, pooling and recurrent temporal models.

## Overview
- Graph data model: directed/undirected `Graph` with node and edge features, adjacency/degree/laplacian and normalised adjacency matrices, plus a `GraphDataset` for graph classification.
- Spatial convolution layers: `GCNConvLayer` (normalised `A * H * W`), `GATLayer` (multi-head attention), `LinearLayer`, `ActivationLayer`, `DropoutLayer` and `GlobalPoolingLayer` (mean/max/sum readouts).
- Models: `GCNModel` for node classification and `GraphClassificationModel` (GCN -> pooling -> linear -> softmax) for graph-level labels.
- Temporal graphs: `TemporalGraph` snapshots with `TGCNModel` (GCN + GRU), `A3TGCNModel` (adds temporal attention) and `TemporalGNNTrainer`.
- Training: `Trainer` loop with masked train/validation splits, `SGDOptimizer` and `AdamOptimizer`, and shared activation/loss modules.

## Key Types
- `Microsoft.VisualBasic.DeepLearning.GNN.Graph` / `Edge` — graph structure with `AddEdge`, `AddUndirectedEdge`, `GetNeighbors`, `GetNormalizedAdjacencyMatrix`.
- `Microsoft.VisualBasic.DeepLearning.GNN.GCNConvLayer` — graph convolution `H' = sigma(A_norm * H * W)`.
- `Microsoft.VisualBasic.DeepLearning.GNN.GATLayer` — graph attention aggregation (`numHeads`, `leakyReluSlope`).
- `Microsoft.VisualBasic.DeepLearning.GNN.GCNModel` — `GCN -> ReLU -> GCN -> Softmax` node classifier (`Forward`, `ForwardLogits`, `Backward`).
- `Microsoft.VisualBasic.DeepLearning.GNN.GraphClassificationModel` — graph-level classifier used by `GraphClassificationTrainer`.
- `Microsoft.VisualBasic.DeepLearning.GNN.TemporalGraph` / `TGCNModel` / `A3TGCNModel` — temporal snapshot graph and recurrent temporal models.
- `Microsoft.VisualBasic.DeepLearning.GNN.Trainer` — `TrainEpoch`, `Evaluate`, `Train` with loss/accuracy history.
- `Microsoft.VisualBasic.DeepLearning.GNN.SGDOptimizer` / `AdamOptimizer` — optimizers over parameter and gradient tensor lists.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DeepLearning.GNN

Dim g As New Graph(numNodes:=100, featureDim:=16)
Call g.AddUndirectedEdge(0, 1)

Dim model As New GCNModel(inputDim:=16, hiddenDim:=32, outputDim:=5)
Dim opt As New AdamOptimizer(model.GetParameters(), model.GetGradients(), learningRate:=0.01F)
Dim trainer As New Trainer(model, opt)

Call trainer.Train(g, labels, trainMask, valMask, epochs:=200)
```

## Package
- Assembly: `Microsoft.VisualBasic.DeepLearning.GNN`
- TargetFramework: `net10.0`
- Tags: `scibasic;graph-neural-network;gcn;graph-attention;temporal-graph`

## License
GPL-3.0-or-later
