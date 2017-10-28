#Region "Microsoft.VisualBasic::6f3fa82bfe95c4647ac1606f699abe54, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\Testing\ZScorePlotTest.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
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
        Dim labels = Microsoft.VisualBasic.Data.csv.IO.Tokenizer.CharsParser(csv.ReadFirstLine).Skip(1).AsList

        With New Dictionary(Of String, String())
            !Case = {
                "20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1",
                "13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2",
                "7_4", "6_4", "1_3", "1_4", "1_5"
            }
            !Control = labels - !Case

            Dim colors = New Dictionary(Of String, String)

            For Each label In !Case
                colors.Add(label, "green")
            Next
            For Each label In !Control
                colors.Add(label, "darkblue")
            Next

            Dim matrix = DataSet.LoadDataSet(csv).Project(colors.Keys.ToArray).ToArray

            Call Heatmap.Heatmap.Plot(matrix,
                                      size:="3800,5000",
                                      drawScaleMethod:=Heatmap.DrawElements.Rows,
                                      min:=0,
                                      colLabelFontStyle:=CSSFont.Win7LittleLarge,
                                      mapName:=ColorBrewer.SequentialSchemes.YlGnBu9,
                                      drawClass:=(Nothing, colors),
                                      mainTitle:="predictions_ko.L3").Save(csv.TrimSuffix & ".png")
        End With

        End
    End Sub
End Module
