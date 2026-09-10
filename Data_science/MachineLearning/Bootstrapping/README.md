# Graph and Knowledge Graph Embedding Toolkit (node2vec, graph2vec, ComplEx)

Graph representation learning for sciBASIC#: node-level and whole-graph embeddings plus ComplEx family knowledge-graph models.

## Overview
- node2vec: builds a graph from edge lists, simulates biased random walks with alias sampling, and trains a skip-gram word2vec model to give every node a vector.
- graph2vec: ranks embedded nodes by vector energy, maps them onto a character alphabet and fits a Sequence Graph Transform (SGT) vector for the entire graph.
- ComplEx knowledge-graph embedding in four flavours — `ComplEx`, `complex_NNE`, `complex_NNE_AER` (approximate entailment regularised) and `complex_R` (rule regularised) — all trained by AdaGrad with evaluation over train/valid/test triple sets.
- Supporting structures for triples, rules, matrices and negative sampling.

## Key Types
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.node2vec.Solver` — `CreateGraph` from source/target/weight vectors; `CreateEmbedding` runs walks plus word2vec.
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.node2vec.Graph` — node2vec graph with `preprocess` and `simulateWalks` alias-sampled walks.
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.Graph2Vec` — `Setup(terms)` then `GraphVector(g)` for a whole-graph vector.
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.Algorithm` — entry points `ComplEx`, `complex_NNE`, `complex_NNE_AER`, `complex_R`.
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.Arguments` — hyper-parameters (`k`, `lmbda`, `gamma`, `neg`, `iterations`, `skip`) and triple file paths.
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.complex.ComplEx` — base ComplEx model (real/imaginary entity and relation matrices, AdaGrad updates).
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.struct.TripleSet` / `Triple` / `Matrix` — triple storage and embedding matrices.
- `Microsoft.VisualBasic.MachineLearning.Bootstrapping.GraphEmbedding.util.NegativeTripleGenerator` — corrupted head/tail negative sampling.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.node2vec

' u()/v() are the source/target node labels, w() the optional edge weights
Dim g As Graph = Solver.CreateGraph(u, v, w)
Dim model = g.CreateEmbedding(numWalks:=10, walkLength:=80, dimensions:=64)
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.Bootstrapping`
- TargetFramework: `net10.0`
- Tags: `scibasic;graph-embedding;node2vec;knowledge-graph;complex`

## License
GPL-3.0-or-later
