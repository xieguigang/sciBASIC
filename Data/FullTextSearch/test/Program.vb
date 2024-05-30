#Region "Microsoft.VisualBasic::e80732510250e0f454fb2ec4c9ba4176, Data\FullTextSearch\test\Program.vb"

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

    '   Total Lines: 41
    '    Code Lines: 27 (65.85%)
    ' Comment Lines: 3 (7.32%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (26.83%)
    '     File Size: 1.19 KB


    ' Module Program
    ' 
    '     Sub: Main, test_create, test_read
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.Data.FullTextSearch
Imports Microsoft.VisualBasic.Linq

Module Program

    ReadOnly demo_repo As String = $"{App.HOME}/demo_data/"

    Sub Main(args As String())
        Call test_create()
        Call test_read()
    End Sub

    Sub test_read()
        Dim index As New FTSEngine(demo_repo)

        For Each hit As SeqValue(Of String) In index.Search("princess  walking")
            Call Console.WriteLine(hit.value)
        Next
    End Sub

    Sub test_create()
        Dim doc_1 = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Beauty_and_the_Beast.txt".ReadAllLines
        Dim doc_2 = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt".ReadAllLines

        Dim index As New FTSEngine(demo_repo)

        Call index.Indexing(doc_1)
        Call index.Indexing(doc_2)

        'For Each hit In index.Search("baby girl")
        '    Call Console.WriteLine(hit.value)
        'Next

        For Each hit As SeqValue(Of String) In index.Search("princess  walking")
            Call Console.WriteLine(hit.value)
        Next

        Call index.Dispose()
    End Sub
End Module
