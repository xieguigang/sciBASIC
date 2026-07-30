Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VersionControl

Public Module gitTest

    Sub Run()
        Dim diff = Git.diff.GetDiff("G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\dev\VisualStudio")

        Pause()
    End Sub
End Module
