Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language

Namespace BarPlot

    Public Module BarPlotDataExtensions

        Public Function LoadDataSet(path$, Optional schema$ = "", Optional groupByColumn! = yes, Optional tsv! = no) As BarDataGroup
            Dim csv As File = File.Load(path)
            Dim samples As New Dictionary(Of String, BarDataSample)

            If groupByColumn Then
                Dim columns = csv.Columns.ToArray
                Dim serialsName$() = columns(Scan0).Skip(1).ToArray

                For Each column In columns.Skip(1)
                    Dim groupName$ = column(Scan0)
                    Dim data#() = columns.Skip(1)
                Next
            Else

            End If
        End Function
    End Module
End Namespace