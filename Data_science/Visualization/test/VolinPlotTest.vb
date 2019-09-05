Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv.IO

Module VolinPlotTest

    Sub Main()
        Dim data As IEnumerable(Of DataSet) = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.csv").ToArray


        Call VolinPlot.Plot(dataset:=data).Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.VolinPlot.png")
        Call Pause()
    End Sub
End Module
