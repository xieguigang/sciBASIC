#Region "Microsoft.VisualBasic::dcce95c3d7a5ca4f45a16597d7ab3484, Data_science\Visualization\test\BarPlotTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 83
    '    Code Lines: 61
    ' Comment Lines: 5
    '   Blank Lines: 17
    '     File Size: 4.16 KB


    ' Module BarPlotTest
    ' 
    '     Sub: Main, variableWidthTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging

Module BarPlotTest

    Sub variableWidthTest()

        Dim data = {
            New VariableBarData With {.Name = "测试1", .Data = (120, 366)},
            New VariableBarData With {.Name = "测试1", .Data = (220, 1366)},
            New VariableBarData With {.Name = "测试1", .Data = (1120, 36)},
            New VariableBarData With {.Name = "测试1", .Data = (620, 866)},
            New VariableBarData With {.Name = "测试1", .Data = (920, 766)},
            New VariableBarData With {.Name = "测试1", .Data = (1000, 366)},
            New VariableBarData With {.Name = "测试1", .Data = (2120, 466)},
            New VariableBarData With {.Name = "测试1", .Data = (1820, 2366)},
            New VariableBarData With {.Name = "测试1", .Data = (320, 1766)}
        }


        Call VariableWidthBarPlot.Plot(data.OrderByDescending(Function(d) d.Data.height), title:="测试标题").AsGDIImage.SaveAs("./test_bar.png")

        Pause()
    End Sub

    Sub Main()

        Call variableWidthTest()

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
