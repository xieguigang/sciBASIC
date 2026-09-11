# Bipartite Knowledge Graph with Synonym Disambiguation and Ontology Inference

Builds and mines bipartite entity-attribute knowledge graphs, resolving synonyms by statistical test and inferring ontology relations for sciBASIC#.

## Overview
- Bipartite graph store: entity nodes connect only to attribute nodes, with adjacency lists, degree queries and category statistics.
- Similarity metrics over the bipartite structure: Jaccard, Adamic-Adar, cosine, common-neighbour count and inclusion ratio.
- Degree-preserving permutation testing (with z-scores, p-values and Bonferroni correction) to decide whether two entities are really synonyms.
- Ontology inference that classifies entity pairs as `IsA`, `SiblingOf`, `HasFunction` or `RelatedTo` with a confidence score.

## Key Types
- `Microsoft.VisualBasic.Data.NLP.Knowledge.KnowledgeGraph` — bipartite graph container; add entities/attributes/edges and query degrees, neighbours and shared attributes.
- `Microsoft.VisualBasic.Data.NLP.Knowledge.SimilarityMetrics` — computes Jaccard, Adamic-Adar, cosine and inclusion ratios for an entity pair or for all pairs.
- `Microsoft.VisualBasic.Data.NLP.Knowledge.StatisticalTest` — degree-preserving random rewiring permutation test producing p-values and z-scores.
- `Microsoft.VisualBasic.Data.NLP.Knowledge.EntityDisambiguator` — merges entities that pass the Jaccard / p-value thresholds into `SynonymGroup` results.
- `Microsoft.VisualBasic.Data.NLP.Knowledge.OntologyInferer` — infers typed ontology relations from the graph plus the resolved synonym groups.
- `Microsoft.VisualBasic.Data.NLP.Knowledge.EntityNode` / `AttributeNode` — the two node kinds (knowledge term vs. knowledge attribute).
- `Microsoft.VisualBasic.Data.NLP.Knowledge.OntologyRelation` — one inferred relation with type, scores, shared attributes and confidence.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.NLP.Knowledge

Dim graph As New KnowledgeGraph()
Dim water As Integer = graph.AddEntity("water", "en", "compound")
Dim h2o As Integer = graph.AddEntity("H2O", "en", "compound")

Call graph.AddEntityAttribute(water, "formula", "chemical")
Call graph.AddEntityAttribute(h2o, "formula", "chemical")
Call graph.AddEntityAttribute(water, "boiling-point", "physical")

Dim synonyms = New EntityDisambiguator(graph).Disambiguate()
Dim relations = New OntologyInferer(graph).InferRelations(synonyms)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.NLP.Knowledge`
- TargetFramework: `net10.0`
- Tags: `scibasic;knowledge-graph;entity-disambiguation;ontology;similarity`

## License
GPL-3.0-or-later
