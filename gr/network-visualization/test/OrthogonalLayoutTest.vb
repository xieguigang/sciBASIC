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
Imports System.IO
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.optimization
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports inode = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module OrthogonalLayoutTest

    Sub Main()
        Call test2()
    End Sub

    Sub test1()
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

        Call Orthogonal.DoLayout(g)
        Call NetworkVisualizer.DrawImage(g, "3000,3000").Save("./Orthogonal.png")

        Pause()
    End Sub

    Private Sub saveEmbedding(oe As OrthographicEmbeddingResult)
        Dim fw As StreamWriter = Console.Out

        For i = 0 To oe.nodeIndexes.Length - 1
            For j = 0 To oe.nodeIndexes.Length - 1
                If oe.edges(i)(j) OrElse oe.edges(j)(i) Then
                    fw.Write("1" & ", ")
                Else
                    fw.Write("0" & ", ")
                End If
            Next
            fw.Write(vbLf)
        Next
        For i = 0 To oe.nodeIndexes.Length - 1
            fw.Write(i.ToString() & ", " & oe.nodeIndexes(i).ToString() & ", " & oe.x(i).ToString() & ", " & oe.y(i).ToString() & vbLf)
        Next

        fw.Flush()
    End Sub

    Sub test2()
        Dim graph As Integer()() = "E:\GCModeller\src\runtime\sciBASIC#\gr\network-visualization\network_layout\Orthogonal\examples\graph4.txt" _
            .ReadAllLines _
            .Select(Function(l) l.Split.AsInteger) _
            .ToArray
        Dim numberOfAttempts As Integer = 1
        Dim optimize As Boolean = True
        Dim simplify As Boolean = True
        Dim fixNonOrthogonal As Boolean = True
        Dim disconnectedGraphSet As IList(Of IList(Of Integer)) = DisconnectedGraphs.findDisconnectedGraphs(graph)
        Dim disconnectedEmbeddings As IList(Of OrthographicEmbeddingResult) = New List(Of OrthographicEmbeddingResult)()
        For Each nodeSubset In disconnectedGraphSet
            ' calculate the embedding:
            Dim best_g_oe As OrthographicEmbeddingResult = Nothing
            Dim g As Integer()() = DisconnectedGraphs.subgraph(graph, nodeSubset)
            Dim comparator As SegmentLengthEmbeddingComparator = New SegmentLengthEmbeddingComparator()
            For attempt = 0 To numberOfAttempts - 1
                Dim g_oe As OrthographicEmbeddingResult = OrthographicEmbedding.orthographicEmbedding(g, simplify, fixNonOrthogonal)
                If g_oe Is Nothing Then
                    Continue For
                End If
                If Not g_oe.sanityCheck(False) Then
                    Throw New Exception("The orthographic projection contains errors!")
                End If
                If optimize Then
                    g_oe = OrthographicEmbeddingOptimizer.optimize(g_oe, g, comparator)
                    If Not g_oe.sanityCheck(False) Then
                        Throw New Exception("The orthographic projection after optimization contains errors!")
                    End If
                End If
                If best_g_oe Is Nothing Then
                    best_g_oe = g_oe
                Else
                    If comparator.compare(g_oe, best_g_oe) < 0 Then
                        best_g_oe = g_oe
                    End If
                End If
            Next
            disconnectedEmbeddings.Add(best_g_oe)
        Next
        Dim oe As OrthographicEmbeddingResult = DisconnectedGraphs.mergeDisconnectedEmbeddingsSideBySide(disconnectedEmbeddings, disconnectedGraphSet, 1.0)

        ' save the results:
        saveEmbedding(oe)

        ' save image:
        'If Not ReferenceEquals(outputPNGName, Nothing) Then
        '    SavePNG.savePNG(outputPNGName, oe, PNGcellWidth, PNGcellHeight, PNGlabelVertices)
        'End If
    End Sub
End Module
