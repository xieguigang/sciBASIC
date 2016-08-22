Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/file", Usage:="/file /in <file.txt> [/out <out.txt>]")>
    Public Function OnlySupportsFile(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out As String = args.GetValue("/out", [in].TrimSuffix & ".output.json")
        Return [in].ReadAllLines.GetJson.SaveTo(out).CLICode
    End Function

    <ExportAPI("/std", Usage:="/std <input> <output>")>
    Public Function JustStdDevice() As Integer
        Using input = Console.OpenStandardInput, output = New StreamWriter(Console.OpenStandardOutput)
            Call output.Write(New StreamReader(input).ReadToEnd.lTokens.GetJson)
        End Using

        Return 0
    End Function

    <ExportAPI("/pipe.Test", Usage:="/pipe.Test /in <file.txt/std_in> [/out <out.txt/std_out>]")>
    Public Function SupportsBothFileAndPipeline(args As CommandLine) As Integer
        Using out = args.OpenStreamOutput("/out")
            Dim inData As String() = args.OpenStreamInput("/in").ReadToEnd.lTokens
            Call out.Write(inData.GetJson)
        End Using

        Return 0
    End Function
End Module
