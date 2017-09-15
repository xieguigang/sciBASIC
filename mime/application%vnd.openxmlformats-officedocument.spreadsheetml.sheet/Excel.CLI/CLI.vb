Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Contract = Microsoft.VisualBasic.Data.csv.DATA.DataFrame

<CLI> Module CLI

    <ExportAPI("/Cbind")>
    <Usage("/cbind /in <a.csv> /append <b.csv> [/out <ALL.csv>]")>
    <Description("Join of two table by a unique ID.")>
    <Argument("/in", False, CLITypes.File,
              Description:="The table for append by column, its row ID can be duplicated.")>
    <Argument("/append", False, CLITypes.File,
              Description:="The target table that will be append into the table ``a``, the row ID must be unique!")>
    Public Function cbind(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim append$ = args <= "/append"
        Dim out$ = args.GetValue("/out", [in].TrimSuffix & "+" & append.BaseName & ".csv")

        Return Contract.Append(EntityObject.LoadDataSet([in]), Contract.Load(append)) _
            .SaveTo(out) _
            .CLICode
    End Function

    <ExportAPI("/rbind")>
    <Description("Row bind(merge tables directly) of the csv tables")>
    <Usage("/rbind /in <*.csv.DIR> [/out <EXPORT.csv>]")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              Description:="A directory path that contains csv files that will be merge into one file directly.")>
    Public Function rbind(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out As String = args.GetValue("/out", [in].TrimSuffix & ".MERGE.csv")

        Return DocumentExtensions _
            .MergeTable(
            out, ls - l - r - "*.csv" <= [in])
    End Function

    <ExportAPI("/push")>
    Public Function pushTable(args As CommandLine) As Integer

    End Function

    <ExportAPI("/Create")>
    Public Function newEmpty(args As CommandLine) As Integer

    End Function

    <ExportAPI("Extract")>
    Public Function extract(args As CommandLine) As Integer

    End Function
End Module
