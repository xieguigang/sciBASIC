# nlp/KnowledgeGraph/KnowledgeGraph.vbproj

- RootNamespace : Microsoft.VisualBasic.Data.NLP.Knowledge
- AssemblyName  : Microsoft.VisualBasic.Data.NLP.Knowledge
- TargetFramework: net10.0
- Source files  : 5
- Existing Title: Bipartite Knowledge Graph with Synonym Disambiguation and Ontology Inference
- Existing Desc : Builds and mines bipartite entity-attribute knowledge graphs for sciBASIC#: scores entity pairs with Jaccard, Adamic-Adar and cosine metrics, runs permutation tests to merge synonyms, and infers ontology relations.
- Existing Tags : scibasic;knowledge-graph;entity-disambiguation;ontology;similarity

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Data.NLP.Knowledge)

## Public types
- Class DisambiguationOptions (EntityDisambiguator.vb) - 实体消歧配置参数。
- Class SynonymGroup (EntityDisambiguator.vb) - 同义实体组：被判定为指代同一知识对象的实体集合。
- Class SynonymPairResult (EntityDisambiguator.vb) - 一对同义实体的详细测试结果。
- Class EntityDisambiguator (EntityDisambiguator.vb) - 实体消歧器。
- Class EntityNode (KnowledgeGraph.vb) - 知识词条节点：表示一个知识实体。
- Class AttributeNode (KnowledgeGraph.vb) - 属性节点：表示知识的一条属性。
- Class KnowledgeGraph (KnowledgeGraph.vb) - 知识图谱：管理实体节点、属性节点及其连接关系。 内部使用邻接表存储二部图结构。
- Enum OntologyRelationType (OntologyInferer.vb) - 本体论关系类型。
- Class OntologyRelation (OntologyInferer.vb) - 推断出的本体论关系。
- Class OntologyOptions (OntologyInferer.vb) - 本体论推断配置。
- Class OntologyInferer (OntologyInferer.vb) - 本体论推断器。
- Class SimilarityResult (SimilarityMetrics.vb) - 相似度度量结果：包含一对实体之间的所有相似度指标。
- Module SimilarityMetrics (SimilarityMetrics.vb) - 图相似度计算模块。
- Class PermutationTestResult (StatisticalTest.vb) - 置换检验结果。
- Class StatisticalTest (StatisticalTest.vb) - 置换检验器：在二部图上进行度保持的随机重连，计算相似度的统计显著性。

## Notable public members
- Public Property SynonymJaccardThreshold As Double = 0.45
- Public Property SynonymPValueThreshold As Double = 0.01
- Public Property PermutationIterations As Integer = 5000
- Public Property ApplyBonferroniCorrection As Boolean = True
- Public Property Alpha As Double = 0.05
- Public Property EntityIds As New List(Of Integer)
- Public Property PairwiseResults As New List(Of SynonymPairResult)
- Public Property Confidence As Double
- Public Property CanonicalName As String
- Public Property CanonicalLanguage As String
- Public Property EntityAId As Integer
- Public Property EntityBId As Integer
- Public Property NameA As String
- Public Property NameB As String
- Public Property Jaccard As Double
- Public Property AdamicAdar As Double
- Public Property JaccardPValue As Double
- Public Property AAPValue As Double
- Public Property JaccardZScore As Double
- Public Property AAZScore As Double
- Public Property BonferroniCorrectedPValue As Double
- Public Property IsSignificant As Boolean
- Public Sub New(graph As KnowledgeGraph, Optional options As DisambiguationOptions = Nothing)
- Public Function Disambiguate() As List(Of SynonymGroup)
- Public Property Id As Integer
- Public Property Name As String
- Public Property Language As String
- Public Property EntityType As String
- Public Overrides Function ToString() As String
- Public Property Id As Integer
- Public Property Name As String
- Public Property Category As String
- Public Overrides Function ToString() As String
- Public ReadOnly Property Entities As IReadOnlyList(Of EntityNode)
- Public ReadOnly Property Attributes As IReadOnlyList(Of AttributeNode)
- Public ReadOnly Property TotalEdges As Integer
- Public Function AddEntity(name As String, language As String, entityType As String) As Integer
- Public Function AddAttribute(name As String, category As String) As Integer
- Public Sub AddEdge(entityId As Integer, attributeId As Integer)
- Public Sub AddEntityAttribute(entityId As Integer, attrName As String, attrCategory As String)
- Public Function GetEntityAttributes(entityId As Integer) As HashSet(Of Integer)
- Public Function GetAttributeEntities(attributeId As Integer) As HashSet(Of Integer)
- Public Function GetEntityDegree(entityId As Integer) As Integer
- Public Function GetAttributeDegree(attributeId As Integer) As Integer
- Public Function FindEntity(name As String, Optional language As String = "") As EntityNode
- Public Function GetEntityName(entityId As Integer) As String
- Public Function GetAttributeName(attrId As Integer) As String
- Public Function GetEntity(entityId As Integer) As EntityNode
- Public Function GetAttribute(attrId As Integer) As AttributeNode
- Public Function GetSharedAttributes(entityA As Integer, entityB As Integer) As HashSet(Of Integer)
- Public Function GetAttributeNames(attrIds As IEnumerable(Of Integer)) As List(Of String)
- Public Function GetAttributeCategoryStats() As Dictionary(Of String, Integer)
- Public Function GetDegreeSequence() As Integer()
- Public Property SubjectId As Integer
- Public Property ObjectId As Integer
- Public Property RelationType As OntologyRelationType
- Public Property Jaccard As Double
- Public Property AdamicAdar As Double
- Public Property InclusionRatio As Double
- Public Property Confidence As Double
- ... and 41 more

## Imports
- std = System.Math
- System.Collections.Generic

## File tree
- EntityDisambiguator.vb
- KnowledgeGraph.vb
- OntologyInferer.vb
- SimilarityMetrics.vb
- StatisticalTest.vb

