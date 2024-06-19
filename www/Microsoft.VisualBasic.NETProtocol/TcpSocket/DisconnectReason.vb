﻿Namespace SuperSimpleTcp
    ''' <summary>
    ''' Reason why a client disconnected.
    ''' </summary>
    Public Enum DisconnectReason
        ''' <summary>
        ''' Normal disconnection.
        ''' </summary>
        Normal = 0
        ''' <summary>
        ''' Client connection was intentionally terminated programmatically or by the server.
        ''' </summary>
        Kicked = 1
        ''' <summary>
        ''' Client connection timed out; server did not receive data within the timeout window.
        ''' </summary>
        Timeout = 2
        ''' <summary>
        ''' The connection was not disconnected.
        ''' </summary>
        None = 3
    End Enum
End Namespace
