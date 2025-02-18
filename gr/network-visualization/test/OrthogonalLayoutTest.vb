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
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports inode = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module OrthogonalLayoutTest

    Sub Main()
        Call test1()
        ' Call test2()
    End Sub

    Sub test1()
        Dim g As New NetworkGraph

        For Each label As String In {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"}
            Call g.AddNode(New inode With {.label = label, .data = New NodeData With {.initialPostion = New FDGVector2, .size = {5, 5}}})
        Next

        Call g.AddEdge("0", "1")
        Call g.AddEdge("0", "2")
        Call g.AddEdge("1", "2")

        Call g.AddEdge("3", "4")
        Call g.AddEdge("3", "5")
        Call g.AddEdge("5", "4")

        Call g.AddEdge("2", "6")
        Call g.AddEdge("6", "7")
        Call g.AddEdge("7", "8")
        Call g.AddEdge("8", "9")
        Call g.AddEdge("9", "5")

        Call g.AddEdge("0", "9")

        ' Call Orthogonal.DoLayout(g)

        Dim graph = g.AsGraphMatrix

        For Each line As Integer() In graph
            Call Console.WriteLine(line.GetJson)
        Next

        Dim oe = graph.RunLayoutMatrix

        saveEmbedding(oe)

        ' save image:
        'If Not ReferenceEquals(outputPNGName, Nothing) Then
        savePNG("./demo_layout222.png", oe, 100, 100, True)


        g = g.DoLayout

        Call NetworkVisualizer.DrawImage(g, "1000,1000", labelerIterations:=-1, minLinkWidth:=8).Save("./Orthogonal.png")

        Pause()
    End Sub

    Private Sub saveEmbedding(oe As OrthographicEmbeddingResult)
        Dim fw As New StreamWriter(Console.OpenStandardOutput)

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
        Dim graph As Integer()() = "G:\GCModeller\src\runtime\sciBASIC#\gr\network-visualization\network_layout\Orthogonal\examples\graph4.txt" _
            .ReadAllLines _
            .Select(Function(l) l.Split(","c).AsInteger) _
            .ToArray
        Dim oe = graph.RunLayoutMatrix(numberOfAttempts:=10)

        If Not oe Is Nothing Then
            ' save the results:
            saveEmbedding(oe)

            ' save image:
            'If Not ReferenceEquals(outputPNGName, Nothing) Then
            savePNG("./demo_layout.png", oe, 100, 100, True)
            'End If
        End If

        Pause()
    End Sub

    Public Sub savePNG(fileName As String, oe As OrthographicEmbeddingResult, cell_width As Integer, cell_height As Integer, label_vertices As Boolean)
        Dim vertex_scale = 0.666
        Dim auxiliary_vertex_scale = 0.333
        Dim edge_scale = 0.1666
        Dim fontSize As Integer = cell_height * vertex_scale * 0.5

        Dim minx As Double = -1
        Dim maxx As Double = -1
        Dim miny As Double = -1
        Dim maxy As Double = -1
        For i = 0 To oe.nodeIndexes.Length - 1
            If i = 0 Then
                maxx = oe.x(i)
                minx = oe.x(i)
                maxy = oe.y(i)
                miny = oe.y(i)
            Else
                If oe.x(i) < minx Then
                    minx = oe.x(i)
                End If
                If oe.x(i) > maxx Then
                    maxx = oe.x(i)
                End If
                If oe.y(i) < miny Then
                    miny = oe.y(i)
                End If
                If oe.y(i) > maxy Then
                    maxy = oe.y(i)
                End If
            End If
        Next
        Dim width_in_cells = maxx - minx + 1
        Dim height_in_cells = maxy - miny + 1

        Dim width As Integer = width_in_cells * cell_width
        Dim height As Integer = height_in_cells * cell_height
        Dim g As IGraphics = New Size(width, height).CreateGDIDevice(filled:=Color.White)
        Dim font As New Font(FontFace.MicrosoftYaHeiUI, 12)

        ' g.Font = new Font("TimesRoman", Font.PLAIN, fontSize);
        ' draw edges
        For i = 0 To oe.nodeIndexes.Length - 1
            For j = 0 To oe.nodeIndexes.Length - 1
                If oe.edges(i)(j) Then
                    Dim x0 = Math.Min(oe.x(i), oe.x(j)) - minx
                    Dim y0 = Math.Min(oe.y(i), oe.y(j)) - miny
                    Dim x1 = Math.Max(oe.x(i), oe.x(j)) - minx
                    Dim y1 = Math.Max(oe.y(i), oe.y(j)) - miny
                    '                    System.out.println("drawing edge between " + i + " and " + j + ": " + x0 +"," + y0 + " -> " + x1 + "," + y1);
                    ' g.Color = Color.darkGray;
                    Dim rect As New RectangleF(x0 * cell_width + cell_width * (0.5 - edge_scale / 2), y0 * cell_height + cell_height * (0.5 - edge_scale / 2), (x1 - x0) * cell_width + cell_width * edge_scale, (y1 - y0) * cell_height + cell_height * edge_scale)

                    Call g.FillRectangle(Brushes.DarkGray, rect)

                    Dim pointx = x1 * cell_width + 0.5 * cell_width
                    Dim pointy = y1 * cell_width + 0.5 * cell_width
                    Dim vx = x1 - x0
                    Dim vy = y1 - y0
                    Dim vn = Math.Sqrt(vx * vx + vy * vy)
                    Dim w = cell_width / 2
                    vx /= vn
                    vy /= vn
                    Dim wx = vy
                    Dim wy = -vx
                    g.FillPolygon(Brushes.DarkGreen, New Double() {pointx, pointx - vx * w + wx * w / 2, pointx - vx * w - wx * w / 2}, New Double() {pointy, pointy - vy * w + wy * w / 2, pointy - vy * w - wy * w / 2})
                End If
            Next
        Next
        For i = 0 To oe.nodeIndexes.Length - 1
            If oe.nodeIndexes(i) >= 0 Then
                Dim x0 = oe.x(i) - minx
                Dim y0 = oe.y(i) - miny
                ' g.Color = Color.black;
                Dim rect As New RectangleF(x0 * cell_width + cell_width * (0.5 - vertex_scale / 2), y0 * cell_height + cell_height * (0.5 - vertex_scale / 2), cell_width * vertex_scale, cell_height * vertex_scale)

                g.FillRectangle(Brushes.Black, rect)

                If label_vertices Then
                    ' g.Color = Color.white;
                    Dim text As String = "" & oe.nodeIndexes(i).ToString()
                    Dim w As Integer = g.MeasureString(text, font).Width
                    g.DrawString(text, font, Brushes.Red, x0 * cell_width + cell_width * 0.5 - w / 2, y0 * cell_height + cell_height * 0.5 + fontSize / 2)
                End If
            Else
                Dim x0 = oe.x(i) - minx
                Dim y0 = oe.y(i) - miny
                ' g.Color = Color.black;
                Dim rect As New RectangleF(x0 * cell_width + cell_width * (0.5 - auxiliary_vertex_scale / 2), y0 * cell_height + cell_height * (0.5 - auxiliary_vertex_scale / 2), cell_width * auxiliary_vertex_scale, cell_height * auxiliary_vertex_scale)

                g.FillRectangle(Brushes.Blue, rect)
            End If
        Next

        DirectCast(g, Graphics2D).ImageResource.SaveAs(fileName)
    End Sub
End Module
