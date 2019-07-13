#Region "Microsoft.VisualBasic::287c1a39afa8da80bea3ecda95b010cc, Data_science\Visualization\test\SampleVisualizeTest.vb"

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

    ' Module SampleVisualizeTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module SampleVisualizeTest

    Sub Main()
        Dim norm As Vector = rand(1000)

        Call norm.NormalDistributionPlot.AsGDIImage.SaveAs("./nd.png")


        Pause()
    End Sub
End Module
