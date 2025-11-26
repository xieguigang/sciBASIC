#Region "Microsoft.VisualBasic::94891ab95f46940f1d391acf7193f4bd, Data_science\Visualization\Plots\PrincipalCurveVisualizer.vb"

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

    '   Total Lines: 74
    '    Code Lines: 55 (74.32%)
    ' Comment Lines: 5 (6.76%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (18.92%)
    '     File Size: 2.79 KB


    ' Class PrincipalCurveVisualizer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: CalculateBounds, Draw
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports std = System.Math

Public Class PrincipalCurveVisualizer
    Private _dataPoints As List(Of Vector2D)
    Private _curvePoints As List(Of Vector2D)
    Private _bounds As RectangleF

    Public Sub New(dataPoints As List(Of Vector2D), curvePoints As List(Of Vector2D))
        Me._dataPoints = dataPoints
        Me._curvePoints = curvePoints
        CalculateBounds()
    End Sub

    Private Sub CalculateBounds()
        If _dataPoints.Count = 0 Then
            _bounds = New RectangleF(0, 0, 100, 100)
            Return
        End If

        Dim minX = _dataPoints.Min(Function(p) p.X)
        Dim maxX = _dataPoints.Max(Function(p) p.X)
        Dim minY = _dataPoints.Min(Function(p) p.Y)
        Dim maxY = _dataPoints.Max(Function(p) p.Y)

        ' 添加一些边距
        Dim marginX = (maxX - minX) * 0.1
        Dim marginY = (maxY - minY) * 0.1

        _bounds = New RectangleF(CSng(minX - marginX), CSng(minY - marginY),
                                CSng(maxX - minX + 2 * marginX), CSng(maxY - minY + 2 * marginY))
    End Sub

    ' 在Graphics对象上绘制
    Public Sub Draw(g As IGraphics, width As Integer, height As Integer)
        If _dataPoints.Count = 0 Then Return

        ' 计算缩放和偏移
        Dim scaleX = width / _bounds.Width
        Dim scaleY = height / _bounds.Height
        Dim scale = std.Min(scaleX, scaleY) * 0.9 ' 保留边距

        Dim offsetX = -_bounds.Left + (width / scale - _bounds.Width) / 2
        Dim offsetY = -_bounds.Top + (height / scale - _bounds.Height) / 2

        ' 绘制数据点
        Dim pointBrush As New SolidBrush(Color.Blue)
        Dim pointRadius As Single = 3

        For Each point In _dataPoints
            Dim screenX = CSng((point.X + offsetX) * scale)
            Dim screenY = CSng((point.Y + offsetY) * scale)
            g.FillEllipse(pointBrush, screenX - pointRadius, screenY - pointRadius,
                         pointRadius * 2, pointRadius * 2)
        Next

        ' 绘制主曲线
        If _curvePoints.Count > 1 Then
            Dim curvePen As New Pen(Color.Red, 2)
            Dim curvePoints = _curvePoints.Select(Function(p)
                                                      Return New PointF(CSng((p.X + offsetX) * scale), CSng((p.Y + offsetY) * scale))
                                                  End Function).ToArray()

            g.DrawLines(curvePen, curvePoints)
            curvePen.Dispose()
        End If

        pointBrush.Dispose()
    End Sub
End Class
