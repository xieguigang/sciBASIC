Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace PushAPI

    <Protocol(GetType(NETProtocol.InvokeAPI.Protocols))>
    Public Class InvokeAPI : Inherits APIBase

        Sub New(push As PushServer)
            Call MyBase.New(push)
        End Sub

    End Class
End Namespace