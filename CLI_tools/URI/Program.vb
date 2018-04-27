Imports Microsoft.VisualBasic.Net.Http

Module Program

    Sub Main()
        Dim file$ = App.CommandLine.Name
        Dim uri As New DataURI(file)

        Call Console.Write(uri.ToString)
    End Sub
End Module
