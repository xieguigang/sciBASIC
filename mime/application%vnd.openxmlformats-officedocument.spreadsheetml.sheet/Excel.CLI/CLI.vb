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
    Public Function cbind(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim append$ = args <= "/append"
        Dim out$ = args.GetValue("/out", [in].TrimSuffix & "+" & append.BaseName & ".csv")

        Return Contract.Append(EntityObject.LoadDataSet([in]), Contract.Load(append)) _
            .SaveTo(out) _
            .CLICode
    End Function

    <ExportAPI("/rbind")>
    <Usage("/rbind /in <*.csv.DIR> [/out <EXPORT.csv>]")>
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
