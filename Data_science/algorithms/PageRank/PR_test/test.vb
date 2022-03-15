#Region "Microsoft.VisualBasic::efab0a3b492516d6b494387cb3a1e2f1, sciBASIC#\Data_science\algorithms\PageRank\PR_test\test.vb"

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

    '   Total Lines: 56
    '    Code Lines: 42
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 1.51 KB


    ' Module test
    ' 
    '     Sub: Main, Test_weightedPR, TestPageRank
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.PageRank
Imports Microsoft.VisualBasic.Serialization.JSON

Module test

    Sub Main()

        Call "PageRank:".__DEBUG_ECHO
        Call TestPageRank()
        Call "Weighted PageRank:".__DEBUG_ECHO
        Call Test_weightedPR()

        Pause()
    End Sub

    Sub TestPageRank()
        Dim g As New Graph

        For Each label In "ABCDEFGHIJK"
            Call g.AddVertex(CStr(label))
        Next

        Call g.AddEdges("B", {"C"})
        Call g.AddEdges("C", {"B"})
        Call g.AddEdges("D", {"A", "B"})
        Call g.AddEdges("E", {"D", "B", "F"})
        Call g.AddEdges("F", {"E", "B"})
        Call g.AddEdges("G", {"E", "B"})
        Call g.AddEdges("H", {"E", "B"})
        Call g.AddEdges("I", {"E", "B"})
        Call g.AddEdges("J", {"E"})
        Call g.AddEdges("K", {"E"})

        Dim matrix As New GraphMatrix(g)
        Dim pr As New PageRank(matrix)

        Dim result = matrix.TranslateVector(pr.ComputePageRank)

        Call result.GetJson(True).EchoLine
    End Sub

    Sub Test_weightedPR()
        Dim graph As New WeightedPRGraph

        graph.AddEdge(1, 2, 1.0)
        graph.AddEdge(1, 3, 2.0)
        graph.AddEdge(2, 3, 3.0)
        graph.AddEdge(2, 4, 4.0)
        graph.AddEdge(3, 1, 5.0)

        Dim ranks = graph.Rank(0.85, 0.000001)

        Call ranks.GetJson(True).EchoLine
    End Sub
End Module
