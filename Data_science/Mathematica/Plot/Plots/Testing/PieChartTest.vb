Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Public Module PieChartTest

    Sub Main()

        Dim rnd As New Random
        Dim data As New List(Of NamedValue(Of Integer))

        For i As Integer = 0 To 6
            data.Add(
                New NamedValue(Of Integer) With {
                     .Name = Guid.NewGuid.ToString,
                     .Value = rnd.Next(300)
                })
        Next

        Call data.Fractions(ColorBrewer.QualitativeSchemes.Set3_12).Plot().Save("./test_pie.png")
    End Sub
End Module
