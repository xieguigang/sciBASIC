Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Module ZScorePlotTest

    Sub Main()

        Call analysis()
        Call Pause()

        Dim csv$ = "D:\OneDrive\Report_soil\16s-Desktop\8.24\predictions_ko.L3.xls"
        Dim labels = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser(csv.ReadFirstLine, ASCII.TAB).Skip(1).AsList

        With New Dictionary(Of String, String())
            !Case = {"20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1"}
            !Test = {"13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2"}
            !QC = {"7_4", "6_4", "1_3", "1_4", "1_5"}

            !Control = labels - !Case - !Test - !QC

            Dim data = ZScores.Load(csv, .ref, ColorBrewer.QualitativeSchemes.Set2_4)

            ' data.shapes!Case = LegendStyles.Triangle

            Call ZScoresPlot.Plot(data).Save(csv.ParentPath & "/16S-KO-level3-Z-scores.png")
        End With

    End Sub

    Sub analysis()
        ' plotBox()
        plotHeatmap()
    End Sub

    Sub plotBox()
        Dim csv$ = "D:\OneDrive\Report_soil\16s-Desktop\8.24\predictions_ko.L3.csv"
        Dim labels = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser(csv.ReadFirstLine).Skip(1).AsList

        With New Dictionary(Of String, String())
            !Case = {
                "20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1",
                "13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2",
                "7_4", "6_4", "1_3", "1_4", "1_5"
            }
            !Control = labels - !Case

            Call .GetJson.__DEBUG_ECHO

            Dim data = DataSet.LoadDataSet(csv)

            For Each pathway As DataSet In data
                Dim name$ = pathway.ID
                Dim save$ = csv.ParentPath & $"/boxplot/{name.NormalizePathString}.png"
                Dim groups = .Select(Function(x)
                                         Return New NamedValue(Of Vector) With {
                                            .Name = x.Key,
                                            .Value = pathway(x.Value).AsVector
                                         }
                                     End Function) _
                             .ToArray
                Dim boxData As New BoxData With {
                    .SerialName = name,
                    .Groups = groups
                }

                Call boxData.Plot.Save(save)
            Next
        End With
    End Sub

    Sub plotHeatmap()
        Dim csv$ = "D:\OneDrive\Report_soil\16s-Desktop\8.24\predictions_ko.L3.csv"
        Call Heatmap.Heatmap.Plot(DataSet.LoadDataSet(csv)).Save(csv.TrimSuffix & ".png")
    End Sub
End Module
