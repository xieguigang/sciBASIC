
Imports System.Reflection
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Parallel

    ''' <summary>
    ''' 主服务和子服务之间的相互作用的特点是子服务不会知道主服务节点的数据接口，所有的交互都是通过子服务上面的一个模块来监听主服务来实现的
    ''' 当主服务有数据需要向子服务更新的时候，会主动发送数据请求至子服务节点
    ''' </summary>
    ''' <remarks>当前的用户规模还比较小这里仅仅是实现了本地的调用，后面考虑到业务吞吐量的问题，会将服务的调用分开到两台物理主机之上</remarks>
    Public Module ServicesFolk

        ''' <summary>
        ''' 函数返回子进程的交互数据通信的端口号
        ''' </summary>
        ''' <param name="assm"></param>
        ''' <param name="CLI">命令行参数字符串，可以在这里加入一些其他的自定义数据</param>
        ''' <returns>函数返回子服务的交互端口</returns>
        Public Function Folk(assm As String, ByRef CLI As String, Optional ByRef folked As Process = Nothing) As Integer
            Dim Portal As Integer

            '开通一个临时的端口用来和子服务交互
            Using TempListen As New Net.TcpSynchronizationServicesSocket(
                Net.TCPExtensions.GetFirstAvailablePort,
                Sub(ex) App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName))

                Dim __getChildPortal As New __getChildPortal

                Call TempListen.InvokeSet(Of Net.Abstract.DataRequestHandler)(
                    NameOf(TempListen.Responsehandler),
                    AddressOf __getChildPortal.HandleRequest)

                Call RunTask(AddressOf TempListen.Run)
                Call TempListen.WaitForStart()

                Dim path As String =
                    If(assm.FileExists, FileIO.FileSystem.GetFileInfo(assm).FullName, $"{App.HOME}/{assm}")
#If DEBUG Then
                Call $"Invoke start  {path.ToFileURL}  @{MethodBase.GetCurrentMethod.GetFullName}".__DEBUG_ECHO
#End If
                Dim FolkSvr As Process =
                    Process.Start(path, $"{CLI} {ParentPortal} {TempListen.LocalPort}")

                __getChildPortal.PID = FolkSvr.Id
                Portal = __getChildPortal.WaitForPortal
                folked = FolkSvr
                CLI = __getChildPortal.addArgs
            End Using

            Dim Message As String = $"Get folked child services {assm} local_services:={Portal}".__DEBUG_ECHO

            If WindowsServices.Initialized Then
                Call ServicesLogs.WriteEntry(Message, EventLogEntryType.SuccessAudit)
            End If

            Return Portal
        End Function

        Private Class __getChildPortal

            Public PID As Integer
            Public Portal As Integer = -100
            ''' <summary>
            ''' 所返回来的额外的参数信息
            ''' </summary>
            Public addArgs As String

            Public Function HandleRequest(uid As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
                If uid <> PID Then
                    Return NetResponse.RFC_TOKEN_INVALID
                End If

                Dim result As String = request.GetUTF8String

                Portal = Scripting.CTypeDynamic(Of Integer)(result)
                addArgs = CommandLine.GetTokens(result).Get(1)

                Return NetResponse.RFC_OK
            End Function

            Public Function WaitForPortal() As Integer
                Do While Portal < 0
                    Call Threading.Thread.Sleep(1)
                Loop

                Return Portal
            End Function
        End Class

        Const ParentPortal As String = "--portal"

        ''' <summary>
        ''' 子服务向服务主节点返回端口号数据，这个方法需要要在子服务上面的服务程序启动之后再调用
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <param name="Port"></param>
        ''' <param name="addArgs">额外返回的参数信息</param>
        ''' <returns></returns>
        Public Function ReturnPortal(CLI As CommandLine.CommandLine, Port As Integer, Optional addArgs As String = "") As Boolean
            Dim parentPortal As Integer = CLI.GetInt32(ServicesFolk.ParentPortal)
            Dim Client As New Net.AsynInvoke("127.0.0.1", parentPortal)
#If DEBUG Then
            Call $"{MethodBase.GetCurrentMethod.GetFullName} ==> ""{CLI}"" returns {Port}".__DEBUG_ECHO
#End If
            Dim request As New RequestStream(Process.GetCurrentProcess.Id, 0, $"{CStr(Port)} ""{addArgs}""") With {
                .uid = Process.GetCurrentProcess.Id
            }
            Dim response As RequestStream = Client.SendMessage(request)
            Return response.Protocol = HTTP_RFC.RFC_OK
        End Function

        ''' <summary>
        ''' 当所需要进行计算的数据量比较大的时候，建议分块使用本函数生成多个进程进行批量计算以获得较好的计算效率
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="getCLI"></param>
        ''' <param name="getExe"></param>
        ''' <param name="numThreads">-1表示使用系统自动配置的参数，一次性提交所有的计算任务可能会是计算效率变得很低，所以需要使用这个参数来控制计算的线程数量</param>
        ''' <param name="TimeInterval">默认的任务提交时间间隔是一秒钟提交一个新的计算任务</param>
        Public Sub BatchTask(Of T)(source As IEnumerable(Of T),
                                   getCLI As Func(Of T, String),
                                   getExe As Func(Of String),
                                   Optional numThreads As Integer = -1,
                                   Optional TimeInterval As Integer = 1000)

            Dim srcArray As Func(Of Integer)() = (From x As T In source
                                                  Let task As CommandLine.IORedirectFile = New CommandLine.IORedirectFile(getExe(), getCLI(x))
                                                  Let runTask As Func(Of Integer) = AddressOf task.Run
                                                  Select runTask).ToArray
            Call BatchTask(srcArray, numThreads, TimeInterval)
        End Sub

        ''' <summary>
        ''' 由于LINQ是分片段来执行的，当某个片段有一个线程被卡住之后整个进程都会被卡住，所以执行大型的计算任务的时候效率不太好，使用这个并行化函数可以避免这个问题，同时也可以自己手动控制线程的并发数
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="actions"></param>
        ''' <param name="numThreads">可以在这里手动的控制任务的并发数</param>
        ''' <param name="TimeInterval"></param>
        Public Function BatchTask(Of T)(actions As Func(Of T)(), Optional numThreads As Integer = -1, Optional TimeInterval As Integer = 1000) As T()
            Dim taskPool As New List(Of AsyncHandle(Of T))
            Dim p As Integer = Scan0
            Dim resultList As New List(Of T)

            If numThreads <= 0 Then
                numThreads = LQuerySchedule.CPU_NUMBER * 2
            End If

            Do While p <= actions.Length - 1
                If taskPool.Count < numThreads Then  ' 向任务池里面添加新的并行任务
                    Call taskPool.Add(New AsyncHandle(Of T)(actions(p)).Run)
                    Call p.MoveNext
                End If

                Dim LQuery = (From task As AsyncHandle(Of T)
                              In taskPool
                              Where task.IsCompleted ' 在这里获得完成的任务
                              Select task).ToArray
                For Each completeTask As AsyncHandle(Of T) In LQuery
                    Call taskPool.Remove(completeTask)
                    Call resultList.Add(completeTask.GetValue)  '  将完成的任务从任务池之中移除然后获取返回值
                Next

                Call Threading.Thread.Sleep(TimeInterval)
            Loop

            Dim WaitForExit As T() = (From task In taskPool.AsParallel  ' 等待剩余的计算任务完成计算过程
                                      Let cli As T = task.GetValue
                                      Select cli).ToArray
            Call resultList.Add(WaitForExit)

            Return resultList.ToArray
        End Function
    End Module
End Namespace