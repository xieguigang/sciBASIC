Namespace Text.Levenshtein

    ''' <summary>
    ''' The string like helper
    ''' </summary>
    Public Structure LevenshteinString

        Private _string$
        Private _chars%()

        Public Overrides Function ToString() As String
            Return _string
        End Function

        Public Shared Operator Like(s$, subject As LevenshteinString) As DistResult
            Return Levenshtein.ComputeDistance(
                s.CharCodes,
                subject._chars,
                Function(a, b) a = b,
                AddressOf ChrW)
        End Operator

        Public Shared Operator Like(query As LevenshteinString, s$) As DistResult
            Return Levenshtein.ComputeDistance(
                query._chars,
                s.CharCodes,
                Function(a, b) a = b,
                AddressOf ChrW)
        End Operator

        Public Shared Operator Like(query As LevenshteinString, subject As LevenshteinString) As DistResult
            Return Levenshtein.ComputeDistance(
                query._chars,
                subject._chars,
                Function(a, b) a = b,
                AddressOf ChrW)
        End Operator

        Public Shared Narrowing Operator CType(s As LevenshteinString) As String
            Return s._string
        End Operator

        Public Shared Widening Operator CType(s$) As LevenshteinString
            Return New LevenshteinString With {
                ._string = s,
                ._chars = s.Select(AddressOf AscW).ToArray
            }
        End Operator
    End Structure
End Namespace