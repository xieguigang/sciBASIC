Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language

Module ZScorePlotTest

    Sub Main()

        Dim csv$ = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\images\stat\16S-KO-level3.csv"
        Dim labels = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser(csv.ReadFirstLine).Skip(1).AsList

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
End Module
