Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

#If NET48 Then
#Else
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
#End If

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
            Dim color As Color = TryCast(v.data.color, SolidBrush)?.Color

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
            Dim color As Color = pen?.Color

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
