#Region "Microsoft.VisualBasic::f02374e201bdff19fead4d9efa7c5450, gr\network-visualization\Network.IO.Extensions\Gephi\GML.vb"

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

    '   Total Lines: 95
    '    Code Lines: 75 (78.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (21.05%)
    '     File Size: 3.39 KB


    '     Class GephiML
    ' 
    '         Properties: Creator, directed, edge, node
    ' 
    '         Function: (+2 Overloads) BuildGML
    ' 
    '     Class edge
    ' 
    '         Properties: fill, id, source, target, value
    ' 
    '     Class node
    ' 
    '         Properties: graphics, id, label
    ' 
    '     Class graphics
    ' 
    '         Properties: d, fill, h, w, x
    '                     y, z
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Gephi

    Public Class GephiML

        Public Property Creator As String = "scibasic.net"
        Public Property directed As Integer = 1
        Public Property node As node()
        Public Property edge As edge()

        Public Shared Function BuildGML(g As NetworkGraph) As GephiML
            Dim vertex = g.vertex.Select(Function(v) New node With {.id = v.ID, .label = v.text, .graphics = New graphics With {.w = v.data.size(0), .h = .w, .d = .w, .fill = "#000000"}}).ToArray
            Dim links = g.graphEdges.Select(Function(e, i) New edge With {.id = i, .fill = "#000000", .source = e.U.ID, .target = e.V.ID, .value = e.weight}).ToArray

            Return New GephiML With {
                .node = vertex,
                .edge = links
            }
        End Function

        Public Function BuildGML() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine("graph")
            Call sb.AppendLine("[")
            Call sb.AppendLine($"Creator ""{Creator}""")
            Call sb.AppendLine($"directed {directed}")

            For Each node As node In Me.node
                Call sb.AppendLine("node")
                Call sb.AppendLine("[")
                Call sb.AppendLine($"id {node.id}")
                Call sb.AppendLine($"label ""{node.label}""")
                Call sb.AppendLine("graphics")
                Call sb.AppendLine("[")
                Call sb.AppendLine($"x {node.graphics.x}")
                Call sb.AppendLine($"y {node.graphics.y}")
                Call sb.AppendLine($"z {node.graphics.z}")
                Call sb.AppendLine($"w {node.graphics.w}")
                Call sb.AppendLine($"h {node.graphics.h}")
                Call sb.AppendLine($"d {node.graphics.d}")
                Call sb.AppendLine($"fill ""{node.graphics.fill}""")
                Call sb.AppendLine("]")
                Call sb.AppendLine("]")
            Next
            For Each edge As edge In Me.edge
                Call sb.AppendLine("edge")
                Call sb.AppendLine("[")
                Call sb.AppendLine($"id {edge.id}")
                Call sb.AppendLine($"source {edge.source}")
                Call sb.AppendLine($"target {edge.target}")
                Call sb.AppendLine($"value {edge.value}")
                Call sb.AppendLine($"fill ""{edge.id}""")
                Call sb.AppendLine("]")
            Next

            Call sb.AppendLine("]")

            Return sb.ToString
        End Function

    End Class

    Public Class edge

        Public Property id As Integer
        Public Property source As Integer
        Public Property target As Integer
        Public Property value As Double
        Public Property fill As String

    End Class

    Public Class node

        Public Property id As Integer
        Public Property label As String
        Public Property graphics As graphics

    End Class

    Public Class graphics

        Public Property x As Double
        Public Property y As Double
        Public Property z As Double
        Public Property w As Double
        Public Property h As Double
        Public Property d As Double
        Public Property fill As String

    End Class
End Namespace
