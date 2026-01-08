#Region "Microsoft.VisualBasic::1bce6871822a3a7b46065a24fc3865dc, Microsoft.VisualBasic.Core\src\ApplicationServices\App.vb"

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


    ' Code Statistics:

    '   Total Lines: 1599
    '    Code Lines: 779 (48.72%)
    ' Comment Lines: 634 (39.65%)
    '    - Xml Docs: 83.60%
    ' 
    '   Blank Lines: 186 (11.63%)
    '     File Size: 68.75 KB


    ' Module App
    ' 
    '     Properties: AppSystemTemp, AssemblyName, BufferSize, Command, CommandLine
    '                 CPUCoreNumbers, CurrentDirectory, CurrentProcessTemp, CurrentUnixTimeMillis, Desktop
    '                 EnableAnsiColor, EnableTqdm, ExecutablePath, GetLastError, Github
    '                 HOME, Info, InputFile, IsConsoleApp, IsMicrosoftPlatform
    '                 LocalData, LocalDataTemp, LogErrDIR, LogFile, MemoryLoad
    '                 n_threads, NanoTime, NextTempName, OutFile, PID
    '                 Platform, PreviousDirectory, Process, ProductName, ProductProgramData
    '                 ProductSharedDIR, ProductSharedTemp, Running, RunningInGitBash, RunTimeDirectory
    '                 StartTime, StartupDirectory, StdErr, StdInput, StdOut
    '                 SysTemp, UnixTimeStamp, UserHOME, Version
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: __listFiles, (+2 Overloads) Argument, CheckIsMicrosoftPlatform, CLICode, (+2 Overloads) ElapsedMilliseconds
    '               Exit, finalizeCLI, FormatTime, (+2 Overloads) GetAppLocalData, GetAppVariables
    '               GetFile, GetNextUniqueName, GetProductSharedDIR, GetProductSharedTemp, GetTempFile
    '               GetVariable, (+3 Overloads) LogException, NullDevice, RedirectErrLogging, RedirectLogging
    '               (+13 Overloads) RunCLI, RunCLIInternal, SelfFolk, Shell, tempCode
    '               TemporaryEnvironment, TraceBugs
    ' 
    '     Sub: __GCThreadInvoke, __removesTEMP, [Stop], AddExitCleanHook, (+2 Overloads) DoNothing
    '          FlushMemory, Free, JoinVariable, (+2 Overloads) JoinVariables, Pause
    '          (+2 Overloads) println, SetBufferSize, SetSystemTemp, StartGC, StopGC
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.My.UNIX
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes
Imports CommandLineArgs = Microsoft.VisualBasic.CommandLine.CommandLine
Imports DevAssmInfo = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo
Imports FS = Microsoft.VisualBasic.FileIO.FileSystem
Imports std = System.Math

'                   _ooOoo_
'                  o8888888o
'                  88" . "88
'                  (| -_- |)
'                  O\  =  /O
'               ____/`---'\____
'             .'  \\|     |//  `.
'            /  \\|||  :  |||//  \
'           /  _||||| -:- |||||-  \
'           |   | \\\  -  /// |   |
'           | \_|  ''\---/''  |   |
'           \  .-\__  `-`  ___/-. /
'         ___`. .'  /--.--\  `. . __
'      ."" '<  `.___\_<|>_/___.'  >'"".
'     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
'     \  \ `-.   \_ __\ /__ _/   .-` /  /
'======`-.____`-.___\_____/___.-`____.-'======
'                   `=---='
'^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
'           佛祖保佑       永无BUG
'           心外无法       法外无心

''' <summary>
''' Provides information about, and means to manipulate, the current environment Application information collection.
''' (More easily runtime environment information provider on <see cref="PlatformID.Unix"/>/LINUX server platform for VisualBasic program.)
''' (从命令行之中使用``/@set``参数赋值环境变量的时候，每一个变量之间使用分号进行分隔)
''' </summary>
'''
<Package("App", Description:="More easily runtime environment information provider on LINUX platform for VisualBasic program.",
                  Publisher:="amethyst.asuka@gcmodeller.org",
                  Url:="http://SourceForge.net/projects/shoal")>
