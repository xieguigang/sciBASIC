#Region "Microsoft.VisualBasic::b4f5cf6c6ec79697c77296151d6f893f, ..\sciBASIC#\Data_science\Mathematica\Plot\Testing\BarPlotTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
        Dim boxGroups = BoxData.Load("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\images\boxplot\alpha-box.csv", groups).ToArray

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
                .Takes(30)

            Call StackedBarPlot.Plot(
                data,
                YaxisTitle:="Relative abundance") _
                .Save(path.TrimSuffix & ".png")
        Next
    End Sub
End Module
