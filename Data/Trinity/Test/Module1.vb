#Region "Microsoft.VisualBasic::0414c1f8f64a4775be05bc560774f8ae, Data\Trinity\Test\Module1.vb"

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

    '   Total Lines: 38
    '    Code Lines: 22
    ' Comment Lines: 6
    '   Blank Lines: 10
    '     File Size: 1.32 KB


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.PageRank
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()
        Dim s = "
This module implements TextRank, an unsupervised keyword
significance scoring algorithm. TextRank builds a weighted
graph representation Of a document Using words As nodes
And coocurrence frequencies between pairs of words as edge 
weights.It then applies PageRank to this graph, And
treats the PageRank score of each word as its significance.
The original research paper proposing this algorithm Is
available here"


        '        s$ = "the important pagerank. show on pagerank. have significance pagerank. implements pagerank algorithm. textrank base on pagerank."

        Dim ps = TextRank.Sentences(s.TrimNewLine)
        Dim g As GraphMatrix = ps.TextRankGraph
        Dim pr As New PageRank(g)
        Dim result = g.TranslateVector(pr.ComputePageRank, True)

        Call result.GetJson(True).EchoLine

        'Dim net = g.GetNetwork

        'For Each node In net.Nodes
        '    node.Properties.Add("PageRank", result(node.ID))
        'Next

        'Call net.Save("G:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\")

        Pause()
    End Sub
End Module
