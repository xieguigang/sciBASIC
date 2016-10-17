Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    Const Condition$ = " '$(Configuration)|$(Platform)' == '%s|%s' "

    <ExportAPI("/config.output",
               Usage:="/config.output /in <*.vbproj/DIR> /output <DIR> /c 'config=<Name>;platform=<type>'")>
    Public Function ConfigOutputPath(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim output As String = args("/output")
        Dim c As Dictionary(Of String, String) = args.GetDictionary("/c")
        Dim files$()
        Dim condition$

        Try
            condition$ = Program.Condition <= {
                c("config"), c("platform")
            }.xFormat
        Catch ex As Exception
            ex = New Exception(c.GetJson, ex)
            Throw ex
        End Try

        If [in].FileExists Then
            files = {[in]}
        Else
            files = (ls - l - r - wildcards("*.vbproj") <= [in]).ToArray
        End If

        For Each xml As String In files
            Dim vbproj As Project = xml.LoadXml(Of Project)(,, AddressOf Project.RemoveNamespace)
            Dim config = vbproj.GetProfile(condition$)
            Dim relOut$ = RelativePath(xml.ParentPath, output)
#If DEBUG Then
            xml = xml.TrimSuffix & "_updated.vbproj"
#End If
            config.OutputPath = relOut
            vbproj.Save(xml, Encodings.UTF8)
        Next

        Return 0
    End Function
End Module
