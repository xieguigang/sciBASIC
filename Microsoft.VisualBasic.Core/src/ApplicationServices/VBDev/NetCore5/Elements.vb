Namespace ApplicationServices.Development.NetCore5

    Public Class target
        Public Property dependencies As Dictionary(Of String, String)
    End Class

    Public Class library
        Public Property type As String
        Public Property serviceable As Boolean
        Public Property sha512 As String
        Public Property path As String
        Public Property hashPath As String
    End Class

    Public Class runtimeTarget

        Public Property name As String
        Public Property signature As String

    End Class

    Public Class compilationOptions

    End Class
End Namespace