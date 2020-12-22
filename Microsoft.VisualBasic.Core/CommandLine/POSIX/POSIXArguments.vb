Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace CommandLine.POSIX

    Public Class POSIXArguments

        Public Shared Sub PrintHelp(mail$, home$, help$)
            Call New StringBuilder() _
                .AppendLine($"Report bugs to: {mail}") _
                .AppendLine($"pkg home page: <{home}>") _
                .AppendLine($"General help using GNU software: <{help}>") _
                .DoCall(Sub(str)
                            Console.WriteLine(str.ToString)
                        End Sub)
        End Sub

    End Class
End Namespace