Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting

Namespace ServicesComponents

    <[Namespace]("NodeServices.API")>
    Public Class NodeServicesAPI

        Dim _NodeServices As NodeServices

        Sub New(NodeServices As NodeServices)
            _NodeServices = NodeServices
        End Sub

        <ExportAPI("Shutdown")>
        Public Function ShutdownNodeServices() As Boolean

            Return True
        End Function

    End Class
End Namespace