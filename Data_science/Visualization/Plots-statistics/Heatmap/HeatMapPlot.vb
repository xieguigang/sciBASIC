
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Data.csv.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Namespace Heatmap


    Public Class HeatMapPlot : Inherits Graphic.HeatMapPlot

        Dim array As DataSet()
        Dim dlayout As (A!, B!)

        Public Sub New(matrix As IEnumerable(Of DataSet), dlayout As SizeF, theme As Theme)
            MyBase.New(theme)

            Me.array = matrix.ToArray
            Me.dlayout = (dlayout.Width, dlayout.Height)
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim margin As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, Padding)
            ' 根据布局计算出矩阵的大小和位置
            Dim left! = margin.Left + rowXOffset, top! = margin.Top    ' 绘图区域的左上角位置
            ' 计算出右边的行标签的最大的占用宽度
            Dim maxRowLabelSize As SizeF = g.MeasureString(array.Keys.MaxLengthString, rowLabelfont)
            Dim maxColLabelSize As SizeF = g.MeasureString(keys.MaxLengthString, colLabelFont)
            Dim llayout As New Rectangle With {
                .Location = New Point(left, top),
                .Size = legendSize
            }

            If legendLayout = Layouts.Horizon Then
                ' legend位于整个图片的左上角
                Call Legends.ColorLegendHorizontal(Colors, ticks, g, llayout, scientificNotation:=True)
            Else
                ' legend位于整个图片的右上角
                Call Legends.ColorMapLegend(g, llayout, Colors, ticks,
                                            css.GetFont(CSSFont.TryParse(CSSFont.Win7LargerNormal)),
                                            legendTitle,
                                            css.GetFont(CSSFont.TryParse(CSSFont.Win7Normal)),
                                            css.GetPen(Stroke.TryParse(Stroke.StrongHighlightStroke)))
            End If

            ' 宽度与最大行标签宽度相减得到矩阵的绘制宽度
            Dim plotRect = rect.PlotRegion(css)
            Dim dw = plotRect.Width - maxRowLabelSize.Width
            Dim dh = plotRect.Height - maxColLabelSize.Width - legendSize.Height

            top += legendSize.Height + 20

            ' 1. 首先要确定layout
            ' 因为行和列的聚类树需要相互依赖对方来确定各自的绘图区域
            ' 所以在这里需要分为两步来完成绘制
            Dim layoutA, layoutB As Integer

            ' 有行的聚类树
            If drawDendrograms.HasFlag(DrawElements.Rows) Then
                ' A
                left += dendrogramLayout.A
                dw = dw - dendrogramLayout.A
                layoutA = dendrogramLayout.A
            Else
                layoutA = 0
            End If
            If Not DrawClass.rowClass.IsNullOrEmpty Then
                Dim d = dendrogramLayout.A / 3

                layoutA += d
                left += d
                dw -= d
            End If

            ' 有列的聚类树
            If drawDendrograms.HasFlag(DrawElements.Cols) Then
                ' B
                top += dendrogramLayout.B
                dh = dh - dendrogramLayout.B
                layoutB = dendrogramLayout.B
            Else
                layoutB = 0
            End If
            If Not DrawClass.colClass.IsNullOrEmpty Then
                Dim d = dendrogramLayout.B / 3

                layoutB += d
                top += d
                dh -= d
            End If

            Dim interval% = 10  ' 层次聚类树与热图矩阵之间的距离

            left += interval
            top += interval

            Dim matrixPlotRegion As New Rectangle With {
                .Location = New Point(left, top),
                .Size = New Size With {
                    .Width = dw - interval,
                    .Height = dh - interval
                }
            }

            ' 2. 然后才能够进行绘图
            If drawDendrograms.HasFlag(DrawElements.Rows) Then

                ' Try
                ' 绘制出聚类树
                Dim cluster As Cluster = Time(AddressOf array.RunCluster)
                Dim topleft As New Point With {
                        .X = margin.Left,
                        .Y = top
                    }
                Dim dsize As New Size With {
                        .Width = dendrogramLayout.A,
                        .Height = matrixPlotRegion.Height
                    }
                rowKeys = configDendrogramCanvas(cluster, DrawClass.rowClass) _
                        .Paint(g, New Rectangle(topleft, dsize)) _
                        .OrderBy(Function(x) x.Value.Y) _
                        .Keys
                'Catch ex As Exception
                '    ex.PrintException
                '    rowKeys = array.Keys
                'End Try

            Else
                rowKeys = array.Keys

                If Not DrawClass.rowClass.IsNullOrEmpty Then
                    ' 没有绘制层次聚类树，但是行的class有值，则会绘制行的class legend
                    Call g.DrawClass(rowKeys, DrawClass.rowClass, matrixPlotRegion, True, dendrogramLayout.A, interval)
                End If
            End If

            If drawDendrograms.HasFlag(DrawElements.Cols) Then
                Dim cluster As Cluster = Time(AddressOf array.Transpose.RunCluster)

                colKeys = configDendrogramCanvas(cluster, DrawClass.colClass) _
                    .Paint(g, New Rectangle(300, 100, 500, 500)) _
                    .OrderBy(Function(x) x.Value.X) _
                    .Keys
            Else
                colKeys = array.PropertyNames

                If Not DrawClass.colClass.IsNullOrEmpty Then
                    ' 没有绘制层次聚类树，但是列的class有值，则会绘制列的class legend
                    Call g.DrawClass(colKeys, DrawClass.colClass, matrixPlotRegion, False, dendrogramLayout.B, interval)
                End If
            End If



            Dim args As New PlotArguments With {
                .colors = Colors,
                .left = left,
                .levels = array.DataScaleLevels(keys, logScale, scaleMethod, Colors.Length),
                .top = top,
                .ColOrders = colKeys,
                .RowOrders = rowKeys,
                .matrixPlotRegion = matrixPlotRegion
            }

            ' 绘制heatmap之中的矩阵内容
            Call Plot(g, rect, args)

            dw = args.dStep.Width
            left = args.left
            top = args.top
            left += dw / 2   ' x坐标已经向方格的中间移动了，后面就不需要额外的移动操作了

            ' 绘制下方的矩阵的列标签
            If drawLabels = DrawElements.Both OrElse drawLabels = DrawElements.Cols Then
                'Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                'Dim format As New StringFormat() With {
                '    .FormatFlags = StringFormatFlags.MeasureTrailingSpaces
                '}

                For Each key$ In keys
                    Dim sz = g.MeasureString(key$, colLabelFont) ' 得到斜边的长度
                    Dim dx! = sz.Width * std.Cos(angle) + sz.Height / 2
                    Dim dy! = sz.Width * std.Sin(angle) + (sz.Width / 2) * std.Cos(angle) - sz.Height
                    Dim pos As New PointF(left - dx, top - dy)

                    Call g.DrawString(key$, colLabelFont, Brushes.Black, pos.X, pos.Y, angle)

                    left += dw
                Next
            End If

            Dim titleSize = g.MeasureString(main, titleFont)
            Dim titlePosi As New PointF With {
                .X = args.matrixPlotRegion.Left + (args.matrixPlotRegion.Width - titleSize.Width) / 2, ' 标题在所绘制的矩阵上方居中
                .Y = (margin.Top - titleSize.Height) / 2
            }

            Call g.DrawString(main, titleFont, Brushes.Black, titlePosi)
        End Sub
    End Class
End Namespace