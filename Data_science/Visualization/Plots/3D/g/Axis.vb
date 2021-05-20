#Region "Microsoft.VisualBasic::d0c85cfae97a6f91db89ea16450d2251, Data_science\Visualization\Plots\3D\g\Axis.vb"

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

    '     Module AxisDraw
    ' 
    '         Function: Axis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Plot3D.Model

    ''' <summary>
    ''' 在这个模块之中生成Axis的绘制元素
    ''' </summary>
    Public Module AxisDraw

        Public Function Axis(xrange As DoubleRange,
                             yrange As DoubleRange,
                             zrange As DoubleRange,
                             labelFont As Font,
                             labels As (X$, Y$, Z$),
                             Optional strokeCSS$ = Stroke.AxisStroke,
                             Optional arrowFactor$ = "2,2") As Element3D()

            ' 交汇于xmax, ymin, zmin
            Dim ZERO As New Point3D With {.X = xrange.Max, .Y = yrange.Min, .Z = zrange.Min}
            ' X: xmin, ymin, zmin
            ' Y: ymax, xmax, zmin
            ' Z: xmax, ymin, zmax
            Dim X As New Point3D With {.X = xrange.Min, .Y = yrange.Min, .Z = zrange.Min}
            Dim Y As New Point3D With {.X = xrange.Max, .Y = yrange.Max, .Z = zrange.Min}
            Dim Z As New Point3D With {.X = xrange.Max, .Y = yrange.Min, .Z = zrange.Max}
            Dim color As Pen = Stroke.TryParse(strokeCSS).GDIObject
            Dim bigArrow As AdjustableArrowCap

            With arrowFactor.FloatSizeParser
                bigArrow = New AdjustableArrowCap(
                    color.Width * .Width,
                    color.Width * .Height
                )
            End With

            color.CustomEndCap = bigArrow

            Dim Xaxis As New Line(ZERO, X) With {.Stroke = color}
            Dim Yaxis As New Line(ZERO, Y) With {.Stroke = color}
            Dim Zaxis As New Line(ZERO, Z) With {.Stroke = color}

            Dim labX As New Label With {.Location = X, .Font = labelFont, .Color = Brushes.Black, .Text = labels.X}
            Dim labY As New Label With {.Location = Y, .Font = labelFont, .Color = Brushes.Black, .Text = labels.Y}
            Dim labZ As New Label With {.Location = Z, .Font = labelFont, .Color = Brushes.Black, .Text = labels.Z}

            Return New Element3D() {Xaxis, Yaxis, Zaxis, labX, labY, labZ}
        End Function
    End Module
End Namespace
