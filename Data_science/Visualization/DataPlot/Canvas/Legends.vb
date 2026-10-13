#Region "Microsoft.VisualBasic::40b391906b4a317bf7846989c4f294d1, Data_science\Visualization\DataPlot\Canvas\Legends.vb"

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

    '   Total Lines: 54
    '    Code Lines: 36 (66.67%)
    ' Comment Lines: 8 (14.81%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.98 KB


    '     Class CircleSizeLegend
    ' 
    '         Properties: circleStroke, radius, radiusFont, title, titleFont
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  Legends.vb - Canvas 兼容层的辅助图例
'
'  从旧 Plots 项目的 g/Legends/CircleSizeLegend.vb 迁移而来。
' ============================================================================

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Font = System.Drawing.Font
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Font = Microsoft.VisualBasic.Imaging.Font
#End If

Namespace Canvas

    ''' <summary>
    ''' 气泡尺寸图例：绘制一组嵌套的圆圈以及各自的数值标注
    ''' </summary>
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
