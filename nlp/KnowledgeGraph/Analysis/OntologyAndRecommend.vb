Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Similarity
Imports std = System.Math

''' <summary>An inferred taxonomic / associative relation between two entities.</summary>
Public Class OntologyLink

    Public Property SubjectId As Integer
    Public Property ObjectId As Integer
    Public Property SubjectName As String = ""
    Public Property ObjectName As String = ""
    Public Property RelationType As OntologyRelationType
    Public Property Inclusion As Double
    Public Property Jaccard As Double
    Public Property Confidence As Double
    Public Property SharedAttributes As String() = New String() {}

    Public ReadOnly Property Description As String
        Get
            Return $"{SubjectName} {RelationType} {ObjectName} " &
                   $"(inclusion={Inclusion.ToString("F3")}, jaccard={Jaccard.ToString("F3")})"
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Description
    End Function

End Class

''' <summary>A recommended related entity or a missing attribute.</summary>
Public Class RecommendedLink

    Public Property EntityId As Integer = -1
    Public Property AttributeId As Integer = -1
    Public Property Name As String = ""
    Public Property Category As String = ""
    Public Property Score As Double
    Public Property SharedAttributeCount As Integer
    Public Property SharedAttributes As String() = New String() {}
    Public Property Reason As String = ""

    Public ReadOnly Property IsEntity As Boolean
        Get
            Return EntityId >= 0
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{Name} score={Score.ToString("F4")} [{Reason}]"
    End Function

End Class

