#Region "Microsoft.VisualBasic::d6efb5d0b795b760c96c7061629a661e, Data_science\Visualization\test\CSSTest.vb"

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

    ' Module CSSTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Imaging.Driver.CSS

Module CSSTest

    Sub Main()

        Call GetType(DensityPlot).LoadDriver(DensityPlot.DriverName).CSSTemplate.SaveTo("./density.plot.css")
    End Sub
End Module
