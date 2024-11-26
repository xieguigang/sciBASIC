#Region "Microsoft.VisualBasic::9461b05ebffb2d7b809583469b447b8b, Data\DataFrame\test\fileParser2.vb"

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

    '   Total Lines: 21
    '    Code Lines: 12 (57.14%)
    ' Comment Lines: 1 (4.76%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (38.10%)
    '     File Size: 558 B


    ' Module fileParser2
    ' 
    '     Sub: Main, multipleLineRowtest1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO

Module fileParser2

    Sub Main()

        Call HeaderTest()

        ' Call multipleLineRowtest1()

        Dim df As DataFrame = DataFrame.Load("E:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\test\Food.csv", simpleRowIterators:=False)

        Pause()
    End Sub

    Sub HeaderTest()
        Dim headers As String() = {"uid", "time", "url", "ip", "ua"}
        Dim score = GetType(Visitor).HeaderMatchScore(headers)

        Pause()
    End Sub

    Sub multipleLineRowtest1()
        Dim reader As New RowIterator("E:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\test\single_row.csv".OpenReadonly)
        Dim r As RowObject() = reader.GetRows.ToArray

        Pause()
    End Sub

End Module
