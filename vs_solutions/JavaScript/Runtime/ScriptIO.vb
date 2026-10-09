Namespace Runtime

    ''' <summary>Sink for console.log etc.; records lines for test/e2e comparison.</summary>
    Public MustInherit Class ScriptIO

        Public MustOverride Sub WriteLine(text As String)

    End Class

    Public Class LogTextIO : Inherits ScriptIO

        Public ReadOnly Property Lines As New List(Of String)

        Public Overrides Sub WriteLine(text As String)
            Lines.Add(text)
        End Sub

    End Class

    Public Class ConsoleIO : Inherits ScriptIO

        Public Overrides Sub WriteLine(text As String)
            Call Console.WriteLine(text)
        End Sub
    End Class
End Namespace