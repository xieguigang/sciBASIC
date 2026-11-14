Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Similarity
Imports std = System.Math

''' <summary>Importance / hub profile of one attribute node.</summary>
Public Class AttributeScore

    Public Property Id As Integer
    Public Property Name As String = ""
    Public Property Category As String = ""
    Public Property Degree As Integer
    Public Property Idf As Double
    Public Property IsHub As Boolean

    ''' <summary>Rarity-weighted discriminative value.</summary>
    Public ReadOnly Property Score As Double
        Get
            Return Idf
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{Name} [{Category}] degree={Degree} idf={Idf.ToString("F3")}"
    End Function

End Class

''' <summary>Structural profile of one entity node.</summary>
Public Class NodeScore

    Public Property EntityId As Integer
    Public Property Name As String = ""
    Public Property Degree As Integer
    Public Property PageRank As Double
    Public Property Betweenness As Double
    Public Property ComponentId As Integer
    Public Property CommunityId As Integer

End Class

''' <summary>Aggregate statistics of the knowledge network.</summary>
Public Class NetworkSummary

    Public Property EntityCount As Integer
    Public Property AttributeCount As Integer
    Public Property EdgeCount As Integer
    Public Property Density As Double
    Public Property AverageEntityDegree As Double
    Public Property AverageAttributeDegree As Double
    Public Property ComponentCount As Integer
    Public Property LargestComponentSize As Integer
    Public Property CommunityCount As Integer
    Public Property HubAttributeCount As Integer
    Public Property TopHubs As New List(Of AttributeScore)()

    ''' <summary>Plain-text summary, ready for a console report.</summary>
    Public Function ToText() As String
        Dim text As New StringBuilder()

        text.AppendLine($"entities          : {EntityCount}")
        text.AppendLine($"attributes        : {AttributeCount}")
        text.AppendLine($"edges             : {EdgeCount}")
        text.AppendLine($"density           : {Density.ToString("F6")}")
        text.AppendLine($"avg entity degree : {AverageEntityDegree.ToString("F3")}")
        text.AppendLine($"avg attr degree   : {AverageAttributeDegree.ToString("F3")}")
        text.AppendLine($"components        : {ComponentCount} (largest {LargestComponentSize})")
        text.AppendLine($"communities       : {CommunityCount}")
        text.AppendLine($"hub attributes    : {HubAttributeCount}")

        For Each hub As AttributeScore In TopHubs
            text.AppendLine($"  - {hub}")
        Next

        Return text.ToString()
    End Function

    Public Overrides Function ToString() As String
        Return $"V={EntityCount}+{AttributeCount}, E={EdgeCount}, components={ComponentCount}"
    End Function

End Class

''' <summary>One entity-to-attribute edge, used for export / interoperability.</summary>
Public Class NetworkEdgeRecord

    Public Property Source As String = ""
    Public Property Target As String = ""
    Public Property Category As String = ""
    Public Property Weight As Double

    Public Overrides Function ToString() As String
        Return $"{Source} -[{Category}]-> {Target} ({Weight.ToString("F2")})"
    End Function

End Class

