Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic

''' <summary>
''' Configurable weights, thresholds and optional name/semantic signals for
''' entity equivalence scoring.
''' </summary>
''' <remarks>
''' Name and semantic signals are injected as delegates rather than concrete
''' NLP/word2vec types. This keeps the KnowledgeGraph assembly free of a
''' dependency on those libraries and lets the host application decide how to
''' compose them (see the composition root in graphQL).
''' </remarks>
Public Class SimilarityOptions

    ''' <summary>
    ''' Relative weight of each metric. Recognized keys: jaccard, cosine,
    ''' adamicadar, inclusion, name, semantic. A missing key disables that metric.
    ''' </summary>
    Public Property MetricWeights As Dictionary(Of String, Double)

    ''' <summary>
    ''' Relative importance of attribute categories (field groups). A missing
    ''' category falls back to <see cref="DefaultCategoryWeight"/> instead of
    ''' being silently dropped.
    ''' </summary>
    Public Property CategoryWeights As Dictionary(Of String, Double)

    ''' <summary>Fallback weight for categories not listed in CategoryWeights.</summary>
    Public Property DefaultCategoryWeight As Double = 1.0

    ''' <summary>Use inverse-document-frequency rarity weights for attributes.</summary>
    Public Property UseIdf As Boolean = True

    ''' <summary>
    ''' Attributes with degree strictly greater than this value are ignored.
    ''' 0 disables the cut-off. Useful for suppressing hub attributes.
    ''' </summary>
    Public Property HubDegreeCutoff As Integer = 0

    ''' <summary>Apply a mild log-scaled bonus from edge confidence counts.</summary>
    Public Property UseEdgeWeights As Boolean = True

    ''' <summary>Half-saturation constant used to compress Adamic-Adar into [0, 1].</summary>
    Public Property AdamicAdarHalfSaturation As Double = 2.0

    ''' <summary>
    ''' Score at or above which two entities are declared equivalent.
    ''' Calibrated on the graphQL CSV fixtures (sweep.csv): equivalent pairs
    ''' score 0.226-0.534 while non-equivalent pairs stay at or below 0.137,
    ''' so 0.20 sits inside the separation gap and yields P=R=F1=1.0.
    ''' </summary>
    Public Property EquivalenceThreshold As Double = 0.20

    ''' <summary>Maximum number of shared attributes to report in a detail object.</summary>
    Public Property MaxReportedAttributes As Integer = 50

    ''' <summary>Optional name clue; Nothing disables the name metric.</summary>
    Public Property NameSimilarity As Func(Of String, String, Double)

    ''' <summary>Optional distributional clue; Nothing disables the semantic metric.</summary>
    Public Property SemanticSimilarity As Func(Of String, String, Double)

    ''' <summary>
    ''' True when a semantic model is actually loaded. When False the semantic
    ''' metric is removed and its weight redistributed to the structure metrics.
    ''' </summary>
    Public Property SemanticAvailable As Boolean = False

    ''' <summary>
    ''' Return the configured weight of a metric, or 0 when it is not configured.
    ''' </summary>
    Public Function MetricWeight(metric As String) As Double
        Dim weight As Double = 0.0

        If MetricWeights IsNot Nothing AndAlso MetricWeights.TryGetValue(metric, weight) Then
            Return weight
        End If
        Return 0.0
    End Function

    ''' <summary>
    ''' Return the weight of an attribute category. Missing keys get the default
    ''' weight (not zero), which is what makes partial weight dictionaries safe.
    ''' </summary>
    Public Function CategoryWeight(category As String) As Double
        If String.IsNullOrEmpty(category) Then
            category = ""
        End If

        Dim weight As Double = 0.0

        If CategoryWeights IsNot Nothing AndAlso CategoryWeights.TryGetValue(category, weight) Then
            Return weight
        End If
        Return DefaultCategoryWeight
    End Function

    ''' <summary>Balanced default configuration (pure structure when no delegates are set).</summary>
    Public Shared Function CreateDefault() As SimilarityOptions
        Dim weights As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase) From {
            {"jaccard", 0.32},
            {"cosine", 0.18},
            {"adamicadar", 0.22},
            {"inclusion", 0.10},
            {"name", 0.08},
            {"semantic", 0.10}
        }

        Return New SimilarityOptions With {
            .MetricWeights = weights,
            .CategoryWeights = Nothing,
            .DefaultCategoryWeight = 1.0,
            .UseIdf = True,
            .HubDegreeCutoff = 0,
            .UseEdgeWeights = True,
            .AdamicAdarHalfSaturation = 2.0,
            .EquivalenceThreshold = 0.20,
            .MaxReportedAttributes = 50
        }
    End Function

