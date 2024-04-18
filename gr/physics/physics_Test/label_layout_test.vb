Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Physics.layout
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module label_layout_test

    Sub Main()

        Dim nodes As Node() = New Node(20 - 1) {}
        Dim w As Double = 1000
        Dim h As Double = 1000
        Dim labels As New Dictionary(Of Node, TextProperties)
        Dim label_w As New DoubleRange(10, 50)
        Dim label_h As New DoubleRange(10, 30)
        Dim r As New DoubleRange(0, 1)

        For i As Integer = 0 To nodes.Length - 1
            nodes(i) = New Node() With {.X = w * randf.NextDouble, .Y = h * randf.NextDouble, .fixed = False, .size = 60}
            labels.Add(nodes(i), New TextProperties With {.Width = r.ScaleMapping(randf.NextDouble, label_w), .Height = r.ScaleMapping(randf.NextDouble, label_h)})
        Next

        Dim algo As New LabelAdjust

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

            For i As Integer = 0 To nodes.Length - 1
                Dim n As Node = nodes(i)
                Dim size = labels(n)

                Call g.DrawRectangle(Pens.Red, New RectangleF(n.X, n.Y, size.Width, size.Height))
            Next

            Call g.Flush()
            Call g.ImageResource.SaveAs("./after-layout.png")
        End Using

        Pause()
    End Sub
End Module
