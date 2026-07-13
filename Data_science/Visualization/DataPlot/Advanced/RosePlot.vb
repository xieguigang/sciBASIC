#Region "Microsoft.VisualBasic::d47a44b7329833b744adc961f5a7a05e, Data_science\Visualization\DataPlot\Advanced\RosePlot.vb"

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

    '   Total Lines: 109
    '    Code Lines: 87 (79.82%)
    ' Comment Lines: 11 (10.09%)
    '    - Xml Docs: 45.45%
    ' 
    '   Blank Lines: 11 (10.09%)
    '     File Size: 4.34 KB


    ' Class RosePlot
    ' 
    '     Properties: Colors, Donut, DonutRadius, Labels, ShowPercentage
    '                 ShowValues, StartAngle, Values
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>南丁格尔玫瑰图（等角度扇区，半径与数值成比例）</summary>
Public Class RosePlot
    Inherits PlotEngine

    Public Property Labels As String() = {}
    Public Property Values As Double() = {}
    Public Property Colors As Color() = Nothing
    ''' <summary>是否环形（中心挖空）</summary>
    Public Property Donut As Boolean = False
    Public Property DonutRadius As Single = 0.3F
    ''' <summary>显示百分比标签</summary>
    Public Property ShowPercentage As Boolean = False
    ''' <summary>起始角度（度，默认 -90 即 12 点方向）</summary>
    Public Property StartAngle As Single = -90
    ''' <summary>显示数值标签</summary>
    Public Property ShowValues As Boolean = True

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim n = Values.Length
        Dim palette = If(Colors, Theme.Palette)
        Dim total = Values.Sum()
        Dim vmax = If(total > 0, Values.Max(), 1)
        If vmax <= 0 Then vmax = 1

        ' 圆心与最大半径
        Dim cx = _width / 2.0F
        Dim cy = (_height + Theme.MarginTop) / 2.0F
        Dim maxR = Math.Min(_width - Theme.MarginLeft - Theme.MarginRight,
                            _height - Theme.MarginTop - Theme.MarginBottom) * 0.42F

        ' 同心参考圆（网格）
        If Theme.ShowGrid Then
            Using pen As New Pen(Theme.GridColor, Theme.GridLineWidth)
                For g = 1 To 4
                    Dim r = maxR * g / 4
                    _g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2)
                Next
            End Using
        End If

        ' 等角度扇区
        Dim sweep = 360.0F / Math.Max(1, n)
        Dim startA = StartAngle
        For i = 0 To n - 1
            Dim ri = CSng(Math.Abs(Values(i)) / vmax * maxR)
            If ri < 1 Then ri = 1
            Dim color = palette(i Mod palette.Length)
            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BackgroundColor, 1.5F)
                _g.FillPie(br, cx - ri, cy - ri, ri * 2, ri * 2, startA, sweep)
                _g.DrawPie(pen, cx - ri, cy - ri, ri * 2, ri * 2, startA, sweep)
            End Using
            startA += sweep
        Next

        ' 环形挖空
        If Donut Then
            Using br As New SolidBrush(Theme.BackgroundColor)
                _g.FillEllipse(br, cx - maxR * DonutRadius, cy - maxR * DonutRadius,
                               maxR * DonutRadius * 2, maxR * DonutRadius * 2)
            End Using
        End If

        ' 标签
        startA = StartAngle
        For i = 0 To n - 1
            Dim midA = (startA + sweep / 2) * Math.PI / 180
            Dim labelR = maxR * 1.12F
            Dim lx = cx + CSng(Math.Cos(midA)) * labelR
            Dim ly = cy + CSng(Math.Sin(midA)) * labelR
            Dim label = If(i < Labels.Length, Labels(i), "")
            If ShowPercentage AndAlso total > 0 Then
                label &= String.Format(" ({0:P1})", Values(i) / total)
            ElseIf ShowValues Then
                label &= " " & FormatNumber(Values(i))
            End If
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(label, Theme.TickLabelFont, br, lx, ly)
            End Using
            startA += sweep
        Next

        ' 图例（多类别且无自定义颜色时可选）
        If ShowLegend AndAlso n > 1 AndAlso Colors Is Nothing AndAlso Labels.Length = n Then
            Dim seriesList As New List(Of Series)()
            For i = 0 To n - 1
                seriesList.Add(New Series With {
                    .Name = Labels(i),
                    .Color = palette(i Mod palette.Length),
                    .MarkerShape = MarkerShape.Square
                })
            Next
            DrawLegend(seriesList)
        End If
    End Sub
End Class

