Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.TimeTrends
Imports Microsoft.VisualBasic.Data.csv.IO

Module timeRangesTest

    Sub Main()
        Dim avg = DataSet.LoadDataSet("..\data\history.average.csv")
        Dim range = DataSet.LoadDataSet("..\data\history.range.csv").ToDictionary
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

        Call data.Plot().Save("../data/time.trends.jpg")
    End Sub
End Module
