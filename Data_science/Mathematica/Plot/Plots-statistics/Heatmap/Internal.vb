Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramVisualize
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Heatmap

    Public Class PlotArguments

        Public left!
        ''' <summary>
        ''' 绘制矩阵之中的方格在xy上面的步进值
        ''' </summary>
        Public dStep As SizeF
        ''' <summary>
        ''' 矩阵区域的大小和位置
        ''' </summary>
        Public matrixPlotRegion As Rectangle
        Public levels As Dictionary(Of String, DataSet)
        Public top!
        Public colors As SolidBrush()
        Public RowOrders$()
        Public ColOrders$()

    End Class

    Public Enum DrawElements As Byte
        None = 0
        Rows = 2
        Cols = 4
        Both = 8
    End Enum

    ''' <summary>
    ''' heatmap plot internal
    ''' </summary>
    Module Internal

        ''' <summary>
        ''' 返回来的都是0-1之间的数，乘以颜色数组长度之后即可庸作为颜色的index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleByRow(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim levelRange As DoubleRange = {0R, levels}
            Return data _
                .Select(Function(x)
                            Dim range As DoubleRange = x.Properties.Values.Range
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = x _
                                    .Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key) range.ScaleMapping(x(key), levelRange))
                            }
                        End Function)
        End Function

        ''' <summary>
        ''' 返回来的都是0-1之间的数，乘以颜色数组长度之后即可庸作为颜色的index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScaleByCol(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim list = data.ToArray
            Dim keys = list.PropertyNames
            Dim ranges = keys.ToDictionary(
                Function(key) key,
                Function(key) list.Select(Function(x) x(key)).Range)
            Dim levelRange As DoubleRange = {0R, levels}

            Return list _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key) ranges(key).ScaleMapping(x(key), levelRange))
                            }
                        End Function)
        End Function

        <Extension>
        Public Function ScaleByALL(data As IEnumerable(Of DataSet), levels%) As IEnumerable(Of DataSet)
            Dim list = data.ToArray
            Dim keys = list.PropertyNames
            Dim range As DoubleRange = list _
                .Select(Function(x) x.Properties.Values) _
                .IteratesALL _
                .Range
            Dim levelRange As DoubleRange = {0R, levels}

            Return data _
                .Select(Function(x)
                            Return New DataSet With {
                                .ID = x.ID,
                                .Properties = x _
                                    .Properties _
                                    .Keys _
                                    .ToDictionary(Function(key) key,
                                                  Function(key) range.ScaleMapping(x(key), levelRange))
                           }
                        End Function)
        End Function

        ''' <summary>
        ''' 因为只是想要缩小距离，并不是真正的数学上的log计算
        ''' 故而，0的log值为0
        ''' 负数的log值为绝对值的log乘上-1
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="base#"></param>
        ''' <returns></returns>
        <Extension> Public Function Log(v As Vector, base#) As Vector
            Return v _
                .Select(Function(x)
                            If x = 0R Then
                                Return 0
                            Else
                                Return Math.Sign(x) * Math.Log(x, base)
                            End If
                        End Function) _
                .AsVector
        End Function

        ''' <summary>
        ''' 一些共同的绘图元素过程
        ''' </summary>
        ''' <param name="drawLabels">是否绘制下面的标签，对于下三角形的热图而言，是不需要绘制下面的标签的，则设置这个参数为False</param>
        ''' <param name="legendSize">这个对象定义了图示的大小</param>
        ''' <param name="rowLabelfont">对行标签或者列标签的字体的定义</param>
        ''' <param name="array">Name为行名称，字典之中的key为列名称</param>
        ''' <param name="scaleMethod">
        ''' + 如果是<see cref="DrawElements.Cols"/>表示按列赋值颜色
        ''' + 如果是<see cref="DrawElements.Rows"/>表示按行赋值颜色
        ''' + 如果是<see cref="DrawElements.None"/>或者<see cref="DrawElements.Both"/>则是表示按照整体数据
        ''' </param>
        <Extension>
        Friend Function __plotInterval(plot As Action(Of IGraphics, GraphicsRegion, PlotArguments),
                                       array As DataSet(),
                                       rowLabelfont As Font, colLabelFont As Font,
                                       logScale#,
                                       scaleMethod As DrawElements,
                                       drawLabels As DrawElements,
                                       drawDendrograms As DrawElements,
                                       dendrogramLayout As (A%, B%),
                                       reverseClrSeq As Boolean,
                                       Optional colors As SolidBrush() = Nothing,
                                       Optional mapLevels% = 100,
                                       Optional mapName$ = ColorMap.PatternJet,
                                       Optional size As Size = Nothing,
                                       Optional padding As Padding = Nothing,
                                       Optional bg$ = "white",
                                       Optional legendTitle$ = "Heatmap Color Legend",
                                       Optional legendFont As Font = Nothing,
                                       Optional legendLabelFont As Font = Nothing,
                                       Optional min# = -1,
                                       Optional max# = 1,
                                       Optional mainTitle$ = "heatmap",
                                       Optional titleFont As Font = Nothing,
                                       Optional legendWidth! = -1,
                                       Optional legendHasUnmapped As Boolean = True,
                                       Optional legendSize As Size = Nothing) As GraphicsData

            Dim keys$() = array.PropertyNames
            Dim angle! = -45

            If colors.IsNullOrEmpty Then
                colors = Designer.GetColors(mapName, mapLevels).GetBrushes
                If reverseClrSeq Then
                    colors = colors.Reverse.ToArray
                End If
            End If

            Dim rowKeys$() ' 经过聚类之后得到的新的排序顺序
            Dim colKeys$()

            Dim configDendrogramCanvas =
                Function(cluster As Cluster)
                    Return New DendrogramPanel With {
                        .LineColor = Color.Black,
                        .ScaleValueDecimals = 0,
                        .ScaleValueInterval = 1,
                        .Model = cluster,
                        .ShowScale = False,
                        .ShowDistanceValues = False,
                        .ShowLeafLabel = False,
                        .LinkDotRadius = 0
                    }
                End Function
            Dim DATA#() = array _
                .Select(Function(x) x.Properties.Values) _
                .IteratesALL _
                .Join(min, max) _
                .Distinct _
                .ToArray
            Dim ticks = AxisScalling.CreateAxisTicks(DATA, ticks:=5)

            Dim plotInternal =
                Sub(ByRef g As IGraphics, rect As GraphicsRegion)

                    ' 根据布局计算出矩阵的大小和位置
                    Dim left! = padding.Left, top! = padding.Top    ' 绘图区域的左上角位置
                    ' 计算出右边的行标签的最大的占用宽度
                    Dim maxRowLabelSize As SizeF = g.MeasureString(array.Keys.MaxLengthString, rowLabelfont)
                    Dim maxColLabelSize As SizeF = g.MeasureString(keys.MaxLengthString, colLabelFont)
                    Dim llayout As New Rectangle(New Point(left, top), legendSize)

                    ' legend位于整个图片的左上角
                    Call Legends.ColorLegendHorizontal(colors, ticks, g, llayout, scientificNotation:=True)

                    ' 宽度与最大行标签宽度相减得到矩阵的绘制宽度
                    Dim dw = rect.PlotRegion.Width - maxRowLabelSize.Width
                    Dim dh = rect.PlotRegion.Height - maxColLabelSize.Width - legendSize.Height

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
                    ' 有列的聚类树
                    If drawDendrograms.HasFlag(DrawElements.Cols) Then
                        ' B
                        top += dendrogramLayout.B
                        dh = dh - dendrogramLayout.B
                        layoutB = dendrogramLayout.B
                    Else
                        layoutB = 0
                    End If

                    ' 2. 然后才能够进行绘图
                    If drawDendrograms.HasFlag(DrawElements.Rows) Then

                        Try
                            ' 绘制出聚类树
                            Dim cluster As Cluster = Time(AddressOf array.RunCluster)
                            Dim topleft As New Point With {
                                .X = rect.Padding.Left,
                                .Y = top
                            }
                            Dim dsize As New Size With {
                                .Width = dendrogramLayout.A,
                                .Height = dh
                            }
                            rowKeys = configDendrogramCanvas(cluster) _
                                .Paint(DirectCast(g, Graphics2D), New Rectangle(topleft, dsize)) _
                                .OrderBy(Function(x) x.Value.Y) _
                                .Keys
                        Catch ex As Exception
                            ex.PrintException
                            rowKeys = array.Keys
                        End Try

                    Else
                        rowKeys = array.Keys
                    End If
                    If drawDendrograms.HasFlag(DrawElements.Cols) Then
                        Dim cluster As Cluster = Time(AddressOf array.Transpose.RunCluster)
                        Dim dp As New DendrogramPanel With {
                            .LineColor = Color.Black,
                            .ScaleValueDecimals = 0,
                            .ScaleValueInterval = 1,
                            .Model = cluster,
                            .ShowScale = False,
                            .ShowDistanceValues = False
                        }
                        colKeys = dp _
                            .Paint(DirectCast(g, Graphics2D), New Rectangle(300, 100, 500, 500)) _
                            .OrderBy(Function(x) x.Value.X) _
                            .Keys
                    Else
                        colKeys = array.PropertyNames
                    End If

                    left += 10

                    Dim matrixPlotRegion As New Rectangle With {
                        .Location = New Point(left, top),
                        .Size = New Size(dw, dh)
                    }

                    dw /= keys.Length
                    dh /= array.Length

                    Dim levels As New Dictionary(Of String, DataSet)
                    Dim scaleData As DataSet()

                    If logScale > 0 Then
                        Dim names As New NamedVectorFactory(keys)

                        scaleData = array _
                            .Select(Function(x)
                                        Dim vector As Vector = names _
                                            .AsVector(x.Properties) _
                                            .Log(logScale)

                                        Return New DataSet With {
                                            .ID = x.ID,
                                            .Properties = names.Translate(vector)
                                        }
                                    End Function) _
                            .ToArray
                    Else
                        scaleData = array
                    End If

                    Select Case scaleMethod
                        Case DrawElements.Cols
                            levels = scaleData _
                                .ScaleByCol(colors.Length - 1) _
                                .ToDictionary(Function(x) x.ID)
                        Case DrawElements.Rows
                            levels = scaleData _
                                .ScaleByRow(colors.Length - 1) _
                                .ToDictionary(Function(x) x.ID)

                        Case Else
                            levels = scaleData _
                                .ScaleByALL(colors.Length - 1) _
                                .ToDictionary(Function(x) x.ID)

                    End Select

                    Dim args As New PlotArguments With {
                        .colors = colors,
                        .dStep = New SizeF(dw, dh),
                        .left = left,
                        .levels = levels,
                        .top = top,
                        .ColOrders = colKeys,
                        .RowOrders = rowKeys,
                        .matrixPlotRegion = matrixPlotRegion
                    }
#If DEBUG Then
                    ' Call levels.GetJson().Warning
#End If
                    ' 绘制heatmap之中的矩阵内容
                    Call plot(g, rect, args)

                    left = args.left
                    top = args.top
                    left += dw / 2

                    ' 绘制下方的矩阵的列标签
                    If drawLabels = DrawElements.Both OrElse drawLabels = DrawElements.Cols Then
                        Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                        Dim format As New StringFormat() With {
                            .FormatFlags = StringFormatFlags.MeasureTrailingSpaces
                        }

                        For Each key$ In keys
                            Dim sz = g.MeasureString(key$, colLabelFont) ' 得到斜边的长度
                            Dim dx! = sz.Width * Math.Cos(angle)
                            Dim dy! = sz.Width * Math.Sin(angle)
                            Dim pos As New PointF(left - dx, top - dy)

                            Call text.DrawString(key$, colLabelFont, Brushes.Black, pos, angle, format)

                            left += dw
                        Next
                    End If

                    Dim titleSize = g.MeasureString(mainTitle, titleFont)
                    Dim titlePosi As New PointF With {
                        .X = args.matrixPlotRegion.Left + (args.matrixPlotRegion.Width - titleSize.Width) / 2, ' 标题在所绘制的矩阵上方居中
                        .Y = (padding.Top - titleSize.Height) / 2
                    }

                    Call g.DrawString(mainTitle, titleFont, Brushes.Black, titlePosi)
                End Sub

            Return g.GraphicsPlots(size, padding, bg$, plotInternal)
        End Function
    End Module
End Namespace