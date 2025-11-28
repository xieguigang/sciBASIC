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
