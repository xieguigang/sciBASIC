Imports System

Namespace SuperSimpleTcp
    ''' <summary>
    ''' Arguments for connection events.
    ''' </summary>
    Public Class ConnectionEventArgs
        Inherits EventArgs
        Friend Sub New(ipPort As String, Optional reason As DisconnectReason = DisconnectReason.None)
            Me.IpPort = ipPort
            Me.Reason = reason
        End Sub

        ''' <summary>
        ''' The IP address and port number of the connected peer socket.
        ''' </summary>
        Public ReadOnly Property IpPort As String

        ''' <summary>
        ''' The reason for the disconnection, if any.
        ''' </summary>
        Public ReadOnly Property Reason As DisconnectReason = DisconnectReason.None
    End Class
End Namespace
