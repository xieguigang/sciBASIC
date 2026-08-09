Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln

Module vsTest

    Sub Main()
        Dim sln As sln.Solution = Solution.Load("G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\dev\VisualStudio.sln")
        Dim ws = sln.LoadWorkspace


        Pause()
    End Sub
End Module
