Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining

Module ROCplotTest

    Sub Main()
        Dim data = EntityObject.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\ROC\identify.csv").ToArray
        Dim test = Validation.ROC(data, Function(d, p) d!class = "p", Function(d, p) d!score >= p).CreateSerial

        Call ROCPlot.Plot(test).Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\ROC\identify_ROC.csv")
    End Sub
End Module
