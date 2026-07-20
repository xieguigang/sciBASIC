Imports System.Drawing
Imports System.IO
Imports DataPlot
Imports Microsoft.VisualBasic.Math.Scripting

''' <summary>
''' 二维绘图桥接：将脚本引擎产出的二维 PlotCommand 转交给 DataPlot 的
''' ScatterPlot / LinePlot 渲染到内存位图，供主窗口 PictureBox 显示。
''' 该类型仅依赖 DataPlot 与本程序集内的 PlotCommand，不引用任何 3D 组件。
''' </summary>
Public Class DataPlotView

    ' 预定义调色板（System.Drawing.Color）
    Private Shared ReadOnly Palette As Color() = {
        Color.FromArgb(33, 102, 172),
        Color.FromArgb(178, 24, 43),
        Color.FromArgb(255, 127, 0),
        Color.FromArgb(51, 160, 44),
        Color.FromArgb(106, 61, 154),
        Color.FromArgb(214, 96, 77),
        Color.FromArgb(0, 153, 153),
        Color.FromArgb(204, 121, 167)
    }

    ''' <summary>
    ''' 将脚本产生的二维绘图指令渲染到位图（System.Drawing.Bitmap，供 PictureBox 使用）。
    ''' 只要存在散点（Scatter）指令就使用 ScatterPlot 引擎，否则使用 LinePlot 引擎。
    ''' </summary>
    Public Shared Function Render(commands As List(Of PlotCommand), w As Integer, h As Integer) As System.Drawing.Bitmap
        If commands Is Nothing OrElse commands.Count = 0 Then Return Nothing
        If w < 1 Then w = 1
        If h < 1 Then h = 1

        ' DataPlot 底层使用 Microsoft.VisualBasic.Imaging.Bitmap（GDI 内存位图）
        Dim bmp As New Microsoft.VisualBasic.Imaging.Bitmap(w, h)

        Dim seriesList As New List(Of Series)()
        Dim idx As Integer = 0

        For Each c In commands
            If c.Is3D Then Continue For
            If c.X Is Nothing OrElse c.Y Is Nothing Then Continue For

            Dim s As New Series() With {
                .Name = If(String.IsNullOrEmpty(c.Label), c.Kind.ToString(), c.Label),
                .X = c.X,
                .Y = c.Y,
                .Color = Palette(idx Mod Palette.Length)
            }

            If c.Kind = PlotKind.Scatter Then
                s.MarkerShape = MarkerShape.Circle
                ' 用 Custom 关闭连线（ScatterPlot 中 <> Custom 才画线）
                s.LineStyle = Microsoft.VisualBasic.Imaging.DashStyle.Custom
            Else
                s.MarkerShape = MarkerShape.None
                s.LineStyle = Microsoft.VisualBasic.Imaging.DashStyle.Solid
            End If

            seriesList.Add(s)
            idx += 1
        Next

        If seriesList.Count = 0 Then
            Return ConvertBitmap(bmp)
        End If

        Dim rendered As Microsoft.VisualBasic.Imaging.Bitmap

        If commands.Any(Function(c) c.Kind = PlotKind.Scatter AndAlso Not c.Is3D) Then
            Using p As New ScatterPlot(bmp)
                p.Plot(seriesList)
                rendered = p.ToBitmap()
            End Using
        Else
            Using p As New LinePlot(bmp)
                p.Plot(seriesList)
                rendered = p.ToBitmap()
            End Using
        End If

        Return ConvertBitmap(rendered)
    End Function

    ''' <summary>
    ''' 将 Microsoft.VisualBasic.Imaging.Bitmap（GDI 内存位图）转换为
    ''' System.Drawing.Bitmap，以便直接赋值给 PictureBox.Image。
    ''' </summary>
    Private Shared Function ConvertBitmap(src As Microsoft.VisualBasic.Imaging.Bitmap) As System.Drawing.Bitmap
        Using ms As New MemoryStream()
            src.Save(ms, Microsoft.VisualBasic.Imaging.ImageFormats.Bmp)
            ms.Seek(0, SeekOrigin.Begin)
            ' 从流加载后立刻复制一份，使返回的位图不再依赖原始流
            Using loaded = System.Drawing.Bitmap.FromStream(ms)
                Return New System.Drawing.Bitmap(loaded)
            End Using
        End Using
    End Function

End Class
