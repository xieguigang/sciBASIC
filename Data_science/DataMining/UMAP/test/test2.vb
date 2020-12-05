#Region "Microsoft.VisualBasic::95eda058d22267591944b2430b8fba8e, Data_science\DataMining\UMAP\test\test2.vb"

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

    ' Module test2
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports test.Tester

Module test2

    Sub Main()
        Dim data As New List(Of LabelledVector)

        For Each row In "E:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\data.csv".IterateAllLines.SeqIterator
            data.Add(New LabelledVector With {.UID = row.i, .Vector = row.value.Split(","c).Select(Function(str) CSng(Val(str))).ToArray})
        Next

        Call Tester.Program.RunTest(data.ToArray)

        Pause()
    End Sub
End Module

