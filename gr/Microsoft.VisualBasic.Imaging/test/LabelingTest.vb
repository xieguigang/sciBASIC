#Region "Microsoft.VisualBasic::0dc1acb63ba04eafcf3b371f704582aa, gr\Microsoft.VisualBasic.Imaging\test\LabelingTest.vb"

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

    '   Total Lines: 52
    '    Code Lines: 46 (88.46%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (11.54%)
    '     File Size: 2.10 KB


    ' Module LabelingTest
    ' 
    '     Sub: Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS

Module LabelingTest

    Sub Main1()
        Using g As Graphics2D = New Size(1024, 1024).CreateGDIDevice(filled:=Color.White)
            Dim labelFont As Font = CSSFont _
                .TryParse(CSSFont.PlotLabelNormal) _
                .GDIObject(300)
            Dim rand As New Random
            Dim labels As Label() = g.Label(130.SeqRandom.Select(Function(i) rand.NextDouble.ToString("F4"))).ToArray
            Dim anchors = labels _
                .Select(Function(i)
                            Return New Anchor With {
                                .r = 10,
                                .x = rand.Next(g.Width),
                                .y = rand.Next(g.Height)
                            }
                        End Function) _
                .ToArray

            labels = d3js.labeler(maxMove:=20, maxAngle:=2) _
                .Height(g.Height) _
                .Width(g.Width) _
                .Labels(labels) _
                .Anchors(anchors) _
                .Start(500) _
                .ToArray

            For Each i As SeqValue(Of Label) In labels.SeqIterator
                Dim label As Label = i
                Dim anchor = anchors(i)
                Dim labelLayout As New Rectangle With {
                    .Location = New Point(label.X, label.Y),
                    .Size = g.MeasureString(label.text, labelFont).ToSize
                }

                Call g.DrawCircle(anchor, anchor.r, Brushes.Red)
                Call g.DrawString(label.text, labelFont, Brushes.Black, labelLayout.Location)
                Call g.DrawLine(Pens.Green, anchor, labelLayout.GetTextAnchor(anchor))
            Next

            Call g.Save("./test_labels.png", ImageFormats.Png)
        End Using
    End Sub
End Module
