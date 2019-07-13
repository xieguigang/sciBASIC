#Region "Microsoft.VisualBasic::639eff1cc46dc6adeae52d7a6b82763c, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI\Normalize.vb"

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

    ' Module CLI
    ' 
    '     Function: FillZero
    ' 
    ' /********************************************************************************/

#End Region

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
