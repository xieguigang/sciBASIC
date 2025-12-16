#Region "Microsoft.VisualBasic::83ef6e9a2f3ad63ef048251adcff0ac9, gr\network-visualization\Network.IO.Extensions\SigmaJs.vb"

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

    '   Total Lines: 52
    '    Code Lines: 41 (78.85%)
    ' Comment Lines: 3 (5.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (15.38%)
    '     File Size: 1.81 KB


    ' Module SigmaJs
    ' 
    '     Function: AsGraphology, graphologyEdges, graphologyNodes
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Imaging

''' <summary>
''' Create json for sigma.js
''' </summary>
Public Module SigmaJs

    <Extension>
    Public Function AsGraphology(g As NetworkGraph) As graphology.graph
        Return New graphology.graph(g.graphologyNodes, g.graphologyEdges)
    End Function

    <Extension>
    Private Iterator Function graphologyNodes(g As NetworkGraph) As IEnumerable(Of graphology.node)
        For Each v As Node In g.vertex
            Dim fill As SolidBrush = TryCast(v.data.color, SolidBrush)
            Dim color As Color = If(fill Is Nothing, Color.Brown, fill.Color)

            Yield New graphology.node With {
                .id = v.label,
                .label = If(v.data.label, v.label),
                .color = color.ToHtmlColor,
                .size = v.data.size.ElementAtOrDefault(0, 1),
                .x = v.data.initialPostion.x,
                .y = v.data.initialPostion.y
            }
        Next
    End Function

    <Extension>
    Private Iterator Function graphologyEdges(g As NetworkGraph) As IEnumerable(Of graphology.edge)
        Dim i As i32 = 1

        For Each link As Edge In g.graphEdges
            Dim pen As Pen = link.data.style
            Dim color As Color = If(pen Is Nothing, Color.LightGray, pen.Color)

            Yield New graphology.edge With {
                .id = $"e{++i}",
                .source = link.U.label,
                .target = link.V.label,
                .color = color.ToHtmlColor,
                .size = If(pen Is Nothing, 1, pen.Width)
            }
        Next
    End Function

End Module
