Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Imaging.Driver.CSS

Module CSSTest

    Sub Main()

        Call GetType(DensityPlot).LoadDriver(DensityPlot.DriverName).CSSTemplate.SaveTo("./density.plot.css")
    End Sub
End Module
