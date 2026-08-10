#Region "Microsoft.VisualBasic::ac5a7200ef51c66b2e5f149cbcba2e15, vs_solutions\dev\VisualStudio\test\vsTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 19
    '    Code Lines: 15 (78.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 739 B


    ' Module vsTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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

