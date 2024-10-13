#Region "Microsoft.VisualBasic::529a94a4af503878839eb45ec5a007a0, Data_science\Visualization\Plots\g\Legends\CircleSizeLegend.vb"

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

    '   Total Lines: 59
    '    Code Lines: 50 (84.75%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (15.25%)
    '     File Size: 2.27 KB


    '     Class CircleSizeLegend
    ' 
    '         Properties: circleStroke, radius, radiusFont, title, titleFont
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
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

    Public Class CircleSizeLegend

        Public Property radius As Integer()
        Public Property title As String
        Public Property titleFont As Font
        Public Property radiusFont As Font
        Public Property circleStroke As Pen

        Public Sub Draw(g As IGraphics, layout As Rectangle)
            Dim titleSize As SizeF = g.MeasureString(title, titleFont)
            Dim tickSize As SizeF = g.MeasureString("0", radiusFont)
            Dim y As Single = layout.Top + titleSize.Height * 2
            Dim r As Single
            Dim max_r As Single = radius.Max
            Dim tick_left As Single = layout.Left + max_r * 3
            Dim left As Single = layout.Left
            Dim dy As Single = radius.Average

            Call g.DrawString(title, titleFont, Brushes.Black, layout.Left, layout.Top)

            For Each radius As Integer In Me.radius
                r = radius * 2
                y += r

                Call g.DrawEllipse(circleStroke, New RectangleF(left + (max_r * 2 - r) / 2, y - r, r, r))
                Call g.DrawString(radius, radiusFont, Brushes.Black, New PointF(tick_left, y - r / 2 - tickSize.Height / 2))

                y += dy
            Next
        End Sub
    End Class
End Namespace
