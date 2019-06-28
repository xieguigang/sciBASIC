Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv

Partial Module CLI

    <ExportAPI("/fill.zero")>
    <Usage("/fill.zero /in <dataset.csv> [/out <out.csv>]")>
    Public Function FillZero(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.fillZero.csv"
        Dim dataset = Microsoft.VisualBasic.Data.csv.IO _
            .DataSet _
            .LoadDataSet([in]) _
            .ToArray

        Return dataset.SaveTo(out).CLICode
    End Function
End Module