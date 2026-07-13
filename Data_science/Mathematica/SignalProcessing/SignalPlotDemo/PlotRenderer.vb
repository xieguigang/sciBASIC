#Region "Microsoft.VisualBasic::d80227fee8e7f2894e80a040f8382dfb, Data_science\Mathematica\SignalProcessing\SignalPlotDemo\PlotRenderer.vb"

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

    '   Total Lines: 86
    '    Code Lines: 52 (60.47%)
    ' Comment Lines: 24 (27.91%)
    '    - Xml Docs: 79.17%
    ' 
    '   Blank Lines: 10 (11.63%)
    '     File Size: 3.51 KB


    ' Module PlotRenderer
    ' 
    '     Function: Render, RenderSingle
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.SignalProcessing.Source.Generators
Imports DataPlot
Imports Microsoft.VisualBasic.Drawing

''' <summary>
''' 将信号生成器的结果渲染为 DataPlot 折线图，输出 System.Drawing.Bitmap 供 PictureBox 显示。
''' </summary>
Public Module PlotRenderer

    ''' <summary>
    ''' 将一个或多个 BasisFunction 渲染到一张折线图 Bitmap 上。
    ''' </summary>
    ''' <param name="xMin">采样起始 x</param>
    ''' <param name="xMax">采样结束 x</param>
    ''' <param name="n">采样点数</param>
    ''' <param name="width">图表像素宽度</param>
    ''' <param name="height">图表像素高度</param>
    ''' <param name="functions">要绘制的信号函数（名称 + 函数对象）</param>
    ''' <param name="title">图表标题</param>
    ''' <param name="xLabel">x 轴标签</param>
    ''' <param name="yLabel">y 轴标签</param>
    ''' <param name="theme">绘图主题</param>
    Public Function Render(xMin As Double, xMax As Double, n As Integer,
                           width As Integer, height As Integer,
                           functions As IEnumerable(Of (name As String, f As BasisFunction)),
                           Optional title As String = "",
                           Optional xLabel As String = "x",
                           Optional yLabel As String = "y",
                           Optional theme As PlotTheme = Nothing) As Bitmap

        If theme Is Nothing Then theme = PlotTheme.Light()

        ' 生成 x 网格
        Dim xs(n - 1) As Double
        Dim stepX = (xMax - xMin) / (n - 1)
        For i As Integer = 0 To n - 1
            xs(i) = xMin + i * stepX
        Next

        ' 构建 Series 列表
        Dim seriesList As New List(Of Series)
        For Each fn In functions
            Dim yData = fn.f.Sample(xs)
            seriesList.Add(New Series With {
                .Name = fn.name,
                .X = xs,
                .Y = yData,
                .MarkerShape = MarkerShape.None,
                .LineStyle = Drawing2D.DashStyle.Solid
            })
        Next

        ' 使用 LinePlot 绘制
        Using plot As New LinePlot(width, height, theme)
            plot.Title = title
            plot.XLabel = xLabel
            plot.YLabel = yLabel
            plot.ShowLegend = True
            plot.Plot(seriesList)

            ' 提取 Bitmap — GdiRasterGraphics 底层存储为 Bitmap，但接口声明为 Image
            ' 通过 Object 中转以绕过编译时窄化转换检查
            Dim g = TryCast(plot.GetGraphics(), GdiRasterGraphics)
            If g IsNot Nothing AndAlso g.ImageResource IsNot Nothing Then
                Return g.ImageResource.CTypeGdiImage
            End If
        End Using

        Return Nothing
    End Function

    ''' <summary>
    ''' 重载：渲染单个 BasisFunction
    ''' </summary>
    Public Function RenderSingle(f As BasisFunction, name As String,
                                  xMin As Double, xMax As Double, n As Integer,
                                  width As Integer, height As Integer,
                                  Optional theme As PlotTheme = Nothing) As Bitmap
        Return Render(xMin, xMax, n, width, height,
                     {(name, f)},
                     title:=name, theme:=theme)
    End Function

End Module

