#Region "Microsoft.VisualBasic::19550160d12739be889a364350134ee8, Microsoft.VisualBasic.Core\test\test\indexTest.vb"

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

    '   Total Lines: 54
    '    Code Lines: 40 (74.07%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (25.93%)
    '     File Size: 1.63 KB


    ' Module indexTest
    ' 
    '     Sub: Main1, stringTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.Trinity

Module indexTest

    Sub Main1()
        Call stringTest()

        Dim index As New WordSimilarityIndex(Of String)

        For Each item As String In 5000.SeqRandom.Select(Function(l) RandomASCIIString(20, True))
            If Not index.HaveKey(item) Then
                Call index.AddTerm(item, item)
            End If
        Next

        Dim result = index.FindMatches("Aaaaaaaaaaaaaaaaaaaa").ToArray

        Pause()
    End Sub

    Sub stringTest()
        Dim key As String = "abc"
        Dim keyRegexp As New Regex("[a][b][c]", RegexOptions.Singleline Or RegexOptions.Compiled)
        Dim target As String = "sdfm,sdfjklsdfsabcklfs"
        Dim loops As Integer = 100000000

        Call BENCHMARK(Sub()
                           For i As Integer = 0 To loops
                               If target.Contains(key) Then

                               End If
                           Next
                       End Sub)

        Call BENCHMARK(Sub()
                           For i As Integer = 0 To loops
                               If target.IndexOf(key) > -1 Then

                               End If
                           Next
                       End Sub)

        Call BENCHMARK(Sub()
                           For i As Integer = 0 To loops
                               If keyRegexp.Match(key).Success Then

                               End If
                           Next
                       End Sub)

        Pause()
    End Sub
End Module
