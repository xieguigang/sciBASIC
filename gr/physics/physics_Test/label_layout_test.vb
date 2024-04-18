Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Imaging.Physics.layout
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module label_layout_test

    Sub Main()

        Dim nodes As Node() = New Node(60) {}
        Dim w As Double = 4000
        Dim h As Double = 2000
        Dim labels As New Dictionary(Of Node, TextProperties)
        Dim label_w As New DoubleRange(30, 200)
        Dim label_h As New DoubleRange(10, 50)
        Dim r As New DoubleRange(0, 1)
        Dim source As New List(Of PointF)

        For i As Integer = 0 To nodes.Length - 1
            nodes(i) = New Node() With {.X = w * randf.NextDouble, .Y = h * randf.NextDouble, .fixed = False, .size = 60}
            source.Add(New PointF(nodes(i).X, nodes(i).Y))
            labels.Add(nodes(i), New TextProperties With {.Width = r.ScaleMapping(randf.NextDouble, label_w), .Height = r.ScaleMapping(randf.NextDouble, label_h)})
        Next

        Dim algo As New LabelAdjust With {.canvas = New SizeF(w, h)}

        Using g As Graphics2D = New Size(w, h).CreateGDIDevice

            For i As Integer = 0 To nodes.Length - 1
                Dim n As Node = nodes(i)
                Dim size = labels(n)

                Call g.DrawRectangle(Pens.Red, New RectangleF(n.X, n.Y, size.Width, size.Height))
            Next

            Call g.Flush()
            Call g.ImageResource.SaveAs("./before.png")
        End Using

        Call algo.Solve(nodes, labels)


        Using g As Graphics2D = New Size(w, h).CreateGDIDevice

            Dim arrow As New Pen(Color.Black, 2) With {.DashStyle = Drawing.Drawing2D.DashStyle.Dash, .EndCap = Drawing.Drawing2D.LineCap.ArrowAnchor}

            For i As Integer = 0 To nodes.Length - 1
                Dim n As Node = nodes(i)
                Dim size = labels(n)
                Dim r1 As New RectangleF(source(i).X, source(i).Y, size.Width, size.Height)
                Dim r2 As New RectangleF(n.X, n.Y, size.Width, size.Height)

                Call g.DrawLine(arrow, r1.Centre, r2.Centre)
                Call g.DrawString((i + 1).ToString, New Font(FontFace.SegoeUI, 24), Brushes.Blue, r2.Centre)

                Call g.DrawRectangle(Pens.Green, r1)
                Call g.DrawRectangle(Pens.Red, r2)
            Next

            Call g.Flush()
            Call g.ImageResource.SaveAs("./after-layout.png")
        End Using

        Pause()
    End Sub
End Module
