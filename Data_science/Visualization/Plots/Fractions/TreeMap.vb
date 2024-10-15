#Region "Microsoft.VisualBasic::1a4b2f1a7405a74ce926670c7714308c, Data_science\Visualization\Plots\Fractions\TreeMap.vb"

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

'   Total Lines: 114
'    Code Lines: 88 (77.19%)
' Comment Lines: 10 (8.77%)
'    - Xml Docs: 60.00%
' 
'   Blank Lines: 16 (14.04%)
'     File Size: 4.67 KB


'     Module TreeMap
' 
'         Function: GetPercentage, Plot
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.MIME.Html.Render




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
#End If

Namespace Fractions

    Public Module TreeMap

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="size"></param>
        ''' <param name="bg$"></param>
        ''' <returns></returns>
        Public Function Plot(data As IEnumerable(Of FractionData),
                         Optional size As Size = Nothing,
                         Optional padding$ = "padding: 350 100 350 100;",
                         Optional bg$ = "white") As GraphicsData

            Dim array As List(Of FractionData) = data _
                .OrderByDescending(Function(x) x.Percentage) _
                .AsList
            Dim margin As Padding = padding
            Dim theme As New Theme With {.padding = margin, .background = bg}
            Dim app As New TreeMapPlot(array, theme)

            Return app.Plot(size)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetPercentage(f As FractionData, all As IEnumerable(Of FractionData)) As Double
            Return f.Percentage / all.Sum(Function(x) x.Percentage)
        End Function
    End Module

    Public Class TreeMapPlot : Inherits Plot

        ReadOnly array As List(Of FractionData)

        Public Sub New(array As List(Of FractionData), theme As Theme)
            MyBase.New(theme)
            Me.array = array
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim margin = canvas.Padding
            Dim size As Size = g.Size
            Dim rect As New RectangleF With {
                .Location = New PointF(css.GetValue(margin.Left), css.GetValue(margin.Top)),
                .Size = New SizeF With {
                    .Width = size.Width - margin.Horizontal(css),
                    .Height = size.Height - css.GetValue(margin.Left) - css.GetValue(margin.Top)
                }
            }

            ' true -> width percentage; false -> height percentage
            Dim f As Boolean = True
            Dim width! = rect.Width, height! = rect.Height
            Dim x! = css.GetValue(margin.Left), y! = css.GetValue(margin.Top)
            Dim drawW!, drawH!
            Dim labels As New List(Of FractionData)

            Do While array.Count > 0
                Dim p As FractionData = array.First

                If f Then
                    ' 计算宽度百分比
                    drawW = p.GetPercentage(array) * width
                    drawH = height
                    rect = New RectangleF(New PointF(x, y), New SizeF(drawW, drawH))

                    Call g.FillRectangle(New SolidBrush(p.Color), rect)

                    x = x + drawW
                    width = width - drawW
                Else
                    ' 计算高度百分比
                    drawW = width
                    drawH = p.GetPercentage(array) * height
                    rect = New RectangleF(New PointF(x, y), New SizeF(drawW, drawH))

                    Call g.FillRectangle(New SolidBrush(p.Color), rect)

                    y += drawH
                    height = height - drawH
                End If

                f = Not f  ' swap

                Call labels.Add(item:=p)
                Call array.RemoveAt(Scan0)
            Loop
        End Sub
    End Class
End Namespace
