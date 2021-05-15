''' <summary>
''' the object model of a query
''' </summary>
Public Class Query

    ''' <summary>
    ''' the query name
    ''' </summary>
    ''' <returns></returns>
    Public Property name As String
    Public Property members As Query()
    Public Property parser As Parse

    Public Overrides Function ToString() As String
        Return name
    End Function

End Class

