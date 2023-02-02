Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Structure ConsoleOutLine
        Public ReadOnly Property [Error] As Boolean
        Public ReadOnly Property Line As String

        Public Sub New(line As String, Optional [error] As Boolean = False)
            Me.Error = [error]
            Me.Line = line
        End Sub
    End Structure
End Namespace
