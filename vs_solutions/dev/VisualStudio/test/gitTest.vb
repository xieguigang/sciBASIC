#Region "Microsoft.VisualBasic::b8b974ef4a520e9aa999706b7389e67a, vs_solutions\dev\VisualStudio\test\gitTest.vb"

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

    '   Total Lines: 14
    '    Code Lines: 10 (71.43%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (28.57%)
    '     File Size: 475 B


    ' Module gitTest
    ' 
    '     Sub: Run
    ' 
    ' /********************************************************************************/

#End Region

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
