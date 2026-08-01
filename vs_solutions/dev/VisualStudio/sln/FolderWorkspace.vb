Namespace sln

    Public Class FolderWorkspace : Implements IProjectWorkspace

        ''' <summary>
        ''' the name of the folder
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
            Get
                Return Path.BaseName
            End Get
        End Property

        ''' <summary>
        ''' the folder full path
        ''' </summary>
        ''' <returns></returns>
        Public Property Path As String

        Public Function GetCompileFiles() As IEnumerable(Of String) Implements IProjectWorkspace.GetCompileFiles

        End Function
    End Class
End Namespace