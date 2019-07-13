#Region "Microsoft.VisualBasic::3d4190451a65fec77d27638cc378da79, tutorials\socketTest\Program.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Program
    ' 
    '     Sub: Main, pushTest, streamEncodingTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Tcp.Persistent.Socket
Imports Microsoft.VisualBasic.Parallel

Module Program

    Sub Main()

        Dim testPort% = 2233

        Call streamEncodingTest()
        Call pushTest()

        Pause()

        Call New ThreadStart(AddressOf New server(testPort).Run).RunTask
        Call Thread.Sleep(1000)

        Dim invoke As New ProtocolInvoker(Of Protocols.Test)("localhost", testPort)

        Dim response As RequestStream


        response = invoke.SendMessage(Test.A, "[AAAA]")
        response = invoke.SendMessage(Test.B, "adada")
        response = invoke.SendMessage(Test.C, "54555")

        Pause()
    End Sub

    Sub pushTest()
        Dim server As New ServicesSocket(12345) With {
            .AcceptCallbackHandleInvoke = Sub()

                                          End Sub
        }

        Call New ThreadStart(AddressOf server.Run).RunTask
        Call Thread.Sleep(1000)

        Dim client As New PersistentClient("127.0.0.1", 12345) With {
            .MessageHandler = Sub(request As RequestStream)
                                  Call request.ToString.__DEBUG_ECHO
                              End Sub
        }

        Call client.BeginConnect()
        Call Thread.Sleep(1000)

        For i As Integer = 0 To 10
            Dim iii = i

            Call server.Connections.ForEach(Sub(c, j)
                                                Call c.PushMessage($"{iii}-{j}, hello.")
                                            End Sub)
            Call Thread.Sleep(1000)
            Call Console.WriteLine("-")
        Next

        Call server.Dispose()
    End Sub

    Sub streamEncodingTest()
        Dim request As New RequestStream(12345, 555, "Hello world!")
        Dim buffer = request.Serialize
        Dim parsed = New RequestStream(buffer)

        Call parsed.GetUTF8String.__DEBUG_ECHO
    End Sub
End Module
