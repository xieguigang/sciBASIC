Imports Microsoft.VisualBasic.Language.UnixBash

Namespace Terminal

    Public Class Shell

        ReadOnly __ps1 As PS1
        ReadOnly __shell As Action(Of String)

        Public Property Quite As String = ":q"
        Public Property History As String = ":h"

        Sub New(ps1 As PS1, exec As Action(Of String))
            __ps1 = ps1
            __shell = exec
        End Sub

        Public Sub Run()
            Dim cli As String

            Do While True
                Call Console.Write(__ps1.ToString)

                cli = Console.ReadLine

                If String.Equals(cli, Quite) Then
                    Exit Do
                End If

                Call __shell(cli)
            Loop
        End Sub
    End Class
End Namespace