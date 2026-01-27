Public Module LLMs

    Dim ollama As Func(Of String, String)

    Public Sub HookOllama(chat As Func(Of String, String))
        ollama = chat
    End Sub

    ''' <summary>
    ''' A proxy function for LLMs talk in framework runtime
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <returns>
    ''' this proxy function will returns nothing if there is no ollama client is hooked
    ''' </returns>
    Public Function LLMsTalk(msg As String) As String
        If ollama Is Nothing Then
            Return Nothing
        Else
            Return ollama(msg)
        End If
    End Function
End Module
