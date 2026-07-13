#Region "Microsoft.VisualBasic::f5b08197728759a96b271cdff274de43, Data_science\Visualization\DataPlot\Advanced\PiePlot.vb"

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

    '   Total Lines: 94
    '    Code Lines: 79 (84.04%)
    ' Comment Lines: 5 (5.32%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 10 (10.64%)
    '     File Size: 3.63 KB


    ' Class PiePlot
    ' 
    '     Properties: Colors, Donut, DonutRadius, ExplodeIndex, Labels
    '                 ShowPercentage, StartAngle, Values
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>饼图</summary>
Public Class PiePlot
    Inherits PlotEngine

    Public Property Labels As String() = {}
    Public Property Values As Double() = {}
    Public Property Colors As Color() = Nothing
    Public Property Donut As Boolean = False
    Public Property DonutRadius As Single = 0.5F
    Public Property ShowPercentage As Boolean = True
    Public Property ExplodeIndex As Integer = -1
    Public Property StartAngle As Single = -90

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim total = Values.Sum()
        Dim n = Values.Length
        Dim palette = If(Colors, Theme.Palette)

        ' 饼图区域
        Dim cx = _width / 2.0F
        Dim cy = (_height + Theme.MarginTop) / 2.0F
        Dim radius = Math.Min(_width, _height) * 0.32F

        Dim startA = StartAngle
        For i = 0 To n - 1
            Dim sweep = CSng(Values(i) / total * 360)
            Dim color = palette(i Mod palette.Length)
            Dim offsetX = 0.0F, offsetY = 0.0F
            If i = ExplodeIndex Then
                Dim midA = (startA + sweep / 2) * Math.PI / 180
                offsetX = CSng(Math.Cos(midA)) * 10
                offsetY = CSng(Math.Sin(midA)) * 10
            End If
            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BackgroundColor, 2)
                _g.FillPie(br, cx - radius + offsetX, cy - radius + offsetY,
                           radius * 2, radius * 2, startA, sweep)
                _g.DrawPie(pen, cx - radius + offsetX, cy - radius + offsetY,
                           radius * 2, radius * 2, startA, sweep)
            End Using
            startA += sweep
        Next

        ' 环形图中心挖空
        If Donut Then
            Using br As New SolidBrush(Theme.BackgroundColor)
                _g.FillEllipse(br, cx - radius * DonutRadius, cy - radius * DonutRadius,
                               radius * DonutRadius * 2, radius * DonutRadius * 2)
            End Using
        End If

        ' 标签
        startA = StartAngle
        For i = 0 To n - 1
            Dim sweep = CSng(Values(i) / total * 360)
            Dim midA = (startA + sweep / 2) * Math.PI / 180
            Dim labelR = radius * 1.15F
            Dim lx = cx + CSng(Math.Cos(midA)) * labelR
            Dim ly = cy + CSng(Math.Sin(midA)) * labelR
            Dim label = Labels(i)
            If ShowPercentage Then
                label &= String.Format(" ({0:P1})", Values(i) / total)
            End If
            Using br As New SolidBrush(Theme.TextColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(label, Theme.TickLabelFont, br, lx, ly)
            End Using
            startA += sweep
        Next

        ' 中心文字（环形图）
        If Donut Then
            Using br As New SolidBrush(Theme.TitleColor),
                  sf As New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                _g.DrawString(If(String.IsNullOrEmpty(Title), "", ""),
                              Theme.AxisLabelFont, br, cx, cy)
            End Using
        End If
    End Sub
End Class
