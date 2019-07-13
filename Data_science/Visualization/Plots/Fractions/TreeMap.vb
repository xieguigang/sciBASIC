#Region "Microsoft.VisualBasic::4c97ff1f97f0c15c1155d1901d7ac2fb, Data_science\Visualization\Plots\Fractions\TreeMap.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

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

            Dim array As List(Of FractionData) =
                data _
                .OrderByDescending(Function(x) x.Percentage) _
                .AsList
            Dim margin As Padding = padding
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim rect As New RectangleF With {
                        .Location = New PointF(margin.Left, margin.Top),
                        .Size = New SizeF With {
                            .Width = size.Width - margin.Horizontal,
                            .Height = size.Height - margin.Left - margin.Top
                        }
                    }

                    ' true -> width percentage; false -> height percentage
                    Dim f As Boolean = True
                    Dim width! = rect.Width, height! = rect.Height
                    Dim x! = margin.Left, y! = margin.Top
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

            Return g.GraphicsPlots(size, margin, bg, plotInternal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetPercentage(f As FractionData, all As IEnumerable(Of FractionData)) As Double
            Return f.Percentage / all.Sum(Function(x) x.Percentage)
        End Function
    End Module
End Namespace
