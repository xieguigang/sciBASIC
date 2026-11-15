Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace Analysis.Similarity

    ''' <summary>
    ''' Inverse-document-frequency index over a bipartite (entity - attribute) graph.
    ''' </summary>
    ''' <remarks>
    ''' A shared attribute that appears on almost every entity (a hub such as
    ''' "name" or a very common "formula") carries almost no evidence that two
    ''' entities are the same object. A rare attribute (CAS number, database
    ''' accession, InChI) is strong evidence. This index turns attribute degrees
    ''' into a smooth [0, 1] rarity weight:
    '''
    '''     idf(a) = log(1 + N / (1 + degree(a))) / log(1 + N)
    '''
    ''' where N is the number of entities. degree = 0 yields 1.0 and
    ''' degree = N yields the smallest non-zero weight. Build the index once
    ''' (O(V + E)) and reuse it for every pairwise comparison.
    ''' </remarks>
    Public NotInheritable Class IdfIndex

        Private ReadOnly _degree As New Dictionary(Of Integer, Integer)()
        Private ReadOnly _idf As New Dictionary(Of Integer, Double)()
        Private ReadOnly _maxIdf As Double
        Private ReadOnly _entityCount As Integer
        Private ReadOnly _degreeCutoff As Integer

        ''' <summary>Number of entity rows used to build this index.</summary>
        Public ReadOnly Property EntityCount As Integer
            Get
                Return _entityCount
            End Get
        End Property

        ''' <summary>Number of distinct attribute identifiers observed.</summary>
        Public ReadOnly Property AttributeCount As Integer
            Get
                Return _degree.Count
            End Get
        End Property

        ''' <summary>
        ''' Attributes whose degree is strictly greater than this value are skipped
        ''' by <see cref="WeightOf"/>. Zero disables the cut-off.
        ''' </summary>
        Public ReadOnly Property DegreeCutoff As Integer
            Get
                Return _degreeCutoff
            End Get
        End Property

        ''' <summary>
        ''' Build the index from an enumeration of entity adjacency sets.
        ''' </summary>
        ''' <param name="entities">Each row is the set of attributes of one entity.</param>
        ''' <param name="degreeCutoff">
        ''' Optional protection against hub attributes. 0 keeps every attribute.
        ''' </param>
        Public Sub New(entities As IEnumerable(Of IEnumerable(Of Integer)),
                       Optional degreeCutoff As Integer = 0)

            If degreeCutoff < 0 Then
                degreeCutoff = 0
            End If
            _degreeCutoff = degreeCutoff

            If entities Is Nothing Then
                _entityCount = 0
                _maxIdf = 1.0
                Return
            End If

            Dim n As Integer = 0

            For Each row As IEnumerable(Of Integer) In entities
                n += 1

                If row Is Nothing Then
                    Continue For
                End If

                For Each attrId As Integer In row
                    If _degree.ContainsKey(attrId) Then
                        _degree(attrId) += 1
                    Else
                        _degree(attrId) = 1
                    End If
                Next
            Next

            _entityCount = n
            _maxIdf = std.Log(1.0 + CDbl(n))

            If _maxIdf <= 0.0 OrElse Double.IsNaN(_maxIdf) Then
                _maxIdf = 1.0
            End If

            For Each item As KeyValuePair(Of Integer, Integer) In _degree
                Dim raw As Double = std.Log(1.0 + CDbl(n) / (1.0 + CDbl(item.Value)))
                _idf(item.Key) = SetSimilarity.Clamp01(raw / _maxIdf)
            Next
        End Sub

        ''' <summary>Number of entities connected to an attribute (0 when unknown).</summary>
        Public Function Degree(attrId As Integer) As Integer
            Dim d As Integer = 0

            If _degree.TryGetValue(attrId, d) Then
                Return d
            End If
            Return 0
        End Function

        ''' <summary>
        ''' Normalized rarity in [0, 1]. Unknown attributes are treated as maximally rare.
        ''' </summary>
        Public Function Idf(attrId As Integer) As Double
            Dim value As Double = 0.0

            If _idf.TryGetValue(attrId, value) Then
                Return value
            End If
            Return 1.0
        End Function

        ''' <summary>Alias of <see cref="Idf"/> kept for readability at call sites.</summary>
        Public Function Weight(attrId As Integer) As Double
            Return Idf(attrId)
        End Function

        ''' <summary>Whether an attribute is excluded by the configured hub cut-off.</summary>
        Public Function IsExcluded(attrId As Integer) As Boolean
            If _degreeCutoff <= 0 Then
                Return False
            End If
            Return Degree(attrId) > _degreeCutoff
        End Function

        ''' <summary>
        ''' Effective per-attribute weight used by a similarity engine.
        ''' Returns 0 for excluded hub attributes, 1 when IDF is disabled.
        ''' </summary>
        Public Function WeightOf(attrId As Integer, useIdf As Boolean) As Double
            If IsExcluded(attrId) Then
                Return 0.0
            End If
            If Not useIdf Then
                Return 1.0
            End If
            Return Idf(attrId)
        End Function

        ''' <summary>
        ''' Enumerate attributes ordered from rarest to most common. Handy for
        ''' explaining a match or for generating candidate pairs.
        ''' </summary>
        Public Function RareAttributes() As List(Of KeyValuePair(Of Integer, Integer))
            Dim result As New List(Of KeyValuePair(Of Integer, Integer))(_degree)
            result.Sort(Function(a, b) a.Value.CompareTo(b.Value))
            Return result
        End Function

    End Class

End Namespace
