
''' <summary>
''' 我想要构建的是一个去中心化的网格计算框架
''' </summary>
Module ServicesProgram

    Sub Main()

        Call Microsoft.VisualBasic.ComputingServices.TaskHost.test()

        '  Call Microsoft.VisualBasic.ComputingServices.CLI_InitStart("./GridNode.exe", "cli /wan 127.0.0.1")

        Dim master As New Microsoft.VisualBasic.ComputingServices.Asymmetric.Master("1234567890")
        Call Microsoft.VisualBasic.Parallel.Run(AddressOf master.Run)
        Call Threading.Thread.Sleep(1000)

        Dim nodeMgr As New Microsoft.VisualBasic.ComputingServices.Asymmetric.Parasitifer("./GridNode.exe", "127.0.0.1", "1234567890")
    End Sub
End Module
