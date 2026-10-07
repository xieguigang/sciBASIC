#Region "Microsoft.VisualBasic::30f0e198a36ddc84bc4a9fe2f2201d3a, Data_science\Visualization\DataPlot\Extensions.vb"

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

    '   Total Lines: 34
    '    Code Lines: 29 (85.29%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (14.71%)
    '     File Size: 1.37 KB


    ' Module Extensions
    ' 
    '     Function: DataSerials
    ' 
    '     Sub: SavePng
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module Extensions

    ''' <summary>
    ''' 把图表导出为图片文件，格式由绘图驱动与文件扩展名共同决定：
    ''' GDI 驱动输出 png/jpeg/bmp 等栅格图，SVG 驱动输出矢量 svg 文件，
    ''' PostScript 驱动输出 ps 文件，PDF 驱动输出 pdf 文件。
    ''' </summary>
    ''' <remarks>
    ''' 适用于由 <c>New(width, height)</c> 构造（引擎自建画布）的图表，
    ''' 画布驱动类型在构造函数中通过 <c>driver</c> 参数指定。
    ''' 外部注入画布时（例如画在 WinForms 控件上）没有可导出的图形数据，这里会明确报错，
    ''' 而不是让调用方拿到一个空的 NullReference。
    ''' </remarks>
    <Extension>
    Public Sub SavePng(plot As PlotEngine, filepath As String, Optional dpi As Integer = 300)
        Call plot.AsGraphicsData().Save(filepath)
    End Sub

    ''' <summary>
    ''' 把图表导出为图片文件，格式由扩展名决定（PNG / JPEG / BMP 等栅格图，
    ''' 或者 SVG / PS 等矢量图形文件）。
    ''' </summary>
    ''' <remarks>
    ''' 适用于自带画布的图表（由 <c>New(width, height)</c> 构造）。
    ''' 外部注入画布时（例如画在 WinForms 控件上）没有可导出的图形数据，这里会明确报错，
    ''' 而不是让调用方拿到一个空的 NullReference。
    ''' </remarks>
    <Extension>
    Public Sub Save(plot As PlotEngine, filepath As String, Optional dpi As Integer = 300)
        Call plot.AsGraphicsData().Save(filepath)
    End Sub

    ''' <summary><see cref="Save"/> 的同义写法，方便链式书写。</summary>
    <Extension>
    Public Sub SaveChanges(plot As PlotEngine, filepath As String, Optional dpi As Integer = 300)
        Call plot.Save(filepath, dpi)
    End Sub

    ''' <summary>
    ''' 取回图表渲染结果的图形数据对象（<c>GraphicsData</c>），
    ''' 这样老代码里「返回 GraphicsData 再 .Save(...)」的写法可以无缝换成 DataPlot。
    ''' </summary>
    ''' <remarks>
    ''' 输出的具体 <c>GraphicsData</c> 派生类型由画布所使用的绘图驱动决定：
    ''' GDI 驱动得到 <see cref="ImageData"/>，SVG 驱动得到 <see cref="SVGData"/>，
    ''' PostScript 驱动得到 <see cref="PostScriptData"/>。
    ''' 与 <see cref="Save"/> 一样，只有引擎自建画布的图表（<c>New(width, height)</c>）才能取出图形数据。
    ''' </remarks>
    <Extension>
    Public Function AsGraphicsData(plot As PlotEngine) As GraphicsData
        If plot Is Nothing Then Throw New ArgumentNullException(NameOf(plot))

        Dim g As IGraphics = plot.GetGraphics
        Dim raster As GdiRasterGraphics = TryCast(g, GdiRasterGraphics)

        If Not raster Is Nothing Then
            ' GDI 栅格画布：直接包装位图，保持与旧版本一致的向后兼容行为
            ' （外部注入的 GDI 画布也可以在这里取出图形数据）
            Return New ImageData(
                raster.ImageResource,
                New System.Drawing.Size(plot.CanvasWidth, plot.CanvasHeight),
                Microsoft.VisualBasic.MIME.Html.CSS.Padding.Zero)
        Else
            ' 非 GDI 驱动（SVG / PostScript / PDF / Skia 等）：
            ' 通过驱动注册表按 g.Driver 反查对应的驱动实现，
            ' 由驱动自己产出对应格式的 GraphicsData 派生对象
            Dim data As IGraphicsData = DriverLoad.GetData(g, New Integer() {})

            If TypeOf data Is GraphicsData Then
                Return DirectCast(data, GraphicsData)
            Else
                Throw New InvalidOperationException(
                    $"The graphics driver '{g.Driver.Description}' was not returns a valid GraphicsData object.")
            End If
        End If
    End Function

    Public Iterator Function DataSerials(x As Double(), y As Double(), class_id As String()) As IEnumerable(Of Series)
        Dim groups = class_id.Select(Function(cid, i) (x(i), y(i), cid)).GroupBy(Function(a) a.cid)
        Dim colors As LoopArray(Of Color) = Designer.GetColors("paper")

        For Each serial_group In groups
            Yield New Series With {
                .Color = ++colors,
                .LineStyle = DashStyle.Dot,
                .MarkerShape = MarkerShape.Circle,
                .Name = serial_group.Key,
                .Visible = True,
                .X = serial_group.Select(Function(a) a.Item1).ToArray,
                .Y = serial_group.Select(Function(a) a.Item2).ToArray
            }
        Next
    End Function
End Module
