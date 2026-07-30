Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VersionControl
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module gitTest

    Sub Run()
        Dim diff = Git.diff.GetDiff("G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\dev")

        Call Console.WriteLine(diff.GetJson)
        Call diff.GetJson.SaveTo("G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\dev\data\git_diff.json")

        Pause()
    End Sub
End Module
