Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Language.UnixBash

Module terminalTest

    Sub Main()
        Dim shell As New Shell(PS1.Fedora12, AddressOf Console.WriteLine)

        shell.autoCompleteCandidates.Add("file.copy", "file.delete", "file.cache", "file.rename")
        shell.Run()
    End Sub
End Module
