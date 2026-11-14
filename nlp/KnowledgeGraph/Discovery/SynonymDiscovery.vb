Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Similarity
Imports std = System.Math

''' <summary>
''' Scalable discovery of alias (equivalent entity) groups.
''' </summary>
''' <remarks>
''' The existing EntityDisambiguator.Disambiguate() enumerates all n^2 entity
''' pairs and runs 5000 permutation iterations on every pair above threshold,
''' which is unusable on a real knowledge network. This implementation instead:
'''   1. generates candidate pairs from the attribute-to-entity inverted index
'''      (only pairs sharing at least one attribute are ever considered);
'''   2. skips hub attributes and caps the candidate count;
'''   3. filters with the cheap fused score before any significance test;
'''   4. optionally reuses StatisticalTest (when the underlying view is a
'''      KnowledgeGraph) or an empirical null model for p-value / z-score;
'''   5. merges accepted pairs with a union-find transitive closure;
'''   6. selects a canonical name using degree + injected priors (mentions,
'''      source count, language) instead of a short-name heuristic.
''' </remarks>
Public NotInheritable Class SynonymDiscovery

    Private ReadOnly _engine As BipartiteSimilarityEngine
    Private ReadOnly _view As IBipartiteView
    Private ReadOnly _options As AliasDiscoveryOptions
    Private ReadOnly _graph As KnowledgeGraph
    Private ReadOnly _rng As New Random(42)

    ''' <summary>
    ''' </summary>
    ''' <param name="engine">The weighted similarity engine (holds the inverted index).</param>
    ''' <param name="options">Discovery thresholds; Nothing uses defaults.</param>
    ''' <param name="graph">
    ''' Optional concrete KnowledgeGraph backing <paramref name="engine"/>. When
    ''' supplied and significance testing is enabled, the existing
    ''' StatisticalTest permutation test is reused. Entity ids must match.
    ''' </param>
    Public Sub New(engine As BipartiteSimilarityEngine,
                   Optional options As AliasDiscoveryOptions = Nothing,
                   Optional graph As KnowledgeGraph = Nothing)

        If engine Is Nothing Then
            Throw New ArgumentNullException(NameOf(engine))
        End If

        _engine = engine
        _view = engine.View
        _options = If(options, New AliasDiscoveryOptions())
        _graph = graph
    End Sub

    Private Shared Function PairKey(a As Integer, b As Integer) As Long
        Dim low As Integer = std.Min(a, b)
        Dim high As Integer = std.Max(a, b)
        Return (CLng(low) << 32) Or (CLng(high) And &HFFFFFFFFL)
    End Function

    ''' <summary>
    ''' Generate every pair that shares at least one attribute, without the
    ''' O(n^2) all-pairs enumeration. Hub attributes are skipped.
    ''' </summary>
    Public Function GenerateCandidates() As List(Of KeyValuePair(Of Integer, Integer))
        Dim seen As New HashSet(Of Long)()
        Dim result As New List(Of KeyValuePair(Of Integer, Integer))()
        Dim limit As Integer = std.Max(1, _options.MaxCandidates)
        Dim hitLimit As Boolean = False

        For Each item As KeyValuePair(Of Integer, List(Of Integer)) In _engine.InvertedIndex
            Dim attrId As Integer = item.Key
            Dim linked As List(Of Integer) = item.Value

            If linked Is Nothing OrElse linked.Count < 2 Then Continue For
            If _options.HubDegreeCutoff > 0 AndAlso
               _engine.Idf.Degree(attrId) > _options.HubDegreeCutoff Then
                Continue For
            End If

            linked.Sort()

            For i As Integer = 0 To linked.Count - 2
                For j As Integer = i + 1 To linked.Count - 1
                    Dim a As Integer = linked(i)
                    Dim b As Integer = linked(j)
                    Dim key As Long = PairKey(a, b)

                    If seen.Add(key) Then
                        result.Add(New KeyValuePair(Of Integer, Integer)(a, b))

                        If result.Count >= limit Then
                            hitLimit = True
                            Exit For
                        End If
                    End If
                Next

                If hitLimit Then Exit For
            Next

            If hitLimit Then Exit For
        Next

        Return result
    End Function

    ''' <summary>
    ''' Score candidate pairs and return those above the configured thresholds.
    ''' </summary>
    Public Function FindPairs() As List(Of SimilarityDetail)
        Dim pairs As List(Of KeyValuePair(Of Integer, Integer)) = GenerateCandidates()
        Dim scored As New List(Of SimilarityDetail)()

        For Each pair As KeyValuePair(Of Integer, Integer) In pairs
            Dim sharedCount As Integer = _view.GetSharedAttributes(pair.Key, pair.Value).Count

            If sharedCount < _options.MinimumSharedAttributes Then
                Continue For
            End If

            Dim detail As SimilarityDetail = _engine.Compute(pair.Key, pair.Value)

            If detail.Score >= _options.MinimumScore Then
                scored.Add(detail)
            End If
        Next

        If _options.PermutationIterations > 0 AndAlso scored.Count > 0 Then
            Call ApplySignificance(scored)
        End If

        scored.Sort(Function(x As SimilarityDetail, y As SimilarityDetail)
                        Return y.Score.CompareTo(x.Score)
                    End Function)

        Return scored
    End Function

    ''' <summary>
    ''' Attach p-value / z-score and drop pairs that are not significant after
    ''' optional Bonferroni correction.
    ''' </summary>
    Private Sub ApplySignificance(scored As List(Of SimilarityDetail))
        Dim tester As StatisticalTest = If(_graph Is Nothing, Nothing,
                                           New StatisticalTest(_graph, seed:=42))
        Dim iterations As Integer = _options.PermutationIterations
        Dim tested As Integer = std.Max(1, scored.Count)
        Dim alpha As Double = If(_options.ApplyBonferroniCorrection,
                                 _options.Alpha / CDbl(tested), _options.Alpha)
        Dim kept As New List(Of SimilarityDetail)(scored.Count)

        For Each detail As SimilarityDetail In scored
            Dim pValue As Double
            Dim zScore As Double

            If tester IsNot Nothing Then
                Dim result As PermutationTestResult = tester.TestJaccard(detail.EntityAId,
                                                                        detail.EntityBId,
                                                                        iterations)
                pValue = result.PValue
                zScore = result.ZScore
            Else
                Dim empirical = EmpiricalNull(detail, iterations)
                pValue = empirical.pValue
                zScore = empirical.zScore
            End If

            detail.PValue = pValue
            detail.ZScore = zScore

            If pValue <= alpha Then
                kept.Add(detail)
            End If
        Next

        scored.Clear()
        scored.AddRange(kept)
    End Sub

    ''' <summary>
    ''' Degree-aware empirical null model for views that are not a concrete
    ''' KnowledgeGraph. Draws random entity pairs and estimates how often their
    ''' Jaccard reaches the observed value.
    ''' </summary>
    Private Function EmpiricalNull(detail As SimilarityDetail,
                                   iterations As Integer) As (pValue As Double, zScore As Double)
        Dim ids As Integer() = _engine.EntityIds

        If ids Is Nothing OrElse ids.Length < 2 Then
            Return (1.0, 0.0)
        End If

        Dim observed As Double = 0.0
        If detail.Metrics.ContainsKey("jaccard") Then
            observed = detail.Metrics("jaccard")
        End If

        Dim samples As New List(Of Double)(iterations)

        For i As Integer = 1 To iterations
            Dim a As Integer = ids(_rng.Next(ids.Length))
            Dim b As Integer = ids(_rng.Next(ids.Length))

            If a = b Then Continue For

            Dim sharedSet As HashSet(Of Integer) = _view.GetSharedAttributes(a, b)
            Dim value As Double = SetSimilarity.Jaccard(sharedSet.Count,
                                                       _view.GetEntityDegree(a),
                                                       _view.GetEntityDegree(b))
            samples.Add(value)
        Next

        If samples.Count = 0 Then
            Return (1.0, 0.0)
        End If

        Dim total As Double = 0.0
        For Each value As Double In samples
            total += value
        Next
        Dim mean As Double = total / CDbl(samples.Count)

        Dim variance As Double = 0.0
        For Each value As Double In samples
            variance += (value - mean) * (value - mean)
        Next
        Dim sd As Double = std.Sqrt(variance / CDbl(samples.Count))

        Dim extreme As Integer = 0
        For Each value As Double In samples
            If value >= observed Then extreme += 1
        Next

        Dim pValue As Double = CDbl(extreme + 1) / CDbl(samples.Count + 1)
        Dim zScore As Double = If(sd > 0.0, (observed - mean) / sd, 0.0)

        Return (pValue, zScore)
    End Function

    ''' <summary>Discover transitive-closure alias groups.</summary>
    Public Function Discover() As List(Of AliasGroup)
        Dim pairs As List(Of SimilarityDetail) = FindPairs()
        Dim ids As Integer() = _engine.EntityIds

        If ids Is Nothing OrElse ids.Length = 0 OrElse pairs.Count = 0 Then
            Return New List(Of AliasGroup)()
        End If

        Dim index As New Dictionary(Of Integer, Integer)()
        Dim parent(ids.Length - 1) As Integer

        For i As Integer = 0 To ids.Length - 1
            parent(i) = i
            index(ids(i)) = i
        Next

        For Each pair As SimilarityDetail In pairs
            Dim a As Integer
            Dim b As Integer

            If Not index.TryGetValue(pair.EntityAId, a) Then Continue For
            If Not index.TryGetValue(pair.EntityBId, b) Then Continue For

            Dim rootA As Integer = Find(parent, a)
            Dim rootB As Integer = Find(parent, b)

            If rootA <> rootB Then parent(rootA) = rootB
        Next

        Dim clusters As New Dictionary(Of Integer, List(Of Integer))()

        For i As Integer = 0 To ids.Length - 1
            Dim root As Integer = Find(parent, i)

            If Not clusters.ContainsKey(root) Then
                clusters(root) = New List(Of Integer)()
            End If
            clusters(root).Add(ids(i))
        Next

        Dim pairIndex As New Dictionary(Of Long, SimilarityDetail)()
        For Each pair As SimilarityDetail In pairs
            pairIndex(PairKey(pair.EntityAId, pair.EntityBId)) = pair
        Next

        Dim result As New List(Of AliasGroup)()

        For Each cluster As KeyValuePair(Of Integer, List(Of Integer)) In clusters
            If cluster.Value.Count < 2 Then Continue For

            Dim group As New AliasGroup With {
                .Members = cluster.Value
            }
            Dim names As New List(Of String)(cluster.Value.Count)
            Dim maxScore As Double = 0.0

            For Each id As Integer In cluster.Value
                names.Add(If(_view.GetEntityName(id), "?"))
            Next
            group.Names = names.ToArray()

            For i As Integer = 0 To cluster.Value.Count - 1
                For j As Integer = i + 1 To cluster.Value.Count - 1
                    Dim detail As SimilarityDetail = Nothing
                    Dim key As Long = PairKey(cluster.Value(i), cluster.Value(j))

                    If pairIndex.TryGetValue(key, detail) Then
                        group.Pairwise.Add(detail)
                        If detail.Score > maxScore Then maxScore = detail.Score
                    End If
                Next
            Next

            group.Confidence = maxScore

            Dim canonicalId As Integer = ChooseCanonical(cluster.Value)
            group.CanonicalName = If(_view.GetEntityName(canonicalId), "")
            group.CanonicalLanguage = If(_view.GetEntityLanguage(canonicalId), "")

            result.Add(group)
        Next

        result.Sort(Function(x As AliasGroup, y As AliasGroup)
                        Return y.Confidence.CompareTo(x.Confidence)
                    End Function)

        Return result
    End Function

    Private Function ChooseCanonical(members As List(Of Integer)) As Integer
        Dim best As Integer = members(0)
        Dim bestScore As Double = CanonicalScore(best)

        For Each id As Integer In members
            Dim score As Double = CanonicalScore(id)

            If score > bestScore Then
                bestScore = score
                best = id
            End If
        Next

        Return best
    End Function

    ''' <summary>
    ''' Degree is the primary signal, with optional injected priors for mention
    ''' count, source coverage and preferred language.
    ''' </summary>
    Private Function CanonicalScore(id As Integer) As Double
        Dim degree As Integer = _view.GetEntityDegree(id)
        Dim score As Double = std.Log(1.0 + CDbl(std.Max(0, degree)))

        If _options.EntityPrior IsNot Nothing Then
            Dim prior As Double = _options.EntityPrior(id)
            If prior > 0.0 AndAlso Not Double.IsNaN(prior) Then
                score += 0.15 * std.Log(1.0 + prior)
            End If
        End If

        If _options.LanguagePrior IsNot Nothing Then
            Dim language As String = _view.GetEntityLanguage(id)
            Dim bonus As Double = 0.0

            If language IsNot Nothing AndAlso _options.LanguagePrior.TryGetValue(language, bonus) Then
                score += bonus
            End If
        End If

        Return score
    End Function

    ''' <summary>
    ''' Convert to the legacy SynonymGroup type so the result can be passed to
    ''' OntologyInferer.InferRelations without changing that module.
    ''' </summary>
    Public Function ToSynonymGroups(groups As List(Of AliasGroup)) As List(Of SynonymGroup)
        Dim result As New List(Of SynonymGroup)()

        If groups Is Nothing Then Return result

        For Each group As AliasGroup In groups
            Dim converted As New SynonymGroup With {
                .EntityIds = New List(Of Integer)(group.Members),
                .Confidence = group.Confidence,
                .CanonicalName = group.CanonicalName,
                .CanonicalLanguage = group.CanonicalLanguage
            }

            For Each detail As SimilarityDetail In group.Pairwise
                converted.PairwiseResults.Add(New SynonymPairResult With {
                    .EntityAId = detail.EntityAId,
                    .EntityBId = detail.EntityBId,
                    .NameA = detail.Query,
                    .NameB = detail.Target,
                    .Jaccard = MetricOf(detail, "jaccard"),
                    .AdamicAdar = MetricOf(detail, "adamicadar"),
                    .JaccardPValue = detail.PValue,
                    .AAPValue = detail.PValue,
                    .JaccardZScore = detail.ZScore,
                    .AAZScore = detail.ZScore,
                    .BonferroniCorrectedPValue = detail.PValue,
                    .IsSignificant = True
                })
            Next

            result.Add(converted)
        Next

        Return result
    End Function

    Private Shared Function MetricOf(detail As SimilarityDetail, key As String) As Double
        Dim value As Double = 0.0
        If detail.Metrics.TryGetValue(key, value) Then Return value
        Return 0.0
    End Function

    Private Shared Function Find(parent As Integer(), x As Integer) As Integer
        While parent(x) <> x
            parent(x) = parent(parent(x))
            x = parent(x)
        End While
        Return x
    End Function

End Class
