#Region "Microsoft.VisualBasic::f6f6385ee5ba7943985a3d6e0dd5dd97, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\App.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Debugging
Imports Microsoft.VisualBasic.Emit.CodeDOM_VBC
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Parallel.Threads
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.SoftwareToolkits
Imports Microsoft.VisualBasic.Terminal
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Windows.Forms.VistaSecurity

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
''' (More easily runtime environment information provider on <see cref="PlatformID.Unix"/>/LINUX platform for visualbasic program.)
''' (从命令行之中使用``/@set``参数赋值环境变量的时候，每一个变量之间使用分号进行分隔)
''' </summary>
'''
<PackageNamespace("App", Description:="More easily runtime environment information provider on LINUX platform for visualbasic program.",
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
    ''' is between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NanoTime As Long
        Get
            Return Now.Ticks
        End Get
    End Property

    ''' <summary>
    ''' Numbers of the CPU kernels on the current machine.(获取当前的系统主机的CPU核心数)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CPUCoreNumbers As Integer = LQuerySchedule.CPU_NUMBER
    ''' <summary>
    ''' 判断当前运行的程序是否为Console类型的应用和程序，由于在执行初始化的时候，
    ''' 最先被初始化的是这个模块，所以没有任何代码能够先执行<see cref="Console.IsErrorRedirected"/>了，
    ''' 在这里使用<see cref="Console.IsErrorRedirected"/>这个来进行判断是可靠的
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsConsoleApp As Boolean = Not Console.IsErrorRedirected
    ''' <summary>
    ''' 获取得到当前的这个所运行的应用程序所引用的dll文件列表
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property References As New Lazy(Of String())(Function() ReferenceSolver.ExecutingReferences)
    ''' <summary>
    ''' Gets a path name pointing to the Desktop directory.
    ''' </summary>
    ''' <returns>The path to the Desktop directory.</returns>
    Public ReadOnly Property Desktop As String
    Public ReadOnly Property StdErr As New StreamWriter(Console.OpenStandardError)

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
    Public ReadOnly Property CommandLine As CommandLine.CommandLine = __CLI()

    Const gitBash As String = "C:/Program Files/Git"

    ''' <summary>
    ''' Makes compatibility with git bash: <see cref="gitBash"/> = ``C:/Program Files/Git``
    ''' </summary>
    ''' <returns></returns>
    Private Function __CLI() As CommandLine.CommandLine
        Dim tokens As String() = ' 第一个参数为应用程序的文件路径，不需要
            Environment.GetCommandLineArgs.Skip(1).ToArray
        Dim CLI As String = String _
            .Join(" ", tokens.ToArray(Function(s) s.CLIToken)) _
            .Replace(gitBash, "")
        Return Microsoft.VisualBasic.CommandLine.TryParse(CLI)
    End Function

    Public ReadOnly Property Github As String = "https://github.com/xieguigang/sciBASIC"

    ''' <summary>
    ''' Returns the argument portion of the <see cref="Microsoft.VisualBasic.CommandLine.CommandLine"/> used to start Visual Basic or
    ''' an executable program developed with Visual Basic. The My feature provides greater
    ''' productivity and performance than the <see cref="microsoft.VisualBasic.Interaction.Command"/> function. For more information,
    ''' see <see cref="ConsoleApplicationBase.CommandLineArgs"/>.
    ''' </summary>
    ''' <returns>Gets the command-line arguments for this process.</returns>
    Public ReadOnly Property Command As String =
        Microsoft.VisualBasic.CommandLine.Join(App.CommandLine.Tokens)

    ''' <summary>
    ''' The file path of the current running program executable file.(本应用程序的可执行文件的文件路径)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ExecutablePath As String
    Public ReadOnly Property Info As ApplicationDetails

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
    Public ReadOnly Property InputFile As String
        Get
            Return App.CommandLine("/in")
        End Get
    End Property

    Dim _out$

    ''' <summary>
    ''' Gets the ``/out`` commandline value as the output file path.
    ''' </summary>
    ''' <returns></returns>
    Public Property OutFile As String
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
        Get  ' 由于会因为切换目录而发生变化，所以这里不适用简写形式了
            Return FileIO.FileSystem.CurrentDirectory
        End Get
        Set(value As String)
            If String.Equals(value, "-") Then  ' 切换到前一个工作目录
                value = _preDIR
            Else
                _preDIR = FileIO.FileSystem.CurrentDirectory
            End If

            FileIO.FileSystem.CreateDirectory(value)
            FileIO.FileSystem.CurrentDirectory = value
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
    Dim _preDIR As String

    Public ReadOnly Property PreviousDirectory As String
        Get
            Return _preDIR
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
    Public ReadOnly Property ProductProgramData As String

    ''' <summary>
    ''' The shared program data directory for a group of app which have the same product series name.
    ''' (同一產品程序集所共享的數據文件夾)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ProductSharedDIR As String

    Sub New()
        On Error Resume Next

        Call FileIO.FileSystem.CreateDirectory(AppSystemTemp)
        Call FileIO.FileSystem.CreateDirectory(App.HOME & "/Resources/")

        _preDIR = App.StartupDirectory

#Region "公共模块内的所有的文件路径初始化"
        ' 因为vb的基础运行时环境在Linux平台上面对文件系统的支持还不是太完善，所以不能够放在属性的位置直接赋值，否则比较难处理异常
        ' 现在放在这个构造函数之中，强制忽略掉错误继续执行，提升一些稳定性，防止出现程序无法启动的情况出现。

        ' 请注意，这里的变量都是有先后的初始化顺序的

        App.RunTimeDirectory = FileIO.FileSystem _
            .GetDirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory) _
            .FullName _
            .Replace("/", "\")
        App.Desktop = My.Computer.FileSystem.SpecialDirectories.Desktop
        App.ExecutablePath = FileIO.FileSystem.GetFileInfo(Application.ExecutablePath).FullName    ' (Process.GetCurrentProcess.StartInfo.FileName).FullName
        App.Info = ApplicationDetails.CurrentExe()
        App.AssemblyName = BaseName(App.ExecutablePath)
        App.ProductName = If(
            String.IsNullOrEmpty(Application.ProductName.Trim),
            AssemblyName,
            Application.ProductName.Trim)
        App.HOME = FileIO.FileSystem.GetParentPath(App.ExecutablePath)
        App.UserHOME = PathMapper.HOME.GetDirectoryFullPath
        App.ProductProgramData = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{ProductName}".GetDirectoryFullPath
        App.ProductSharedDIR = $"{ProductProgramData}/.shared".GetDirectoryFullPath
        App.LocalData = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{ProductName}/{AssemblyName}".GetDirectoryFullPath
        App.CurrentProcessTemp = GenerateTemp(App.SysTemp & "/tmp.io", App.PID).GetDirectoryFullPath
        App.ProductSharedTemp = App.ProductSharedDIR & "/tmp/"
        App.LogErrDIR = App.LocalData & $"/.logs/err/"
#End Region
    End Sub

#Region "这里的环境变量方法主要是操作从命令行之中所传递进来的额外的参数的"

    Dim __joinedVariables As New Dictionary(Of NamedValue(Of String))

    ''' <summary>
    ''' 添加参数到应用程序的环境变量之中
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="value$"></param>
    Public Sub JoinVariable(name$, value$)
        __joinedVariables(name) =
            New NamedValue(Of String) With {
                .Name = name,
                .Value = value
        }
    End Sub

    ''' <summary>
    ''' 添加参数集合到应用程序的环境变量之中
    ''' </summary>
    ''' <param name="vars"></param>
    Public Sub JoinVariables(ParamArray vars As NamedValue(Of String)())
        For Each v As NamedValue(Of String) In vars
            __joinedVariables(v.Name) = v
        Next
    End Sub

    Public Sub JoinVariables(vars As Dictionary(Of String, String))
        Call App.JoinVariables(vars.Select(
            Function(x)
                Return New NamedValue(Of String) With {
                    .Name = x.Key,
                    .Value = x.Value
                }
            End Function).ToArray)
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
    Public Function GetVariable(<CallerMemberName> Optional name$ = Nothing) As String
        If __joinedVariables.ContainsKey(name) Then
            Return __joinedVariables(name).Value
        Else
            For Each v As NamedValue(Of String) In __joinedVariables.Values
                If v.Name.TextEquals(name) Then
                    Return v.Value
                End If
            Next
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' 获取<see cref="App"/>的可读属性值来作为环境变量
    ''' </summary>
    ''' <returns></returns>
    Public Function GetAppVariables() As NamedValue(Of String)()
        Dim type As Type = GetType(App)
        Dim pros = type.Schema(PropertyAccess.Readable, BindingFlags.Public Or BindingFlags.Static)
        Dim out As New List(Of NamedValue(Of String))(__joinedVariables.Values)

        For Each prop As PropertyInfo
            In pros.Values.Where(
                Function(p) _
                    p.PropertyType.Equals(GetType(String)) AndAlso
                    p.GetIndexParameters.IsNullOrEmpty)

            out += New NamedValue(Of String) With {
                .Name = prop.Name,
                .Value = prop.GetValue(Nothing, Nothing)
            }
        Next

        Return out.ToArray
    End Function

#End Region

    ''' <summary>
    ''' 其他的模块可能也会依赖于这个初始化参数
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BufferSize As Integer = 4 * 1024

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
    ''' <see cref="printf"/> + <see cref="Console.WriteLine(String)"/>
    ''' </summary>
    ''' <param name="s$"></param>
    ''' <param name="args"></param>
    Public Sub println(s$, ParamArray args As Object())
        If args.IsNullOrEmpty Then
            Call Console.WriteLine(s)
        Else
            Dim out As String = sprintf(s, args)
            Call Console.WriteLine(out)
        End If
    End Sub

    Public Sub println()
        Call Console.WriteLine()
    End Sub

    Public Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (process As IntPtr, minimumWorkingSetSize As Integer, maximumWorkingSetSize As Integer) As Integer

    ''' <summary>
    ''' Rabbish collection to free the junk memory.(垃圾回收)
    ''' </summary>
    ''' <remarks></remarks>
    '''
    <ExportAPI("FlushMemory", Info:="Rabbish collection To free the junk memory.")>
    Public Sub FlushMemory()
        Call GC.Collect()
        Call GC.WaitForPendingFinalizers()

        If (Environment.OSVersion.Platform = PlatformID.Win32NT) Then
            Call SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1)
        End If
    End Sub

    ''' <summary>
    ''' Free this variable pointer in the memory.(销毁本对象类型在内存之中的指针)
    ''' </summary>
    ''' <typeparam name="T">假若该对象类型实现了<see cref="System.IDisposable"></see>接口，则函数还会在销毁前调用该接口的销毁函数</typeparam>
    ''' <param name="obj"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub Free(Of T As Class)(ByRef obj As T)
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
    <ExportAPI("Pause", Info:="Pause the console program.")>
    Public Sub Pause(Optional Prompted As String = "Press any key to continute...")
        Call InnerQueue.WaitQueue()
        Call Console.WriteLine(Prompted)
        Call Console.Read()
    End Sub

    ''' <summary>
    ''' 使用<see cref="ProductSharedDIR"/>的位置会变化的，则使用本函数则会使用获取当前的模块的文件夹，即使其不是exe程序而是一个dll文件
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Public Function GetProductSharedDIR(type As Type) As String
        Dim assm As Assembly = type.Assembly
        Dim productName As String = ApplicationDetails.GetProductName(assm)
        If String.IsNullOrEmpty(productName) Then
            productName = BaseName(assm.Location)
        End If
        Return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{productName}"
    End Function

    ''' <summary>
    ''' The time tag of the application started.(应用程序的启动的时间)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property StartTime As Long = Now.ToBinary

    ''' <summary>
    ''' The distance of time that this application running from start and to current time.
    ''' (当前距离应用程序启动所逝去的时间)
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("Elapsed.Milliseconds")>
    Public Function ElapsedMilliseconds() As Long
        Dim nowLng As Long = Now.ToBinary
        Dim d As Long = nowLng - StartTime
        Return d
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
        Dim DIR As String = Environment.GetEnvironmentVariable("TMP") ' Linux系统可能没有这个东西

        If String.IsNullOrEmpty(DIR) Then
            DIR = IO.Path.GetTempPath
        End If

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
            Return Trim(Application.ProductVersion)
        End Get
    End Property

    ''' <summary>
    ''' Simply log application exception data into a log file which saves at location: %<see cref="App.LocalData"/>%/.logs/err/.
    ''' (简单日志记录，函数返回空值)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="Trace">调用函数的位置，这个参数一般为空，编译器会自动生成Trace位点参数</param>
    ''' <returns></returns>
    '''
    <ExportAPI("LogException")>
    Public Function LogException(ex As Exception, <CallerMemberName> Optional Trace$ = "") As Object
        Try
            Call App.TraceBugs(ex, Trace)
        Catch ex2 As Exception
            ' 错误日志文件的存放位置不可用或者被占用了不可写，则可能会出错，
            ' 在这里将原来的错误打印在终端上面就行了， 扔弃掉这个错误日志
            Call ex.PrintException
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Function returns the file path of the application log file.
    ''' (函数返回的是日志文件的文件路径)
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("TraceBugs")>
    Public Function TraceBugs(ex As Exception, <CallerMemberName> Optional Trace$ = "") As String
        Dim Entry As String = App.__getTEMPhash
        Entry = $"{Now.Year}-{Now.Month}-{Now.Day}, {Format(Now.Hour, "00")}-{Format(Now.Minute, "00")}-{Format(Now.Second, "00")}_{Entry}"
        Dim log As String = $"{App.LogErrDIR}/{Entry}.log"
        Call App.LogException(ex, Trace, log)
        Return log
    End Function

    ''' <summary>
    ''' Is this application running on a Microsoft OS platform.(是否是运行于微软的操作系统平台？)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsMicrosoftPlatform As Boolean = App.__isMicrosoftPlatform

    ''' <summary>
    ''' 这个主要是判断一个和具体的操作系统平台相关的Win32 API是否能够正常的工作？
    ''' </summary>
    ''' <returns></returns>
    Private Function __isMicrosoftPlatform() As Boolean
        Dim pt As PlatformID = Platform

        Return pt = PlatformID.Win32NT OrElse
            pt = PlatformID.Win32S OrElse
            pt = PlatformID.Win32Windows OrElse
            pt = PlatformID.WinCE OrElse
            pt = PlatformID.Xbox
    End Function

    ''' <summary>
    ''' Example: ``tmp2A10.tmp``
    ''' </summary>
    Dim _tmpHash As New Uid(Not IsMicrosoftPlatform)

    Private Function __getTEMPhash() As String
        SyncLock _tmpHash
            Return FormatZero(+_tmpHash, "00000")
        End SyncLock
    End Function

    ''' <summary>
    ''' 由于可能会运行多个使用本模块的进程，单独考哈希来作为表示会产生冲突，所以这里使用应用程序的启动时间戳以及当前的哈希值来生成唯一标示
    ''' </summary>
    ''' <returns></returns>
    Private Function __getTEMP() As String
        Return $"tmp{App.__getTEMPhash}"
    End Function

    ''' <summary>
    ''' 是名称，不是文件路径
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NextTempName As String
        Get
            Return __getTEMP()
        End Get
    End Property

    ''' <summary>
    ''' Error default log fie location from function <see cref="App.LogException(Exception, String)"/>.(存放自动存储的错误日志的文件夹)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LogErrDIR As String

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
    Public Function LogException(ex As Exception, Trace$, FileName$) As Object
        Call BugsFormatter(ex, Trace).SaveTo(FileName)
        Return Nothing
    End Function

    ''' <summary>
    ''' Generates the formatted error log file content.(生成简单的日志板块的内容)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="Trace"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Bugs.Formatter")>
    Public Function BugsFormatter(ex As Exception, <CallerMemberName> Optional Trace$ = "") As String
        Dim exMsg As StringBuilder = New StringBuilder()
        Call exMsg.AppendLine("TIME:  " & Now.ToString)
        Call exMsg.AppendLine("TRACE: " & Trace)
        Call exMsg.AppendLine(New String("=", 120))
        Call exMsg.Append(Logging.LogFile.SystemInfo)
        Call exMsg.AppendLine(New String("=", 120))
        Call exMsg.AppendLine($"Environment Variables from {GetType(App).FullName}:")
        Call exMsg.AppendLine()
        Call exMsg.AppendLine(ConfigEngine.Prints(App.GetAppVariables))
        Call exMsg.AppendLine()
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
    Public Function LogException(exMsg$, <CallerMemberName> Optional Trace$ = "") As Object
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

    Public ReadOnly Property Running As Boolean = True

    ''' <summary>
    '''  Terminates this <see cref="System.Diagnostics.Process"/> and gives the underlying operating system the specified exit code.
    '''  (这个方法还会终止本应用程序里面的自动GC线程)
    ''' </summary>
    ''' <param name="state">Exit code to be given to the operating system. Use 0 (zero) to indicate that the process completed successfully.</param>
    '''
    <SecuritySafeCritical> Public Function Exit%(Optional state% = 0)
        App._Running = False

        Call Terminal.InnerQueue.WaitQueue()
        Call App.StopGC()
        Call __GCThread.Dispose()
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
    Public Function RunCLI(Interpreter As Type, args As String, <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Dim CLI As CommandLine.CommandLine = TryParse(args)
        Call CLI.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter).Execute(args))
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
    <Extension> Public Function RunCLI(Interpreter As Type, args As CommandLine.CommandLine, <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.CLICommandArgvs.__DEBUG_ECHO
#End If
        Call args.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter).Execute(args))
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
    <Extension> Public Function RunCLI(Interpreter As Type,
                                       args As CommandLine.CommandLine,
                                       executeEmpty As __ExecuteEmptyCLI,
                                       <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.CLICommandArgvs.__DEBUG_ECHO
#End If
        Call args.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
            .ExecuteEmptyCli = executeEmpty
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String, executeEmpty As __ExecuteEmptyCLI, <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Dim CLI As CommandLine.CommandLine = TryParse(args)
        Call CLI.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
            .ExecuteEmptyCli = executeEmpty
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String,
                                       executeEmpty As __ExecuteEmptyCLI,
                                       executeNotFound As __ExecuteNotFound,
                                       <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Dim CLI As CommandLine.CommandLine = TryParse(args)
        Call CLI.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
            .ExecuteEmptyCli = executeEmpty,
            .ExecuteNotFound = executeNotFound
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type,
                                       args As CommandLine.CommandLine,
                                       executeEmpty As __ExecuteEmptyCLI,
                                       executeNotFound As __ExecuteNotFound, <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Call args.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
            .ExecuteEmptyCli = executeEmpty,
            .ExecuteNotFound = executeNotFound
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String, executeFile As __ExecuteFile, <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Dim CLI As CommandLine.CommandLine = TryParse(args)
        Call CLI.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
            .ExecuteFile = executeFile
        }.Execute(args))
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
    <Extension> Public Function RunCLI(Interpreter As Type,
                                       args As CommandLine.CommandLine,
                                       executeFile As __ExecuteFile,
                                       <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Call args.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
            .ExecuteFile = executeFile
        }.Execute(args))
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type, args As String,
                                       executeFile As __ExecuteFile,
                                       executeEmpty As __ExecuteEmptyCLI, <CallerMemberName> Optional caller As String = Nothing) As Integer
        Return Interpreter.RunCLI(TryParse(args), executeFile, executeEmpty, caller)
    End Function

    ''' <summary>
    ''' Running the string as a cli command line.(请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待回车退出)
    ''' </summary>
    ''' <param name="args">The command line arguments value, which its value can be gets from the <see cref="Command()"/> function.</param>
    ''' <returns>Returns the function execute result to the operating system.</returns>
    '''
    <ExportAPI("RunCLI")>
    <Extension> Public Function RunCLI(Interpreter As Type,
                                       args As CommandLine.CommandLine,
                                       executeFile As __ExecuteFile,
                                       executeEmpty As __ExecuteEmptyCLI, <CallerMemberName> Optional caller As String = Nothing) As Integer
#If DEBUG Then
        Call args.__DEBUG_ECHO
#End If
        Call args.InitDebuggerEnvir(caller)

        Return App.__completeCLI(New Interpreter(Interpreter) With {
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
    '''
    <ExportAPI("GetTempFile")>
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
    '''
    <ExportAPI("GetTempFile.AppSys")>
    Public Function GetAppSysTempFile(Optional ext$ = ".tmp", Optional sessionID$ = "") As String
        Dim tmp As String = App.SysTemp & "/" & __getTEMP() & ext  '  FileIO.FileSystem.GetTempFileName.Replace(".tmp", ext)
        tmp = GenerateTemp(tmp, sessionID)
        Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(tmp))
        tmp = FileIO.FileSystem.GetFileInfo(tmp).FullName.Replace("\", "/")
        Return tmp
    End Function

    Public ReadOnly Property CurrentProcessTemp As String

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="sysTemp">临时文件路径</param>
    ''' <returns></returns>
    '''
    <ExportAPI("CreateTempFile")>
    Public Function GenerateTemp(sysTemp$, SessionID$) As String
        Dim Dir As String = FileIO.FileSystem.GetParentPath(sysTemp)
        Dim Name As String = FileIO.FileSystem.GetFileInfo(sysTemp).Name
        sysTemp = $"{Dir}/{App.AssemblyName}/{SessionID}/{Name}"
        Return sysTemp
    End Function

    ''' <summary>
    ''' Gets a temp file name which is located at directory <see cref="App.ProductSharedDIR"/>.
    ''' (获取位于共享文件夹<see cref="App.ProductSharedDIR"/>里面的临时文件)
    ''' </summary>
    ''' <returns></returns>
    '''
    <ExportAPI("Shared.TempFile")>
    Public Function GetProductSharedTemp() As String
        'Dim Temp As String = FileIO.FileSystem.GetTempFileName
        Dim Name As String = App.__getTEMPhash  'FileIO.FileSystem.GetFileInfo(Temp).Name
        'Name = Name.ToUpper.Replace("TMP", "")
        Dim Temp = $"{App.ProductSharedTemp}/{App.AssemblyName}-{Name}.tmp"
        Return Temp
    End Function

    Public ReadOnly Property ProductSharedTemp As String

    ''' <summary>
    ''' Gets a <see cref="System.PlatformID"/> enumeration value that identifies the operating system
    ''' platform.
    ''' </summary>
    ''' <remarks>One of the System.PlatformID values.</remarks>
    Public ReadOnly Property Platform As PlatformID = Environment.OSVersion.Platform

    ''' <summary>
    ''' Self call this program itself for batch parallel task calculation.
    ''' (调用自身程序，这个通常是应用于批量的数据的计算任务的实现)
    ''' </summary>
    ''' <param name="CLI"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Folk.Self")>
    Public Function SelfFolk(CLI As String) As IIORedirectAbstract
        Return Shell(App.ExecutablePath, CLI)
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
    Public Function Shell(app$, CLI$, Optional CLR As Boolean = False) As IIORedirectAbstract
        If Platform = PlatformID.MacOSX OrElse
            Platform = PlatformID.Unix Then

            Dim process As New ProcessEx With {
                .Bin = "mono",
                .CLIArguments = app.CLIPath & " " & CLI
            }
            Return process
        Else
            If CLR Then
                Return New IORedirect(app, CLI) ' 由于是重新调用自己，所以这个重定向是没有多大问题的
            Else
                Dim process As New IORedirectFile(app, CLI)
                Return process
            End If
        End If
    End Function

    ''' <summary>
    ''' Folk this program itself for the large amount data batch processing.
    ''' </summary>
    ''' <param name="CLI">Self folk processing commandline collection.</param>
    ''' <param name="parallel">If this parameter value less than 1, then will be a single 
    ''' thread task. Any positive value that greater than 1 will be parallel task.
    ''' (小于等于零表示非并行化，单线程任务)
    ''' </param>
    ''' <param name="smart">Smart mode CPU load threshold, if the <paramref name="parallel"/> 
    ''' parameter value is less than or equals to 1, then this parameter will be disabled.
    ''' </param>
    ''' <returns>
    ''' Returns the total executation time for running this task collection.
    ''' (返回任务的执行的总时长)
    ''' </returns>
    <ExportAPI("Folk.Self")>
    Public Function SelfFolks&(CLI As IEnumerable(Of String),
                               Optional parallel% = 0,
                               Optional smart# = 0)

        Dim sw As Stopwatch = Stopwatch.StartNew

        If parallel <= 0 Then
            For Each args As String In CLI
                Call App.SelfFolk(args).Run()
            Next
        Else
            Dim Tasks As Func(Of Integer)() = LinqAPI.Exec(Of Func(Of Integer)) <=
 _
                From args As String
                In CLI
                Let io As IIORedirectAbstract = App.SelfFolk(args)
                Let task As Func(Of Integer) = AddressOf io.Run
                Select task

            Call BatchTask(Of Integer)(Tasks, parallel, TimeInterval:=200)
        End If

        Return sw.ElapsedMilliseconds
    End Function

#Region "Auto Garbage Cleaner"

    ''' <summary>
    ''' 自动垃圾回收线程
    ''' </summary>
    ReadOnly __GCThread As UpdateThread =
        New UpdateThread(10 * 60 * 1000, AddressOf App.__GCThreadInvoke)

    Dim _CLIAutoClean As Boolean = False
    Dim __exitHooks As New List(Of Action)

    ''' <summary>
    ''' 这里添加在应用程序退出执行的时候所需要完成的任务
    ''' </summary>
    ''' <param name="hook"></param>
    Public Sub AddExitCleanHook(hook As Action)
        Call __exitHooks.Add(hook)
    End Sub

    ''' <summary>
    ''' 自动停止GC当前程序的线程
    ''' </summary>
    ''' <param name="state"></param>
    ''' <returns></returns>
    Private Function __completeCLI(state As Integer) As Integer
        App._Running = False

        If _CLIAutoClean Then
            Call StopGC()
        End If

        ' 在这里等待终端的内部线程输出工作完毕，防止信息的输出错位

        Call Terminal.WaitQueue()
        Call Console.WriteLine()

        For Each hook As Action In __exitHooks
            Call hook()
        Next

        Call Terminal.WaitQueue()
        Call Console.WriteLine()

#If DEBUG Then
        ' 应用程序在 debug 模式下会自动停止在这里
        Call Pause()
#End If
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
            Finally
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

    ''' <summary>
    ''' Restart the current process with administrator credentials.(以管理员的身份重启本应用程序)
    ''' </summary>
    Public Sub RunAsAdmin(Optional args$ = "")
        Call RestartElevated(args)
    End Sub
End Module
