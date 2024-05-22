#Region "Microsoft.VisualBasic::1315b2ebc0434929d7963011e68a9241, Data_science\DataMining\DynamicProgramming\test\Module1.vb"

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

    '   Total Lines: 44
    '    Code Lines: 16 (36.36%)
    ' Comment Lines: 18 (40.91%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (22.73%)
    '     File Size: 1.67 KB


    ' Module Module1
    ' 
    '     Sub: Main, scoreTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch

Module Module1
    Dim q = "AGTCGCCCCGTCCC"
    Dim S = "AGTCGCCCCGTCGG"
    Dim s2 = "AGTCGCCCCGTCGGAAAAAAAAA"
    Dim q1 = "GTCCC"
    Dim q2 = "AGTCGCTCCC"
    Dim q3 = "AGTCGCCCCCCC"

    Sub Main()
        Call scoreTest()
    End Sub

    Sub scoreTest()
        'Dim nw As New NeedlemanWunsch(Of Char)(q, q, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q1, q1, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l1 = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q2, q2, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l2 = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q3, q3, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim l3 = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q, S, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim qs = nw.PopulateAlignments.ToArray

        'nw = New NeedlemanWunsch(Of Char)(q, s2, Function(x, y) Char.ToUpper(x) = Char.ToUpper(y), "-"c, Function(x) x)
        'Call nw.compute()
        'Dim qs2 = nw.PopulateAlignments.ToArray

        Pause()
    End Sub

End Module
