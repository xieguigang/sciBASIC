#Region "Microsoft.VisualBasic::d15f44e73daec06469a603dda3199aa6, sciBASIC#\Data\word2vec\test\counterTest.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15
    ' Comment Lines: 1
    '   Blank Lines: 6
    '     File Size: 609.00 B


    ' Module counterTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.Word2Vec.utils

Module counterTest

    Public Sub Main()
        Dim strKeys = New String() {"1", "2", "3", "1", "2", "1", "3", "3", "3", "1", "2"}
        Dim counter As Counter(Of String) = New Counter(Of String)()

        For Each strKey In strKeys
            counter.add(strKey)
        Next

        For Each strKey As String In counter.keySet
            Console.WriteLine(strKey & " : " & counter.get(strKey))
        Next

        Console.WriteLine(counter.get("9"))
        '        System.out.println(Long.MAX_VALUE);

        Pause()
    End Sub
End Module