''' <summary>Complete result of a network analysis pass.</summary>
Public Class NetworkAnalysisResult

    Public Property Summary As New NetworkSummary()
    Public Property Nodes As New List(Of NodeScore)()
    Public Property Hubs As New List(Of AttributeScore)()
    Public Property Components As New List(Of List(Of Integer))()
    Public Property Communities As New Dictionary(Of Integer, Integer)()
    Public Property Edges As New List(Of NetworkEdgeRecord)()

    ''' <summary>CSV table of per-entity structural scores.</summary>
    Public Function ToCsv() As String
        Dim text As New StringBuilder()
        text.AppendLine("entity_id,name,degree,pagerank,betweenness,component,community")

        For Each node As NodeScore In Nodes
            text.AppendLine($"{node.EntityId},{Csv(node.Name)},{node.Degree}," &
                            $"{node.PageRank.ToString("F6")},{node.Betweenness.ToString("F6")}," &
                            $"{node.ComponentId},{node.CommunityId}")
        Next

        Return text.ToString()
    End Function

    Private Shared Function Csv(value As String) As String
        If value Is Nothing Then Return ""
        Return value.Replace("""", """""").Replace(",", ";")
    End Function

End Class

''' <summary>
''' Knowledge-network structure analysis built purely on IBipartiteView.
''' </summary>
''' <remarks>
''' The heavy math (set similarity, IDF) is reused from graph-netcore5. The
''' routines below are deliberately self-contained so they work for any
''' IBipartiteView implementation and do not depend on the node-id assumptions
''' of the Louvain/Dijkstra adapters in graph-netcore5. Callers that want
''' Louvain/Leiden or weighted PageRank can feed <see cref="ToEdgeList"/> into
''' those adapters directly.
'''
''' Complexity: one adjacency/inverted-index pass O(V + E), a projected graph
''' with a configurable per-attribute degree cap, then O(iterations * E) for
''' PageRank and O(sampleLimit * E) for Brandes betweenness.
''' </remarks>
Public Module NetworkAnalysis

    ''' <summary>Default cap: attributes linked to more entities than this are skipped in the projection.</summary>
    Public Const DefaultMaxProjectionDegree As Integer = 200

    ''' <summary>
    ''' Build the weighted entity projection. Two entities are connected when
    ''' they share an attribute; the weight is that attribute's rarity divided
    ''' by a log penalty of its degree.
    ''' </summary>
    Private Function BuildProjection(view As IBipartiteView,
                                     idf As IdfIndex,
                                     maxProjectionDegree As Integer) As Dictionary(Of Integer, Dictionary(Of Integer, Double))
        Dim links As New Dictionary(Of Integer, Dictionary(Of Integer, Double))()

        If maxProjectionDegree <= 0 Then maxProjectionDegree = DefaultMaxProjectionDegree

        For Each attrId As Integer In view.AttributeIds
            Dim degree As Integer = view.GetAttributeDegree(attrId)

            If degree < 2 OrElse degree > maxProjectionDegree Then
                Continue For
            End If
            If idf.IsExcluded(attrId) Then
                Continue For
            End If

            Dim weight As Double = idf.Idf(attrId) / std.Log(2.0 + CDbl(degree))
            If weight <= 0.0 Then Continue For

            Dim linked As HashSet(Of Integer) = view.GetAttributeEntities(attrId)
            If linked Is Nothing Then Continue For

            For Each a As Integer In linked
                Dim row As Dictionary(Of Integer, Double) = Nothing

                If Not links.TryGetValue(a, row) Then
                    row = New Dictionary(Of Integer, Double)()
                    links(a) = row
                End If

                For Each b As Integer In linked
                    If a = b Then Continue For

                    Dim existing As Double = 0.0
                    If row.ContainsKey(b) Then existing = row(b)
                    row(b) = existing + weight
                Next
            Next
        Next

        Return links
    End Function

    ''' <summary>
    ''' Connected components on the bipartite structure (entities linked through
    ''' shared attributes). Isolated entities form single-node components.
    ''' </summary>
    Public Function ConnectedComponents(view As IBipartiteView) As List(Of List(Of Integer))
        Dim visitedEntities As New HashSet(Of Integer)()
        Dim visitedAttrs As New HashSet(Of Integer)()
        Dim result As New List(Of List(Of Integer))()

        For Each start As Integer In view.EntityIds
            If visitedEntities.Contains(start) Then Continue For

            Dim component As New List(Of Integer)()
            Dim queue As New Queue(Of Integer)()

            visitedEntities.Add(start)
            queue.Enqueue(start)

            While queue.Count > 0
                Dim entityId As Integer = queue.Dequeue()
                component.Add(entityId)

                Dim attrs As HashSet(Of Integer) = view.GetEntityAttributes(entityId)
                If attrs Is Nothing Then Continue While

                For Each attrId As Integer In attrs
                    If Not visitedAttrs.Add(attrId) Then Continue For

                    Dim linked As HashSet(Of Integer) = view.GetAttributeEntities(attrId)
                    If linked Is Nothing Then Continue For

                    For Each other As Integer In linked
                        If visitedEntities.Add(other) Then
                            queue.Enqueue(other)
                        End If
                    Next
                Next
            End While

            result.Add(component)
        Next

        Return result
    End Function

    ''' <summary>
    ''' Weighted PageRank on the entity projection using power iteration with
    ''' dangling-mass redistribution.
    ''' </summary>
    Public Function PageRank(view As IBipartiteView,
                             idf As IdfIndex,
                             Optional alpha As Double = 0.85,
                             Optional iterations As Integer = 40,
                             Optional maxProjectionDegree As Integer = DefaultMaxProjectionDegree) As Dictionary(Of Integer, Double)
        Dim ids As New List(Of Integer)(view.EntityIds)
        Dim rank As New Dictionary(Of Integer, Double)()

        If ids.Count = 0 Then Return rank

        Dim n As Integer = ids.Count
        Dim links As Dictionary(Of Integer, Dictionary(Of Integer, Double)) =
            BuildProjection(view, idf, maxProjectionDegree)
        Dim initial As Double = 1.0 / CDbl(n)

        If alpha <= 0.0 OrElse alpha >= 1.0 Then alpha = 0.85
        If iterations <= 0 Then iterations = 40

        For Each id As Integer In ids
            rank(id) = initial
        Next

        For iteration As Integer = 1 To iterations
            Dim nextRank As New Dictionary(Of Integer, Double)()
            Dim dangling As Double = 0.0

            For Each id As Integer In ids
                Dim row As Dictionary(Of Integer, Double) = Nothing

                If Not links.TryGetValue(id, row) OrElse row.Count = 0 Then
                    dangling += rank(id)
                End If
            Next

            Dim baseScore As Double = (1.0 - alpha) / CDbl(n) + alpha * dangling / CDbl(n)

            For Each id As Integer In ids
                nextRank(id) = baseScore
            Next

            For Each item As KeyValuePair(Of Integer, Dictionary(Of Integer, Double)) In links
                Dim total As Double = 0.0

                For Each w As Double In item.Value.Values
                    total += w
                Next
                If total <= 0.0 Then Continue For

                Dim share As Double = alpha * rank(item.Key)

                For Each target As KeyValuePair(Of Integer, Double) In item.Value
                    If Not nextRank.ContainsKey(target.Key) Then Continue For
                    nextRank(target.Key) = nextRank(target.Key) + share * target.Value / total
                Next
            Next

            rank = nextRank
        Next

        Return rank
    End Function

    ''' <summary>
    ''' Brandes betweenness centrality on the projected graph, optionally
    ''' sampling the sources to keep large graphs tractable. Values are
    ''' normalized by the maximum observed score.
    ''' </summary>
    Public Function Betweenness(view As IBipartiteView,
                                idf As IdfIndex,
                                Optional sampleLimit As Integer = 200,
                                Optional maxProjectionDegree As Integer = DefaultMaxProjectionDegree) As Dictionary(Of Integer, Double)
        Dim ids As New List(Of Integer)(view.EntityIds)
        Dim score As New Dictionary(Of Integer, Double)()

        If ids.Count = 0 Then Return score

        Dim links As Dictionary(Of Integer, Dictionary(Of Integer, Double)) =
            BuildProjection(view, idf, maxProjectionDegree)

        For Each id As Integer In ids
            score(id) = 0.0
        Next

        Dim sources As New List(Of Integer)(ids)
        If sampleLimit > 0 AndAlso sources.Count > sampleLimit Then
            sources.RemoveRange(sampleLimit, sources.Count - sampleLimit)
        End If

        For Each source As Integer In sources
            Dim stack As New Stack(Of Integer)()
            Dim predecessors As New Dictionary(Of Integer, List(Of Integer))()
            Dim sigma As New Dictionary(Of Integer, Double)()
            Dim distance As New Dictionary(Of Integer, Integer)()
            Dim queue As New Queue(Of Integer)()

            For Each id As Integer In ids
                predecessors(id) = New List(Of Integer)()
                sigma(id) = 0.0
                distance(id) = -1
            Next

            sigma(source) = 1.0
            distance(source) = 0
            queue.Enqueue(source)

            While queue.Count > 0
                Dim v As Integer = queue.Dequeue()
                stack.Push(v)

                Dim row As Dictionary(Of Integer, Double) = Nothing
                If Not links.TryGetValue(v, row) Then Continue While

                For Each w As Integer In row.Keys
                    If distance(w) < 0 Then
                        distance(w) = distance(v) + 1
                        queue.Enqueue(w)
                    End If
                    If distance(w) = distance(v) + 1 Then
                        sigma(w) += sigma(v)
                        predecessors(w).Add(v)
                    End If
                Next
            End While

            Dim delta As New Dictionary(Of Integer, Double)()
            For Each id As Integer In ids
                delta(id) = 0.0
            Next

            While stack.Count > 0
                Dim w As Integer = stack.Pop()

                For Each v As Integer In predecessors(w)
                    If sigma(w) > 0.0 Then
                        delta(v) += (sigma(v) / sigma(w)) * (1.0 + delta(w))
                    End If
                Next

                If w <> source Then
                    score(w) = score(w) + delta(w)
                End If
            End While
        Next

        Dim maximum As Double = 0.0
        For Each value As Double In score.Values
            If value > maximum Then maximum = value
        Next

        If maximum > 0.0 Then
            Dim keys As New List(Of Integer)(score.Keys)
            For Each key As Integer In keys
                score(key) = score(key) / maximum
            Next
        End If

        Return score
    End Function

    ''' <summary>
    ''' Weighted label-propagation communities on the entity projection.
    ''' Deterministic: ties resolve to the smallest label.
    ''' </summary>
    Public Function LabelPropagationCommunities(view As IBipartiteView,
                                                idf As IdfIndex,
                                                Optional maxIterations As Integer = 30,
                                                Optional maxProjectionDegree As Integer = DefaultMaxProjectionDegree) As Dictionary(Of Integer, Integer)
        Dim ids As New List(Of Integer)(view.EntityIds)
        Dim labels As New Dictionary(Of Integer, Integer)()

        If ids.Count = 0 Then Return labels

        Dim links As Dictionary(Of Integer, Dictionary(Of Integer, Double)) =
            BuildProjection(view, idf, maxProjectionDegree)
        Dim rng As New Random(17)

        For i As Integer = 0 To ids.Count - 1
            labels(ids(i)) = ids(i)
        Next

        If maxIterations <= 0 Then maxIterations = 30

        For iteration As Integer = 1 To maxIterations
            Dim order As New List(Of Integer)(ids)
            Dim changed As Boolean = False

            For i As Integer = order.Count - 1 To 1 Step -1
                Dim j As Integer = rng.Next(i + 1)
                Dim temp As Integer = order(i)
                order(i) = order(j)
                order(j) = temp
            Next

            For Each v As Integer In order
                Dim row As Dictionary(Of Integer, Double) = Nothing
                If Not links.TryGetValue(v, row) Then Continue For

                Dim votes As New Dictionary(Of Integer, Double)()

                For Each item As KeyValuePair(Of Integer, Double) In row
                    If Not labels.ContainsKey(item.Key) Then Continue For

                    Dim label As Integer = labels(item.Key)
                    If votes.ContainsKey(label) Then
                        votes(label) = votes(label) + item.Value
                    Else
                        votes(label) = item.Value
                    End If
                Next

                If votes.Count = 0 Then Continue For

                Dim bestLabel As Integer = Integer.MaxValue
                Dim bestWeight As Double = -1.0

                For Each item As KeyValuePair(Of Integer, Double) In votes
                    If item.Value > bestWeight OrElse
                       (item.Value = bestWeight AndAlso item.Key < bestLabel) Then
                        bestWeight = item.Value
                        bestLabel = item.Key
                    End If
                Next

                If bestLabel <> Integer.MaxValue AndAlso labels(v) <> bestLabel Then
                    labels(v) = bestLabel
                    changed = True
                End If
            Next

            If Not changed Then Exit For
        Next

        Return labels
    End Function

    ''' <summary>
    ''' Rank attributes by degree and flag hubs. Rare, highly discriminative
    ''' attributes get a high Idf; hubs get a low one.
    ''' </summary>
    Public Function HubAttributes(view As IBipartiteView,
                                  idf As IdfIndex,
                                  Optional topN As Integer = 20,
                                  Optional hubFactor As Double = 3.0) As List(Of AttributeScore)
        Dim result As New List(Of AttributeScore)()
        Dim all As New List(Of AttributeScore)()

        For Each attrId As Integer In view.AttributeIds
            Dim degree As Integer = view.GetAttributeDegree(attrId)

            all.Add(New AttributeScore With {
                .Id = attrId,
                .Name = If(view.GetAttributeName(attrId), "?"),
                .Category = If(view.GetAttributeCategory(attrId), ""),
                .Degree = degree,
                .Idf = idf.Idf(attrId)
            })
        Next

        If all.Count = 0 Then Return result

        Dim average As Double = CDbl(idf.EntityCount)
        If average > 0.0 Then
            Dim totalEdges As Integer = 0
            For Each item As AttributeScore In all
                totalEdges += item.Degree
            Next
            average = CDbl(totalEdges) / CDbl(all.Count)
        End If

        For Each item As AttributeScore In all
            item.IsHub = item.Degree > average * hubFactor
        Next

        all.Sort(Function(x As AttributeScore, y As AttributeScore)
                     Return y.Degree.CompareTo(x.Degree)
                 End Function)

        Dim limit As Integer = std.Min(all.Count, std.Max(0, topN))
        For i As Integer = 0 To limit - 1
            result.Add(all(i))
        Next

        Return result
    End Function

    ''' <summary>Export the raw bipartite edge list for external tooling.</summary>
    Public Function ToEdgeList(view As IBipartiteView) As List(Of NetworkEdgeRecord)
        Dim result As New List(Of NetworkEdgeRecord)()

        For Each entityId As Integer In view.EntityIds
            Dim entityName As String = If(view.GetEntityName(entityId), "?")
            Dim attrs As HashSet(Of Integer) = view.GetEntityAttributes(entityId)

            If attrs Is Nothing Then Continue For

            For Each attrId As Integer In attrs
                result.Add(New NetworkEdgeRecord With {
                    .Source = entityName,
                    .Target = If(view.GetAttributeName(attrId), "?"),
                    .Category = If(view.GetAttributeCategory(attrId), ""),
                    .Weight = view.GetEdgeWeight(entityId, attrId)
                })
            Next
        Next

        Return result
    End Function

    ''' <summary>Produce the full analysis result in one pass.</summary>
    Public Function Analyze(view As IBipartiteView,
                            Optional topN As Integer = 20,
                            Optional sampleLimit As Integer = 200,
                            Optional degreeCutoff As Integer = 0,
                            Optional maxProjectionDegree As Integer = DefaultMaxProjectionDegree) As NetworkAnalysisResult
        Dim result As New NetworkAnalysisResult()
        Dim ids As New List(Of Integer)(view.EntityIds)
        Dim attrIds As New List(Of Integer)(view.AttributeIds)
        Dim rows As New List(Of IEnumerable(Of Integer))()
        Dim edgeCount As Integer = 0

        For Each id As Integer In ids
            Dim attrs As HashSet(Of Integer) = view.GetEntityAttributes(id)
            rows.Add(If(attrs, New HashSet(Of Integer)()))
            If attrs IsNot Nothing Then edgeCount += attrs.Count
        Next

        Dim idf As New IdfIndex(rows, degreeCutoff)

        ' --- summary ---
        Dim summary As New NetworkSummary With {
            .EntityCount = ids.Count,
            .AttributeCount = attrIds.Count,
            .EdgeCount = edgeCount
        }

        If ids.Count > 0 Then
            summary.AverageEntityDegree = CDbl(edgeCount) / CDbl(ids.Count)
        End If
        If attrIds.Count > 0 Then
            summary.AverageAttributeDegree = CDbl(edgeCount) / CDbl(attrIds.Count)
        End If
        If ids.Count > 0 AndAlso attrIds.Count > 0 Then
            summary.Density = SetSimilarity.Clamp01(CDbl(edgeCount) / (CDbl(ids.Count) * CDbl(attrIds.Count)))
        End If

        Dim components As List(Of List(Of Integer)) = ConnectedComponents(view)
        summary.ComponentCount = components.Count

        For Each component As List(Of Integer) In components
            If component.Count > summary.LargestComponentSize Then
                summary.LargestComponentSize = component.Count
            End If
        Next

        Dim hubs As List(Of AttributeScore) = HubAttributes(view, idf, topN)
        summary.TopHubs = hubs
        For Each item As AttributeScore In hubs
            If item.IsHub Then summary.HubAttributeCount += 1
        Next

        ' --- per-node scores ---
        Dim ranks As Dictionary(Of Integer, Double) = PageRank(view, idf, maxProjectionDegree:=maxProjectionDegree)
        Dim betweennessScores As Dictionary(Of Integer, Double) = Betweenness(view, idf, sampleLimit,
                                                                              maxProjectionDegree)
        Dim communities As Dictionary(Of Integer, Integer) = LabelPropagationCommunities(view, idf,
                                                                                       maxProjectionDegree:=maxProjectionDegree)
        Dim componentOf As New Dictionary(Of Integer, Integer)()

        For c As Integer = 0 To components.Count - 1
            For Each id As Integer In components(c)
                componentOf(id) = c
            Next
        Next

        Dim distinctCommunities As New HashSet(Of Integer)()

        For Each id As Integer In ids
            Dim node As New NodeScore With {
                .EntityId = id,
                .Name = If(view.GetEntityName(id), "?"),
                .Degree = view.GetEntityDegree(id)
            }

            Dim value As Double = 0.0
            If ranks.TryGetValue(id, value) Then node.PageRank = value
            If betweennessScores.TryGetValue(id, value) Then node.Betweenness = value

            Dim cid As Integer = 0
            If componentOf.TryGetValue(id, cid) Then node.ComponentId = cid
            If communities.TryGetValue(id, cid) Then
                node.CommunityId = cid
                distinctCommunities.Add(cid)
            End If

            result.Nodes.Add(node)
        Next

        summary.CommunityCount = distinctCommunities.Count
        result.Summary = summary
        result.Hubs = hubs
        result.Components = components
        result.Communities = communities
        result.Edges = ToEdgeList(view)

        Return result
    End Function

End Module