Public Module App

    ''' <summary>
    ''' 运行时环境所安装的文件夹的位置
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property RunTimeDirectory As String

    ''' <summary>
    ''' Gets the number of ticks that represent the date and time of this instance.
    ''' 
    ''' The number of ticks that represent the date and time of this instance. The value
    ''' is between <see cref="DateTime.MinValue"/> and <see cref="DateTime.MaxValue"/>.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NanoTime As Long
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Now.Ticks
        End Get
    End Property

    ''' <summary>
    ''' Enable of show tqdm progress bar? default is enable.
    ''' </summary>
    ''' <returns>default is true, get config from ``tqdm`` variable from the runtime framework.</returns>
    ''' <remarks>
    ''' There is a bug about the tqdm progressbar when processing as a slave background process on windows system.
    ''' </remarks>
    Public ReadOnly Property EnableTqdm As Boolean
        Get
            Return App.GetVariable("tqdm", "TRUE").ParseBoolean
        End Get
    End Property

    Public ReadOnly Property EnableAnsiColor As Boolean
        Get
            Return App.GetVariable("ansi_color", "TRUE").ParseBoolean AndAlso
                Not App.GetVariable("internal_pipeline", "false").ParseBoolean
        End Get
    End Property

    ''' <summary>
    ''' Numbers of the CPU kernels on the current machine.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 获取当前的系统主机的CPU逻辑核心数，当不开启超线程的时候才是返回物理核心数
    ''' </remarks>
    Public ReadOnly Property CPUCoreNumbers As Integer = LQuerySchedule.CPU_NUMBER

    Public ReadOnly Property MemoryLoad As MemoryLoads
        Get
            Return m_memoryLoad
        End Get
    End Property

    Friend m_memoryLoad As MemoryLoads = MemoryLoads.Light

    ''' <summary>
    ''' 判断当前运行的程序是否为Console类型的应用和程序，由于在执行初始化的时候，
    ''' 最先被初始化的是这个模块，所以没有任何代码能够先执行<see cref="Console.IsErrorRedirected"/>了，
    ''' 在这里使用<see cref="Console.IsErrorRedirected"/>这个来进行判断是可靠的
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsConsoleApp As Boolean = (Not Console.IsErrorRedirected) OrElse (Not Console.IsOutputRedirected)

    ''' <summary>
    ''' Gets a path name pointing to the Desktop directory.
    ''' </summary>
    ''' <returns>The path to the Desktop directory.</returns>
    Public ReadOnly Property Desktop As String
    Public ReadOnly Property StdErr As New StreamWriter(Console.OpenStandardError)

    ''' <summary>
    ''' <see cref="Console.OpenStandardOutput()"/> as default text output device. [<see cref="StreamWriter"/>]
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StdOut As [Default](Of TextWriter)

    ''' <summary>
    ''' <see cref="Console.OpenStandardInput"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StdInput As [Default](Of TextReader) = New StreamReader(Console.OpenStandardInput)

    ''' <summary>
    ''' Get the <see cref="System.Diagnostics.Process"/> id(PID) of the current program process.
    ''' </summary>
    Public ReadOnly Property PID As Integer = Process.GetCurrentProcess.Id
    ''' <summary>
    ''' Gets a new <see cref="System.Diagnostics.Process"/> component and associates it with the currently active process.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Process As Process = Process.GetCurrentProcess

    ''' <summary>
    ''' Gets the command-line arguments for this <see cref="Process"/>.
    ''' </summary>
    ''' <returns>Gets the command-line arguments for this process.</returns>
    Public ReadOnly Property CommandLine As CommandLineArgs = GitBashEnvironment.GetCommandLineArgs()

    Public ReadOnly Property n_threads As Integer = std.Min(8, LQuerySchedule.CPU_NUMBER)

    ''' <summary>
    ''' Get argument value from <see cref="CommandLine"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="name$"></param>
    ''' <returns></returns>
    Public Function Argument(Of T)(name$) As T
        With CommandLine(name).DefaultValue
            If .StringEmpty Then
                Return Nothing
            Else
                Return Scripting.CTypeDynamic(Of T)(.ByRef)
            End If
        End With
    End Function

    ''' <summary>
    ''' Get argument value string from <see cref="CommandLine"/>.
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Argument(name$) As String
        Return CommandLine(name)
    End Function

    Public ReadOnly Property Github As String = LICENSE.githubURL
    Public ReadOnly Property RunningInGitBash As Boolean = GitBashEnvironment.isRunningOnGitBash()

    ''' <summary>
    ''' Returns the argument portion of the <see cref="Microsoft.VisualBasic.CommandLine.CommandLine"/> used to start Visual Basic or
    ''' an executable program developed with Visual Basic. The My feature provides greater
    ''' productivity and performance than the <see cref="microsoft.VisualBasic.Interaction.Command"/> function. For more information,
    ''' see <see cref="GitBashEnvironment.GetCommandLineArgs()"/>.
    ''' </summary>
    ''' <returns>Gets the command-line arguments for this process.</returns>
    Public ReadOnly Property Command As String = CLITools.Join(App.CommandLine.Tokens)

    ''' <summary>
    ''' The file path of the current running program executable file.(本应用程序的可执行文件的文件路径)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ExecutablePath As String
    ''' <summary>
    ''' Get assembly info of current running ``*.exe`` program.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Info As DevAssmInfo

    ''' <summary>
    ''' Gets the name, without the extension, of the assembly file for the application.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property AssemblyName As String
    Public ReadOnly Property ProductName As String

    ''' <summary>
    ''' The program directory of the current running program.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property HOME As String
    ''' <summary>
    ''' Getting the path of the home directory
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property UserHOME As String

    ''' <summary>
    ''' Gets the ``/in`` commandline value as the input file path.
    ''' </summary>
    ''' <returns></returns>
    Public Property InputFile As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return App.CommandLine("/in")
        End Get
        Friend Set(value As String)
            App.CommandLine.Add("/in", value)
        End Set
    End Property

    Dim _out$

    ''' <summary>
    ''' Gets the ``/out`` commandline value as the output file path.
    ''' </summary>
    ''' <returns></returns>
    Public Property OutFile As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            If _out.StringEmpty Then
                _out = App.CommandLine("/out")
            End If

            Return _out
        End Get
        Set(value As String)
            _out = value
        End Set
    End Property

    ''' <summary>
    ''' Found the file path based on the current application context.
    ''' 
    ''' 1. 直接查找(这个查找已经包含了在当前的文件夹之中查找)
    ''' 2. 从<see cref="App.InputFile"/>所在的文件夹之中查找
    ''' 3. 从<see cref="App.OutFile"/>所在的文件夹之中查找
    ''' 4. 从<see cref="App.Home"/>文件夹之中查找
    ''' 5. 从<see cref="App.UserHOME"/>文件夹之中查找
    ''' 6. 从<see cref="App.ProductProgramData"/>文件夹之中查找
    ''' </summary>
    ''' <param name="fileName$"></param>
    ''' <returns></returns>
    Public Function GetFile(fileName$) As String
        If fileName.FileExists Then
            Return fileName.GetFullPath
        End If

        Dim path As New Value(Of String)

        On Error Resume Next

        If Not App.InputFile.StringEmpty AndAlso
            (path = App.InputFile.ParentPath & "/" & fileName).FileExists Then

            Return path
        End If
        If Not App.OutFile.StringEmpty AndAlso
            (path = App.OutFile.ParentPath & "/" & fileName).FileExists Then

            Return path
        End If

        For Each DIR As String In {
            App.HOME,
            App.UserHOME,
            App.ProductProgramData,
            App.ProductSharedDIR
        }
            If (path = DIR & "/" & fileName).FileExists Then
                Return path
            End If
        Next

        Return App.CurrentDirectory & "/" & fileName
    End Function

    ''' <summary>
    ''' The currrent working directory of this application.(应用程序的当前的工作目录)
    ''' </summary>
    ''' <returns></returns>
    Public Property CurrentDirectory As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            ' 由于会因为切换目录而发生变化，所以这里不适用简写形式了
            Return FS.CurrentDirectory
        End Get
        Set(value As String)
            If String.Equals(value, "-") Then  ' 切换到前一个工作目录
                value = PreviousDirectory
            Else
                _PreviousDirectory = FS.CurrentDirectory
            End If

            FS.CreateDirectory(value)
            FS.CurrentDirectory = value
        End Set
    End Property

    ''' <summary>
    ''' -
    ''' Linux里面的前一个文件夹
    ''' </summary>
    ''' <remarks>
    ''' 假设你之前好不容易进入了一个很深的目录，然后不小心敲了个 ``cd /``，是不是快气晕了啊，不用着急，通过下面的指令可以轻松的回到前一个指令：
    '''
    ''' ```bash
    ''' cd -
    ''' ```
    ''' </remarks>
    Public ReadOnly Property PreviousDirectory As String

    ''' <summary>
    ''' Gets the path for the executable file that started the application, not including the executable name.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StartupDirectory As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Application.StartupPath
        End Get
    End Property

    ''' <summary>
    ''' The repository root of the product application program data.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ProductProgramData As String

    ''' <summary>
    ''' The shared program data directory for a group of app which have the same product series name.
    ''' (同一產品程序集所共享的數據文件夾)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ProductSharedDIR As String

    Sub New()
        ' On Error Resume Next ' 在Linux服务器上面不起作用？？？
        PreviousDirectory = App.StartupDirectory

