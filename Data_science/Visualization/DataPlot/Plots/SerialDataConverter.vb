Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Plots.Canvas
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

' ============================================================================
'  SerialDataConverter.vb - 旧 Canvas 数据模型到新绘图引擎模型的转换
'
'  旧绘图代码（Legacy）以 Canvas.SerialData / Canvas.PointData 作为输入，
'  新引擎以 Series / BubbleSeries 作为输入。两者的表达能力几乎一致：
'
'   · PointData 允许逐点配色，而 Series 只有单一颜色
'     → 这里按颜色把一条 SerialData 拆成多条 Series（图例只保留第一段的名称）
'   · PointData.value 是气泡的第三维 → 映射到 Series.Size
'   · PointData.tag 是点的标签 → 映射到 Series.PointLabels
' ============================================================================

''' <summary>
''' <see cref="SerialData"/> / <see cref="PointData"/> 到新绘图引擎
''' <see cref="Series"/> / <see cref="BubbleSeries"/> 的转换器。
''' </summary>
Public Module SerialDataConverter

    ''' <summary>
    ''' 把旧模型的序列集合转换为新引擎的序列集合。
    ''' </summary>
    ''' <param name="serials">旧模型的序列</param>
    ''' <param name="drawLine">是否把相邻点连成线（False 时使用 <see cref="DashStyle.Custom"/> 抑制连线）</param>
    ''' <returns></returns>
    <Extension>
    Public Function ToSeries(serials As IEnumerable(Of SerialData), Optional drawLine As Boolean = False) As List(Of Series)
        Dim list As New List(Of Series)()

        For Each serial As SerialData In serials.SafeQuery
            Call list.AddRange(serial.ToSeries(drawLine))
        Next

        Return list
    End Function

    ''' <summary>
    ''' 把单条旧模型序列转换为新引擎的序列（逐点配色会按颜色拆分成多条）。
    ''' </summary>
    ''' <param name="serial"></param>
    ''' <param name="drawLine"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ToSeries(serial As SerialData, Optional drawLine As Boolean = False) As Series()
        Dim points As PointData() = serial.pts.SafeQuery.ToArray()

        If points.Length = 0 Then
            Return {}
        End If

        Dim lineStyle As DashStyle = If(drawLine, serial.lineType, DashStyle.Custom)
        Dim groups = points.GroupBy(Function(p) pointColor(p, serial)).ToArray
        Dim result As New List(Of Series)()

        For i As Integer = 0 To groups.Length - 1
            Dim part As PointData() = groups(i).ToArray

            result.Add(New Series With {
                .Name = If(i = 0, If(serial.title, ""), ""),
                .Color = groups(i).Key,
                .X = part.Select(Function(p) CDbl(p.pt.X)).ToArray,
                .Y = part.Select(Function(p) CDbl(p.pt.Y)).ToArray,
                .MarkerShape = toMarkerShape(serial.shape),
                .LineStyle = lineStyle,
                .PointSize = serial.pointSize,
                .Size = bubbleSizes(part),
                .PointLabels = pointLabels(part)
            })
        Next

        Return result.ToArray
    End Function

    ''' <summary>
    ''' 把旧模型序列转换为气泡序列（<see cref="PointData.value"/> 作为气泡大小）。
    ''' </summary>
    ''' <param name="serials"></param>
    ''' <param name="drawLine"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ToBubbleSeries(serials As IEnumerable(Of SerialData), Optional drawLine As Boolean = False) As List(Of BubbleSeries)
        Dim list As New List(Of BubbleSeries)()

        For Each serial As SerialData In serials.SafeQuery
            For Each serie As Series In serial.ToSeries(drawLine)
                list.Add(New BubbleSeries With {
                    .Name = serie.Name,
                    .Color = serie.Color,
                    .X = serie.X,
                    .Y = serie.Y,
                    .MarkerShape = serie.MarkerShape,
                    .LineStyle = serie.LineStyle,
                    .PointSize = serie.PointSize,
                    .PointLabels = serie.PointLabels,
                    .Sizes = If(serie.Size, serie.Y.Select(Function(y) 1.0).ToArray)
                })
            Next
        Next

        Return list
    End Function

    ''' <summary>
    ''' 把旧模型的形状枚举映射到新引擎的标记形状。
    ''' </summary>
    ''' <param name="shape"></param>
    ''' <returns></returns>
    Public Function toMarkerShape(shape As LegendStyles) As MarkerShape
        Select Case shape
            Case LegendStyles.Rectangle : Return MarkerShape.Square
            Case LegendStyles.Diamond : Return MarkerShape.Diamond
            Case LegendStyles.Triangle : Return MarkerShape.Triangle
            Case LegendStyles.Hexagon : Return MarkerShape.Hexagon
            Case LegendStyles.SolidLine, LegendStyles.DashLine : Return MarkerShape.None
            Case Else : Return MarkerShape.Circle
        End Select
    End Function

    ''' <summary>取某个点最终生效的颜色（逐点配色优先，其次序列颜色）</summary>
    Private Function pointColor(pt As PointData, serial As SerialData) As Color
        If pt.color.StringEmpty Then
            Return serial.color
        End If

        Try
            Return pt.color.TranslateColor
        Catch
            Return serial.color
        End Try
    End Function

    ''' <summary>气泡大小：所有点的 value 都是 0 时返回 Nothing（普通散点图不需要大小）</summary>
    Private Function bubbleSizes(part As PointData()) As Double()
        If part.All(Function(p) p.value = 0) Then
            Return Nothing
        Else
            Return part.Select(Function(p) p.value).ToArray
        End If
    End Function

    Private Function pointLabels(part As PointData()) As String()
        If part.All(Function(p) p.tag.StringEmpty) Then
            Return Nothing
        Else
            Return part.Select(Function(p) p.tag).ToArray
        End If
    End Function
End Module
