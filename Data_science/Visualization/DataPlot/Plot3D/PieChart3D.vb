' ============================================================================
'  PieChart3D.vb - 3D 饼图
'
'  从旧 Plots 项目的 3D/PieChart3D.vb 迁移而来：
'   1. 去掉对旧绘图库 ChartPlots.Fractions.FractionData 的依赖，
'      改用本文件内的 PieSlice 数据模型；
'   2. 3D 模型元素（Pie）仍然来自 imaging 项目的 Drawing3D 等距投影模型；
'   3. 渲染输出走 g.GraphicsPlots，支持 driver 参数。
' ============================================================================

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Matrix = Microsoft.VisualBasic.Imaging.Drawing3D.Math3D.Matrix

Namespace Plot3D

    ''' <summary>
    ''' 3D 饼图的数据切片
    ''' </summary>
    Public Class PieSlice

        Public Property Name As String
        Public Property Value As Double
        Public Property Color As Color

        ''' <summary>当前切片所占的百分比（由 <see cref="AsSlices"/> 计算填充）</summary>
        Public Property Percentage As Single

        Public Overrides Function ToString() As String
            Return $"[{Name}, {Percentage.ToString("F2")}%]"
        End Function

        ''' <summary>将一组名称-数值对转换为带有百分比信息的饼图切片</summary>
        Public Shared Function AsSlices(data As IEnumerable(Of NamedValue(Of Double)), colors As IEnumerable(Of Color)) As PieSlice()
            Dim raw As NamedValue(Of Double)() = data.ToArray
            Dim total As Double = raw.Select(Function(a) a.Value).Sum
            Dim c As Color() = colors.ToArray
            Dim slices As PieSlice() = raw _
                .SeqIterator _
                .Select(Function(a)
                            Return New PieSlice With {
                                .Name = a.value.Name,
                                .Value = a.value.Value,
                                .Color = c(a.i Mod c.Length),
                                .Percentage = If(total = 0, 0, CSng(a.value.Value / total * 100))
                            }
                        End Function) _
                .ToArray

            Return slices
        End Function
    End Class

    ''' <summary>
    ''' 3D pie chart
    ''' </summary>
    Public Module PieChart3D

        ''' <summary>
        ''' render the 3d pie chart
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="camera"></param>
        ''' <param name="driver"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot3D(data As IEnumerable(Of PieSlice),
                               camera As Camera,
                               Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim start As New f64
            Dim sweep As New f64
            Dim alpha!
            Dim pt As PointF
            Dim centra As Point3D = camera.screen.GetCenter
            Dim r! = 2.0!
            Dim pie As Pie
            Dim pieChart As New List(Of Surface)

            For Each x As PieSlice In data
                pie = New Pie(centra, r, (start = ((+start) + (sweep = CSng(360 * x.Percentage / 100)))) - sweep.Value, sweep, 20, 1)
                pieChart += pie.Model3D(x.Color)
                alpha = (+start) - (+sweep / 2)
                ' 在这里r/1.5是因为这些百分比的值的标签需要显示在pie的内部
                pt = (r / 1.5, alpha).ToCartesianPoint()
                pt = New PointF(pt.X + centra.X, pt.Y + centra.Y)
            Next

            Dim plot3DInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    With pieChart.Centroid
                        Dim matrix As New Matrix(.Offsets(pieChart))
                        Dim vector = camera.Rotate(matrix.Matrix)
                        Dim model2D = matrix _
                            .TranslateBuffer(camera, vector, True) _
                            .AsList

                        Call g.BufferPainting(model2D)
                    End With
                End Sub

            Return g.GraphicsPlots(
                camera.screen,
                g.DefaultPadding,
                "white",
                plot3DInternal,
                driver:=driver
            )
        End Function
    End Module
End Namespace