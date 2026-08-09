Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.CodeDOM
Imports Microsoft.VisualBasic.Linq

Module vsTest

    Sub Main()
        Dim sln As sln.Solution = Solution.Load("G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\dev\VisualStudio.sln")
        Dim ws As SolutionWorkspace = sln.LoadWorkspace
        Dim symbol As LanguageSymbolType = ws("test.vsTest.Main")

        For Each [partial] In symbol.Source.AsEnumerable
            Call Console.WriteLine([partial].CodeBlock)
        Next

        Pause()
    End Sub
End Module
