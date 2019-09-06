Namespace DOI

    Public Class Response
        Public Property responseCode As Integer
        Public Property handle As String
        Public Property values As DoiValue()
        Public Property message As String
    End Class

    Public Class DoiValue
        Public Property index As Integer
        Public Property type As String
        Public Property data
        Public Property ttl As Integer
        Public Property timestamp As String
    End Class

    Public Class DataValue
        Public Property format As String
        Public Property value As Object
    End Class

    Public Class HS_ADMIN
        Public Property handle As String
        Public Property index As Integer
        Public Property permissions As String
    End Class
End Namespace