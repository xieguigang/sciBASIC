#Region "Microsoft.VisualBasic::ea94fca90af73c644ef18dcfc60c6886, gr\network-visualization\Datavisualization.Network\test\testRouter.vb"

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

    '   Total Lines: 28
    '    Code Lines: 22 (78.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 896 B


    ' Module testRouter
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module testRouter

    Sub Main()
        Dim g As New NetworkGraph

        For Each c As String In {"a", "b", "c", "d", "e", "f", "g", "h"}
            Call g.CreateNode(c)
        Next

        Call g.CreateEdge("a", "b", 1)
        Call g.CreateEdge("b", "c", 1)
        Call g.CreateEdge("c", "e", 1)
        Call g.CreateEdge("d", "e", 1)
        Call g.CreateEdge("e", "g", 1)
        Call g.CreateEdge("g", "f", 1)
        Call g.CreateEdge("f", "a", 1)
        Call g.CreateEdge("f", "h", 1)
        Call g.CreateEdge("h", "a", 1)

        Dim router As DijkstraRouter = DijkstraRouter.FromNetwork(g)
        Dim path = router.CalculateMinCost(router.GetLocation("a"), router.GetLocation("h"))

        Pause()
    End Sub
End Module

