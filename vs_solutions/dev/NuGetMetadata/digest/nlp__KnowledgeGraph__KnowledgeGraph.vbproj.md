# nlp/KnowledgeGraph/KnowledgeGraph.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.NLP.Knowledge
- AssemblyName  : Microsoft.VisualBasic.Data.NLP.Knowledge
- TargetFramework: net10.0
- Source files  : 12
- Existing Title: Bipartite Knowledge Graph with Synonym Disambiguation and Ontology Inference
- Existing Desc : Builds and mines bipartite entity-attribute knowledge graphs for sciBASIC#: scores entity pairs with Jaccard, Adamic-Adar and cosine metrics, runs permutation tests to merge synonyms, and infers ontology relations.
- Existing Tags : scibasic;knowledge-graph;entity-disambiguation;ontology;similarity

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.NLP.Knowledge)

## Public types
- Interface IBipartiteView (Abstractions\IBipartiteView.vb) - A read-only view over a bipartite knowledge network (entities on one side, attributes on the other). All new analysis algorithms depend only on this contract so they can be reused by any graph implementation, not just by
- Class AttributeScore (Analysis\NetworkAnalysis.vb)
- Class NodeScore (Analysis\NetworkAnalysis.vb)
- Class NetworkSummary (Analysis\NetworkAnalysis.vb)
- Class NetworkEdgeRecord (Analysis\NetworkAnalysis.vb)
- Class NetworkAnalysisResult (Analysis\NetworkAnalysis.vb)
- Module NetworkAnalysis (Analysis\NetworkAnalysis.vb) - The heavy math (set similarity, IDF) is reused from graph-netcore5. The routines below are deliberately self-contained so they work for any IBipartiteView implementation and do not depend on the node-id assumptions
- Class OntologyLink (Analysis\OntologyAndRecommend.vb)
- Class RecommendedLink (Analysis\OntologyAndRecommend.vb)
- Module OntologyAndRecommend (Analysis\OntologyAndRecommend.vb) - Ontology (Is-A / sibling / related) inference and two-hop recommendations. Reuses the same weighted engine and inverted index as synonym discovery. The relation vocabulary is the existing OntologyRelationType enum, and the
- Class DisambiguationOptions (EntityDisambiguator.vb) - 实体消歧配置参数。
- Class SynonymGroup (EntityDisambiguator.vb) - 同义实体组：被判定为指代同一知识对象的实体集合。
- Class SynonymPairResult (EntityDisambiguator.vb) - 一对同义实体的详细测试结果。
- Class EntityDisambiguator (EntityDisambiguator.vb) - 实体消歧器。
- Class EntityNode (KnowledgeGraph.vb) - 知识词条节点：表示一个知识实体。
- Class AttributeNode (KnowledgeGraph.vb) - 属性节点：表示知识的一条属性。
- Class KnowledgeGraph (KnowledgeGraph.vb) - 知识图谱：管理实体节点、属性节点及其连接关系。 内部使用邻接表存储二部图结构。
- Class NamespaceDoc (NamespaceDoc.vb) - Bipartite entity-attribute knowledge graph models and mining helpers: graph construction, Jaccard, Adamic-Adar and cosine similarity metrics, permutation test driven synonym disambiguation and ontology relation inference.
- Enum OntologyRelationType (OntologyInferer.vb) - 本体论关系类型。
- Class OntologyRelation (OntologyInferer.vb) - 推断出的本体论关系。
- Class OntologyOptions (OntologyInferer.vb) - 本体论推断配置。
- Class OntologyInferer (OntologyInferer.vb) - 本体论推断器。
- Class SimilarityOptions (Similarity\SimilarityOptions.vb) - Configurable weights, thresholds and optional name/semantic signals for entity equivalence scoring. Name and semantic signals are injected as delegates rather than concrete
- Class SimilarityDetail (Similarity\SimilarityOptions.vb) - Explainable result of a single equivalence comparison: component metrics, per-category scores, shared attributes and the fused score.
- Class AliasDiscoveryOptions (Similarity\SimilarityOptions.vb)
- Class AliasGroup (Similarity\SimilarityOptions.vb)
- Class SimilarityResult (SimilarityMetrics.vb) - 相似度度量结果：包含一对实体之间的所有相似度指标。
- Module SimilarityMetrics (SimilarityMetrics.vb) - 图相似度计算模块。
- Class PermutationTestResult (StatisticalTest.vb) - 置换检验结果。
- Class StatisticalTest (StatisticalTest.vb) - 置换检验器：在二部图上进行度保持的随机重连，计算相似度的统计显著性。

