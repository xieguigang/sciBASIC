Imports System.Runtime.CompilerServices
Imports System.Security
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Interpreter
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' Provides information about, and means to manipulate, the current environment Application information collection.
''' (More easily runtime environment information provider on LINUX platform for visualbasic program.)
''' </summary>
''' 
<PackageNamespace("App", Description:="More easily runtime environment information provider on LINUX platform for visualbasic program.",
                  Publisher:="amethyst.asuka@gcmodeller.org",
                  Url:="http://SourceForge.net/projects/shoal")>
Public Module App

    ''' <summary>
    ''' Get the <see cref="System.Diagnostics.Process"/> id(PID) of the current program process.
    ''' </summary>
    Public ReadOnly Property PID As Integer = System.Diagnostics.Process.GetCurrentProcess.Id
    ''' <summary>
    ''' Gets a new <see cref="System.Diagnostics.Process"/> component and associates it with the currently active process.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Process As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess

    ''' <summary>
    ''' Gets the command-line arguments for this <see cref="System.Diagnostics.Process"/>.
    ''' </summary>
    ''' <returns>Gets the command-line arguments for this process.</returns>
    Public ReadOnly Property CommandLine As CommandLine.CommandLine =
        Microsoft.VisualBasic.CommandLine.TryParse(Environment.GetCommandLineArgs.Skip(1).ToArray)

    ''' <summary>
    ''' Returns the argument portion of the <see cref="Microsoft.VisualBasic.CommandLine.CommandLine"/> used to start Visual Basic or
    ''' an executable program developed with Visual Basic. The My feature provides greater
    ''' productivity and performance than the <see cref="microsoft.VisualBasic.Interaction.Command"/> function. For more information,
    ''' see <see cref="Microsoft.VisualBasic.ApplicationServices.ConsoleApplicationBase.CommandLineArgs"/>.
    ''' </summary>
    ''' <returns>Gets the command-line arguments for this process.</returns>
    Public ReadOnly Property Command As String =
        Microsoft.VisualBasic.CommandLine.Join(App.CommandLine.Tokens)

    ''' <summary>
    ''' The file path of the current running program executable file.(本应用程序的可执行文件的文件路径)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ExecutablePath As String =
        Application.ExecutablePath   'FileIO.FileSystem.GetFileInfo(Process.GetCurrentProcess.StartInfo.FileName).FullName

    ''' <summary>
    ''' Gets the name, without the extension, of the assembly file for the application.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property AssemblyName As String =
        IO.Path.GetFileNameWithoutExtension(App.ExecutablePath)

    Public ReadOnly Property ProductName As String =
        If(String.IsNullOrEmpty(Application.ProductName.Trim), AssemblyName, Application.ProductName.Trim)

    ''' <summary>
    ''' The program directory of the current running program.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property HOME As String = FileIO.FileSystem.GetParentPath(ExecutablePath)

    ''' <summary>
    ''' 应用程序的当前的工作目录
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CurrentWork As String
        Get  ' 由于会因为切换目录而发生变化，所以这里不适用简写形式了
            Return FileIO.FileSystem.CurrentDirectory
        End Get
    End Property

    ''' <summary>
    ''' Gets the path for the executable file that started the application, not including the executable name.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StartupDirectory As String
        Get
            Return Application.StartupPath
        End Get
    End Property

    ''' <summary>
    ''' The repository root of the product application program data.  
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ProductProgramData As String =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{ProductName}"

    ''' <summary>
    ''' The shared program data directory for a group of app which have the same product series name.
    ''' (同一產品程序集所共享的數據文件夾)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ProductSharedDir As String = $"{ProductProgramData}/.shared"

    ''' <summary>
    ''' 应用程序的启动的时间
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StartTime As Long = Now.ToBinary

    ''' <summary>
    ''' 当前距离应用程序启动所逝去的时间
    ''' </summary>
    ''' <returns></returns>
    Public Function ElapsedMilliseconds() As Long
        Dim nowLng As Long = Now.ToBinary
        Dim d As Long = nowLng - StartTime
        Return d
    End Function

    ''' <summary>
    ''' The local data dir of the application in the %user%/&lt;CurrentUser>/Local/Product/App
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LocalData As String =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{ProductName}/{AssemblyName}"

    ''' <summary>
    ''' The temp directory in the application local data.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LocalDataTemp As String
        Get
            Return App.LocalData & "/Temp/"
        End Get
    End Property

    ''' <summary>
    ''' The directory path of the system temp data.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SysTemp As String = App.__sysTEMP

    ''' <summary>
    ''' <see cref="FileIO.FileSystem.GetParentPath"/>(<see cref="FileIO.FileSystem.GetTempFileName"/>) 
    ''' 当临时文件夹被删除掉了的时候，会出现崩溃。。。。所以弃用改用读取环境变量
    ''' </summary>
    ''' <returns></returns>
    Private Function __sysTEMP() As String
        Dim DIR As String = Environment.GetEnvironmentVariable("TMP")
        Try
            Call FileIO.FileSystem.CreateDirectory(DIR)
        Catch ex As Exception
            ' 不知道应该怎样处理，但是由于只是得到一个路径，所以在这里干脆忽略掉这个错误就可以了
            Call New Exception(DIR, ex).PrintException
        End Try

        Return DIR
    End Function

    ''' <summary>
    ''' Application temp data directory in the system temp root: %<see cref="App.SysTemp"/>%/<see cref="App.AssemblyName"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property AppSystemTemp As String
        Get
            Return SysTemp & "/" & App.AssemblyName
        End Get
    End Property

    ''' <summary>
    ''' Gets the product version associated with this application.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Version As String
        Get
            Return Application.ProductVersion
        End Get
    End Property

    ''' <summary>
    ''' Simply log application exception data into a log file which saves at location: %<see cref="App.LocalData"/>%/.logs/err/.
    ''' (简单日志记录，函数返回空值)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="Trace">调用函数的位置，这个参数一般为空，编译器会自动生成Trace位点参数</param>
    ''' <returns></returns>
    Public Function LogException(ex As Exception, <CallerMemberName> Optional Trace As String = "") As Object
        Call App.TraceBugs(ex, Trace)
        Return Nothing
    End Function

    ''' <summary>
    ''' 函数返回的是日志文件的文件路径
    ''' </summary>
    ''' <returns></returns>
    Public Function TraceBugs(ex As Exception, <CallerMemberName> Optional Trace As String = "") As String
        Dim Entry As String = App.__getTEMPhash
        Entry = $"{Now.Year}-{Now.Month}-{Now.Day}, {Format(Now.Hour, "00")}-{Format(Now.Minute, "00")}-{Format(Now.Second, "00")}_{Entry}"
        Dim log As String = $"{App.LogErrDir}/{Entry}.log"
        Call App.LogException(ex, Trace, log)
        Return log
    End Function

    ' tmp2A10.tmp

    Dim _tmpHash As Long = Long.MinValue

    Private Function __getTEMPhash() As String
        If _tmpHash = Long.MaxValue Then
            _tmpHash = Long.MinValue
        End If
        Return CStr(Threading.Interlocked.Increment(_tmpHash))
    End Function

    ''' <summary>
    ''' 由于可能会运行多个使用本模块的进程，单独考哈希来作为表示会产生冲突，所以这里使用应用程序的启动时间戳以及当前的哈希值来生成唯一标示
    ''' </summary>
    ''' <returns></returns>
    Private Function __getTEMP() As String
        Return $"tmp{App.StartTime}_{App.__getTEMPhash}"
    End Function

    ''' <summary>
    ''' 存放自动存储的错误日志的文件夹
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LogErrDir As String = App.LocalData & $"/.logs/err/"

    ''' <summary>
    ''' Simply log application exception data into a log file which saves at a user defined location parameter: <paramref name="FileName"/>.
    ''' (简单日志记录)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="Trace"></param>
    ''' <param name="FileName"></param>
    ''' <returns></returns>
    Public Function LogException(ex As Exception, Trace As String, FileName As String) As Object
        Call BugsFormatter(ex, Trace).SaveTo(FileName)
        Return Nothing
    End Function

    ''' <summary>
    ''' 生成简单的日志板块的内容
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="Trace"></param>
    ''' <returns></returns>
    Public Function BugsFormatter(ex As Exception, <CallerMemberName> Optional Trace As String = "") As String
        Dim exMsg As StringBuilder = New StringBuilder()
        Call exMsg.AppendLine("TIME:  " & Now.ToString)
        Call exMsg.AppendLine("TRACE: " & Trace)
        Call exMsg.AppendLine(New String("=", 120))
        Call exMsg.AppendLine(Logging.LogFile.SystemInfo)
        Call exMsg.AppendLine(New String("=", 120))
        Call exMsg.AppendLine(ex.ToString)
        Return exMsg.ToString
    End Function

    ''' <summary>
    ''' This is the custom message of the exception, not extract from the function <see cref="Exception.ToString()"/>
    ''' </summary>
    ''' <param name="exMsg">This is the custom message of the exception, not extract from the function <see cref="Exception.ToString()"/></param>
    ''' <param name="Trace"></param>
    ''' <returns></returns>
    <ExportAPI("Exception.Log")>
    Public Function LogException(exMsg As String, <CallerMemberName> Optional Trace As String = "") As Object
        Return App.LogException(New Exception(exMsg), Trace)
    End Function

    ''' <summary>
    ''' <see cref="App.LocalData"/>/error.log
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ExceptionLogFile As String
        Get
            Return App.LocalData & "/error.log"
        End Get
    End Property

#Region "CLI interpreter"

    ''' <summary>
    '''  Terminates this <see cref="System.Diagnostics.Process"/> and gives the underlying operating system the specified exit code.
    '''  (这个方法还会终止本应用程序里面的自动GC线程)
    ''' </summary>
    ''' <param name="state">Exit code to be given to the operating system. Use 0 (zero) to indicate that the process completed successfully.</param>
    ''' 
    <SecuritySafeCritical> Public Function [Exit](state As Integer) As Integer
        Call App.StopGC()
        Call Environment.Exit(state)
        Return state
    End Function

#If FRAMEWORD_CORE Then

    ''' <summary>
    ''' Running the <see cref="String"/> as cli command line and the specific type define as a <see cref="CommandLine.Interpreter"/>.
    ''' (请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    ''' 
    <ExportAPI("RunCLI", Info:="Running the string as cli command line and the specific type define as a interpreter.")>
    <Extension>
    Public Function RunCLI(Interpreter As Type, args As String) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Return App.__completeCLI(New CommandLine.Interpreter(Interpreter).Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    ''' 
    <ExportAPI("RunCLI",
             Info:="Running the string as cli command line and the specific type define as a interpreter.")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLine.CommandLine) As Integer
