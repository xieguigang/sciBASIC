Module ParallelTemplate

    Sub Main(argv As String())
        Dim File As String = argv(Scan0)
        Dim Port As Integer = CInt(Val(argv(1)))
        Dim LoadResult = ParallelLoadingTest.Load(File)  '数据加载
        Dim host As String = Microsoft.VisualBasic.Parallel.ParallelLoading.SendMessageAPI(Port)  '返回消息
        Dim Socket As New Microsoft.VisualBasic.MMFProtocol.MMFSocket(hostName:=host) '打开映射的端口
        Call Socket.SendMessage(LoadResult.GetSerializeBuffer) '返回内存数据
    End Sub
End Module
