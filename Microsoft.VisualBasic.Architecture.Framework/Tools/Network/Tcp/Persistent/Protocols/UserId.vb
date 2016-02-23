Namespace Net.Persistent.Application.Protocols

    Public Class UserId

        Public Property Remote As IPEndPoint
        Public Property uid As Long

        Public Function CreateApp(protocols As PushMessage) As USER
            Return New USER(Remote.IPAddress, Remote.Port, uid, protocols)
        End Function
    End Class
End Namespace