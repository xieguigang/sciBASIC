Namespace Data.Trinity.NLP

    Public Interface ITokenCount
        Property Token As String
        Property Id As Integer
        Property Count As Integer
    End Interface

    Public Interface ILink
        Property source As Integer
        Property target As Integer
        Property Count As Integer
    End Interface
End Namespace