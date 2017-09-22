Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Module LabelingTest

    Sub Main()
        Using g As Graphics2D = New Size(1024, 1024).CreateGDIDevice(filled:=Color.White)
            Dim labelFont As Font = CSSFont _
                .TryParse(CSSFont.PlotLabelNormal) _
                .GDIObject
            Dim rand As New Random
            Dim labels As Label() = g.Label(100.SeqRandom.Select(Function(i) rand.NextDouble.ToString("F4"))).ToArray
            Dim anchors = labels _
                .Select(Function(i)
                            Return New Anchor With {
                                .r = 10,
                                .x = rand.Next(g.Width),
                                .y = rand.Next(g.Height)
                            }
                        End Function) _
                .ToArray

            labels = d3js.labeler _
                .Height(g.Height) _
                .Width(g.Width) _
                .Labels(labels) _
                .Anchors(anchors) _
                .Start(2500) _
                .ToArray

            For Each i As SeqValue(Of Label) In labels.SeqIterator
                Dim label As Label = i
                Dim anchor = anchors(i)

                Call g.DrawCircle(anchor, anchor.r, Brushes.Red)
                Call g.DrawString(label.name, labelFont, Brushes.Black, New PointF(label.X, label.Y))
                Call g.DrawLine(Pens.Green, anchor, label)
            Next

            Call g.Save("./test_labels.png", ImageFormats.Png)
        End Using
    End Sub
End Module