''' <summary>
''' Ontology (Is-A / sibling / related) inference and two-hop recommendations.
''' </summary>
''' <remarks>
''' Reuses the same weighted engine and inverted index as synonym discovery.
''' The relation vocabulary is the existing OntologyRelationType enum, and the
''' produced synonym groups can be converted with
''' SynonymDiscovery.ToSynonymGroups for compatibility with OntologyInferer.
''' </remarks>
Public Module OntologyAndRecommend

    ''' <summary>
    ''' Infer relations from containment of attribute sets.
    ''' When B's attributes are almost entirely contained in A's larger set,
    ''' B is-a A. Symmetric high-overlap pairs are siblings.
    ''' </summary>
    Public Function InferOntology(view As IBipartiteView,
                                  engine As BipartiteSimilarityEngine,
                                  Optional minInclusion As Double = 0.65,
                                  Optional siblingJaccard As Double = 0.30,
                                  Optional relatedJaccard As Double = 0.08,
                                  Optional maxRelations As Integer = 20000) As List(Of OntologyLink)
        Dim result As New List(Of OntologyLink)()

        If view Is Nothing OrElse engine Is Nothing Then Return result

        Dim discovery As New SynonymDiscovery(engine)
        Dim pairs As List(Of KeyValuePair(Of Integer, Integer)) = discovery.GenerateCandidates()

        For Each pair As KeyValuePair(Of Integer, Integer) In pairs
            Dim a As Integer = pair.Key
            Dim b As Integer = pair.Value
            Dim detail As SimilarityDetail = engine.Compute(a, b)

            If detail.CommonNeighbors = 0 Then Continue For

            Dim jaccard As Double = 0.0
            Dim inclusionMetric As Double = 0.0
            If detail.Metrics.ContainsKey("jaccard") Then jaccard = detail.Metrics("jaccard")
            If detail.Metrics.ContainsKey("inclusion") Then inclusionMetric = detail.Metrics("inclusion")

            ' Exact set containment ratio (independent of IDF weighting).
            Dim smallest As Integer = std.Min(detail.DegreeA, detail.DegreeB)
            Dim containment As Double = SetSimilarity.SafeDivide(CDbl(detail.CommonNeighbors), CDbl(smallest))
            Dim relation As OntologyRelationType
            Dim subject As Integer
            Dim [object] As Integer

            If containment >= minInclusion AndAlso detail.DegreeA <> detail.DegreeB Then
                ' The entity with fewer attributes is the more specific one.
                If detail.DegreeA < detail.DegreeB Then
                    subject = a
                    [object] = b
                Else
                    subject = b
                    [object] = a
                End If
                relation = OntologyRelationType.IsA
            ElseIf jaccard >= siblingJaccard Then
                subject = a
                [object] = b
                relation = OntologyRelationType.SiblingOf
            ElseIf jaccard >= relatedJaccard Then
                subject = a
                [object] = b
                relation = OntologyRelationType.RelatedTo
            Else
                Continue For
            End If

            Dim confidence As Double = SetSimilarity.Clamp01(0.5 * containment + 0.5 * jaccard)

            result.Add(New OntologyLink With {
                .SubjectId = subject,
                .ObjectId = [object],
                .SubjectName = If(view.GetEntityName(subject), "?"),
                .ObjectName = If(view.GetEntityName([object]), "?"),
                .RelationType = relation,
                .Inclusion = containment,
                .Jaccard = jaccard,
                .Confidence = confidence,
                .SharedAttributes = detail.SharedAttributes
            })

            If result.Count >= std.Max(1, maxRelations) Then Exit For
        Next

        result.Sort(Function(x As OntologyLink, y As OntologyLink)
                        Return y.Confidence.CompareTo(x.Confidence)
                    End Function)

        Return result
    End Function

    ''' <summary>
    ''' Recommend entities that share rare attributes with the query entity but
    ''' are not scored as fully equivalent. Ranked by IDF-weighted shared evidence.
    ''' </summary>
    Public Function RecommendRelatedEntities(view As IBipartiteView,
                                             engine As BipartiteSimilarityEngine,
                                             entityId As Integer,
                                             Optional topN As Integer = 20,
                                             Optional scoreCeiling As Double = 0.95) As List(Of RecommendedLink)
        Dim result As New List(Of RecommendedLink)()
        Dim candidates As List(Of SimilarityDetail) = engine.FindCandidates(entityId)

        For Each detail As SimilarityDetail In candidates
            If detail.Score > scoreCeiling Then Continue For

            Dim evidence As Double = 0.0
            Dim names As New List(Of String)()

            For Each attrId As Integer In detail.SharedAttributeIds
                evidence += engine.Idf.Idf(attrId)
                names.Add(If(view.GetAttributeName(attrId), "?"))
            Next

            result.Add(New RecommendedLink With {
                .EntityId = detail.EntityBId,
                .Name = detail.Target,
                .Score = SetSimilarity.Clamp01(0.6 * detail.Score + 0.4 * SetSimilarity.Saturate(evidence, 4.0)),
                .SharedAttributeCount = detail.CommonNeighbors,
                .SharedAttributes = names.ToArray(),
                .Reason = "shared rare attributes"
            })
        Next

        result.Sort(Function(x As RecommendedLink, y As RecommendedLink)
                        Return y.Score.CompareTo(x.Score)
                    End Function)

        If topN > 0 AndAlso result.Count > topN Then
            Return result.GetRange(0, topN)
        End If
        Return result
    End Function

    ''' <summary>
    ''' Two-hop attribute recommendation: entity -> shared attribute -> similar
    ''' entity -> attributes the query does not yet have. Rare attributes that
    ''' are present on several similar entities rank highest.
    ''' </summary>
    Public Function RecommendAttributes(view As IBipartiteView,
                                        engine As BipartiteSimilarityEngine,
                                        entityId As Integer,
                                        Optional topN As Integer = 20,
                                        Optional minSimilarity As Double = 0.20) As List(Of RecommendedLink)
        Dim result As New List(Of RecommendedLink)()
        Dim own As HashSet(Of Integer) = view.GetEntityAttributes(entityId)

        If own Is Nothing Then own = New HashSet(Of Integer)()

        Dim votes As New Dictionary(Of Integer, Double)()
        Dim voters As New Dictionary(Of Integer, Integer)()

        For Each detail As SimilarityDetail In engine.FindCandidates(entityId)
            If detail.Score < minSimilarity Then Continue For

            Dim neighbors As HashSet(Of Integer) = view.GetEntityAttributes(detail.EntityBId)
            If neighbors Is Nothing Then Continue For

            For Each attrId As Integer In neighbors
                If own.Contains(attrId) Then Continue For
                If engine.Idf.IsExcluded(attrId) Then Continue For

                Dim contribution As Double = detail.Score * engine.Idf.Idf(attrId)

                If votes.ContainsKey(attrId) Then
                    votes(attrId) = votes(attrId) + contribution
                    voters(attrId) = voters(attrId) + 1
                Else
                    votes(attrId) = contribution
                    voters(attrId) = 1
                End If
            Next
        Next

        For Each item As KeyValuePair(Of Integer, Double) In votes
            Dim support As Integer = voters(item.Key)
            Dim score As Double = SetSimilarity.Saturate(item.Value, 2.0) *
                                  (1.0 + 0.15 * std.Log(1.0 + CDbl(support)))

            result.Add(New RecommendedLink With {
                .AttributeId = item.Key,
                .Name = If(view.GetAttributeName(item.Key), "?"),
                .Category = If(view.GetAttributeCategory(item.Key), ""),
                .Score = SetSimilarity.Clamp01(score),
                .SharedAttributeCount = support,
                .Reason = $"present on {support} similar entities"
            })
        Next

        result.Sort(Function(x As RecommendedLink, y As RecommendedLink)
                        Return y.Score.CompareTo(x.Score)
                    End Function)

        If topN > 0 AndAlso result.Count > topN Then
            Return result.GetRange(0, topN)
        End If
        Return result
    End Function

End Module