#If DEBUG Then
        Call args.CLICommandArgvs.__DEBUG_ECHO
#End If
        Return App.__completeCLI(New CommandLine.Interpreter(Interpreter).Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    ''' 
    <ExportAPI("RunCLI",
             Info:="Running the string as cli command line and the specific type define as a interpreter.")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLine.CommandLine, executeEmpty As __ExecuteEmptyCli) As Integer
#If DEBUG Then
        Call args.CLICommandArgvs.__DEBUG_ECHO
#End If
        Return App.__completeCLI(New CommandLine.Interpreter(Interpreter) With {
            .ExecuteEmptyCli = executeEmpty
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String, executeEmpty As CommandLine.Interpreter.__ExecuteEmptyCli) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Return App.__completeCLI(New CommandLine.Interpreter(Interpreter) With {
            .ExecuteEmptyCli = executeEmpty
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String, executeFile As CommandLine.Interpreter.__ExecuteFile) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Return App.__completeCLI(New CommandLine.Interpreter(Interpreter) With {
            .ExecuteFile = executeFile
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String,
                                       executeFile As CommandLine.Interpreter.__ExecuteFile,
                                       executeEmpty As CommandLine.Interpreter.__ExecuteEmptyCli) As Integer
        Return Interpreter.RunCLI(Microsoft.VisualBasic.CommandLine.TryParse(args),
                                  executeFile,
                                  executeEmpty)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLine.CommandLine,
                                       executeFile As CommandLine.Interpreter.__ExecuteFile,
                                       executeEmpty As CommandLine.Interpreter.__ExecuteEmptyCli) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Return App.__completeCLI(New CommandLine.Interpreter(Interpreter) With {
            .ExecuteFile = executeFile,
            .ExecuteEmptyCli = executeEmpty
        }.Execute(args))
    End Function
#End If

    ''' <summary>
    ''' IF the flag is True, that means cli API execute successfully, function returns ZERO, or a negative integer(Default -100) for failures.
    ''' </summary>
    ''' <param name="b"></param>
    ''' <param name="Failed"></param>
    ''' <returns></returns>
    <Extension> Public Function CLICode(b As Boolean, Optional Failed As Integer = -100) As Integer
        Return If(b, 0, Failed)
    End Function

#End Region

    ''' <summary>
    ''' Creates a uniquely named zero-byte temporary file on disk and returns the full
    ''' path of that file.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetTempFile() As String
        Dim Temp As String = FileIO.FileSystem.GetTempFileName
        Return GenerateTemp(Temp, "")
    End Function

    ''' <summary>
    ''' Get temp file name in app system temp directory.
    ''' </summary>
    ''' <param name="ext"></param>
    ''' <param name="sessionID"></param>
    ''' <returns></returns>
    Public Function GetAppSysTempFile(Optional ext As String = ".tmp", Optional sessionID As String = "") As String
        Dim tmp As String = App.SysTemp & "/" & __getTEMP() & ext  '  FileIO.FileSystem.GetTempFileName.Replace(".tmp", ext)
        tmp = GenerateTemp(tmp, sessionID)
        Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(tmp))
        tmp = FileIO.FileSystem.GetFileInfo(tmp).FullName.Replace("\", "/")
        Return tmp
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sysTemp">临时文件路径</param>
    ''' <returns></returns>
    Public Function GenerateTemp(sysTemp As String, SessionID As String) As String
        Dim Dir As String = FileIO.FileSystem.GetParentPath(sysTemp)
        Dim Name As String = FileIO.FileSystem.GetFileInfo(sysTemp).Name
        sysTemp = $"{Dir}/{App.AssemblyName}/{SessionID}/{Name}"
        Return sysTemp
    End Function

    ''' <summary>
    ''' 获取位于共享文件夹<see cref="App.ProductSharedDir"/>里面的临时文件
    ''' </summary>
    ''' <returns></returns>
    Public Function GetProductSharedTemp() As String
        'Dim Temp As String = FileIO.FileSystem.GetTempFileName
        Dim Name As String = App.__getTEMPhash  'FileIO.FileSystem.GetFileInfo(Temp).Name
        'Name = Name.ToUpper.Replace("TMP", "")
        Dim Temp = $"{App.ProductSharedTemp}/{App.AssemblyName}-{Name}.tmp"
        Return Temp
    End Function

    Public ReadOnly Property ProductSharedTemp As String = App.ProductSharedDir & "/tmp/"

    ''' <summary>
    ''' Self call this program itself for batch parallel task calculation.
    ''' (调用自身程序，这个通常是应用于批量的数据的计算任务的实现)
    ''' </summary>
    ''' <param name="CLI"></param>
    ''' <returns></returns>
    Public Function SelfFolk(CLI As String) As Microsoft.VisualBasic.CommandLine.IORedirectFile
        Dim process As CommandLine.IORedirectFile =
            New CommandLine.IORedirectFile(App.ExecutablePath, CLI)
        Return process
    End Function

#Region "Auto Garbage Cleaner"

    ''' <summary>
    ''' 自动垃圾回收线程
    ''' </summary>
    ReadOnly __GCThread As Parallel.UpdateThread =
        New Parallel.UpdateThread(10 * 60 * 1000, AddressOf App.__GCThreadInvoke)

    Dim _CLIAutoClean As Boolean = False

    Private Function __completeCLI(state As Integer) As Integer
        If _CLIAutoClean Then
            Call StopGC()
        End If
        Return state
    End Function

    ''' <summary>
    ''' Start the automatic garbage collection threads.
    ''' (这条线程只会自动清理*.tmp临时文件，因为假若不清理临时文件的话，有时候临时文件比较多的时候，会严重影响性能，甚至无法运行应用程序框架里面的IO重定向操作)
    ''' </summary>
    Public Sub StartGC(autoClose As Boolean)
        Call App.__GCThread.Start()
        App._CLIAutoClean = autoClose
    End Sub

    ''' <summary>
    ''' 自动垃圾回收线程
    ''' </summary>
    Private Sub __GCThreadInvoke()

        Call App.__removesTEMP(App.SysTemp)
        Call App.__removesTEMP(App.AppSystemTemp)
        Call App.__removesTEMP(App.ProductSharedTemp)
        Call App.__removesTEMP(App.LocalDataTemp)

        Call FlushMemory()
    End Sub

    Private Function __listFiles(DIR As String) As String()
        Try
            Return FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchAllSubDirectories, "*.tmp").ToArray
        Catch ex As Exception
            Return New String() {}
        End Try
    End Function

    Private Sub __removesTEMP(TEMP As String)
        Dim listFiles As String() = __listFiles(DIR:=TEMP)

        For Each file As String In listFiles
            Try
                Call FileIO.FileSystem.DeleteFile(file)
            Catch ex As Exception

            End Try
        Next
    End Sub

    ''' <summary>
    ''' Stop the automatic garbage collection threads.
    ''' </summary>
    Public Sub StopGC()
        Call App.__GCThread.Stop()
    End Sub
#End Region

End Module
