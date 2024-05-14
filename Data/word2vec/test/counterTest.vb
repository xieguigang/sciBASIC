#Region "Microsoft.VisualBasic::44e695ce6a6affa80a80a389617b4d77, Data\word2vec\test\counterTest.vb"

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

    '   Total Lines: 37
    '    Code Lines: 26
    ' Comment Lines: 1
    '   Blank Lines: 10
    '     File Size: 1.07 KB


    ' Module counterTest
    ' 
    '     Sub: bigramTest, Main, WordCountTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Serialization.JSON

Module counterTest

    Public Sub Main()
        Call bigramTest()
    End Sub

    Sub bigramTest()
        Dim text As String = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt".ReadAllText
        Dim bi = Bigram.ParseText(text).ToArray

        Call Console.WriteLine(bi.OrderByDescending(Function(a) a.count).ToArray.GetJson)

        Pause()
    End Sub

    Sub WordCountTest()
        Dim strKeys = New String() {"1", "2", "3", "1", "2", "1", "3", "3", "3", "1", "2"}
        Dim counter As New TokenCounter(Of String)()

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
