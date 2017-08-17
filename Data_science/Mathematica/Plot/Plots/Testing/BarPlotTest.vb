Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot

Module BarPlotTest

    Sub Main()

        Dim dara = BarPlotDataExtensions _
            .LoadDataSet("C:\Users\xieguigang\Desktop\test.csv") _
            .Strip(30) _
            .Normalize

        Call StackedBarPlot.Plot(dara, YaxisTitle:="Relative abundance").Save("C:\Users\xieguigang\Desktop\test.png")

    End Sub
End Module
