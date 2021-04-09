Namespace ApplicationServices.Development.NetCore5

    Public Class target

        Public Property dependencies As Dictionary(Of String, String)
        Public Property runtime As Dictionary(Of String, runtime)
        Public Property runtimeTargets As Dictionary(Of String, runtimeTarget)

        Public ReadOnly Property LibraryFile As String
            Get
                If runtime.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return runtime.Keys _
                        .Where(Function(fileName)
                                   ' skip of the system dll files
                                   Return (Not fileName.Contains("/")) AndAlso fileName.ExtensionSuffix("dll")
                               End Function) _
                        .FirstOrDefault
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return LibraryFile
        End Function

    End Class

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