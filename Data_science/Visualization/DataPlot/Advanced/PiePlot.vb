#Region "Microsoft.VisualBasic::e488f6f1ae1b76296c5cba339f6a53a7, Data_science\Visualization\DataPlot\Advanced\PiePlot.vb"

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

    '   Total Lines: 95
    '    Code Lines: 80 (84.21%)
    ' Comment Lines: 5 (5.26%)
    '    - Xml Docs: 20.00%
    ' 
    '   Blank Lines: 10 (10.53%)
    '     File Size: 3.65 KB


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
Imports std = System.Math

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
        Dim radius = std.Min(_width, _height) * 0.32F

        Dim startA = StartAngle
        For i = 0 To n - 1
            Dim sweep = CSng(Values(i) / total * 360)
            Dim color = palette(i Mod palette.Length)
            Dim offsetX = 0.0F, offsetY = 0.0F
            If i = ExplodeIndex Then
                Dim midA = (startA + sweep / 2) * std.PI / 180
                offsetX = CSng(std.Cos(midA)) * 10
                offsetY = CSng(std.Sin(midA)) * 10
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
            Dim midA = (startA + sweep / 2) * std.PI / 180
            Dim labelR = radius * 1.15F
            Dim lx = cx + CSng(std.Cos(midA)) * labelR
            Dim ly = cy + CSng(std.Sin(midA)) * labelR
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
