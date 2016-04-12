Namespace Text.Similarity

    ''' <summary>
    ''' 其他的匹配方法是基于字符的，这个是基于单词的比对
    ''' </summary>
    Public Module StatementMatches

        ''' <summary>
        ''' 基于字符的比较
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Function Match(s1 As String, s2 As String, Optional strict As Boolean = False) As DistResult
            Dim t1 As String() = s1.Split
            Dim t2 As String() = s2.Split
            Dim equals As Equals(Of String) = [If](Of Equals(Of String))(strict, AddressOf __equalsOrder, AddressOf __equalsIgnoredCase)
            Return LevenshteinDistance.ComputeDistance(t1, t2, equals, Function(s) s.FirstOrDefault)
        End Function

        ''' <summary>
        ''' 基于单词的比较
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <param name="strict"></param>
        ''' <param name="cut"></param>
        ''' <returns></returns>
        Public Function MatchFuzzy(s1 As String, s2 As String, Optional strict As Boolean = False, Optional cut As Double = 0.8) As DistResult
            Dim t1 As String() = s1.Split
            Dim t2 As String() = s2.Split
            Dim helper As New __tokenEquals(cut)
            Dim equals As Equals(Of String) = [If](Of Equals(Of String))(strict, AddressOf helper.__equalsOrder, AddressOf helper.__equalsIgnoredCase)
            Return LevenshteinDistance.ComputeDistance(t1, t2, equals, Function(s) s.FirstOrDefault)
        End Function

        ''' <summary>
        ''' 字符串模糊等价
        ''' </summary>
        Private Class __tokenEquals

            ReadOnly __cut As Double

            Sub New(cut As Double)
                __cut = cut
            End Sub

            Public Overloads Function __equalsOrder(s1 As String, s2 As String) As Boolean
                Dim edits As DistResult = LevenshteinDistance.ComputeDistance(s1.ToArray, s2.ToArray, AddressOf __charEquals, Function(c) c)
                If edits Is Nothing Then
                    Return False
                Else
                    Return edits.MatchSimilarity >= __cut
                End If
            End Function

            Public Overloads Function __equalsIgnoredCase(s1 As String, s2 As String) As Boolean
                Dim edits As DistResult = LevenshteinDistance.ComputeDistance(s1.ToArray, s2.ToArray, AddressOf __charEqualsIgnoredCase, Function(c) c)
                If edits Is Nothing Then
                    Return False
                Else
                    Return edits.MatchSimilarity >= __cut
                End If
            End Function

            Private Shared Function __charEquals(a As Char, b As Char) As Boolean
                Return a = b
            End Function

            Private Shared Function __charEqualsIgnoredCase(a As Char, b As Char) As Boolean
                Return Char.ToLower(a) = Char.ToLower(b)
            End Function
        End Class

        Private Function __equalsOrder(s1 As String, s2 As String) As Boolean
            Return String.Equals(s1, s2, StringComparison.Ordinal)
        End Function

        Private Function __equalsIgnoredCase(s1 As String, s2 As String) As Boolean
            Return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase)
        End Function
    End Module
End Namespace