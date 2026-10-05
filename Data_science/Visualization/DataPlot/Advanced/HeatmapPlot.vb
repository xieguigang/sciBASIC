#Region "Microsoft.VisualBasic::dd962bc2891aebbb66ce14d150e21aee, Data_science\Visualization\DataPlot\Advanced\HeatmapPlot.vb"

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

    '   Total Lines: 196
    '    Code Lines: 169 (86.22%)
    ' Comment Lines: 8 (4.08%)
    '    - Xml Docs: 12.50%
    ' 
    '   Blank Lines: 19 (9.69%)
    '     File Size: 7.94 KB


    ' Class HeatmapPlot
    ' 
    '     Properties: ColLabels, ColorMap, Matrix, MaxValue, MinValue
    '                 RowLabels, ShowValues
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    '     Enum ColorMapType
    ' 
    '         CoolWarm, Grayscale, Inferno, Jet, Plasma
    '         Viridis
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: Brightness, GetColor, LerpPalette
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports stdf = System.Math

''' <summary>热图</summary>
Public Class HeatmapPlot
    Inherits PlotEngine

    Public Property Matrix As Double(,) = Nothing
    Public Property RowLabels As String() = Nothing
    Public Property ColLabels As String() = Nothing
    Public Property MinValue As Double? = Nothing
    Public Property MaxValue As Double? = Nothing

    ''' <summary>色阶方案（取色逻辑统一走 <see cref="ColorScale"/>）</summary>
    Public Property ColorMap As ColorScale.ColorMapType = ColorScale.ColorMapType.Viridis
    ''' <summary>是否在每个格子里写出数值</summary>
    Public Property ShowValues As Boolean = False
    ''' <summary>是否绘制色阶图例条</summary>
    Public Property ShowColorLegend As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default)
        MyBase.New(width, height, theme, driver)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim nRow = Matrix.GetLength(0)
        Dim nCol = Matrix.GetLength(1)

        Dim vmin = If(MinValue, Matrix.Cast(Of Double).Min())
        Dim vmax = If(MaxValue, Matrix.Cast(Of Double).Max())
        If vmax <= vmin Then vmax = vmin + 1

        ' 留出标签空间
        Dim leftPad = If(RowLabels IsNot Nothing, 100, Theme.MarginLeft)
        Dim topPad = If(ColLabels IsNot Nothing, 100, Theme.MarginTop)
        Dim rightPad = 80 ' colorbar
        Dim bottomPad = 50

        Dim plotX = leftPad
        Dim plotY = topPad
        Dim plotW = _width - leftPad - rightPad
        Dim plotH = _height - topPad - bottomPad
        _plotArea = New RectangleF(plotX, plotY, plotW, plotH)

        Dim cellW = plotW / nCol
        Dim cellH = plotH / nRow

        ' 绘制单元格
        For r = 0 To nRow - 1
            For c = 0 To nCol - 1
                Dim v = Matrix(r, c)
                Dim color = ColorScale.GetColor(v, vmin, vmax, ColorMap)
                Dim rect = New RectangleF(plotX + c * cellW, plotY + r * cellH, cellW, cellH)
                Using br As New SolidBrush(color)
                    _g.FillRectangle(br, rect)
                End Using
                If ShowValues Then
                    Dim txtColor = ColorScale.ReadableTextColor(color)
                    Using br As New SolidBrush(txtColor),
                          sf As New StringFormat()
                        sf.Alignment = StringAlignment.Center
                        sf.LineAlignment = StringAlignment.Center
                        _g.DrawString(FormatNumber(v), Theme.TickLabelFont, br,
                                      rect.X + rect.Width / 2, rect.Y + rect.Height / 2)
                    End Using
                End If
            Next
        Next

        ' 边框
        Using pen As New Pen(Theme.BorderColor, 0.5F)
            _g.DrawRectangle(pen, plotX, plotY, plotW, plotH)
        End Using

        ' 行标签
        If RowLabels IsNot Nothing Then
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Far
                sf.LineAlignment = StringAlignment.Center
                For r = 0 To nRow - 1
                    _g.DrawString(RowLabels(r), Theme.TickLabelFont, br,
                                  plotX - 6, plotY + (r + 0.5) * cellH)
                Next
            End Using
        End If

        ' 列标签（旋转 45°）
        If ColLabels IsNot Nothing Then
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Far
                sf.LineAlignment = StringAlignment.Center
                For c = 0 To nCol - 1
                    _g.TranslateTransform(plotX + (c + 0.5) * cellW, plotY - 6)
                    _g.RotateTransform(-45)
                    _g.DrawString(ColLabels(c), Theme.TickLabelFont, br, 0, 0)
                    _g.ResetTransform()
                Next
            End Using
        End If

        ' Colorbar（统一复用 ColorScale 的绘制实现）
        If ShowColorLegend Then
            ColorScale.DrawColorLegend(_g, Theme, ColorMap, vmin, vmax,
                                       plotX + plotW + 20, plotY, 16, plotH,
                                       horizontal:=False, tickCount:=5)
        End If
    End Sub
End Class
