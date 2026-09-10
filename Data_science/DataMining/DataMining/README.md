# Core Data Mining Toolkit: Clustering, Association Rules and Evaluation

Core data mining library for sciBASIC#, covering clustering, association rule mining, classifiers and model evaluation.

## Overview
- Clustering: K-means (plus bisecting K-means, k-medoids/PAM, canopy seeding and Lloyd/Voronoi), DBSCAN, HDBSCAN*, fuzzy C-means, spectral clustering and KNN clustering, all sharing the `EntityBase(Of Double)` / `ClusterEntity` vector model.
- Association rules: a full Apriori implementation with frequent, closed and maximal item sets, confidence-based strong-rule generation and transaction encoding helpers.
- Classifiers and models: ID3 decision trees, naive Bayes, Bayesian belief networks, self-organizing maps, a fuzzy-logic (DFL) network node model and a single-neuron classifier.
- Evaluation and preprocessing: ROC/AUC and regression-ROC curves, validation statistics (sensitivity/specificity), discretisers, class and categorical encoders, and per-feature z-score standardisation.

## Key Types
- `Microsoft.VisualBasic.DataMining.KMeans.KMeansAlgorithm(Of T)` — parallel K-means over `EntityBase(Of Double)` entities, returning a `ClusterCollection(Of T)`.
- `Microsoft.VisualBasic.DataMining.HDBSCAN.Runner.HdbscanRunner` — runs HDBSCAN* through `HdbscanParameters` and returns an `HdbscanResult` with outlier scores.
- `Microsoft.VisualBasic.DataMining.DBSCAN.DbscanAlgorithm(Of T)` — generic density-based clustering with noise labelling.
- `Microsoft.VisualBasic.DataMining.AprioriRules.Apriori` (module) — `AnalysisTransactions` producing an `Output` of frequent/closed/maximal item sets and `Rule` objects.
- `Microsoft.VisualBasic.DataMining.DecisionTree.Tree` — ID3 decision tree builder over the `DataTable` training-set model.
- `Microsoft.VisualBasic.DataMining.Evaluation.ROC` (module) — ROC curve and AUC computation.
- `Microsoft.VisualBasic.DataMining.SelfOrganizingMap` — self-organizing map training and mapping.
- `Microsoft.VisualBasic.DataMining.FuzzyCMeans.CMeans` (module) — fuzzy C-means clustering with membership-tagged `FuzzyCMeansEntity` results.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
Imports Microsoft.VisualBasic.DataMining.KMeans

' entities: IEnumerable(Of ClusterEntity), each with a numeric entityVector
Dim kmeans As New KMeansAlgorithm(Of ClusterEntity)()
Dim clusters As ClusterCollection(Of ClusterEntity) = kmeans.ClusterDataSet(entities, k:=3)

For Each cluster In clusters
    Console.WriteLine(cluster.ToString())
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.Framework`
- TargetFramework: `net10.0`
- Tags: `scibasic;data-mining;clustering;association-rules;machine-learning`

## License
GPL-3.0-or-later