## Notable public members
- Public Property Id As Integer
- Public Property Name As String = ""
- Public Property Category As String = ""
- Public Property Degree As Integer
- Public Property Idf As Double
- Public Property IsHub As Boolean
- Public ReadOnly Property Score As Double
- Public Overrides Function ToString() As String
- Public Property EntityId As Integer
- Public Property Name As String = ""
- Public Property Degree As Integer
- Public Property PageRank As Double
- Public Property Betweenness As Double
- Public Property ComponentId As Integer
- Public Property CommunityId As Integer
- Public Property EntityCount As Integer
- Public Property AttributeCount As Integer
- Public Property EdgeCount As Integer
- Public Property Density As Double
- Public Property AverageEntityDegree As Double
- Public Property AverageAttributeDegree As Double
- Public Property ComponentCount As Integer
- Public Property LargestComponentSize As Integer
- Public Property CommunityCount As Integer
- Public Property HubAttributeCount As Integer
- Public Property TopHubs As New List(Of AttributeScore)()
- Public Function ToText() As String
- Public Overrides Function ToString() As String
- Public Property Source As String = ""
- Public Property Target As String = ""
- Public Property Category As String = ""
- Public Property Weight As Double
- Public Overrides Function ToString() As String
- Public Property Summary As New NetworkSummary()
- Public Property Nodes As New List(Of NodeScore)()
- Public Property Hubs As New List(Of AttributeScore)()
- Public Property Components As New List(Of List(Of Integer))()
- Public Property Communities As New Dictionary(Of Integer, Integer)()
- Public Property Edges As New List(Of NetworkEdgeRecord)()
- Public Function ToCsv() As String
- Public Const DefaultMaxProjectionDegree As Integer = 200
- Public Function ConnectedComponents(view As IBipartiteView) As List(Of List(Of Integer))
- Public Function PageRank(view As IBipartiteView,
- Public Function Betweenness(view As IBipartiteView,
- Public Function LabelPropagationCommunities(view As IBipartiteView,
- Public Function HubAttributes(view As IBipartiteView,
- Public Function ToEdgeList(view As IBipartiteView) As List(Of NetworkEdgeRecord)
- Public Function Analyze(view As IBipartiteView,
- Public Property SubjectId As Integer
- Public Property ObjectId As Integer
- Public Property SubjectName As String = ""
- Public Property ObjectName As String = ""
- Public Property RelationType As OntologyRelationType
- Public Property Inclusion As Double
- Public Property Jaccard As Double
- Public Property Confidence As Double
- Public Property SharedAttributes As String() = New String() {}
- Public ReadOnly Property Description As String
- Public Overrides Function ToString() As String
- Public Property EntityId As Integer = -1
- ... and 193 more

## Imports
- Microsoft.VisualBasic.Data.GraphTheory.Analysis.Similarity
- std = System.Math
- System
- System.Collections.Generic
- System.Text

## File tree
- Abstractions\IBipartiteView.vb
- Analysis\NetworkAnalysis.vb
- Analysis\OntologyAndRecommend.vb
- Discovery\SynonymDiscovery.vb
- EntityDisambiguator.vb
- KnowledgeGraph.vb
- NamespaceDoc.vb
- OntologyInferer.vb
- Similarity\BipartiteSimilarityEngine.vb
- Similarity\SimilarityOptions.vb
- SimilarityMetrics.vb
- StatisticalTest.vb

