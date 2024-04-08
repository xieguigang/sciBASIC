
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.orthographicembedding

Namespace Orthogonal.util

    ' 
    ' 	 * To change this license header, choose License Headers in Project Properties.
    ' 	 * To change this template file, choose Tools | Templates
    ' 	 * and open the template in the editor.
    ' 	 

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class SavePNG
        Public Shared Sub savePNG(fileName As String, oe As OrthographicEmbeddingResult, cell_width As Integer, cell_height As Integer, label_vertices As Boolean)
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
                    minx = CSharpImpl.__Assign(maxx, oe.x(i))
                    miny = CSharpImpl.__Assign(maxy, oe.y(i))
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

            Dim img As New Bitmap(width, height)
            Dim g As Graphics

            ' g.Font = new Font("TimesRoman", Font.PLAIN, fontSize);
            For i = 0 To oe.nodeIndexes.Length - 1
                For j = 0 To oe.nodeIndexes.Length - 1
                    If oe.edges(i)(j) Then
                        Dim x0 = Math.Min(oe.x(i), oe.x(j)) - minx
                        Dim y0 = Math.Min(oe.y(i), oe.y(j)) - miny
                        Dim x1 = Math.Max(oe.x(i), oe.x(j)) - minx
                        Dim y1 = Math.Max(oe.y(i), oe.y(j)) - miny
                        '                    System.out.println("drawing edge between " + i + " and " + j + ": " + x0 +"," + y0 + " -> " + x1 + "," + y1);
                        ' g.Color = Color.darkGray;
                        g.fillRect(x0 * cell_width + cell_width * (0.5 - edge_scale / 2), y0 * cell_height + cell_height * (0.5 - edge_scale / 2), (x1 - x0) * cell_width + cell_width * edge_scale, (y1 - y0) * cell_height + cell_height * edge_scale)
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
                        g.fillPolygon(New Integer() {pointx, pointx - vx * w + wx * w / 2, pointx - vx * w - wx * w / 2}, New Integer() {pointy, pointy - vy * w + wy * w / 2, pointy - vy * w - wy * w / 2}, 3)
                    End If
                Next
            Next
            For i = 0 To oe.nodeIndexes.Length - 1
                If oe.nodeIndexes(i) >= 0 Then
                    Dim x0 = oe.x(i) - minx
                    Dim y0 = oe.y(i) - miny
                    ' g.Color = Color.black;
                    g.fillRect(x0 * cell_width + cell_width * (0.5 - vertex_scale / 2), y0 * cell_height + cell_height * (0.5 - vertex_scale / 2), cell_width * vertex_scale, cell_height * vertex_scale)

                    If label_vertices Then
                        ' g.Color = Color.white;
                        Dim text As String = "" & oe.nodeIndexes(i).ToString()
                        Dim w As Integer = g.FontMetrics.stringWidth(text)
                        g.drawString(text, x0 * cell_width + cell_width * 0.5 - w / 2, y0 * cell_height + cell_height * 0.5 + fontSize / 2)
                    End If
                Else
                    Dim x0 = oe.x(i) - minx
                    Dim y0 = oe.y(i) - miny
                    ' g.Color = Color.black;
                    g.fillRect(x0 * cell_width + cell_width * (0.5 - auxiliary_vertex_scale / 2), y0 * cell_height + cell_height * (0.5 - auxiliary_vertex_scale / 2), cell_width * auxiliary_vertex_scale, cell_height * auxiliary_vertex_scale)
                End If
            Next

            ' ImageIO.write(img, "png", new File(fileName));
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class

End Namespace
