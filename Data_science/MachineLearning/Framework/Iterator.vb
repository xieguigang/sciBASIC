Imports Microsoft.VisualBasic.Language

Namespace Framework

    Public Class Iterator

        Public Sub Run(Optional iterations% = 10 * 10000)
            Dim i As int = 0

            Do While ++i <= iterations

            Loop
        End Sub
    End Class
End Namespace