Module Module1

    Sub Main()
        Using svr As New Microsoft.VisualBasic.Net.NETProtocol.PushServer(123, 8852, 6354)
            Call svr.Run()
        End Using
    End Sub

End Module
