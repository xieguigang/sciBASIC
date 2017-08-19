Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot

Module BarPlotTest

    Sub Main()

        'Dim dara = BarPlotDataExtensions _
        '    .LoadDataSet("C:\Users\xieguigang\Desktop\test.csv") _
        '    .Strip(30) _
        '    .Normalize

        'Call StackedBarPlot.Plot(dara, YaxisTitle:="Relative abundance").Save("C:\Users\xieguigang\Desktop\test.png")


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
