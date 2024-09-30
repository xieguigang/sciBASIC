#Region "Microsoft.VisualBasic::0e7252414a40c35c8b18e15c9358f65c, Data_science\Visualization\Plots\3D\g\Axis.vb"

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

'   Total Lines: 65
'    Code Lines: 50 (76.92%)
' Comment Lines: 7 (10.77%)
'    - Xml Docs: 42.86%
' 
'   Blank Lines: 8 (12.31%)
'     File Size: 2.88 KB


'     Module AxisDraw
' 
'         Function: Axis
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
Imports AdjustableArrowCap = System.Drawing.Drawing2D.AdjustableArrowCap
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
Imports AdjustableArrowCap = Microsoft.VisualBasic.Imaging.AdjustableArrowCap
#End If

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
            Dim labX As New Label With {.Location = X, .FontCss = labelFontCss, .Color = labelColor, .Text = labels.X}
            Dim labY As New Label With {.Location = Y, .FontCss = labelFontCss, .Color = labelColor, .Text = labels.Y}
            Dim labZ As New Label With {
                .Location = New Point3D(Z.X, Z.Y, Z.Z + zrange.Length / 20),
                .FontCss = labelFontCss,
                .Color = labelColor,
                .Text = labels.Z
            }

            Return New Element3D() {Xaxis, Yaxis, Zaxis, labX, labY, labZ}
        End Function
    End Module
End Namespace