#Region "公共模块内的所有的文件路径初始化"
        ' 因为vb的基础运行时环境在Linux平台上面对文件系统的支持还不是太完善，所以不能够放在属性的位置直接赋值，否则比较难处理异常
        ' 现在放在这个构造函数之中，强制忽略掉错误继续执行，提升一些稳定性，防止出现程序无法启动的情况出现。

        ' 请注意，这里的变量都是有先后的初始化顺序的
        Try
            App.RunTimeDirectory = FS _
                .GetDirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory) _
                .FullName _
                .Replace("/", "\")
            App.Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            App.ExecutablePath = FS.GetFileInfo(Application.ExecutablePath).FullName    ' (Process.GetCurrentProcess.StartInfo.FileName).FullName
            App.Info = ApplicationInfoUtils.CurrentExe()
            App.AssemblyName = BaseName(App.ExecutablePath)
            App.ProductName = Application.ProductName Or AssemblyName.AsDefault(Function(s) String.IsNullOrEmpty(s))
            App.HOME = FS.GetParentPath(App.ExecutablePath)
            App.UserHOME = PathMapper.HOME.GetDirectoryFullPath("App.New(.cctor)")
            App.ProductProgramData = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{ProductName}".GetDirectoryFullPath("App.New(.cctor)")
            App.ProductSharedDIR = $"{ProductProgramData}/.shared".GetDirectoryFullPath
            App.LocalData = App.GetAppLocalData(ProductName, AssemblyName, "App.New(.cctor)")
            App.CurrentProcessTemp = TempFileSystem.GenerateTemp(App.SysTemp & "/tmp.io", App.PID).GetDirectoryFullPath("App.New(.cctor)")
            App.ProductSharedTemp = App.ProductSharedDIR & "/tmp/"
            App.LogErrDIR = App.LocalData & $"/.logs/err/"
        Catch ex As Exception

        End Try
#End Region

        If App.HOME.StringEmpty Then
            App.HOME = Directory.GetCurrentDirectory
        End If

        Call FS.CreateDirectory(AppSystemTemp)
        ' Call FileIO.FileSystem.CreateDirectory(App.HOME & "/Resources/")

        ' 2018-08-14 因为经过测试发现text encoding模块会优先于命令行参数设置模块的初始化的加载
        ' 所以会导致环境变量为空
        ' 故而text encoding可能总是系统的默认值，无法从命令行设置
        ' 在这里提前进行初始化，可以消除此bug的出现
        Dim envir As Dictionary(Of String, String) = App _
            .CommandLine _
            .EnvironmentVariables

        Call App.JoinVariables(
            envir _
            .SafeQuery _
            .Select(Function(x)
                        Return New NamedValue(Of String) With {
                            .Name = x.Key,
                            .Value = x.Value
                        }
                    End Function) _
            .ToArray)

        ' 20200428
        ' 因为在CodePage拓展函数所属的TextEncodings模块的构造函数之中，会需要调用当前的这个App模块之中的环境变量函数
        ' 进行默认字符编码的设置，所以在这里不可以使用CodePage拓展函数，否则会产生循环引用导致程序初始化错误
        '
        ' System.TypeInitializationException: The type initializer for 'Microsoft.VisualBasic.App' threw an exception. ---> System.TypeInitializationException: The type initializer for 'Microsoft.VisualBasic.Text.TextEncodings' threw an exception. ---> System.NullReferenceException: Object reference not set to an instance of an object
        ' at Microsoft.VisualBasic.App.GetVariable (System.String name) [0x00001] in <eb97044717724341a21be2d5b902e6d1>:0
        ' at Microsoft.VisualBasic.Text.TextEncodings..cctor () [0x00034] in <eb97044717724341a21be2d5b902e6d1>:0
        ' --- End of inner exception stack trace ---
        ' at Microsoft.VisualBasic.App..cctor () [0x0032f] in <eb97044717724341a21be2d5b902e6d1>:0
        ' --- End of inner exception stack trace ---
        ' at Rserve.Program.Main () [0x00001] in <419e486af7e7476b893119a59f5f71e8>:0
        ' 
        ' Encodings.UTF8WithoutBOM.CodePage
        App.StdOut = Console.OpenStandardOutput.OpenTextWriter(New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))
    End Sub

    ''' <summary>
    ''' set log file that write text to <paramref name="file"/>
    ''' </summary>
    ''' <param name="file"></param>
    ''' <remarks>
    ''' this method is usefull for debug current process run as a pipeline sub-process
    ''' </remarks>
    Public Function RedirectLogging(file As String) As StreamWriter
        Dim buffer = file.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
        Dim writer As New StreamWriter(buffer, Encoding.UTF8) With {
            .AutoFlush = True,
            .NewLine = vbLf
        }

        Call Console.SetOut(writer)

        Return writer
    End Function

    Public Function RedirectErrLogging(file As String) As StreamWriter
        Dim buffer = file.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
        Dim writer As New StreamWriter(buffer, Encoding.UTF8) With {
            .AutoFlush = True,
            .NewLine = vbLf
        }

        Call Console.SetError(writer)

        Return writer
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetAppLocalData(app$, assemblyName$, <CallerMemberName> Optional track$ = Nothing) As String
#If NET48 Then
        ' XDG_DATA_HOME make be empty on unix
        Dim localAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
#Else
        Dim localAppData As String = $"{UserHOME}/.local/share/"
#End If

        Return $"{localAppData}/{app}/{assemblyName}".GetDirectoryFullPath(track)
    End Function

    Public Function GetAppLocalData(exe$) As String
        Dim app As DevAssmInfo = Assembly.LoadFile(path:=Path.GetFullPath(exe)).FromAssembly
        Return GetAppLocalData(app:=app.AssemblyProduct, assemblyName:=exe.BaseName)
    End Function

#Region "这里的环境变量方法主要是操作从命令行之中所传递进来的额外的参数的"

    ReadOnly m_joinedVariables As New Dictionary(Of NamedValue(Of String))

    ''' <summary>
    ''' add/update the environment variable in sciBASIC.NET framework.
    ''' </summary>
    ''' <param name="name">
    ''' if target variable symbol name is exists in the framework, 
    ''' then the config value of the variable will be updated.
    ''' or this function will add a new variable into the 
    ''' environment.
    ''' 
    ''' (如果给定的当前这个参数名称存在于当前框架环境中，则会更新原来的值)</param>
    ''' <param name="value"></param>
    ''' <remarks>
    ''' (添加参数到应用程序的环境变量之中)
    ''' </remarks>
    Public Sub JoinVariable(name$, value$)
        SyncLock m_joinedVariables
            m_joinedVariables(name) = New NamedValue(Of String) With {
                .Name = name,
                .Value = value
            }
        End SyncLock
    End Sub

    ''' <summary>
    ''' 添加参数集合到应用程序的环境变量之中
    ''' </summary>
    ''' <param name="vars"></param>
    Public Sub JoinVariables(ParamArray vars As NamedValue(Of String)())
        For Each v As NamedValue(Of String) In vars
            SyncLock m_joinedVariables
                m_joinedVariables(v.Name) = v
            End SyncLock
        Next
    End Sub

    Public Sub JoinVariables(vars As Dictionary(Of String, String))
        Call vars _
            .Select(Function(x)
                        Return New NamedValue(Of String) With {
                            .Name = x.Key,
                            .Value = x.Value
                        }
                    End Function) _
            .ToArray _
            .DoCall(AddressOf App.JoinVariables)
    End Sub

    ''' <summary>
    ''' If the parameter <paramref name="name"/> is ignored, then the value from <see cref="CallerMemberNameAttribute"/> 
    ''' will be used as variable name.
    ''' (这个函数只是会从设置的变量之中查找，本模块之中的变量请直接从属性进行引用，对于查找失败的变量，这个函数会返回空值
    ''' 假若忽略掉<paramref name="name"/>参数的话，则这个函数会使用<see cref="CallerMemberNameAttribute"/>来获取变量
    ''' 的名称)
    ''' </summary>
    ''' <param name="name$">
    ''' 因为由于是从命令行之中输入进来的，所以可能有些时候大小写会影响直接字典查找，在这里需要用字符串手工查找
    ''' </param>
    ''' <returns>当没有查找到相对应的环境变量的时候会返回空值</returns>
    Public Function GetVariable(<CallerMemberName> Optional name$ = Nothing, Optional defaultValue$ = Nothing) As String
        SyncLock m_joinedVariables
            If m_joinedVariables.ContainsKey(name) Then
                Return m_joinedVariables(name).Value
            Else
                For Each v As NamedValue(Of String) In m_joinedVariables.Values
                    If v.Name.TextEquals(name) Then
                        Return v.Value
                    End If
                Next
            End If
        End SyncLock

        Return defaultValue
    End Function

    ''' <summary>
    ''' 获取<see cref="App"/>的可读属性值来作为环境变量
    ''' </summary>
    ''' <returns></returns>
    Public Function GetAppVariables() As NamedValue(Of String)()
        Dim type As Type = GetType(App)
        Dim pros = type.Schema(PropertyAccess.Readable, BindingFlags.Public Or BindingFlags.Static)
        Dim out As New List(Of NamedValue(Of String))(m_joinedVariables.Values)
        Dim value$
        Dim o

        For Each prop As PropertyInfo
            In pros.Values _
                .Where(Function(p)
                           Return p.PropertyType.Equals(GetType(String)) AndAlso
                                  p.GetIndexParameters _
                                   .IsNullOrEmpty
                       End Function)

            o = prop.GetValue(Nothing, Nothing)
            value = Scripting.ToString(o)
            out += New NamedValue(Of String) With {
                .Name = prop.Name,
                .Value = value
            }
        Next

        Return out.ToArray
    End Function

