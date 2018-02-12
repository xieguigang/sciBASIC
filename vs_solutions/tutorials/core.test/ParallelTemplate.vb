#Region "Microsoft.VisualBasic::ab9785f6578b27340a9d868b0b454005, ParallelTemplate.vb"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module ParallelTemplate
    ' 
    '     Sub: Main
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::079c39c18aa05ac55970509e9ad8cfc3, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module ParallelTemplate
    ' 
    '     Sub: Main
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::5f7806e3be017574758ccbbca8142932, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module ParallelTemplate
    ' 
    '     Sub: Main
    ' 
    ' 
    ' 

#End Region

Imports Microsoft.VisualBasic.MMFProtocol

Module ParallelTemplate

    Sub Main(argv As String())
        Dim File As String = argv(Scan0)
        Dim Port As Integer = CInt(Val(argv(1)))
        Dim LoadResult = ParallelLoadingTest.Load(File)  '数据加载
        Dim host As String = Microsoft.VisualBasic.Parallel.ParallelLoading.SendMessageAPI(Port)  '返回消息
        Dim Socket As New MMFSocket(host) '打开映射的端口
        '   Call Socket.SendMessage(LoadResult.GetSerializeBuffer) '返回内存数据
    End Sub
End Module