End Class

''' <summary>
''' Explainable result of a single equivalence comparison: component metrics,
''' per-category scores, shared attributes and the fused score.
''' </summary>
Public Class SimilarityDetail

    Public Property Query As String = ""
    Public Property Target As String = ""
    Public Property EntityAId As Integer = -1
    Public Property EntityBId As Integer = -1

    ''' <summary>Individual metrics in [0, 1].</summary>
    Public Property Metrics As New Dictionary(Of String, Double)()

    ''' <summary>Normalized contribution weight of each metric actually used.</summary>
    Public Property MetricWeights As New Dictionary(Of String, Double)()

    ''' <summary>Weighted Jaccard within each attribute category.</summary>
    Public Property CategoryScores As New Dictionary(Of String, Double)()

    Public Property SharedAttributes As String() = New String() {}
    Public Property SharedAttributeIds As New List(Of Integer)()

    Public Property CommonNeighbors As Integer
    Public Property DegreeA As Integer
    Public Property DegreeB As Integer
    Public Property Score As Double
    Public Property IsEquivalent As Boolean

    ''' <summary>Empirical p-value when a significance test was run; 1 otherwise.</summary>
    Public Property PValue As Double = 1.0

    ''' <summary>Z-score against the empirical null model; 0 when not tested.</summary>
    Public Property ZScore As Double

    ''' <summary>Human readable one-line explanation.</summary>
    Public ReadOnly Property Description As String
        Get
            Return $"{Query} <=> {Target}: score={Score.ToString("F4")}, " &
                   $"shared={CommonNeighbors} (deg {DegreeA}/{DegreeB}), " &
                   $"equivalent={IsEquivalent}"
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Description
    End Function

End Class

''' <summary>Options for large-scale alias (synonym) group discovery.</summary>
Public Class AliasDiscoveryOptions

    ''' <summary>Minimum fused score for a pair to become an alias candidate.</summary>
    Public Property MinimumScore As Double = 0.20

    ''' <summary>Minimum number of shared attributes before a pair is scored.</summary>
    Public Property MinimumSharedAttributes As Integer = 1

    ''' <summary>Hard cap on the number of generated candidate pairs.</summary>
    Public Property MaxCandidates As Integer = 200000

    ''' <summary>
    ''' Permutation iterations for the empirical null model. 0 disables the
    ''' significance filter entirely (fastest mode).
    ''' </summary>
    Public Property PermutationIterations As Integer = 0

    ''' <summary>Global significance level.</summary>
    Public Property Alpha As Double = 0.05

    ''' <summary>Apply Bonferroni correction over the number of tested pairs.</summary>
    Public Property ApplyBonferroniCorrection As Boolean = True

    ''' <summary>
    ''' Optional per-entity prior (mentions, source count, ...), injected by the
    ''' host application and used when choosing the canonical name of a group.
    ''' </summary>
    Public Property EntityPrior As Func(Of Integer, Double)

    ''' <summary>Optional per-language prior for the canonical name.</summary>
    Public Property LanguagePrior As Dictionary(Of String, Double)

    ''' <summary>Skip attributes with degree above this value when generating candidates.</summary>
    Public Property HubDegreeCutoff As Integer = 0

End Class

''' <summary>A transitive-closure group of mutually equivalent entities.</summary>
Public Class AliasGroup

    Public Property Members As New List(Of Integer)()
    Public Property Names As String() = New String() {}
    Public Property CanonicalName As String = ""
    Public Property CanonicalLanguage As String = ""
    Public Property Confidence As Double
    Public Property Pairwise As New List(Of SimilarityDetail)()

    Public ReadOnly Property Size As Integer
        Get
            Return Members.Count
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{Size}] {CanonicalName} (confidence {Confidence.ToString("F3")})"
    End Function

End Class
