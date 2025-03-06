#Region "Microsoft.VisualBasic::889aab01d994c205b33faa318e84062e, Data\DataFrame\test\fileParser2.vb"

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

    '   Total Lines: 31
    '    Code Lines: 19 (61.29%)
    ' Comment Lines: 1 (3.23%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (35.48%)
    '     File Size: 856 B


    ' Module fileParser2
    ' 
    '     Sub: HeaderTest, Main, multipleLineRowtest1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile

Module fileParser2

    Sub Main()

        Call HeaderTest()

        ' Call multipleLineRowtest1()

        Dim df As DataFrameResolver = DataFrameResolver.Load("E:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\test\Food.csv", simpleRowIterators:=False)

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
