Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap

Module Module2

    Sub Main()

        Dim path = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\images\dendrogram\heatmap.Test.csv"
        Dim data = DataSet.LoadDataSet(path)
        Dim experiments As New Dictionary(Of String, String) From {
            {"T1", "red"},
            {"T2", "red"},
            {"T3", "red"},
            {"T4", "red"},
            {"K1", "blue"},
            {"K2", "blue"},
            {"K3", "blue"},
            {"K4", "blue"},
            {"average", "green"}
        }

        Call Heatmap.Plot(data, size:="3200,6000", reverseClrSeq:=True, drawScaleMethod:=DrawElements.Cols, drawClass:=(Nothing, experiments)).Save(path.TrimSuffix & ".png")

        ' Call AxisScalling.CreateAxisTicks({-10.3301, 13.7566}, 20).GetJson(True).__DEBUG_ECHO

        Pause()

    End Sub


End Module
