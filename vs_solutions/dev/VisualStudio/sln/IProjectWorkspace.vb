Namespace sln

    Public Interface IProjectWorkspace

        ReadOnly Property Name As String
        Function GetCompileFiles() As IEnumerable(Of String)

    End Interface
End Namespace