Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot

Module BarPlotTest

    Sub Main()

        Dim dara = BarPlotDataExtensions _
            .LoadDataSet("C:\Users\gg.xie\Desktop\sp.csv") _
            .Strip(30) _
            .Normalize

        Call StackedBarPlot.Plot(dara).Save("./barplot.png")

    End Sub
End Module
