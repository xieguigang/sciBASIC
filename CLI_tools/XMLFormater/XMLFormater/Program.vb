Imports System.Xml
Imports Microsoft.VisualBasic.CommandLine

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine, executeFile:=AddressOf FormatXmlFile)
    End Function

    Private Function FormatXmlFile(file$, args As CommandLine) As Integer
        Dim xmlDoc As New XmlDocument
        Dim out$ = args.GetValue("/out", file.TrimSuffix & ".FormatEdited.XML")
        Call xmlDoc.Load(file)
        Call xmlDoc.Save(out)
        Return 0
    End Function
End Module
