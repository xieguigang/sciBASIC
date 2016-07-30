Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream

Public Module CLI

    <ExportAPI("/Selects", Usage:="/Selects /in <in.Csv> /index <Name> /list <list.key.Csv> [/out <out.Csv>]")>
    Public Function Selects(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim index As String = args.GetValue("/index", "Name")
        Dim list As String = args("/list")
        Dim out As String = args.GetValue("/out", [in].TrimSuffix & "." & list.BaseName & ".Csv")
        Dim maps As New Dictionary(Of String, String) From {{index, NameOf(EntityObject.Identifier)}}
        Dim source As EntityObject() = [in].LoadCsv(Of EntityObject)(maps:=maps)
        Dim keys As String() = list.ReadAllLines
        source = source.Where(Function(x) Array.IndexOf(keys, x.Identifier) > -1).ToArray
        Return source.SaveTo(out).CLICode
    End Function
End Module
