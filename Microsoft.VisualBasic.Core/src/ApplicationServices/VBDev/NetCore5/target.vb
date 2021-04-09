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

End Namespace