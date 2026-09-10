# Dynamic Bayesian Network Structure and Parameter Learning

Learns dynamic Bayesian networks from observation sequences by combining an initial Bayes net with per-transition nets under configurable scoring functions.

## Overview
- Structure learning over candidate parent sets: scores every (past, present) parent configuration and selects an optimal branching/forest with `OptimumBranching` and disjoint-set cycle handling.
- Scoring is pluggable through the `ScoringFunction` interface, with MDL, log-likelihood and random baselines supplied; scores can be computed multi-threaded.
- Parameter learning by maximum-likelihood counting or EM (`learnParameters`, `parameterEM`), plus parameter generation for simulation (`generateParameters`, `generateObservations`).
- Analysis utilities: k-fold cross-validation, forecasting of future time slices, model comparison returning precision/recall/F1, and Graphviz DOT export.

## Key Types
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.Observations` — coded time-series observation cube loaded from CSV with a configurable Markov lag; exposes `Attributes` and imputes missing values.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.BayesNet` — a static (prior or transition) network over `Attribute` nodes with intra- and inter-slice `Edge` relations.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.DynamicBayesNet` — initial net plus transition nets; `learnParameters`, `parameterEM`, `getScore`, `forecast`, `toDot`.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.ScoringFunction` (interface) with `LLScoringFunction`, `MDLScoringFunction` and `RandomScoringFunction`.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.Scores` — caches per-node, per-parent-set scores required by the search.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.OptimumBranching` — optimum branching / spanning arborescence search over the score matrix.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn.CrossValidation` — k-fold evaluation of a scoring function and parent-count setting, optionally writing predictions to file.
- `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.utils.Edge` / `Forest` / `DisjointSets` / `BidirectionalArray` — graph and index helper structures.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.dbn
Imports Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork.utils

' CSV observation table, first-order Markov lag
Dim obs As New Observations("observations.csv", markovLag:=1)
Dim attrs As IList(Of Attribute) = obs.Attributes

Dim intra As IList(Of Edge) = {New Edge(0, 1)}.ToList
Dim inter As IList(Of Edge) = {New Edge(0, 2)}.ToList
Dim dbn As New DynamicBayesNet(
    attrs,
    New BayesNet(attrs, intra),
    {New BayesNet(attrs, markovLag:=1, intraRelations:=intra, interRelations:=inter)}.ToList)

Call dbn.learnParameters(obs)
Dim future As Observations = dbn.forecast(obs, numTransitions:=3, stationaryProcess:=True, mostProbable:=True)
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.DynamicBayesianNetwork`
- TargetFramework: `net10.0`
- Tags: `scibasic;bayesian-network;dynamic-bayesian-network;structure-learning;data-mining`

## License
GPL-3.0-or-later
