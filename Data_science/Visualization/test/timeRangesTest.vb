#Region "Microsoft.VisualBasic::94b7b8c44f02d405ff53e1cab693bcdf, Data_science\Visualization\test\timeRangesTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module timeRangesTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.TimeTrends
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Text

Module timeRangesTest

    Sub Main()
        Dim avg = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\history.average.csv", encoding:=TextEncodings.UTF8)
        Dim range = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\history.range.csv", encoding:=TextEncodings.UTF8).ToDictionary
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

        Call data.Plot(dateFormat:=Function(d) $"{d.Year}/{d.Month.FormatZero("00")}").Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\history.range.jpg")
    End Sub
End Module
