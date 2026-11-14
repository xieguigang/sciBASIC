Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Similarity
Imports std = System.Math

''' <summary>
''' Weighted, explainable entity-equivalence scoring over a bipartite
''' knowledge network.
''' </summary>
''' <remarks>
''' Fixes the weaknesses of naive shared-attribute counting:
''' * attributes are weighted by rarity (IDF), field-group importance and a mild
'''   edge-confidence bonus, so a shared CAS number counts more than a shared
'''   generic name;
''' * weighted Jaccard uses sum(min)/sum(max) - no double counting of the
'''   intersection and no mutation of the compared sets;
''' * every attribute is kept (no Top-N truncation of long tails);
''' * metric weights are normalized, and missing category weights fall back to
'''   the configured default instead of silently dropping the group;
''' * name/semantic clues are optional delegates and are attenuated when the
'''   pair shares no attribute, so they can boost a structural match but can
'''   never create a false positive on their own.
'''
''' Adjacency, IDF and the inverted index are built once in O(V + E); a single
''' comparison then costs O(deg(a) + deg(b) + shared).
''' </remarks>
Public NotInheritable Class BipartiteSimilarityEngine

    Private ReadOnly _view As IBipartiteView
    Private ReadOnly _options As SimilarityOptions
    Private ReadOnly _idf As IdfIndex
    Private ReadOnly _entityIds As Integer()
    Private ReadOnly _inverted As Dictionary(Of Integer, List(Of Integer))

    Public ReadOnly Property View As IBipartiteView
        Get
            Return _view
        End Get
    End Property

    Public ReadOnly Property Options As SimilarityOptions
        Get
            Return _options
        End Get
    End Property

    Public ReadOnly Property Idf As IdfIndex
        Get
            Return _idf
        End Get
    End Property

    ''' <summary>Attribute to entity inverted index (shared with alias discovery).</summary>
    Public ReadOnly Property InvertedIndex As Dictionary(Of Integer, List(Of Integer))
        Get
            Return _inverted
        End Get
    End Property

    Public ReadOnly Property EntityIds As Integer()
        Get
            Return _entityIds
        End Get
    End Property

    Public Sub New(view As IBipartiteView, Optional options As SimilarityOptions = Nothing)
        If view Is Nothing Then
            Throw New ArgumentNullException(NameOf(view))
        End If

        _view = view
        _options = If(options, SimilarityOptions.CreateDefault())

        Dim ids As New List(Of Integer)()
        Dim rows As New List(Of IEnumerable(Of Integer))()

        For Each id As Integer In view.EntityIds
            ids.Add(id)
        Next

        _entityIds = ids.ToArray()

        For Each id As Integer In _entityIds
            Dim attrs As HashSet(Of Integer) = view.GetEntityAttributes(id)
            rows.Add(If(attrs, New HashSet(Of Integer)()))
        Next

        _idf = New IdfIndex(rows, _options.HubDegreeCutoff)
        _inverted = New Dictionary(Of Integer, List(Of Integer))()

        For Each attrId As Integer In view.AttributeIds
            Dim linked As HashSet(Of Integer) = view.GetAttributeEntities(attrId)
            If linked IsNot Nothing AndAlso linked.Count > 0 Then
                _inverted(attrId) = New List(Of Integer)(linked)
            End If
        Next
    End Sub

    Private Function CategoryOf(attrId As Integer) As String
        Dim category As String = _view.GetAttributeCategory(attrId)
        Return If(category, "")
    End Function

    ''' <summary>Build the effective weight vector of one entity.</summary>
    Private Sub BuildWeights(entityId As Integer,
                             attrs As HashSet(Of Integer),
                             target As Dictionary(Of Integer, Double))
        If attrs Is Nothing Then Return

        For Each attrId As Integer In attrs
            Dim weight As Double = _idf.WeightOf(attrId, _options.UseIdf)
            Dim categoryWeight As Double = _options.CategoryWeight(CategoryOf(attrId))

            If weight <= 0.0 OrElse categoryWeight <= 0.0 Then
                Continue For
            End If

            If _options.UseEdgeWeights Then
                Dim edgeWeight As Double = _view.GetEdgeWeight(entityId, attrId)

                If edgeWeight < 0.0 OrElse Double.IsNaN(edgeWeight) OrElse Double.IsInfinity(edgeWeight) Then
                    edgeWeight = 1.0
                End If

                ' Edge weights in graphQL are raw mention counts. A mild log
                ' bonus avoids letting a frequently repeated link dominate.
                weight *= 1.0 + 0.1 * std.Log(1.0 + std.Max(0.0, edgeWeight - 1.0))
            End If

            target(attrId) = weight * categoryWeight
        Next
    End Sub

    Private Shared Function SumOf(values As Dictionary(Of Integer, Double).ValueCollection) As Double
        Dim total As Double = 0.0

        For Each value As Double In values
            total += value
        Next

        Return total
    End Function

    Private Function SharedMinimum(sharedSet As HashSet(Of Integer),
                                   wA As Dictionary(Of Integer, Double),
                                   wB As Dictionary(Of Integer, Double)) As Double
        Dim total As Double = 0.0

        For Each attrId As Integer In sharedSet
            Dim a As Double = 0.0
            Dim b As Double = 0.0

            If wA.ContainsKey(attrId) Then a = wA(attrId)
            If wB.ContainsKey(attrId) Then b = wB(attrId)

            total += std.Min(a, b)
        Next

        Return total
    End Function

    Private Function SharedGeometricMean(sharedSet As HashSet(Of Integer),
                                         wA As Dictionary(Of Integer, Double),
                                         wB As Dictionary(Of Integer, Double)) As Double
        Dim total As Double = 0.0

        For Each attrId As Integer In sharedSet
            Dim a As Double = 0.0
            Dim b As Double = 0.0

            If wA.ContainsKey(attrId) Then a = wA(attrId)
            If wB.ContainsKey(attrId) Then b = wB(attrId)

            total += std.Sqrt(a * b)
        Next

        Return total
    End Function

    ''' <summary>Compute a fully explainable comparison of two entities.</summary>
    Public Function Compute(entityA As Integer, entityB As Integer) As SimilarityDetail
        Dim detail As New SimilarityDetail With {
            .EntityAId = entityA,
            .EntityBId = entityB,
            .Query = If(_view.GetEntityName(entityA), ""),
            .Target = If(_view.GetEntityName(entityB), "")
        }

        If entityA = entityB AndAlso entityA >= 0 Then
            detail.Score = 1.0
            detail.IsEquivalent = True
            Return detail
        End If
        If entityA < 0 OrElse entityB < 0 Then
            Return detail
        End If

        Dim attrsA As HashSet(Of Integer) = _view.GetEntityAttributes(entityA)
        Dim attrsB As HashSet(Of Integer) = _view.GetEntityAttributes(entityB)

        detail.DegreeA = If(attrsA Is Nothing, 0, attrsA.Count)
        detail.DegreeB = If(attrsB Is Nothing, 0, attrsB.Count)

        Dim sharedSet As HashSet(Of Integer) = _view.GetSharedAttributes(entityA, entityB)
        If sharedSet Is Nothing Then sharedSet = New HashSet(Of Integer)()
        detail.CommonNeighbors = sharedSet.Count

        Dim wA As New Dictionary(Of Integer, Double)()
        Dim wB As New Dictionary(Of Integer, Double)()
        BuildWeights(entityA, attrsA, wA)
        BuildWeights(entityB, attrsB, wB)

        Dim sumA As Double = SumOf(wA.Values)
        Dim sumB As Double = SumOf(wB.Values)
        Dim sharedMin As Double = SharedMinimum(sharedSet, wA, wB)

        ' --- structure metrics ---
        Dim jaccard As Double = SetSimilarity.WeightedJaccard(wA, wB)
        Dim cosine As Double = SetSimilarity.SafeDivide(SharedGeometricMean(sharedSet, wA, wB),
                                                       std.Sqrt(sumA) * std.Sqrt(sumB))
        Dim inclusion As Double = SetSimilarity.SafeDivide(sharedMin, std.Min(sumA, sumB))

        Dim aaRaw As Double = 0.0
        For Each attrId As Integer In sharedSet
            Dim degree As Integer = std.Max(2, _view.GetAttributeDegree(attrId))
            aaRaw += _idf.Idf(attrId) / std.Log(1.0 + CDbl(degree))
        Next
        Dim adamicAdar As Double = SetSimilarity.Saturate(aaRaw, _options.AdamicAdarHalfSaturation)

        detail.Metrics("jaccard") = jaccard
        detail.Metrics("cosine") = cosine
        detail.Metrics("adamicadar") = adamicAdar
        detail.Metrics("inclusion") = inclusion

        ' --- optional name / semantic clues ---
        Dim clueAttenuation As Double = If(sharedSet.Count = 0, 0.5, 1.0)

        If _options.NameSimilarity IsNot Nothing Then
            Dim value As Double = _options.NameSimilarity(detail.Query, detail.Target)
            detail.Metrics("name") = SetSimilarity.Clamp01(value) * clueAttenuation
        End If

        If _options.SemanticAvailable AndAlso _options.SemanticSimilarity IsNot Nothing Then
            Dim value As Double = _options.SemanticSimilarity(detail.Query, detail.Target)
            detail.Metrics("semantic") = SetSimilarity.Clamp01(value) * clueAttenuation
        End If

        ' --- per-category (field group) explanation ---
        Dim categories As New HashSet(Of String)()
        If attrsA IsNot Nothing Then
            For Each attrId As Integer In attrsA
                categories.Add(CategoryOf(attrId))
            Next
        End If
        If attrsB IsNot Nothing Then
            For Each attrId As Integer In attrsB
                categories.Add(CategoryOf(attrId))
            Next
        End If

        For Each category As String In categories
            Dim subA As New Dictionary(Of Integer, Double)()
            Dim subB As New Dictionary(Of Integer, Double)()

            For Each item As KeyValuePair(Of Integer, Double) In wA
                If String.Equals(CategoryOf(item.Key), category, StringComparison.OrdinalIgnoreCase) Then
                    subA(item.Key) = item.Value
                End If
            Next
            For Each item As KeyValuePair(Of Integer, Double) In wB
                If String.Equals(CategoryOf(item.Key), category, StringComparison.OrdinalIgnoreCase) Then
                    subB(item.Key) = item.Value
                End If
            Next

            detail.CategoryScores(category) = SetSimilarity.WeightedJaccard(subA, subB)
        Next

        ' --- shared attribute list, rarest first ---
        Dim ordered As New List(Of Integer)(sharedSet)
        ordered.Sort(Function(x As Integer, y As Integer)
                         Return _idf.Degree(x).CompareTo(_idf.Degree(y))
                     End Function)

        Dim limit As Integer = std.Min(ordered.Count, std.Max(0, _options.MaxReportedAttributes))
        Dim names As New List(Of String)(limit)

        For i As Integer = 0 To limit - 1
            detail.SharedAttributeIds.Add(ordered(i))
            names.Add(If(_view.GetAttributeName(ordered(i)), "?"))
        Next
        detail.SharedAttributes = names.ToArray()

        ' --- normalized weighted fusion ---
        Dim totalWeight As Double = 0.0
        Dim effective As New Dictionary(Of String, Double)()

        For Each item As KeyValuePair(Of String, Double) In detail.Metrics
            Dim weight As Double = _options.MetricWeight(item.Key)
            If weight > 0.0 Then
                effective(item.Key) = weight
                totalWeight += weight
            End If
        Next

        If totalWeight <= 0.0 Then
            totalWeight = CDbl(detail.Metrics.Count)
            For Each key As String In detail.Metrics.Keys
                effective(key) = 1.0
            Next
        End If

        If totalWeight > 0.0 Then
            Dim score As Double = 0.0

            For Each item As KeyValuePair(Of String, Double) In detail.Metrics
                Dim weight As Double = 0.0

                If effective.TryGetValue(item.Key, weight) Then
                    Dim normalized As Double = weight / totalWeight
                    detail.MetricWeights(item.Key) = normalized
                    score += normalized * item.Value
                End If
            Next

            detail.Score = SetSimilarity.Clamp01(score)
        End If

        detail.IsEquivalent = detail.Score >= _options.EquivalenceThreshold
        Return detail
    End Function

    ''' <summary>Convenience scalar score in [0, 1].</summary>
    Public Function Score(entityA As Integer, entityB As Integer) As Double
        Return Compute(entityA, entityB).Score
    End Function

    ''' <summary>Compare two entities by name; unknown names produce a zero detail.</summary>
    Public Function ComputeByName(query As String, target As String) As SimilarityDetail
        Dim a As Integer = _view.FindEntityByName(query)
        Dim b As Integer = _view.FindEntityByName(target)

        If a < 0 OrElse b < 0 Then
            Return New SimilarityDetail With {
                .Query = If(query, ""),
                .Target = If(target, ""),
                .EntityAId = a,
                .EntityBId = b
            }
        End If

        Return Compute(a, b)
    End Function

    ''' <summary>Score by name (R# friendly scalar).</summary>
    Public Function ScoreByName(query As String, target As String) As Double
        Return ComputeByName(query, target).Score
    End Function

    ''' <summary>
    ''' All entities sharing at least one non-excluded attribute with the query
    ''' entity, ordered by descending score. Avoids an O(n^2) all-pairs scan.
    ''' </summary>
    Public Function FindCandidates(entityId As Integer,
                                   Optional topN As Integer = Integer.MaxValue) As List(Of SimilarityDetail)
        Dim candidates As New HashSet(Of Integer)()
        Dim attrs As HashSet(Of Integer) = _view.GetEntityAttributes(entityId)

        If attrs IsNot Nothing Then
            For Each attrId As Integer In attrs
                If _idf.IsExcluded(attrId) Then Continue For

                Dim linked As List(Of Integer) = Nothing
                If _inverted.TryGetValue(attrId, linked) Then
                    For Each other As Integer In linked
                        If other <> entityId Then candidates.Add(other)
                    Next
                End If
            Next
        End If

        Dim result As New List(Of SimilarityDetail)(candidates.Count)

        For Each other As Integer In candidates
            Dim item As SimilarityDetail = Compute(entityId, other)
            If item.Score > 0.0 Then result.Add(item)
        Next

        result.Sort(Function(x As SimilarityDetail, y As SimilarityDetail)
                        Return y.Score.CompareTo(x.Score)
                    End Function)

        If topN > 0 AndAlso result.Count > topN Then
            Return result.GetRange(0, topN)
        End If

        Return result
    End Function

End Class
