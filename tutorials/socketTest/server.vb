#Region "Microsoft.VisualBasic::0c338dc8396b46ea1971b640d3da4518, tutorials\socketTest\server.vb"

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

    ' Class server
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: handleA, handleB, ProtocolHandler
    ' 
    '     Sub: LogException
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Text

<Protocol(GetType(Protocols.Test))>
Public Class server : Inherits ServerModule

    Public Sub New(port As Integer)
        MyBase.New(port)
    End Sub

    Protected Overrides Sub LogException(ex As Exception)
        Call ex.PrintException
    End Sub

    ''' <summary>
    ''' protocol C will returns a not implemented error code
    ''' </summary>
    ''' <returns></returns>
    Protected Overrides Function ProtocolHandler() As ProtocolHandler
        Return New ProtocolHandler(Me)
    End Function

    <Protocol(Protocols.Test.A)>
    Private Function handleA(request As RequestStream, RemoteAddress As System.Net.IPEndPoint) As RequestStream
        Return RequestStream.CreatePackage(request.GetString(UTF8WithoutBOM) & "  Hello world")
    End Function

    <Protocol(Protocols.Test.B)>
    Private Function handleB(request As RequestStream, RemoteAddress As System.Net.IPEndPoint) As RequestStream
        Return RequestStream.SystemProtocol(RequestStream.Protocols.InvalidCertificates, "Mismatched!")
    End Function
End Class
