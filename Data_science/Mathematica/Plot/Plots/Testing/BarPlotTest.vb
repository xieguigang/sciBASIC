Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics

Module BarPlotTest

    Sub Main()

        'Dim dara = BarPlotDataExtensions _
        '    .LoadDataSet("C:\Users\xieguigang\Desktop\test.csv") _
        '    .Strip(30) _
        '    .Normalize

        'Call StackedBarPlot.Plot(dara, YaxisTitle:="Relative abundance").Save("C:\Users\xieguigang\Desktop\test.png")

        Dim groups = {
            New NamedCollection(Of String)("T1", {"T1-1", "T1-2", "T1-3", "T1-4", "T1-5", "T1-6"}),
            New NamedCollection(Of String)("T2", {"T2-1", "T2-2", "T2-3", "T2-4", "T2-5", "T2-6"}),
            New NamedCollection(Of String)("T3", {"T3-1", "T3-2", "T3-3", "T3-4", "T3-5", "T3-6"})
        }
        Dim boxGroups = BoxData.Load("C:\Users\xieguigang\Desktop\alpha-box.csv", groups).ToArray

        For Each x As BoxData In boxGroups
            Call x _
                .Plot(YaxisLabel:=$"Alpha Index({x.SerialName})") _
                .Save($"./{x.SerialName}.png")
        Next


        Pause()

        For Each path As String In {
            "D:\projects\8.18\Report_soil\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Species.csv",
            "D:\projects\8.18\Report_soil\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Class.csv",
            "D:\projects\8.18\Report_soil\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Family.csv",
            "D:\projects\8.18\Report_soil\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Genus.csv",
            "D:\projects\8.18\Report_soil\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Order.csv",
            "D:\projects\8.18\Report_soil\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Phylum.csv",
            "D:\projects\8.18\Project_80-69606828_16S----44444\Report\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Species.csv",
            "D:\projects\8.18\Project_80-69606828_16S----44444\Report\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Class.csv",
            "D:\projects\8.18\Project_80-69606828_16S----44444\Report\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Family.csv",
            "D:\projects\8.18\Project_80-69606828_16S----44444\Report\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Genus.csv",
            "D:\projects\8.18\Project_80-69606828_16S----44444\Report\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Order.csv",
            "D:\projects\8.18\Project_80-69606828_16S----44444\Report\Result\01_OTU_Taxa\Taxonomy\Relative_abundance\Phylum.csv"
        }

            Dim data = BarPlotDataExtensions _
                .LoadDataSet(path) _
                .Normalize _
                .Reorder("Unclassified") _
                .Strip(30)

            Call StackedBarPlot.Plot(
                data,
                YaxisTitle:="Relative abundance") _
                .Save(path.TrimSuffix & ".png")
        Next
    End Sub
End Module
