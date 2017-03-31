#Region "Microsoft.VisualBasic::7cdbb76b9dab6bbfc99ea70a5c4dfc51, ..\sciBASIC#\Data_science\Mathematical\Plots\Fractions\TreeMap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.g
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module TreeMap

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="bg$"></param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of Fractions),
                         Optional size As Size = Nothing,
                         Optional padding$ = "padding: 350 100 350 100;",
                         Optional bg$ = "white") As Bitmap

        Dim array As List(Of Fractions) =
            data _
            .OrderByDescending(Function(x) x.Percentage) _
            .ToList
        Dim margin As Padding = padding

        Return GraphicsPlots(
            size, margin,
            bg,
            Sub(ByRef g, region)

                Dim rect As New Rectangle(
                    New Point(margin.Left, margin.Top),
                    New Size(size.Width - margin.Horizontal,
                             size.Height - margin.Left - margin.Top))

                Dim f As Boolean = True ' true -> width percentage; false -> height percentage
                Dim width! = rect.Width, height! = rect.Height
                Dim x! = margin.Left, y! = margin.Top
                Dim drawW!, drawH!
                Dim labels As New List(Of Fractions)

                Do While array.Count > 0
                    Dim p As Fractions = array.First

                    If f Then  ' 计算宽度百分比
                        drawW = p.GetPercentage(array) * width
                        drawH = height

                        Call g.FillRectangle(
                            New SolidBrush(p.Color),
                            New RectangleF(New PointF(x, y), New SizeF(drawW, drawH)))

                        x = x + drawW
                        width = width - drawW
                    Else ' 计算高度百分比
                        drawW = width
                        drawH = p.GetPercentage(array) * height

                        Call g.FillRectangle(
                           New SolidBrush(p.Color),
                           New RectangleF(New PointF(x, y), New SizeF(drawW, drawH)))

                        y += drawH
                        height = height - drawH
                    End If

                    f = Not f  ' swap
                    Call labels.Add(item:=p)
                    Call array.RemoveAt(Scan0)
                Loop
            End Sub)
    End Function

    <Extension>
    Public Function GetPercentage(f As Fractions, all As IEnumerable(Of Fractions)) As Double
        Return f.Percentage / all.Sum(Function(x) x.Percentage)
    End Function
End Module
