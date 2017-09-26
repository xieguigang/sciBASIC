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
                     .Name = "block#" & i,
                     .Value = rnd.Next(300)
                })
        Next

        Call data _
            .Fractions(ColorBrewer.QualitativeSchemes.Accent8) _
            .Plot(legendAlt:=True) _
            .Save("./test_pie.png")
    End Sub
End Module
