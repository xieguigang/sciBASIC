Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection

<CLI> Module CLI

    <ExportAPI("/Cbind")>
    Public Function cbind(args As CommandLine) As Integer

    End Function

    <ExportAPI("/Rbind")>
    Public Function rbind(args As CommandLine) As Integer

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
