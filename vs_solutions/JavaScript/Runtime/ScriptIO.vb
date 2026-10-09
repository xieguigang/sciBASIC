Namespace Runtime

    ''' <summary>Sink for console.log etc.; records lines for test/e2e comparison.</summary>
    Public Class ScriptIO
        Public ReadOnly Property Lines As New List(Of String)

        Public Overridable Sub WriteLine(text As String)
            Lines.Add(text)
        End Sub
    End Class
End Namespace