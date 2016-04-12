Namespace Text.Similarity

    ''' <summary>
    ''' 其他的匹配方法是基于字符的，这个是基于单词的比对
    ''' </summary>
    Public Module StatementMatches

        Public Function Match(s1 As String, s2 As String, Optional strict As Boolean = False) As DistResult
            Dim t1 As String() = s1.Split
            Dim t2 As String() = s2.Split
            Dim equals As Equals(Of String) = [If](Of Equals(Of String))(strict, AddressOf __equalsOrder, AddressOf __equalsIgnoredCase)
            Return LevenshteinDistance.ComputeDistance(t1, t2, equals, Function(s) s.FirstOrDefault)
        End Function

        Private Function __equalsOrder(s1 As String, s2 As String) As Boolean
            Return String.Equals(s1, s2, StringComparison.Ordinal)
        End Function

        Private Function __equalsIgnoredCase(s1 As String, s2 As String) As Boolean
            Return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase)
        End Function
    End Module
End Namespace