#Region "Microsoft.VisualBasic::d8f9d96b5fc8a1142785fa150877cdb8, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\ParallelLoading.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module ParallelLoading
    ' 
    '         Function: __loadEntry, __loadTask, __parallelLoading, __subMain, DynamicsVBCTask
    '                   (+2 Overloads) Load, SendMessageAPI
    '         Class LoadEntry
    ' 
    '             Properties: LoadType, MethodEntryPoint
    ' 
    '             Function: ToString
    ' 
    '         Class LoadTaskInvoker
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: DataProcessor, Load
    ' 
    '             Sub: FillData, GetSendData, StartProcess, WaitForTaskComplete
    ' 
    '         Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Emit.CodeDOM_VBC
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

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
                Call StartProcess($"{url.CLIPath} { Socket.LocalPort}")
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
                '  resultBuffer = byteBuffer.DeSerialize(Of T())
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
            PortValue = [CType](PortValue, GetType(Integer))

            ' Dim Port As Integer = CInt(Val(argv(1)))
            Call invoke.Statements.Add(LocalsInit(Port, GetType(Integer), PortValue))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO),
                                              {
                                                [Call](GetType(String), NameOf(String.Format), {CodeDOMExpressions.Value("Load stream from url:={0}, port:={1}..."), LocalVariable(LoadFile), LocalVariable(Port)})
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
            Call invoke.Statements.Add(LocalsInit(Buffer, GetType(Byte()), [Call](GetType(StructSerializer), NameOf(StructureToByte), {LocalVariable(LoadResult)})))
            Call invoke.Statements.Add([Call](LocalVariable(Socket), NameOf(MMFProtocol.MMFSocket.SendMessage), {LocalVariable(Buffer)}))
            Call invoke.Statements.Add([Call](GetType(Extensions), NameOf(__DEBUG_ECHO), {"Data transportation Job Done!"}))

            Return invoke
        End Function

        Private Function __loadEntry(Type As Type) As LoadEntry
            Dim Entries = Type.GetMethods(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Static)
            If Entries.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim setValue = New SetValue(Of ParallelLoading.LoadEntry)() _
                .GetSet(NameOf(ParallelLoading.LoadEntry.MethodEntryPoint))
            Dim LQuery As LoadEntry =
                LinqAPI.DefaultFirst(Of LoadEntry) <= From EntryPoint As System.Reflection.MethodInfo
                                                      In Entries.AsParallel
                                                      Let attrs As Object() = EntryPoint.GetCustomAttributes(attributeType:=GetType(LoadEntry), inherit:=True)
                                                      Where Not attrs.IsNullOrEmpty
                                                      Let LoadEntry = DirectCast(attrs.First, LoadEntry)
                                                      Select setValue(LoadEntry, EntryPoint)
            Return LQuery
        End Function

        Delegate Function ParallelLoad(Of T)(sourceUrl As String) As T()

    End Module
End Namespace
