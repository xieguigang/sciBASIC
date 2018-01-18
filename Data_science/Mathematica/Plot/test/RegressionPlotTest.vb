Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging

Module RegressionPlotTest
    Sub Main()
        Dim weight = {115.0, 117, 120, 123, 126, 129, 132, 135, 139, 142, 146, 150, 154, 159, 164}
        Dim height = {58.0, 59.0, 60.0, 61.25, 62.0, 63.5, 64.0, 65.0, 66.1, 67.1, 68.0, 69.0, 70.0, 71.8, 72.0}
        Dim linear = LeastSquares.PolyFit(height, weight, 6)

        Call RegressionPlot.Plot(linear).AsGDIImage.SaveAs("./RegressionPlot.png")

        Pause()
    End Sub
End Module
