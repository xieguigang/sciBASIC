Imports Microsoft.VisualBasic.Language.UnixBash

Module Module1

    Sub Main()

        Call App.Command.__DEBUG_ECHO

        Dim ps1 As New PS1("[\u@\h \W \A #\#]\$ ")

        For i As Integer = 0 To 100
            Call $"{ps1.ToString}  ---> {i}%".__DEBUG_ECHO
            Call Threading.Thread.Sleep(300)
        Next
    End Sub

End Module
