Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CodeDOMExtension
Imports Microsoft.VisualBasic.CodeDOMExpressions
Imports Microsoft.VisualBasic.Net.Protocol

Namespace Parallel

    ''' <summary>
    ''' 
    ''' </summary>
    Public Module ParallelLoading

        <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
        Public Class LoadEntry : Inherits Attribute

            ''' <summary>
            ''' 必须满足接口类型： Function(path As String) As T
            ''' </summary>
            ''' <returns></returns>
            Public Property MethodEntryPoint As System.Reflection.MethodInfo

            Public ReadOnly Property LoadType As Type
                Get
                    Return MethodEntryPoint.DeclaringType
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return MethodEntryPoint.ToString
            End Function
        End Class

        ''' <summary>
        ''' 当目标数据集非常的大的时候，在单个应用程序里面进行加载已经回非常缓慢了，
        ''' 则这个时候可以使用这个函数将数据的加载任务分配到多个子进程之中以提高加载的时候的CPU的利用效率
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="sourceURL"></param>
        ''' <returns></returns>
        ''' <remarks>函数会自动从<typeparamref name="T"/></remarks>泛型类型之中解析出加载的函数
        Public Function Load(Of T)(sourceURL As Generic.IEnumerable(Of String), Optional TrimNull As Boolean = False) As KeyValuePair(Of String, T())()
            Dim TypeEntry As Type = GetType(T)
            Dim EntryPoint = Parallel.ParallelLoading.__loadEntry(TypeEntry)

            If EntryPoint Is Nothing Then
                Throw New Exception($"Could not found any entry point for type:={TypeEntry.ToString}!")
            End If

            Dim Process As String = DynamicsVBCTask(EntryPoint)  '开始进行任务进程的动态编译
            Dim LQuery = (From source As String                  '进行并行化任务调度
                          In sourceURL'这里不再使用并行化，因为启动Socket任务的需要为了避免端口占用的情况出现，任务不可以同时启动
                          Select source, Task = Load(Of T)(url:=source, Process:=Process)).ToArray
            Dim WaitTasks = (From mmfTask In LQuery.AsParallel   '等待任务的结束然后返回数据集
                             Let value As T() = mmfTask.Task.GetValue
                             Select New KeyValuePair(Of String, T())(mmfTask.source, value)).ToArray

            If TrimNull Then
                WaitTasks = (From obj In WaitTasks.AsParallel Where Not obj.Value.IsNullOrEmpty Select obj).ToArray
            End If
            Return WaitTasks
        End Function

        ''' <summary>
        ''' 通过与并行进程进行内存共享来传输加载完毕的数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="url"></param>
        ''' <param name="Process"></param>
        ''' <returns></returns>
        Private Function Load(Of T)(url As String, Process As String) As Task(Of String, T())
            Dim Task As New Task(Of String, T())(url, ParallelLoading.__loadTask(Of T)(Process))
            Return Task
        End Function

        Private Function __loadTask(Of T)(process As String) As Func(Of String, T())
            Call Threading.Thread.Sleep(1000)
            Return AddressOf New LoadTaskInvoker(Of T)(process).Load
        End Function

        Private Class LoadTaskInvoker(Of T)

            ReadOnly Process As String

            Sub New(Process As String)
                Me.Process = Process
            End Sub

            Public Function Load(url As String) As T()
                Dim Socket As New Microsoft.VisualBasic.Net.TcpSynchronizationServicesSocket(AddressOf DataProcessor, Net.GetFirstAvailablePort)
                Call New Threading.Thread(AddressOf Socket.Run).Start()
                Call StartProcess($"{url.CliPath} { Socket.LocalPort}")
                Call WaitForTaskComplete()
                Return resultBuffer
            End Function

            Private Sub StartProcess(argvs As String)
                Dim ProcStart = New ProcessStartInfo(Process, arguments:=argvs)
                Dim ProcInvoke As New Process With {.StartInfo = ProcStart}

                ProcStart.CreateNoWindow = True
                ProcInvoke.Start()
            End Sub

            Private Function DataProcessor(uid As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream
                Dim requestData As String = request.GetUTF8String

                If String.IsNullOrEmpty(requestData) Then
                    Return NetResponse.RFC_NO_CONTENT
                End If

                If requestData.StartsWith(MMFProtocol.MMFSocket.MMF_PROTOCOL) Then
                    '进程开始向父进程返回数据了
                    Dim host As String = Mid(requestData, Len(MMFProtocol.MMFSocket.MMF_PROTOCOL) + 1)
                    Call GetSendData(host)
                End If

                Return NetResponse.RFC_OK
            End Function

            Dim TaskComplete As Boolean = False

            Private Sub WaitForTaskComplete()
                Do While Not TaskComplete
                    Call Threading.Thread.Sleep(100)
                Loop
            End Sub

            Dim resultBuffer As T()
            Dim _client As MMFProtocol.MMFSocket

            Private Sub GetSendData(host As String)
                _client = New MMFProtocol.MMFSocket(host, AddressOf FillData)
            End Sub

            Private Sub FillData(byteBuffer As Byte())
                resultBuffer = byteBuffer.DeSerialize(Of T())
                TaskComplete = True
            End Sub
        End Class

        ''' <summary>
        ''' 动态编译的加载进程的调用API来向主进程返回消息
        ''' </summary>
        ''' <param name="Port"></param>
        ''' <returns></returns>
        Public Function SendMessageAPI(Port As Integer) As String
            Dim host As String = "Parallel-" & Process.GetCurrentProcess.Id
            Dim Client As New Microsoft.VisualBasic.Net.AsynInvoke("127.0.0.1", Port)
            Call Client.SendMessage($"{MMFProtocol.MMFSocket.MMF_PROTOCOL}{host}")
            Return host
        End Function

        ''' <summary>
        ''' 递归的获取该类型所处的模块的所有的依赖关系，返回来的是全路径
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        Public Function GetReferences(Type As Type) As String()
            Dim assembly = Type.Assembly
            Return GetReferences(assembly, False)
        End Function

        ''' <summary>
        ''' 有一些会出现循环引用的情况？？？？？
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <param name="i"></param>
        ''' <param name="refList"></param>
        Private Sub getReferences(assembly As System.Reflection.Assembly, i As Integer, ByRef refList As List(Of String))
            Dim refListBuffer = assembly.GetReferencedAssemblies
            Dim Temp = refList
            Dim LQuery = (From ref In refListBuffer
                          Where Not String.IsNullOrEmpty(ref.FullName)
                          Let entry = ref.FullName
                          Select refListValue = getReferences(url:=entry, i:=i + 1, refList:=Temp)).ToList
            Dim resultBuffer = LQuery.MatrixToList
            Call resultBuffer.Add(assembly.Location)
            Call refList.AddRange(resultBuffer)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <param name="removeSystem">是否移除系统引用</param>
        ''' <returns></returns>
        Public Function GetReferences(assembly As System.Reflection.Assembly, removeSystem As Boolean, Optional strict As Boolean = True) As String()
            Dim refList As List(Of String) = New List(Of String)

            Call getReferences(assembly, 0, refList)

            '添加VB_Framework的引用
            Call refList.AddRange((From ref In GetType(Parallel.ParallelLoading).Assembly.GetReferencedAssemblies
                                   Let ass = System.Reflection.Assembly.Load(ref.FullName)
                                   Select ass.Location).ToArray)
            refList = refList.Distinct.ToList

            If removeSystem Then
                refList = (From path As String
                           In refList.AsParallel
                           Where Not IsSystemAssembly(path, strict)
                           Select path).ToList
            End If

            Return refList.ToArray
        End Function

        ''' <summary>
        ''' 放在C:\WINDOWS\Microsoft.Net\这个文件夹下面的所有的引用都是本地编译的，哈希值已经不对了
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Function IsSystemAssembly(url As String, strict As Boolean) As Boolean
            Dim assemblyDir As String = FileIO.FileSystem.GetDirectoryInfo(FileIO.FileSystem.GetParentPath(url)).FullName.Replace("/", "\")

            If Not assemblyDir.Last = "\" Then
                assemblyDir &= "\"
            End If

            If String.Equals(RunTimeDirectory, assemblyDir) OrElse
               assemblyDir.StartsWith("C:\WINDOWS\Microsoft.Net\assembly\GAC_", StringComparison.OrdinalIgnoreCase) OrElse
               assemblyDir.StartsWith("C:\Windows\Microsoft.NET\Framework64", StringComparison.OrdinalIgnoreCase) OrElse
               assemblyDir.StartsWith("C:\Windows\Microsoft.NET\Framework", StringComparison.OrdinalIgnoreCase) Then

                If strict Then
                    Return True
                Else
                    Dim Name As String = IO.Path.GetFileNameWithoutExtension(url)
                    If String.Equals(Name, "mscorlib") OrElse String.Equals(Name, "System") OrElse Name.StartsWith("System.") Then
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url">+特殊符号存在于这个字符串之中的话，函数会出错</param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Private Function getReferences(url As String, i As Integer, ByRef refList As List(Of String)) As String()
            Dim assembly = System.Reflection.Assembly.Load(url)

            If IsSystemAssembly(assembly.Location, True) OrElse refList.IndexOf(assembly.Location) > -1 Then
                Return New String() {}
            Else
#If DEBUG Then
                Call $"{New String(" "c, i)}{assembly.Location}".__DEBUG_ECHO
#End If
                Call refList.Add(assembly.Location)
            End If

            Call getReferences(assembly, i:=i + 1, refList:=refList)
            Return refList.ToArray
        End Function

        Public ReadOnly Property RunTimeDirectory As String = FileIO.FileSystem.GetDirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory).FullName.Replace("/", "\")

        ''' <summary>
        ''' 动态编译
        ''' </summary>
        ''' <param name="LoadEntry"></param>
        ''' <returns></returns>
        Public Function DynamicsVBCTask(LoadEntry As LoadEntry) As String
            Dim refList As String() = GetReferences(LoadEntry.LoadType)
            Dim ns As New CodeDom.CodeNamespace(NameOf(Parallel.ParallelLoading))

            Call ns.Types.Add(__subMain(LoadEntry))
            Call ns.GenerateCode.__DEBUG_ECHO

            Dim assembly As System.Reflection.Assembly = ns.Compile(refList, RunTimeDirectory, CodeDOMExtension.ExecutableProfile)
            Dim Dir As String = FileIO.FileSystem.GetParentPath(assembly.Location)
            For Each File As String In refList
                Dim buffer = IO.File.ReadAllBytes(File)
                Dim Saved As String = $"{Dir}/{FileIO.FileSystem.GetFileInfo(File).Name}"
                Try
                    Call IO.File.WriteAllBytes(Saved, buffer)
                Catch ex As Exception
                    Call ex.PrintException
                End Try
            Next
            Return assembly.Location
        End Function

        Private Function __subMain(loadEntry As LoadEntry) As CodeDom.CodeTypeDeclaration
            Dim ProgramEntry As New CodeDom.CodeTypeDeclaration("Program")
            Dim SubMain As New CodeDom.CodeMemberMethod()

            Call ProgramEntry.Members.Add(SubMain)
            SubMain.Name = "Main"
            SubMain.ReturnType = New CodeDom.CodeTypeReference(GetType(System.Void))
            SubMain.Parameters.Add(New CodeDom.CodeParameterDeclarationExpression(GetType(String()), SubMainArgv))
            SubMain.Attributes = CodeDom.MemberAttributes.Public Or CodeDom.MemberAttributes.Static
            SubMain = __parallelLoading(invoke:=SubMain, loadEntry:=loadEntry)

            Return ProgramEntry
        End Function

        Const SubMainArgv As String = "Argv"
        Const LoadFile As String = "File"
        Const LoadResult As String = "LoadResult"
        Const Port As String = "Port"
        Const Host As String = "host"
        Const Socket As String = "Socket"
        Const Buffer As String = "buffer"

        Private Function __parallelLoading(invoke As CodeDom.CodeMemberMethod, loadEntry As LoadEntry) As CodeDom.CodeMemberMethod

            ' Dim File As String = argv(Scan0)
            Call invoke.Statements.Add(LocalsInit(LoadFile, GetType(String), CodeDOMExpressions.GetValue(New CodeDom.CodeArgumentReferenceExpression(SubMainArgv), Scan0)))

            Dim PortValue As CodeDom.CodeExpression = CodeDOMExpressions.GetValue(New CodeDom.CodeArgumentReferenceExpression(SubMainArgv), 1)
            PortValue = [Call](GetType(Conversion), NameOf(Conversion.Val), {PortValue})
            PortValue = Cast(PortValue, GetType(Integer))

            ' Dim Port As Integer = CInt(Val(argv(1)))
            Call invoke.Statements.Add(LocalsInit(Port, GetType(Integer), PortValue))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO),
                                              {
                                                [Call](GetType(String), NameOf(String.Format), {Value("Load stream from url:={0}, port:={1}..."), LocalVariable(LoadFile), LocalVariable(Port)})
                                              }))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO), {"Start to loading data..."}))
            ' Dim LoadResult = ParallelLoadingTest.Load(File)  '数据加载
            Call invoke.Statements.Add(LocalsInit(LoadResult, loadEntry.LoadType.MakeArrayType, initExpression:=[Call](loadEntry.MethodEntryPoint, {LocalVariable(LoadFile)})))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO), {"Data loading Job Done!"}))

            '得到结果之后进行序列化通过内存映射共享返回给主程序
            ' Dim host As String = Microsoft.VisualBasic.Parallel.ParallelLoading.SendMessageAPI(Port)  '返回消息
            Call invoke.Statements.Add(LocalsInit(Host, GetType(String), [Call](GetType(ParallelLoading), NameOf(SendMessageAPI), {LocalVariable(Port)})))
            ' Dim Socket As New Microsoft.VisualBasic.MMFProtocol.MMFSocket(hostName:=host) '打开映射的端口
            Call invoke.Statements.Add(LocalsInit(Socket, GetType(MMFProtocol.MMFSocket), [New](GetType(MMFProtocol.MMFSocket), {LocalVariable(Host)})))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO), {"Init transfer device job done, start to transferred data!"}))
            ' Call Socket.SendMessage(LoadResult.GetSerializeBuffer) '返回内存数据
            Call invoke.Statements.Add(LocalsInit(Buffer, GetType(Byte()), [Call](GetType(Extensions), NameOf(GetSerializeBuffer), {LocalVariable(LoadResult)})))
            Call invoke.Statements.Add([Call](LocalVariable(Socket), NameOf(MMFProtocol.MMFSocket.SendMessage), {LocalVariable(Buffer)}))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO), {"Data transportation Job Done!"}))

            Return invoke
        End Function

        Private Function __loadEntry(Type As Type) As LoadEntry
            Dim Entries = Type.GetMethods(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Static)
            If Entries.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim LQuery = (From EntryPoint As System.Reflection.MethodInfo
                              In Entries.AsParallel
                          Let attrs As Object() = EntryPoint.GetCustomAttributes(attributeType:=GetType(LoadEntry), inherit:=True)
                          Where Not attrs.IsNullOrEmpty
                          Let LoadEntry = DirectCast(attrs.First, LoadEntry)
                          Select LoadEntry.InvokeSet(Of System.Reflection.MethodInfo)(NameOf(ParallelLoading.LoadEntry.MethodEntryPoint), EntryPoint)).ToArray
            If LQuery.IsNullOrEmpty Then
                Return Nothing
            Else
                Return LQuery.First
            End If
        End Function

        Delegate Function ParallelLoad(Of T)(sourceUrl As String) As T()

    End Module
End Namespace