#End Region

    ''' <summary>
    ''' 其他的模块可能也会依赖于这个初始化参数
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BufferSize As Integer = 4 * 1024 * 1024

    ''' <summary>
    ''' Set value of <see cref="BufferSize"/>
    ''' </summary>
    ''' <param name="size">
    ''' the size string description, could be:
    ''' 
    ''' 1. integer value string for set in bytes
    ''' 2. number value with unit suffix, example as 64MB, 1GB etc
    ''' </param>
    Public Sub SetBufferSize(size As String)
        _BufferSize = Unit.ParseByteSize(size)
    End Sub

    ''' <summary>
    ''' This delegate function do nothing
    ''' </summary>
    Public Sub DoNothing()
        ' do nothing
    End Sub

    ''' <summary>
    ''' This delegate function do nothing
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="arg">any kind of the parameter type</param>
    ''' <remarks></remarks>
    Public Sub DoNothing(Of T)(arg As T)
        ' do nothing
    End Sub

    ''' <summary>
    ''' 假若有些时候函数的参数要求有一个输出流，但是并不想输出任何数据的话，则可以使用这个进行输出
    ''' </summary>
    ''' <returns></returns>
    Public Function NullDevice(Optional encoding As Encodings = Encodings.ASCII) As StreamWriter
        Dim ms As New MemoryStream(capacity:=BufferSize)
        Dim codePage As Encoding = encoding.CodePage
        Return New StreamWriter(ms, encoding:=codePage)
    End Function

    ''' <summary>
    ''' java <see cref="printf"/> + <see cref="Console.WriteLine(String)"/>
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="args"></param>
    Public Sub println(s$, ParamArray args As Object())
        If Not args.IsNullOrEmpty Then
            s = sprintf(s, args)
        Else
            s = CLangStringFormatProvider.ReplaceMetaChars(s)
        End If

        Call VBDebugger.EchoLine(s)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub println()
        Call My.InnerQueue.AddToQueue(AddressOf Console.WriteLine)
    End Sub

    Public Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (process As IntPtr, minimumWorkingSetSize As Integer, maximumWorkingSetSize As Integer) As Integer

    ''' <summary>
    ''' Rabbish collection to free the junk memory.
    ''' </summary>
    ''' <remarks>(垃圾回收)</remarks>
    '''
    <ExportAPI("FlushMemory")>
    Public Sub FlushMemory()
        Call GC.Collect()
        Call GC.WaitForPendingFinalizers()

        If Environment.OSVersion.Platform = PlatformID.Win32NT Then
            Try
                Call SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1)
            Catch ex As Exception
                Call App.LogException(ex)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Free this variable pointer in the memory.
    ''' </summary>
    ''' <typeparam name="T">假若该对象类型实现了<see cref="System.IDisposable"></see>接口，则函数还会在销毁前调用该接口的销毁函数</typeparam>
    ''' <param name="obj"></param>
    ''' <remarks>(销毁本对象类型在内存之中的指针)</remarks>
    <Extension>
    Public Sub Free(Of T As Class)(ByRef obj As T)
        If Not obj Is Nothing Then
            Dim TypeInfo As Type = obj.GetType
            If Array.IndexOf(TypeInfo.GetInterfaces, GetType(IDisposable)) > -1 Then
                Try
                    Call DirectCast(obj, IDisposable).Dispose()
                Catch ex As Exception

                End Try
            End If
        End If

        obj = Nothing

        ' Will not working on Linux platform
        If App.IsMicrosoftPlatform Then
            Call FlushMemory()
        End If
    End Sub

    ''' <summary>
    ''' Pause the console program.
    ''' </summary>
    ''' <param name="Prompted"></param>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Pause")>
    Public Sub Pause(Optional prompted$ = "Press any key to continute...")
        Call My.InnerQueue.WaitQueue()
        Call Console.WriteLine(prompted)

        ' 2018-6-26 如果不是命令行程序的话，可能会因为没有地方进行输入而导致程序在这里停止运行
        ' 所以会需要进行一些判断，只在命令行模式下才会要求输入
        If App.IsConsoleApp Then
            Call Console.Read()
        End If
    End Sub

    ''' <summary>
    ''' Pause and then exit the application.
    ''' </summary>
    Public Sub [Stop]()
        Call App.Pause()
        Call App.Exit()
    End Sub

    ''' <summary>
    ''' 使用<see cref="ProductSharedDIR"/>的位置会变化的，则使用本函数则会使用获取当前的模块的文件夹，即使其不是exe程序而是一个dll文件
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Public Function GetProductSharedDIR(type As Type) As String
        Dim assm As Assembly = type.Assembly
        Dim productName As String = ApplicationInfoUtils.GetProductName(assm)

        If String.IsNullOrEmpty(productName) Then
            productName = BaseName(assm.Location)
        End If

        Return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{productName}"
    End Function

    ''' <summary>
    ''' Get current time <see cref="Date"/> in ``xxxxx.xxxx`` unix time stamp format.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property UnixTimeStamp As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return DateTimeHelper.UnixTimeStamp(Now)
        End Get
    End Property

    Public ReadOnly Property CurrentUnixTimeMillis() As Long
        Get
            Return DateTimeHelper.UnixTimeStampMillis(Now)
        End Get
    End Property

    ''' <summary>
    ''' The time tag of the application started.(应用程序的启动的时间)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StartTime As Double = App.UnixTimeStamp

    ''' <summary>
    ''' The distance of time that this application running from start and to current time.
    ''' (当前距离应用程序启动所逝去的时间)
    ''' </summary>
    ''' <returns>ms timespan</returns>
    ''' <remarks>
    ''' 通过<see cref="App.UnixTimeStamp"/>以及<see cref="StartTime"/>得到的时间都是带小数的秒数
    ''' 所以在这里计算出当前时间点与启动时间点之间的差值之后，还需要乘以1000才可以得到毫秒数
    ''' </remarks>
    <ExportAPI("Elapsed.Milliseconds")>
    Public Function ElapsedMilliseconds() As Long
        Dim nowLng As Double = App.UnixTimeStamp
        Dim d As Long = (nowLng - StartTime) * 1000
        Return d
    End Function

    Public Function ElapsedMilliseconds(startTime As Long) As TimeSpan
        Return TimeSpan.FromMilliseconds(App.ElapsedMilliseconds - startTime)
    End Function

    ''' <summary>
    ''' The local data dir of the application in the %user%/&lt;CurrentUser>/Local/Product/App
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LocalData As String

    ''' <summary>
    ''' The temp directory in the application local data.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LocalDataTemp As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return App.LocalData & "/Temp/"
        End Get
    End Property

    ''' <summary>
    ''' The directory path of the system temp data.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SysTemp As String = TempFileSystem.__sysTEMP

    Public Sub SetSystemTemp(Optional dir As String = Nothing)
        If dir Is Nothing Then
            dir = TempFileSystem.__sysTEMP
        End If

        _SysTemp = dir
    End Sub

    ''' <summary>
    ''' Application temp data directory in the system temp root: %<see cref="App.SysTemp"/>%/<see cref="App.AssemblyName"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property AppSystemTemp As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return SysTemp & "/" & App.AssemblyName
        End Get
    End Property

    ''' <summary>
    ''' Gets the product version associated with this application.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Version As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Trim(Application.ProductVersion)
        End Get
    End Property

    Public Property GetLastError As Exception

    ''' <summary>
    ''' Simply log application exception data into a log file which saves at location: %<see cref="App.LocalData"/>%/.logs/err/.
    ''' (简单日志记录，函数返回空值)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="trace">调用函数的位置，这个参数一般为空，编译器会自动生成Trace位点参数</param>
    ''' <returns>这个函数总是返回空值的</returns>
    ''' <remarks>
    ''' this debug logging function will returns nothing always!
    ''' </remarks>
    <ExportAPI("LogException")>
    Public Function LogException(ex As Exception, <CallerMemberName> Optional ByRef trace$ = "") As Object
        Try
            GetLastError = ex
            trace = App.TraceBugs(ex, trace)
        Catch ex2 As Exception
            ' 错误日志文件的存放位置不可用或者被占用了不可写，则可能会出错，
            ' 在这里将原来的错误打印在终端上面就行了， 扔弃掉这个错误日志
            Call ex.PrintException
        End Try

        Return Nothing
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function TemporaryEnvironment(newLocation As String) As FileIO.TemporaryEnvironment
        Return New FileIO.TemporaryEnvironment(newLocation)
    End Function

    ''' <summary>
    ''' Function returns the file path of the application log file.
    ''' (函数返回的是日志文件的文件路径)
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("TraceBugs")>
    Public Function TraceBugs(ex As Exception, <CallerMemberName> Optional trace$ = Nothing) As String
        If trace.StringEmpty Then
            trace = "trace_bug"
        End If

        SyncLock LogFile
            Call LogFile.LogException(ex, trace)
        End SyncLock

        Return Nothing
    End Function

    ''' <summary>
    ''' MySql时间格式： ``yy-mm-dd, 00:00:00``
    ''' </summary>
    ''' <param name="time"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FormatTime(time As DateTime, Optional sep$ = ":") As String
        Dim yy = Strings.Format(time.Year, "0000")
        Dim mm = Strings.Format(time.Month, "00")
        Dim dd = Strings.Format(time.Day, "00")
        Dim hh = Strings.Format(time.Hour, "00")
        Dim mi = Strings.Format(time.Minute, "00")
        Dim ss = Strings.Format(time.Second, "00")

        Return $"{yy}-{mm}-{dd}, {hh}{sep}{mi}{sep}{ss}"
    End Function

    ''' <summary>
    ''' Is this application running on a Microsoft OS platform.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' (是否是运行于微软的操作系统平台？)
    ''' </remarks>
    Public ReadOnly Property IsMicrosoftPlatform As Boolean = App.CheckIsMicrosoftPlatform

    ''' <summary>
    ''' 这个主要是判断一个和具体的操作系统平台相关的Win32 API是否能够正常的工作？
    ''' </summary>
    ''' <returns></returns>
    Private Function CheckIsMicrosoftPlatform() As Boolean
        Select Case App.Platform
            Case PlatformID.Win32NT,
                 PlatformID.Win32S,
                 PlatformID.Win32Windows,
                 PlatformID.WinCE,
                 PlatformID.Xbox

                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Example: ``tmp2A10.tmp``
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function tempCode() As String
        Static tempHashCode As Uid = Uid.GetRandomId

        SyncLock tempHashCode
            Return FormatZero(++tempHashCode, "000000")
        End SyncLock
    End Function

    ''' <summary>
    ''' 由于可能会运行多个使用本模块的进程，单独考哈希来作为表示会产生冲突，所以这里使用应用程序的启动时间戳以及当前的哈希值来生成唯一标示
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetNextUniqueName(prefix As String) As String
        Static tmp As [Default](Of String) = NameOf(tmp)
        Return $"{prefix Or tmp}{App.tempCode}"
    End Function

    ''' <summary>
    ''' 是名称，不是文件路径
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NextTempName As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return GetNextUniqueName(Nothing)
        End Get
    End Property

    ''' <summary>
    ''' Error default log fie location from function <see cref="App.LogException(Exception, ByRef String)"/>.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' (存放自动存储的错误日志的文件夹)
    ''' </remarks>
    Public ReadOnly Property LogErrDIR As String

    Public ReadOnly Property LogFile As LogFile
        Get
            Static log As LogFile

            If log Is Nothing Then
                log = New LogFile($"{App.LogErrDIR}/error_{LogFile.NowTimeNormalizedString}.log", append:=False)
                log.log(MSG_TYPES.INF, ErrorLog.EnvironmentInfo, "app_debug_info")
            End If

            Return log
        End Get
    End Property

    ''' <summary>
    ''' Simply log application exception data into a log file which saves at a user defined location parameter: <paramref name="FileName"/>.
    ''' (简单日志记录)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="Trace"></param>
    ''' <param name="FileName"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("LogException")>
    Public Function LogException(ex As Exception, fileName$, <CallerMemberName> Optional trace$ = Nothing) As Object
        Call ErrorLog.BugsFormatter(ex, trace).SaveTo(fileName)
        Return Nothing
    End Function

    ''' <summary>
    ''' This is the custom message of the exception, not extract from the function <see cref="Exception.ToString()"/>
    ''' </summary>
    ''' <param name="exMsg">
    ''' This is the custom message of the exception, not extract from the function <see cref="Exception.ToString()"/>
    ''' </param>
    ''' <param name="trace"></param>
    ''' <returns></returns>
    <ExportAPI("Exception.Log")>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function LogException(exMsg$, <CallerMemberName> Optional trace$ = "") As Object
        Return App.LogException(New Exception(exMsg), trace)
    End Function

