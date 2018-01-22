#Region "Microsoft.VisualBasic::ac64527fc6947ad7d75d2daaaecd017f, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\test\SCStest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.Text

Module SCStest

    Sub Main()


        SAMTest()

        Dim path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input.txt"
        Dim SCS = ShortestCommonSuperString(path.ReadAllLines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output.txt".OpenWriter
            Call path.ReadAllLines.TableView(SCS, txt)
        End Using

        path$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\input2.txt"
        SCS = ShortestCommonSuperString(path.ReadAllLines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\output2.txt".OpenWriter
            Call path.ReadAllLines.TableView(SCS, txt)
        End Using

        Pause()
    End Sub

    Sub SAMTest()
        Dim lines = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\EF4.sam" _
            .ReadAllLines _
            .Select(Function(s) s.Split(ASCII.TAB)(9)) _
            .ToArray

        Dim SCS = ShortestCommonSuperString(lines.AsList)

        Using txt As StreamWriter = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\SAM_output.txt".OpenWriter
            Call lines.TableView(SCS, txt)
        End Using

        'Dim assembly = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\SCS\SAM_output.txt".ReadAllLines
        'Dim coverageValue = Coverage(assembly)


        Pause()
    End Sub
End Module
