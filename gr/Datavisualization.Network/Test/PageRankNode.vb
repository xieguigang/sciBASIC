Imports System

Namespace pagerank

	Public Class PageRankNode
		Implements IComparable(Of PageRankNode)

        Public Sub New(identifier As String, rank As Double)
            Me.Identifier = identifier
            Me.Rank = rank
        End Sub

        Public Overridable Property Identifier As String

        Public Overridable Property Rank As Double

        Public Overridable Function compareTo(other As PageRankNode) As Integer Implements IComparable(Of PageRankNode).CompareTo
            Return If(Rank > other.Rank, -1, If(Rank < other.Rank, 1, 0))
        End Function
    End Class

End Namespace