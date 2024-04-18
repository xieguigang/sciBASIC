Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
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
            nodes(i) = New Node() With {.X = w * randf.NextDouble, .Y = h * randf.NextDouble, .fixed = False, .size = 1}
            labels.Add(nodes(i), New TextProperties With {.Width = r.ScaleMapping(randf.NextDouble, label_w), .Height = r.ScaleMapping(randf.NextDouble, label_h)})
        Next

        Dim algo As New LabelAdjust

        Call algo.initAlgo()
        Call algo.goAlgo(nodes, labels)


        Pause()
    End Sub
End Module
