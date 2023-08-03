Imports Microsoft.VisualBasic.Linq

Namespace Model

    Public Class Counter

        Public Property token As String

        Public ReadOnly Property nchar As Integer
            Get
                Return token.Length
            End Get
        End Property

        Public Property total As Integer
        Public Property paragraph As Integer
        Public Property sentences As Integer

        Public Shared Iterator Function Count(par As Paragraph) As IEnumerable(Of Counter)
            Dim tokens = par.sentences _
                .SafeQuery _
                .Select(Function(s) s.segments.SafeQuery.Select(Function(c) c.tokens.Select(Function(t) (t, c, s)))) _
                .IteratesALL _
                .IteratesALL _
                .GroupBy(Function(a) a.t.ToLower) _
                .ToArray

            For Each item In tokens
                Yield New Counter With {
                    .token = item.Key,
                    .total = item.Count,
                    .sentences = item.Select(Function(a) a.s).Distinct.Count,
                    .paragraph = 1
                }
            Next
        End Function

        Public Shared Iterator Function Count(par As IEnumerable(Of Paragraph)) As IEnumerable(Of Counter)
            Dim allTokens As New List(Of (par As Integer, counts As Counter))

            For Each p As Paragraph In par.SafeQuery
                Dim hc As Integer = p.GetHashCode

                For Each t As Counter In Count(p)
                    allTokens.Add((hc, t))
                Next
            Next

            Dim tokens = allTokens _
                .GroupBy(Function(t) t.counts.token.ToLower) _
                .ToArray

            For Each item In tokens
                Yield New Counter With {
                    .token = item.Key,
                    .paragraph = item.Select(Function(a) a.par).Distinct.Count,
                    .sentences = item.Select(Function(a) a.counts.sentences).Sum,
                    .total = item.Select(Function(a) a.counts.total).Sum
                }
            Next
        End Function
    End Class
End Namespace