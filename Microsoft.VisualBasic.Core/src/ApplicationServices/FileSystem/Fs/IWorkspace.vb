Namespace ApplicationServices

    ''' <summary>
    ''' A abstract reference to a workdir
    ''' </summary>
    Public Interface IWorkspace

        ''' <summary>
        ''' the workdir path of current workspace object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Workspace As String

    End Interface
End Namespace