Imports Microsoft.VisualBasic.CommandLine

Namespace VersionControl

    Public Module svn

        Public Function getLogText(file As String) As String
            Dim command$ = $"log {file.CLIPath}"
            Dim svn As New IORedirectFile("svn", command, debug:=False)

            Call svn.Run()

            Return svn.StandardOutput
        End Function
    End Module
End Namespace