#Region "Microsoft.VisualBasic::0746efd6b07879d919c0a30dcfbdb161, Data_science\Visualization\Plots\g\Legends\ColorBar\DualColorBar.vb"

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

    '   Total Lines: 84
    '    Code Lines: 70 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (16.67%)
    '     File Size: 3.33 KB


    '     Module DualColorBar
    ' 
    '         Sub: DrawDualColorBar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

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
#End If

Namespace Graphic.Legend

    Public Module DualColorBar

        <Extension>
        Public Sub DrawDualColorBar(canvas As IGraphics,
                                    color1 As SolidBrush(),
                                    color2 As SolidBrush(),
                                    layout As Rectangle,
                                    ticks As Double(),
                                    axisPen As Pen,
                                    tickPen As Pen,
                                    title As String,
                                    titleFont As Font,
                                    tickFont As Font,
                                    tickFormat As String)

            Dim width As Integer = layout.Width / 2.5
            Dim d As Double = layout.Width - width * 2
            Dim dy As Double = layout.Height / color1.Length
            Dim x As Double = layout.Left
            Dim y As Double = layout.Top

            For Each color As SolidBrush In color1.Reverse
                canvas.FillRectangle(color, New Rectangle(x, y, width, dy))
                y += dy
            Next

            x += d + width
            y = layout.Top

            For Each color As SolidBrush In color2.Reverse
                canvas.FillRectangle(color, New Rectangle(x, y, width, dy))
                y += dy
            Next

            Dim titleSize As SizeF = canvas.MeasureString(title, titleFont)

            x = (layout.Width - titleSize.Width) / 2 + layout.Left
            y = layout.Top - titleSize.Height - 20

            Call canvas.DrawString(title, titleFont, Brushes.Black, New PointF(x, y))
            Call canvas.DrawLine(axisPen, New PointF(layout.Top, layout.Right), New PointF(layout.Bottom, layout.Right))

            x = layout.Right + 10
            y = layout.Top
            dy = layout.Height / (ticks.Length + 1)
            d = canvas.MeasureString("0", tickFont).Height / 2

            For Each tick As Double In ticks.Reverse
                canvas.DrawString(tick.ToString(tickFormat), tickFont, Brushes.Black, New PointF(x, y - d))
                canvas.DrawLine(tickPen, New PointF(layout.Right, y), New PointF(x, y))
                y += dy
            Next
        End Sub

    End Module
End Namespace
