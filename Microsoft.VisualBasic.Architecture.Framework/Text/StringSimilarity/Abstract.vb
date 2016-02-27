Namespace Text.Similarity

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    ''' 
    Public Delegate Function Similarity(s1 As String, s2 As String) As Single

    ''' <summary>
    ''' Summary description for IEditDistance.
    ''' </summary>
    Interface ISimilarity
        Function GetSimilarity(string1 As String, string2 As String) As Single
    End Interface
End Namespace