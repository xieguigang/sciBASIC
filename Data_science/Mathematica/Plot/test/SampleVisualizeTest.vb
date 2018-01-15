Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module SampleVisualizeTest

    Sub Main()
        Dim norm As Vector = rand(100)

        Call norm.NormalDistributionPlot.AsGDIImage.SaveAs("./nd.png")
    End Sub
End Module
