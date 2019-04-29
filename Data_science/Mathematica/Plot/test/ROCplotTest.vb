Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining

Module ROCplotTest

    Sub Main()
        ' Dim data = EntityObject.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\ROC\identify.csv").ToArray
        ' Dim test = Validation.ROC(data, Function(d, p) d!class = "p", Function(d, p) d!score >= p).CreateSerial

        Dim data = EntityObject.LoadDataSet("D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\build_tools\CVD_kb\duke\out\CHD_20190411_IVD\[ALL-validates]validate-result_4markers.csv").ToArray
        Dim test = Validation.ROC(data, Function(d, p) d!CHD > 0, Function(d, p) d("CHD(predicted)") >= p).CreateSerial

        Call ROCPlot.Plot(test).Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\ROC\identify_ROC2.png")
    End Sub
End Module
