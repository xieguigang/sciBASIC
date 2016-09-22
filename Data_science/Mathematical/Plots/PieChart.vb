Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes

Public Module PieChart

    <Extension>
    Public Function Plot(data As IEnumerable(Of Pie),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional legend As Boolean = True,
                         Optional legendBorder As Border = Nothing) As Bitmap

        Return g.GraphicsPlots(size, margin, bg,
                 Sub(g)
                     Dim r = (Math.Min(size.Width, size.Height) - Math.Max(margin.Width, margin.Height)) / 2
                     Dim topLeft = New Point(size.Width / 2 - r, size.Height / 2 - r)
                     Dim rect As New Rectangle(topLeft, New Size(r * 2, r * 2))
                     Dim a As New Value(Of Single)
                     Dim sweep As New Value(Of Single)

                     Call g.FillPie(Brushes.LightGray, rect, 0, 360)

                     For Each x As Pie In data
                         Call g.FillPie(New SolidBrush(x.Color), rect, (a = (a.value + (sweep = CSng(360 * x.Percentage)))) - sweep.value, sweep)
                     Next

                     If legend Then
                         Dim font As New Font(FontFace.MicrosoftYaHei, 20)
                         Dim maxL = data.Select(Function(x) g.MeasureString(x.Name, font).Width).Max
                         Dim left = size.Width - (margin.Width * 2) - maxL
                         Dim top = margin.Height
                         Dim legends As New List(Of Legend)

                         For Each x As Pie In data
                             legends += New Legend With {
                                .color = x.Color.RGBExpression,
                                .style = LegendStyles.Rectangle,
                                .title = x.Name,
                                .fontstyle = CSSFont.GetFontStyle(font.Name, font.Style, font.Size)
                             }
                         Next

                         Call g.DrawLegends(New Point(left, top), legends, ,, legendBorder)
                     End If
                 End Sub)
    End Function

    <Extension>
    Public Function FromData(data As IEnumerable(Of NamedValue(Of Integer)), Optional colors As String() = Nothing) As Pie()
        Dim all = data.Select(Function(x) x.x).Sum
        Dim s = From x
                In data
                Select New NamedValue(Of Double) With {
                    .Name = x.Name,
                    .x = x.x / all
                }
        Return s.FromPercentages(colors)
    End Function

    <Extension>
    Public Function FromPercentages(data As IEnumerable(Of NamedValue(Of Double)), Optional colors As String() = Nothing) As Pie()
        Dim array = data.ToArray
        Dim out As Pie() = New Pie(array.Length - 1) {}
        Dim c As Color() = If(
            colors.IsNullOrEmpty,
            ChartColors.Shuffles,
            colors.ToArray(AddressOf ToColor)
        )

        For Each x In array.SeqIterator
            out(x.i) = New Pie With {
                .Color = c(x.i),
                .Name = x.obj.Name,
                .Percentage = x.obj.x
            }
        Next

        Return out
    End Function
End Module

Public Class Pie
    Public Property Percentage As Double
    Public Property Name As String
    Public Property Color As Color

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class