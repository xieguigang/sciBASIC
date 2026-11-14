Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Similarity
Imports std = System.Math

' General purpose set / vector similarity math.
'
' This module is intentionally free of any business model: it only knows about
' integer item identifiers and dense double vectors. Knowledge graph, NLP and
' clustering code can therefore share exactly the same definitions of Jaccard,
' Overlap, Dice, Cosine, Inclusion and Adamic-Adar instead of re-implementing
' (slightly different) copies of them.
'
' Every function is total: null, empty and degenerate inputs produce a
' deterministic 0.0 instead of NaN, infinity or an exception. All returned
' values are clamped into [0, 1].
Namespace Analysis.Similarity

    Public Module SetSimilarity

        ''' <summary>Clamp a score into [0, 1]; non-finite values become 0.</summary>
        Public Function Clamp01(value As Double) As Double
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Return 0.0
            End If
            If value < 0.0 Then
                Return 0.0
            End If
            If value > 1.0 Then
                Return 1.0
            End If
            Return value
        End Function

        ''' <summary>Division that is safe for a zero / non-finite denominator.</summary>
        Public Function SafeDivide(numerator As Double, denominator As Double) As Double
            If denominator <= 0.0 OrElse Double.IsNaN(denominator) OrElse Double.IsInfinity(denominator) Then
                Return 0.0
            End If
            If Double.IsNaN(numerator) OrElse Double.IsInfinity(numerator) Then
                Return 0.0
            End If
            Return Clamp01(numerator / denominator)
        End Function

        ''' <summary>Count the intersection of two identifier sets by iterating the smaller one.</summary>
        Public Function IntersectionCount(a As HashSet(Of Integer), b As HashSet(Of Integer)) As Integer
            If a Is Nothing OrElse b Is Nothing Then
                Return 0
            End If

            Dim small As HashSet(Of Integer) = a
            Dim large As HashSet(Of Integer) = b

            If a.Count > b.Count Then
                small = b
                large = a
            End If

            Dim n As Integer = 0

            For Each id As Integer In small
                If large.Contains(id) Then
                    n += 1
                End If
            Next

            Return n
        End Function

        ''' <summary>J(A, B) = |A intersect B| / |A union B|.</summary>
        Public Function Jaccard(a As HashSet(Of Integer), b As HashSet(Of Integer)) As Double
            If a Is Nothing OrElse b Is Nothing Then
                Return 0.0
            End If

            Dim sharedCount As Integer = IntersectionCount(a, b)
            Dim union As Integer = a.Count + b.Count - sharedCount

            Return SafeDivide(CDbl(sharedCount), CDbl(union))
        End Function

        ''' <summary>Jaccard from pre-computed intersection size and degrees.</summary>
        Public Function Jaccard(intersect As Integer, degA As Integer, degB As Integer) As Double
            If intersect <= 0 OrElse degA <= 0 OrElse degB <= 0 Then
                Return 0.0
            End If

            Dim union As Integer = degA + degB - intersect

            Return SafeDivide(CDbl(intersect), CDbl(union))
        End Function

        ''' <summary>
        ''' Overlap coefficient = |A intersect B| / min(|A|, |B|).
        ''' Useful when one entity is a strict subset of the other (Is-A inference).
        ''' </summary>
        Public Function Overlap(a As HashSet(Of Integer), b As HashSet(Of Integer)) As Double
            If a Is Nothing OrElse b Is Nothing Then
                Return 0.0
            End If

            Dim sharedCount As Integer = IntersectionCount(a, b)
            Dim smallest As Integer = std.Min(a.Count, b.Count)

            Return SafeDivide(CDbl(sharedCount), CDbl(smallest))
        End Function

        ''' <summary>Overlap coefficient from pre-computed values.</summary>
        Public Function Overlap(intersect As Integer, degA As Integer, degB As Integer) As Double
            If intersect <= 0 Then
                Return 0.0
            End If
            Return SafeDivide(CDbl(intersect), CDbl(std.Min(degA, degB)))
        End Function

        ''' <summary>Dice / Sorensen coefficient = 2|A intersect B| / (|A| + |B|).</summary>
        Public Function Dice(intersect As Integer, degA As Integer, degB As Integer) As Double
            If intersect <= 0 Then
                Return 0.0
            End If
            Return SafeDivide(2.0 * CDbl(intersect), CDbl(degA) + CDbl(degB))
        End Function

        ''' <summary>Inclusion ratio = |A intersect B| / |B| (how much of B is contained in A).</summary>
        Public Function Inclusion(intersect As Integer, degSubset As Integer) As Double
            Return SafeDivide(CDbl(intersect), CDbl(degSubset))
        End Function

        ''' <summary>Binary cosine = |A intersect B| / sqrt(|A| * |B|).</summary>
        Public Function Cosine(common As Integer, degA As Integer, degB As Integer) As Double
            If common <= 0 OrElse degA <= 0 OrElse degB <= 0 Then
                Return 0.0
            End If
            Return SafeDivide(CDbl(common), std.Sqrt(CDbl(degA) * CDbl(degB)))
        End Function

        ''' <summary>
        ''' Weighted (generalised) Jaccard = sum(min(wa, wb)) / sum(max(wa, wb))
        ''' over the union of keys. Weights may come from IDF, edge confidence,
        ''' source counts or any other positive signal.
        ''' </summary>
        Public Function WeightedJaccard(a As IDictionary(Of Integer, Double),
                                        b As IDictionary(Of Integer, Double)) As Double
            If a Is Nothing OrElse b Is Nothing Then
                Return 0.0
            End If

            Dim keys As New HashSet(Of Integer)(a.Keys)
            keys.UnionWith(b.Keys)

            Dim minSum As Double = 0.0
            Dim maxSum As Double = 0.0

            For Each key As Integer In keys
                Dim wa As Double = 0.0
                Dim wb As Double = 0.0

                If a.ContainsKey(key) Then wa = a(key)
                If b.ContainsKey(key) Then wb = b(key)

                minSum += std.Min(wa, wb)
                maxSum += std.Max(wa, wb)
            Next

            Return SafeDivide(minSum, maxSum)
        End Function

        ''' <summary>Cosine similarity between two dense (possibly weighted) vectors.</summary>
        Public Function WeightedCosine(x As Double(), y As Double()) As Double
            If x Is Nothing OrElse y Is Nothing Then
                Return 0.0
            End If

            Dim n As Integer = std.Min(x.Length, y.Length)
            Dim dot As Double = 0.0
            Dim nx As Double = 0.0
            Dim ny As Double = 0.0

            For i As Integer = 0 To n - 1
                dot += x(i) * y(i)
                nx += x(i) * x(i)
                ny += y(i) * y(i)
            Next

            ' A trailing part of the longer vector still contributes to its norm.
            If x.Length > n Then
                For i As Integer = n To x.Length - 1
                    nx += x(i) * x(i)
                Next
            ElseIf y.Length > n Then
                For i As Integer = n To y.Length - 1
                    ny += y(i) * y(i)
                Next
            End If

            Return SafeDivide(dot, std.Sqrt(nx) * std.Sqrt(ny))
        End Function

        ''' <summary>
        ''' Raw Adamic-Adar index = sum(1 / log(degree)) over shared neighbours.
        ''' Attributes with degree &lt;= 1 are skipped (log(1) = 0).
        ''' </summary>
        Public Function AdamicAdarRaw(attributeDegrees As IEnumerable(Of Integer)) As Double
            If attributeDegrees Is Nothing Then
                Return 0.0
            End If

            Dim sum As Double = 0.0

            For Each degree As Integer In attributeDegrees
                If degree > 1 Then
                    sum += 1.0 / std.Log(CDbl(degree))
                End If
            Next

            Return sum
        End Function

        ''' <summary>
        ''' Map an unbounded non-negative index (e.g. Adamic-Adar) into [0, 1].
        ''' </summary>
        ''' <param name="halfSaturation">Value at which the curve reaches 0.5.</param>
        Public Function Saturate(value As Double, Optional halfSaturation As Double = 2.0) As Double
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0.0 Then
                Return 0.0
            End If
            If halfSaturation <= 0.0 Then
                halfSaturation = 1.0
            End If
            Return Clamp01(value / (value + halfSaturation))
        End Function

    End Module

End Namespace

' ScoreMetric adapters so that the same set similarity definitions can be
' consumed by the existing KNN / nearest-neighbour infrastructure.
' A vector is treated as a set: index i is a member when its value is > 0.
Namespace KNearNeighbors

    Public Class JaccardScore : Inherits ScoreMetric

        Public Sub New()
            Me.cutoff = 0.0
        End Sub

        Public Overrides Function eval(x() As Double, y() As Double) As Double
            If x Is Nothing OrElse y Is Nothing Then
                Return 0.0
            End If

            Dim a As New HashSet(Of Integer)()
            Dim b As New HashSet(Of Integer)()
            Dim i As Integer

            For i = 0 To x.Length - 1
                If x(i) > 0.0 Then a.Add(i)
            Next
            For i = 0 To y.Length - 1
                If y(i) > 0.0 Then b.Add(i)
            Next

            Return SetSimilarity.Jaccard(a, b)
        End Function

        Public Overrides Function ToString() As String
            Return "jaccard"
        End Function
    End Class

    Public Class WeightedCosineScore : Inherits ScoreMetric

        Public Sub New()
            Me.cutoff = 0.0
        End Sub

        Public Overrides Function eval(x() As Double, y() As Double) As Double
            Return SetSimilarity.WeightedCosine(x, y)
        End Function

        Public Overrides Function ToString() As String
            Return "weighted_cosine"
        End Function
    End Class

End Namespace
