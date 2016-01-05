Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting

Namespace ServicesComponents

    <[Namespace]("ClusterServices.API", Description:="")>
    Public Class ClusterServicesAPI

        Dim _ClusterServices As ClusterServices

        Sub New(ClusterServices As ClusterServices)
            _ClusterServices = ClusterServices
        End Sub

        <ExportAPI("View.Cnn")>
        Public Sub ConnectionView()
            Dim RefreshThread As New Threading.Thread(AddressOf InternalRefleshCnn)
            RefreshThread.Start()
            Call Console.ReadLine()
            RefreshThread.Abort()
        End Sub

        <ExportAPI("Node.New")>
        Public Sub NewNode(Cli As String)
            Call _ClusterServices.CreateNewNode(Cli)
        End Sub

        Private Sub InternalRefleshCnn()
            Do While True
                Call Console.Clear()
                Call Console.WriteLine(Now.ToString)
                Call Console.WriteLine("Connected Nodes:")
                Call Console.WriteLine(String.Join(vbCrLf, (From Node As PbsThread
                                                                In _ClusterServices.NodeServices
                                                            Select $"{Node.Guid}   {Node.RemoteHostEp.GetIPEndPoint.ToString}   {Node.ServerLoad}").ToArray))

                Call Threading.Thread.Sleep(1000)
            Loop
        End Sub

        <ExportAPI("Shutdown")>
        Public Sub Shutdown()

        End Sub
    End Class
End Namespace