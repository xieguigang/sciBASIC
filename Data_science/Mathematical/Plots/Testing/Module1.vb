Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Imaging

Module Module1

    Sub Main()
        Call {New csv.SerialData}.SaveTo("./template.csv")

        Dim example = csv.SerialData.GetData("G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\ManhattanStatics\example.csv").First

        Call ManhattanStatics.Plot(example).SaveAs("G:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\ManhattanStatics/demo.png")

        Pause()
    End Sub
End Module
