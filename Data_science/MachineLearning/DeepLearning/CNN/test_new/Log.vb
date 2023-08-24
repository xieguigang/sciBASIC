Namespace CNN

    Public Class Log
        'internal static PrintStream stream = System.out;

        Public Shared Sub i(tag As String, msg As String)
            Call VBDebugger.EchoLine(tag & vbTab & msg)
        End Sub

        Public Shared Sub i(msg As String)
            Call VBDebugger.EchoLine(msg)
        End Sub

    End Class

End Namespace
