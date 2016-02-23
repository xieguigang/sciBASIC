Namespace PushAPI

    Public MustInherit Class APIBase

        Public ReadOnly Property PushServer As PushServer

        Sub New(push As PushServer)
            PushServer = push
        End Sub
    End Class
End Namespace