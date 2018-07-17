Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.TimeTrends
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Text

Module timeRangesTest

    Sub Main()
        Dim avg = DataSet.LoadDataSet("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\history.average.csv", encoding:=TextEncodings.UTF8)
        Dim range = DataSet.LoadDataSet("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\history.range.csv", encoding:=TextEncodings.UTF8).ToDictionary
        Dim data = avg _
            .Select(Function(d)
                        Dim r As DataSet = range(d.ID)

                        Return New TimePoint With {
                            .[date] = Date.Parse(d.ID),
                            .average = d!value,
                            .range = {r!min, r!max}
                        }
                    End Function) _
            .ToArray

        Call data.Plot(dateFormat:=Function(d) $"{d.Year}/{d.Month.FormatZero("00")}").Save("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\history.range.jpg")
    End Sub
End Module
