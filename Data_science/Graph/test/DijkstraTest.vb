#Region "Microsoft.VisualBasic::d5c1571d2a2e4babdccb6e8da2357382, Data_science\Graph\test\DijkstraTest.vb"

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

    '   Total Lines: 32
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 928 B


    ' Module DijkstraTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Graph

Module DijkstraTest

    Sub Main()
        Dim g As New Graph

        For i As Integer = 10 To 20
            g.AddVertex(label:=i)
        Next

        Dim weight As New Random

        g.AddEdge(0, 1, weight.NextDouble)
        g.AddEdge(1, 2, weight.NextDouble)
        g.AddEdge(2, 3, weight.NextDouble)
        g.AddEdge(3, 4, weight.NextDouble)
        g.AddEdge(4, 5, weight.NextDouble)
        g.AddEdge(5, 2, weight.NextDouble)
        g.AddEdge(5, 3, weight.NextDouble)
        g.AddEdge(4, 8, weight.NextDouble)
        g.AddEdge(8, 9, weight.NextDouble)
        g.AddEdge(3, 7, weight.NextDouble)
        g.AddEdge(7, 9, weight.NextDouble)
        g.AddEdge(9, 6, weight.NextDouble)

        Dim Dijkstra As New Dijkstra.DijkstraRouteFind(g)
        Dim route = Dijkstra.CalculateMinCost(g.Vertex(0))

        Pause()
    End Sub
End Module
