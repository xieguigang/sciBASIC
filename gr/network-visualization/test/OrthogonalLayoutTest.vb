#Region "Microsoft.VisualBasic::24cc48f16d141a8b041740efc0966ca5, sciBASIC#\gr\network-visualization\test\OrthogonalLayoutTest.vb"

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

    '   Total Lines: 35
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.26 KB


    ' Module OrthogonalLayoutTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports inode = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module OrthogonalLayoutTest

    Sub Main()
        Dim g As New NetworkGraph

        For Each label As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "single"}
            Call g.AddNode(New inode With {.label = label, .data = New NodeData With {.initialPostion = New FDGVector2, .size = {5, 5}}})
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

        Call Orthogonal.Algorithm.DoLayout(g, New Size(100, 60), 10)
        Call NetworkVisualizer.DrawImage(g, "3000,3000").Save("./Orthogonal.png")

        Pause()
    End Sub
End Module
