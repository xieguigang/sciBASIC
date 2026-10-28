#Region "Microsoft.VisualBasic::f30c1d354ea5aec397eed0da56e18b8a, Data_science\Visualization\DataPlot\Plot3D\Model\Axis.vb"

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

    '   Total Lines: 91
    '    Code Lines: 75 (82.42%)
    ' Comment Lines: 7 (7.69%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 9 (9.89%)
    '     File Size: 4.11 KB


    '     Module AxisDraw
    ' 
    '         Function: Axis
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Plot3D.Model

    ''' <summary>
    ''' 在这个模块之中生成Axis的绘制元素
    ''' </summary>
    Public Module AxisDraw

        <Extension>
        Public Function Axis(css As CSSEnvirnment,
                             xrange As DoubleRange,
                             yrange As DoubleRange,
                             zrange As DoubleRange,
                             labelFontCss As String,
                             labelColorVal As String,
                             labels As (X$, Y$, Z$),
                             Optional strokeCSS$ = Stroke.AxisStroke,
                             Optional arrowFactor$ = "1,2") As Element3D()

            ' 交汇于xmax, ymin, zmin
            Dim ZERO As New Point3D With {.X = xrange.Max, .Y = yrange.Min, .Z = zrange.Min}
            ' X: xmin, ymin, zmin
            ' Y: ymax, xmax, zmin
            ' Z: xmax, ymin, zmax
            Dim X As New Point3D With {.X = xrange.Min, .Y = yrange.Min, .Z = zrange.Min}
            Dim Y As New Point3D With {.X = xrange.Max, .Y = yrange.Max, .Z = zrange.Min}
            Dim Z As New Point3D With {.X = xrange.Max, .Y = yrange.Min, .Z = zrange.Max}
            Dim color As Pen = css.GetPen(Stroke.TryParse(strokeCSS))
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
            Dim labelColor As Brush = labelColorVal.GetBrush

            ' 轴标题沿各自的轴向外再推出一段距离，避免与新增的刻度值文本相互叠压
            Dim dx As Double = xrange.Length * 0.08
            Dim dy As Double = yrange.Length * 0.08
            Dim dz As Double = zrange.Length * 0.18
            Dim labX As New Label With {
                .Location = New Point3D(X.X - dx, X.Y, X.Z),
                .FontCss = labelFontCss,
                .Color = labelColor,
                .Text = labels.X
            }
            Dim labY As New Label With {
                .Location = New Point3D(Y.X, Y.Y + dy, Y.Z),
                .FontCss = labelFontCss,
                .Color = labelColor,
                .Text = labels.Y
            }
            Dim labZ As New Label With {
                .Location = New Point3D(Z.X, Z.Y, Z.Z + dz),
                .FontCss = labelFontCss,
                .Color = labelColor,
                .Text = labels.Z
            }

            Return New Element3D() {Xaxis, Yaxis, Zaxis, labX, labY, labZ}
        End Function
    End Module
End Namespace
