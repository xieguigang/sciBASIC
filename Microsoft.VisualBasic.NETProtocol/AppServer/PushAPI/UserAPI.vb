Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace PushAPI

    <Protocol(GetType(NETProtocol.UserAPI.Protocols))>
    Public Class UserAPI : Inherits APIBase



        Sub New(push As PushServer)
            Call MyBase.New(push)
        End Sub

    End Class
End Namespace