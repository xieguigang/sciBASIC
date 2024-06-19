Imports System

Namespace SuperSimpleTcp
    ''' <summary>
    ''' Arguments for data received from connected endpoints.
    ''' </summary>
    Public Class DataReceivedEventArgs
        Inherits EventArgs
        Friend Sub New(ipPort As String, data As ArraySegment(Of Byte))
            Me.IpPort = ipPort
            Me.Data = data
        End Sub

        ''' <summary>
        ''' The IP address and port number of the connected endpoint.
        ''' </summary>
        Public ReadOnly Property IpPort As String

        ''' <summary>
        ''' The data received from the endpoint.
        ''' </summary>
        Public ReadOnly Property Data As ArraySegment(Of Byte)
    End Class
End Namespace
