Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace BarPlot

    Public Class SimpleBarPlot : Inherits Plot

        ReadOnly data As BarDataGroup

        Public Property stackReorder As Boolean = True
        Public Property stacked As Boolean = False

        Public Sub New(data As BarDataGroup, theme As Theme)
            MyBase.New(theme)

            Me.data = data
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim scaler As New Scaling(Data, stacked, False)
            Dim mapper As New Mapper(scaler)
            Dim n As Integer

            If stacked Then
                n = Data.Samples.Length
            Else
                n = Data.Samples.Sum(Function(x) x.data.Length) - 1
            End If

            Dim dxStep As Double = (canvas.Size.Width - canvas.Padding.Horizontal - 2 * canvas.Padding.Horizontal) / n
            Dim interval As Double = canvas.Padding.Horizontal / n
            Dim left As Single = canvas.Padding.Left
            Dim sy As Func(Of Single, Single) = mapper.YScaler(canvas.Size, canvas.Padding)
            Dim bottom = canvas.Size.Height - canvas.Padding.Bottom
            Dim angle! = -45
            Dim leftMargins As New List(Of Double)

            ' Call g.DrawAxis(grect.Size, grect.Padding, mapper, showGrid)

            For Each sample As SeqValue(Of BarDataSample) In Data.Samples.SeqIterator
                Dim x = left + interval

                leftMargins += x

                If stacked Then
                    ' 改变Y
                    Dim right = x + dxStep
                    Dim top = sy(sample.value.StackedSum)
                    ' 畫布的高度
                    Dim canvasHeight = canvas.Size.Height - (canvas.Padding.Vertical)
                    ' 底部減去最高的就是實際的高度（縂的）
                    Dim actualHeight = bottom - top
                    Dim barWidth = dxStep

                    Dim stack As IEnumerable(Of SeqValue(Of Double))

                    If stackReorder Then
                        stack = sample.value _
                            .data _
                            .SeqIterator _
                            .OrderBy(Function(o) o.value)
                    Else
                        stack = sample.value _
                            .data _
                            .SeqIterator
                    End If

                    For Each val As SeqValue(Of Double) In stack
                        Dim topleft As New Point(x, top)
                        ' 百分比
                        Dim barHeight! = (+val) / (+sample).StackedSum
                        barHeight = barHeight * actualHeight
                        Dim barSize As New Size(barWidth, barHeight)
                        Dim rect As New Rectangle(topleft, barSize)

                        Call g.FillRectangle(New SolidBrush(Data.Serials(val.i).Value), rect)

                        top += barHeight
                    Next

                    x += dxStep
                Else
                    ' 改变X
                    For Each val As SeqValue(Of Double) In sample.value.data.SeqIterator
                        Dim right = x + dxStep
                        Dim top = sy(val.value)
                        Dim rect As Rectangle = BarPlotAPI.Rectangle(top, x, right, canvas.Size.Height - canvas.Padding.Bottom)

                        Call g.DrawRectangle(Pens.Black, rect)
                        Call g.FillRectangle(
                            New SolidBrush(data.Serials(val.i).Value),
                            BarPlotAPI.Rectangle(top + 1,
                                      x + 1,
                                      right - 1,
                                     canvas.Size.Height - canvas.Padding.Bottom - 1))
                        x += dxStep
                    Next
                End If

                left = x
            Next

            Dim keys$() = Data.Samples _
                .Select(Function(s) s.Tag) _
                .ToArray
            Dim font As New Font(FontFace.SegoeUI, 28)
            Dim dd As Double

            If leftMargins.Count = 1 Then
                dd = 0
            Else
                dd = leftMargins(1) - leftMargins(0)
            End If

            bottom += 80

            For Each key As SeqValue(Of String) In keys.SeqIterator
                left = leftMargins(index:=key.i) + dd / 2 - If(Not stacked, dxStep / 2, 0)

                ' 得到斜边的长度
                Dim sz = g.MeasureString((+key), font)
                Dim dx! = sz.Width * stdNum.Cos(angle)
                Dim dy! = sz.Width * stdNum.Sin(angle)

                Call g.DrawString(key, font, Brushes.Black, left - dx, bottom, angle)
            Next

            If theme.drawLegend Then
                Dim cssStyle As String = theme.legendLabelCSS
                Dim legends As LegendObject() = LinqAPI.Exec(Of LegendObject) <=
                                                                                _
                From x As NamedValue(Of Color)
                In data.Serials
                Select New LegendObject With {
                    .color = x.Value.RGBExpression,
                    .fontstyle = cssStyle,
                    .style = LegendStyles.Circle,
                    .title = x.Name
                }

                Call DrawLegends(g, legends, False, canvas)
            End If
        End Sub
    End Class
End Namespace