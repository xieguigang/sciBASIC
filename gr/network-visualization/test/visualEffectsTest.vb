#Region "Microsoft.VisualBasic::85b8293db6093a0d6e83562c56d3e3c2, sciBASIC#\gr\network-visualization\test\visualEffectsTest.vb"

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

    '   Total Lines: 31
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 1.12 KB


    ' Module visualEffectsTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Module visualEffectsTest
    Sub Main()
        Dim g As New NetworkGraph
        Dim nodes As New Dictionary(Of String, visualize.Network.Graph.Node)

        For i As Integer = 0 To 10
            Call nodes.Add(i, g.AddNode(New visualize.Network.Graph.Node With {.ID = i, .Label = "#" & i, .Data = New NodeData With {.Properties = New Dictionary(Of String, String)}}))
        Next

        Call g.AddEdge(0, 1)
        Call g.AddEdge(1, 2)
        Call g.AddEdge(2, 3)
        Call g.AddEdge(2, 4)
        Call g.AddEdge(2, 5)
        Call g.AddEdge(7, 8)
        Call g.AddEdge(8, 9)
        Call g.AddEdge(6, 7)
        Call g.AddEdge(7, 0)
        Call g.AddEdge(7, 10)

        Call g.doForceLayout
        Call g.ComputeNodeDegrees
        Call g.DrawImage("2000,2000").Save("./test.png")
    End Sub
End Module
