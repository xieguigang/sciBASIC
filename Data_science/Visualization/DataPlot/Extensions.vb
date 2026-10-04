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

    <Extension>
    Public Sub SavePng(plot As PlotEngine, filepath As String, Optional dpi As Integer = 300)
        Dim g As GdiRasterGraphics = DirectCast(plot.GetGraphics, GdiRasterGraphics)
        Dim res = g.ImageResource

        Call res.SaveAs(filepath)
    End Sub

    ''' <summary>
    ''' 把图表导出为图片文件，格式由扩展名决定（PNG / JPEG / BMP 等）。
    ''' </summary>
    ''' <remarks>
    ''' 只能用于自带位图的图表（由 <c>New(width, height)</c> 构造）。
    ''' 外部注入画布时（例如画在 WinForms 控件上）没有可导出的位图，这里会明确报错，
    ''' 而不是让调用方拿到一个空的 NullReference。
    ''' </remarks>
    <Extension>
    Public Sub Save(plot As PlotEngine, filepath As String, Optional dpi As Integer = 300)
        If plot Is Nothing Then Throw New ArgumentNullException(NameOf(plot))

        Dim raster = TryCast(plot.GetGraphics, GdiRasterGraphics)

        If raster Is Nothing Then
            Throw New InvalidOperationException(
                "This plot draws on an external graphics device and owns no bitmap; " &
                "it cannot be saved to a file directly.")
        End If

        Call raster.ImageResource.SaveAs(filepath)
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
    ''' 与 <see cref="Save"/> 一样，只有自带位图的图表（<c>New(width, height)</c>）才能取出图形数据。
    ''' </remarks>
    <Extension>
    Public Function AsGraphicsData(plot As PlotEngine) As GraphicsData
        If plot Is Nothing Then Throw New ArgumentNullException(NameOf(plot))

        Dim raster = TryCast(plot.GetGraphics, GdiRasterGraphics)

        If raster Is Nothing Then
            Throw New InvalidOperationException(
                "This plot draws on an external graphics device and owns no bitmap; " &
                "it cannot be converted into GraphicsData.")
        End If

        Return New ImageData(
            raster.ImageResource,
            New System.Drawing.Size(plot.CanvasWidth, plot.CanvasHeight),
            Microsoft.VisualBasic.MIME.Html.CSS.Padding.Zero)
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
