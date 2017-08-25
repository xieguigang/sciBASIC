Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Language

Module ZScorePlotTest

    Sub Main()

        Dim labels = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser("C:\Users\xieguigang\Desktop\8.4\ko-lv3.csv".ReadFirstLine).Skip(1).AsList

        With New Dictionary(Of String, String())
            !Case = {"20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1", "13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2", "7_4", "6_4", "1_3", "1_4", "1_5"}
            !Control = labels - !Case

            Dim data = ZScores.Load("C:\Users\xieguigang\Desktop\8.4\ko-lv3.csv", .ref)

            Call ZScoresPlot.Plot(data).Save("./Z-scores.png")
        End With

    End Sub
End Module