#Region "CLI interpreter"

    ''' <summary>
    ''' 当前的应用程序是否退出运行了? 当调用<see cref="App.Exit(Integer)"/>方法的时候, 除了会终止程序的运行
    ''' 还会讲这个属性设置为False
    ''' 
    ''' 在应用程序框架中, 有一些组件的线程会需要依赖于这个属性值来自动停止运行
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Running As Boolean = True

    ''' <summary>
    '''  Terminates this <see cref="System.Diagnostics.Process"/> and gives the underlying operating system the specified exit code.
    '''  (这个方法还会终止本应用程序里面的自动GC线程)
    ''' </summary>
    ''' <param name="state">Exit code to be given to the operating system. Use 0 (zero) to indicate that the process completed successfully.</param>
    '''
    <SecuritySafeCritical>
    Public Function Exit%(Optional state% = 0)
        App._Running = False

        Try
            Call My.InnerQueue.WaitQueue()
            Call App.StopGC()
            Call __GCThread.Dispose()
            Call Environment.Exit(state)
        Catch ex As Exception
            Process.GetCurrentProcess.Kill()
        End Try

        Return state
    End Function

    ''' <summary>
    ''' Running the <see cref="String"/> as cli command line and the specific type define as a <see cref="CommandLine.Interpreter"/>.
    ''' (请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension>
    Public Function RunCLI(Interpreter As Type, args$, <CallerMemberName> Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(Parsers.TryParse(args), caller, Nothing, Nothing, Nothing, Nothing)
    End Function

    <Extension>
    Public Function RunCLI(Interpreter As Type, args As String(),
                           Optional executeEmpty As ExecuteEmptyCLI = Nothing,
                           Optional executeNotFound As ExecuteNotFound = Nothing,
                           Optional executeFile As ExecuteFile = Nothing,
                           Optional executeQuery As ExecuteQuery = Nothing) As Integer

        Return Interpreter.RunCLIInternal(CommandLineArgs.BuildFromArguments(args), "Main", executeEmpty, executeNotFound, executeFile, executeQuery)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line, Running the string as cli command line and the specific type define as a interpreter.
    ''' (请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLineArgs, <CallerMemberName> Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(args, caller, Nothing, Nothing, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLineArgs, executeEmpty As ExecuteEmptyCLI,
                                       <CallerMemberName>
                                       Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(args, caller, executeEmpty, Nothing, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args$, executeEmpty As ExecuteEmptyCLI,
                                       <CallerMemberName>
                                       Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(Parsers.TryParse(args), caller, executeEmpty, Nothing, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">
    ''' The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.
    ''' </param>
    ''' <param name="executeNotFound">
    ''' ```vbnet
    ''' Public Delegate Function ExecuteNotFound(args As CommandLine) As Integer
    ''' ```
    ''' </param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args$, executeEmpty As ExecuteEmptyCLI, executeNotFound As ExecuteNotFound,
                                       <CallerMemberName>
                                       Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(Parsers.TryParse(args), caller, executeEmpty, executeNotFound, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">
    ''' The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.
    ''' </param>
    ''' <param name="executeNotFound">
    ''' ```vbnet
    ''' Public Delegate Function ExecuteNotFound(args As <see cref="CommandLineArgs"/>) As <see cref="Integer"/>
    ''' ```
    ''' </param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLineArgs,
                                       executeEmpty As ExecuteEmptyCLI,
                                       executeFile As ExecuteFile,
                                       executeNotFound As ExecuteNotFound,
                                       <CallerMemberName>
                                       Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(args, caller, executeEmpty, executeNotFound, executeFile, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">
    ''' The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.
    ''' </param>
    ''' <param name="executeNotFound">
    ''' ```vbnet
    ''' Public Delegate Function ExecuteNotFound(args As <see cref="CommandLineArgs"/>) As <see cref="Integer"/>
    ''' ```
    ''' </param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("RunCLI")>
    <Extension>
    Public Function RunCLI(Interpreter As Type, args As CommandLineArgs,
                           executeEmpty As ExecuteEmptyCLI,
                           executeFile As ExecuteFile,
                           executeNotFound As ExecuteNotFound,
                           executeQuery As ExecuteQuery,
                           <CallerMemberName>
                           Optional caller$ = Nothing) As Integer

        Return Interpreter.RunCLIInternal(args, caller, executeEmpty, executeNotFound, executeFile, executeQuery)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">
    ''' The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.
    ''' </param>
    ''' <param name="executeNotFound">
    ''' ```vbnet
    ''' Public Delegate Function ExecuteNotFound(args As <see cref="CommandLineArgs"/>) As <see cref="Integer"/>
    ''' ```
    ''' </param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLineArgs, executeEmpty As ExecuteEmptyCLI, executeNotFound As ExecuteNotFound,
                                       <CallerMemberName>
                                       Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(args, caller, executeEmpty, executeNotFound, Nothing, Nothing)
    End Function

    <Extension>
    Private Function RunCLIInternal(App As Type, args As CommandLineArgs, caller$,
                                    executeEmpty As ExecuteEmptyCLI,
                                    executeNotFound As ExecuteNotFound,
                                    executeFile As ExecuteFile,
                                    executeQuery As ExecuteQuery) As Integer
#If DEBUG Then
        Call args.debug
#End If
        Call args.InitDebuggerEnvir(caller)

        If args.Name.TextEquals("/i") Then
            ' 交互式终端模式
            Return finalizeCLI(New InteractiveConsole(App).RunApp)
        Else
            Dim program As New Interpreter(App, caller:=caller) With {
                .ExecuteEmptyCli = executeEmpty,
                .ExecuteNotFound = executeNotFound,
                .ExecuteFile = executeFile,
                .ExecuteQuery = executeQuery
            }

            Return finalizeCLI(program.Execute(args))
        End If
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension>
    Public Function RunCLI(Interpreter As Type, args$, executeFile As ExecuteFile, <CallerMemberName> Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(Parsers.TryParse(args), caller, Nothing, Nothing, executeFile, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    ''' <param name="executeFile">
    ''' 函数指针：
    ''' ```vbnet
    ''' Function ExecuteFile(path As <see cref="String"/>, args As <see cref="CommandLine"/>) As <see cref="Integer"/>
    ''' ```
    ''' </param>
    <ExportAPI("RunCLI")>
    <Extension>
    Public Function RunCLI(Interpreter As Type, args As CommandLineArgs, executeFile As ExecuteFile, <CallerMemberName> Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(args, caller, Nothing, Nothing, executeFile, Nothing)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension>
    Public Function RunCLI(Interpreter As Type, args$, executeFile As ExecuteFile, executeEmpty As ExecuteEmptyCLI,
                           <CallerMemberName>
                           Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLI(Parsers.TryParse(args), executeFile, executeEmpty, caller)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <param name="executeEmpty">
    ''' this function pointer accepts no parameter.
    ''' </param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLineArgs, executeFile As ExecuteFile, executeEmpty As ExecuteEmptyCLI,
                                       <CallerMemberName>
                                       Optional caller$ = Nothing) As Integer
        Return Interpreter.RunCLIInternal(args, caller, executeEmpty, Nothing, executeFile, Nothing)
    End Function

    ''' <summary>
    ''' IF the flag is True, that means cli API execute successfully, function returns ZERO, or a negative integer(Default -100) for failures.
    ''' </summary>
    ''' <param name="b"></param>
    ''' <param name="Failed"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function CLICode(b As Boolean, Optional Failed As Integer = -100) As Integer
        Return If(b, 0, Failed)
    End Function

#End Region

    ''' <summary>
    ''' Creates a uniquely named zero-byte temporary file on disk and returns the full
    ''' path of that file.
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("GetTempFile")>
    Public Function GetTempFile() As String
        Dim temp As String = FS.GetTempFileName
        Return TempFileSystem.GenerateTemp(temp, "")
    End Function

    ''' <summary>
    ''' Get temp data directory path of current app process instance
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CurrentProcessTemp As String

    ''' <summary>
    ''' Gets a temp file name which is located at directory <see cref="App.ProductSharedDIR"/>.
    ''' (获取位于共享文件夹<see cref="App.ProductSharedDIR"/>里面的临时文件)
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("Shared.TempFile")>
    Public Function GetProductSharedTemp() As String
        'Dim Temp As String = FileIO.FileSystem.GetTempFileName
        Dim Name As String = App.tempCode  'FileIO.FileSystem.GetFileInfo(Temp).Name
        'Name = Name.ToUpper.Replace("TMP", "")
        Dim Temp = $"{App.ProductSharedTemp}/{App.AssemblyName}-{Name}.tmp"
        Return Temp
    End Function

    Public ReadOnly Property ProductSharedTemp As String

    ''' <summary>
    ''' Gets a <see cref="System.PlatformID"/> enumeration value that identifies the operating system
    ''' platform.
    ''' </summary>
    ''' <remarks>One of the <see cref="PlatformID"/> values. this property value may affected 
    ''' by the ``--unix`` commandline debug options when do application startup.</remarks>
    Public ReadOnly Property Platform As PlatformID
        Get
            Return My.FrameworkInternal.InternalPlatformID
        End Get
    End Property

    ''' <summary>
    ''' Self call this program itself for batch parallel task calculation.
    ''' (调用自身程序，这个通常是应用于批量的数据的计算任务的实现)
    ''' </summary>
    ''' <param name="CLI"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Folk.Self")>
    Public Function SelfFolk(CLI As String) As IIORedirectAbstract
        Return Shell(App.ExecutablePath, CLI, CLR:=True)
    End Function

    ''' <summary>
    ''' Folk helper for running the other .NET application.
    ''' (请注意，这个函数只能够运行.NET程序, 假若是在Linux系统之上，还需要安装mono运行时环境)
    ''' </summary>
    ''' <param name="app">External application file full path</param>
    ''' <param name="CLI">Commandline string that running the application <paramref name="app$"/></param>
    ''' <param name="CLR">Is the calling external application is a .NET application?
    ''' (是否为.NET程序?)
    ''' </param>
    ''' <returns></returns>
    ''' <remarks><see cref="IORedirectFile"/>这个建议在进行外部调用的时候才使用</remarks>
    Public Function Shell(app$, CLI$,
                          Optional CLR As Boolean = False,
                          Optional stdin$ = Nothing,
                          Optional ioRedirect As Boolean = False,
                          Optional debug As Boolean = False) As IIORedirectAbstract

#If NETCOREAPP Then
        Const PLATFORM As String = "dotnet"
#Else
        Const PLATFORM As String = "mono"
#End If
        If Not IsMicrosoftPlatform Then
            If CLR Then
                Dim process As New ProcessEx With {
                    .Bin = PLATFORM,
                    .CLIArguments = app.CLIPath & " " & CLI
                }
                Return process
            Else
                Dim process As New IORedirectFile(app, CLI, stdin:=stdin)
                Return process
            End If
        Else
            If CLR Then
                ' 由于是重新调用自己，所以这个重定向是没有多大问题的
                Return New IORedirect(app, CLI, IOredirect:=ioRedirect, hide:=Not debug)
            Else
                Dim process As New IORedirectFile(app, CLI, stdin:=stdin)
                Return process
            End If
        End If
    End Function

#Region "Auto Garbage Cleaner"

    ''' <summary>
    ''' 自动垃圾回收线程
    ''' </summary>
    ReadOnly __GCThread As New UpdateThread(10 * 60 * 1000, AddressOf App.__GCThreadInvoke)

    Dim _CLIAutoClean As Boolean = False
    Dim appExitHooks As New List(Of Action)

    ''' <summary>
    ''' 这里添加在应用程序退出执行的时候所需要完成的任务
    ''' </summary>
    ''' <param name="hook"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddExitCleanHook(hook As Action)
        SyncLock appExitHooks
            With appExitHooks
                Call .Add(hook)
            End With
        End SyncLock
    End Sub

    Public Const FlagInternalPipeline As String = "internal_pipeline"

    ''' <summary>
    ''' 自动停止GC当前程序的线程
    ''' </summary>
    ''' <param name="state"></param>
    ''' <returns></returns>
    Private Function finalizeCLI(state As Integer) As Integer
        Dim internalPipelineMode As String = App.CommandLine.Tokens _
            .DoCall(Function(t) JoinElement(Of String).FindElement(t, Function(s) s = FlagInternalPipeline)) _
            .FirstOrDefault _
           ?.next

        App._Running = False

        If _CLIAutoClean Then
            Call StopGC()
        End If

        ' 在这里等待终端的内部线程输出工作完毕，防止信息的输出错位

        Call My.InnerQueue.WaitQueue()

        If Not internalPipelineMode.TextEquals("TRUE") Then
            Call Console.WriteLine()
        End If

        For Each hook As Action In appExitHooks
            Try
                Call hook()
            Catch ex As Exception
                ' just ignores of the cleanup events
            End Try
        Next

        Call My.InnerQueue.WaitQueue()

        If Not LogFile Is Nothing Then
            Call LogFile.Save()
            Call LogFile.Dispose()
        End If

        If Not internalPipelineMode.TextEquals("TRUE") Then
            Try
                Call Console.WriteLine()
            Catch ex As Exception
                ' `appExitHooks` may contains some code to
                ' redirect the stdout and close the stdout
                ' this will cause the I/O exception at here
                ' no needs to handling this exception any
                ' more just eat it and keeps silent at here
            End Try
        End If

#If DEBUG Then
        ' 20190623 在这里禁止程序在调试模式下，如果处于内部流程的调用
        ' 可能会导致调试不方便，在这里通过设置这个流程标记来禁用掉
        ' 被调用的内部流程子进程的调试暂停功能

        ' app /command /opts /@set --internal_pipeline=TRUE

        ' this option enable you disable the pause in debug mode 
        ' when the program is going to end.
        If Not App.GetVariable(name:=FlagInternalPipeline).ParseBoolean = True Then
            ' 应用程序在 debug 模式下会自动停止在这里
            ' 在这里调试模式下结束之前自动暂停是为了
            ' 方便查看程序的命令行终端上面的输出信息
            '
#If NET48 Then
            Call Pause()
#End If
        End If
#End If

        Return state
    End Function

    ''' <summary>
    ''' Start the automatic garbage collection threads.
    ''' (这条线程只会自动清理*.tmp临时文件，因为假若不清理临时文件的话，有时候临时文件比较多的时候，会严重影响性能，甚至无法运行应用程序框架里面的IO重定向操作)
    ''' </summary>
    Public Sub StartGC(autoClose As Boolean)
        ' 因为有一部分程序假若在执行一个很长的任务的话，是会将一些中间文件存放在临时文件夹的
        ' 使用这个自动清理功能的函数，可能会将这些有用的中间文件给删除掉
        ' 所以在这里给出一条警告信息，方便在调试的时候了解这个自动垃圾回收线程是否被启动了
        Call App.__GCThread.Start()
        Call "Garbage auto collection thread started!".Warning

        App._CLIAutoClean = autoClose
    End Sub

    ''' <summary>
    ''' 自动垃圾回收线程
    ''' </summary>
    Private Sub __GCThreadInvoke()
        On Error Resume Next

        Call App.__removesTEMP(App.SysTemp)
        Call App.__removesTEMP(App.AppSystemTemp)
        Call App.__removesTEMP(App.ProductSharedTemp)
        Call App.__removesTEMP(App.LocalDataTemp)

        Call FlushMemory()
    End Sub

    <Extension>
    Private Function __listFiles(DIR As String) As IEnumerable(Of String)
        Try
            Return ls - l - r - {"*.tmp", "*.temp"} <= DIR
        Catch ex As Exception
            Call App.LogException(ex)
            Return {}
        End Try
    End Function

    ''' <summary>
    ''' The Windows file system have a limit of the numbers in a folder, so the long time running application 
    ''' required a thread to make the temp directory cleanup, or the application will no able to create temp 
    ''' file when the temp folder reach its file number upbound(This may caused the application crashed).
    ''' </summary>
    ''' <param name="TEMP"></param>
    Private Sub __removesTEMP(TEMP As String)
        For Each file As String In TEMP.__listFiles
            Try
                Call FS.DeleteFile(file)
            Finally
                ' DO Nothing
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
