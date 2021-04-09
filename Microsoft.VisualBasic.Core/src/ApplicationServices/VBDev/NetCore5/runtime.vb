Namespace ApplicationServices.Development.NetCore5

    Public Class runtime

        Public Property assemblyVersion As String
        Public Property fileVersion As String

    End Class

    Public Class runtimeTarget : Inherits runtime

        Public Property rid As String
        Public Property assetType As String

    End Class

    Public Class library
        Public Property type As String
        Public Property serviceable As Boolean
        Public Property sha512 As String
        Public Property path As String
        Public Property hashPath As String
    End Class

    Public Class frameworkTarget

        Public Property name As String
        Public Property signature As String

        Public Overrides Function ToString() As String
            Return name
        End Function

    End Class

    Public Class compilationOptions

    End Class
End Namespace