﻿#Region "Microsoft.VisualBasic::09b6db20ed99b91932bdc8e6e828c08f, G:/GCModeller/src/runtime/sciBASIC#/gr/network-visualization/Datavisualization.Network//test/OrthogonalTest.vb"

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

    '   Total Lines: 39
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.31 KB


    ' Module OrthogonalTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module OrthogonalTest

    Sub Main()
        Dim g As New NetworkGraph

        For Each label As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "single"}
            Call g.AddNode(New node With {.label = label, .data = New NodeData With {.initialPostion = New FDGVector2, .size = {5, 5}}})
        Next

        Call g.AddEdge("A", "B")
        Call g.AddEdge("B", "C")
        Call g.AddEdge("C", "D")
        Call g.AddEdge("D", "E")
        Call g.AddEdge("C", "E")
        Call g.AddEdge("A", "E")
        Call g.AddEdge("A", "I")
        Call g.AddEdge("A", "J")
        Call g.AddEdge("J", "K")
        Call g.AddEdge("K", "H")
        Call g.AddEdge("F", "G")
        Call g.AddEdge("B", "F")
        Call g.AddEdge("G", "K")

        Call Orthogonal.Algorithm.DoLayout(g, New Size(10, 10), 10)

        Dim result = g.Tabular()

        Call result.Save("./OrthogonalTest")

        Pause()
    End Sub
End Module
