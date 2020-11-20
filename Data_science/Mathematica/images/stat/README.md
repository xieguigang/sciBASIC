## Z-scores plot

[test code](https://github.com/xieguigang/sciBASIC/blob/35b840c5261c8bbbe552af8f8d7e410d08445698/Data_science/Mathematica/Plot/Plots/Testing/ZScorePlotTest.vb)

```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.csv.IO.Tokenizer

Dim csv$ = "./16S-KO-level3.csv"
Dim labels = csv.ReadFirstLine.CharsParser.Skip(1).AsList

With New Dictionary(Of String, String())

    !Case = {"20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1"}
    !Test = {"13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2"}
    !QC = {"7_4", "6_4", "1_3", "1_4", "1_5"}

    !Control = labels - !Case - !Test - !QC

    Dim data = ZScores.Load(csv, .ref, ColorBrewer.QualitativeSchemes.Set2_4)
          
    Call ZScoresPlot _
		.Plot(data) _
		.Save(csv.ParentPath & "/16S-KO-level3-Z-scores.png")
End With
```

![](./16S-KO-level3-Z-scores.png)