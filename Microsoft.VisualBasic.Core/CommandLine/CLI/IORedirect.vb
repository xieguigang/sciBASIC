#Region "Microsoft.VisualBasic::d082aedd7c045ae5a26d30b664903b85, Microsoft.VisualBasic.Core\CommandLine\CLI\IORedirect.vb"

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

    '     Class IORedirect
    ' 
    '         Properties: Bin, CLIArguments, ProcessInfo, StandardOutput
    '         Delegate Function
    ' 
    ' 
    '         Delegate Sub
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: __tryGetSTDOUT, GetError, Read, ReadLine, Run
    '                       Shell, (+3 Overloads) Start, ToString
    ' 
    '             Sub: __detectProcessExit, __listenSTDOUT, (+2 Overloads) Dispose, (+2 Overloads) WriteLine
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Runtime.CompilerServices
Imports System.Reflection
Imports System.Threading
Imports Microsoft.VisualBasic.Terminal.STDIO
Imports Microsoft.VisualBasic.Terminal.STDIO__
Imports Microsoft.VisualBasic.Parallel

Namespace CommandLine

    ''' <summary>
    ''' A communication fundation class type for the commandline program interop.
    ''' (一个简单的用于从当前进程派生子进程的Wrapper对象，假若需要folk出来的子进程对象
    ''' 不需要终端交互功能，则更加推荐使用<see cref="IORedirectFile"/>对象来进行调用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IORedirect : Implements I_ConsoleDeviceHandle
        Implements IDisposable, IIORedirectAbstract

        ''' <summary>
        ''' 当前的这个进程实例是否处于运行的状态
        ''' </summary>
        ''' <remarks></remarks>
        Dim _processStateRunning As Boolean
        Dim _consoleDevice As IO.StreamReader
        Dim _inputDevice As IO.StreamWriter
        Dim _errorLogsDevice As IO.StreamReader
        Dim _DispInvokedSTDOUT As Boolean

        ''' <summary>
        ''' The target invoked process event has been exit with a specific return code.
        ''' (目标派生子进程已经结束了运行并且返回了一个错误值)
        ''' </summary>
        ''' <param name="exitCode"></param>
        ''' <param name="exitTime"></param>
        ''' <remarks></remarks>
        Public Event ProcessExit(exitCode As Integer, exitTime As String) Implements IIORedirectAbstract.ProcessExit
        Public Event DataArrival(s As String)

        ''' <summary>
        ''' The process invoke interface of current IO redirect operation.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ProcessInfo As Process

        ''' <summary>
        ''' Gets the standard output for the target invoke process.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property StandardOutput As String Implements IIORedirectAbstract.StandardOutput
            Get
                Return __STDOUT_BUFFER.ToString
            End Get
        End Property

        Public ReadOnly Property Bin As String Implements IIORedirectAbstract.Bin
            Get
                Return ProcessInfo.StartInfo.FileName
            End Get
        End Property
        Public ReadOnly Property CLIArguments As String Implements IIORedirectAbstract.CLIArguments
            Get
                Return ProcessInfo.StartInfo.Arguments
            End Get
        End Property

        Dim __STDOUT_BUFFER As StringBuilder = New StringBuilder(1024)

        Public Delegate Function ProcessAyHandle(WaitForExit As Boolean, PushingData As String(), _DISP_DEBUG_INFO As Boolean) As Integer

        ''' <summary>
        ''' A function pointer for process the events when the target invoked child process was terminated and exit.
        ''' (当目标进程退出的时候所调用的过程)
        ''' </summary>
        ''' <param name="exitCode">The exit code for the target sub invoke process.进程的退出代码</param>
        ''' <param name="exitTime">The exit time for the target sub invoke process.(进程的退出时间)</param>
        ''' <remarks></remarks>
        Public Delegate Sub ProcessExitCallBack(exitCode As Integer, exitTime As String)

        ''' <summary>
        ''' Gets a <see cref="String"/> used to read the error output of the application.
        ''' </summary>
        ''' <returns>A <see cref="String"/> text value that read from the std_error of <see cref="System.IO.StreamReader"/> 
        ''' that can be used to read the standard error stream of the application.</returns>
        Public Function GetError() As String
            Return _errorLogsDevice.ReadToEnd
        End Function

        ''' <summary>
        ''' Start the target process. If the target invoked process is currently on the running state, 
        ''' then this function will returns the -100 value as error code and print the warning 
        ''' information on the system console.(启动目标进程)
        ''' </summary>
        ''' <param name="WaitForExit">
        ''' Indicate that the program code wait for the target process exit or not?
        ''' (参数指示应用程序代码是否等待目标进程的结束)
        ''' </param>
        ''' <returns>
        ''' 当发生错误的时候会返回错误代码，当当前的进程任然处于运行的状态的时候，程序会返回-100错误代码并在终端之上打印出警告信息
        ''' </returns>
        ''' <remarks></remarks>
        Public Function Start(Optional WaitForExit As Boolean = False,
                              Optional PushingData As String() = Nothing,
                              Optional _DISP_DEBUG_INFO As Boolean = False) As Integer

            If _processStateRunning Then
                Dim msg As String = $"Target process ""{ProcessInfo.StartInfo.FileName.ToFileURL}"" is currently in the running state, Operation abort!"
                Call VBDebugger.Warning(msg)
                Return -100
            End If

            If _DISP_DEBUG_INFO Then
                Dim Exe As String = FileIO.FileSystem.GetFileInfo(ProcessInfo.StartInfo.FileName).FullName.Replace("\", "/")
                Dim argvs As String = If(String.IsNullOrEmpty(ProcessInfo.StartInfo.Arguments), "", " " & ProcessInfo.StartInfo.Arguments)
                Call Console.WriteLine("      ---> system(""file:''{0}""{1})", Exe, argvs)
            End If

            Try
                Call ProcessInfo.Start()
            Catch ex As Exception
                Call Printf("FATAL_ERROR::%s", ex.ToString)
                Call Console.WriteLine("  Exe ==> " & ProcessInfo.StartInfo.FileName)
                Call Console.WriteLine("argvs ==> " & ProcessInfo.StartInfo.Arguments)

                ' Return ex.HResul '4.5
                Return -1
            End Try

            _processStateRunning = True
            If _IORedirect Then
                _consoleDevice = ProcessInfo.StandardOutput
                _errorLogsDevice = ProcessInfo.StandardError
            End If

            _inputDevice = ProcessInfo.StandardInput

            Dim ListeningThreadA As System.Action = AddressOf Me.__listenSTDOUT
            Dim DetectExitThread As System.Action = AddressOf Me.__detectProcessExit

            If _IORedirect Then
                Call ListeningThreadA.BeginInvoke(Nothing, Nothing)
            End If
            Call DetectExitThread.BeginInvoke(Nothing, Nothing)

            If Not PushingData.IsNullOrEmpty Then

                For Each Line As String In PushingData
                    Call _inputDevice.WriteLine(Line)
                    Call _inputDevice.Flush()
                    Call Console.WriteLine("  >>>   " & Line)
                Next

            End If

            If WaitForExit Then
                Call ProcessInfo.WaitForExit()
                Call Thread.Sleep(100)
                Call OperationTimeOut(AddressOf __tryGetSTDOUT, 2)  '将剩余的标准输出之中的数据完全的打印出来

                Return ProcessInfo.ExitCode
            Else
                Call Parallel.RunTask(AddressOf ProcessInfo.WaitForExit)
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Start the target process.(启动目标进程)
        ''' </summary>
        ''' <returns>当发生错误的时候会返回错误代码</returns>
        ''' <remarks></remarks>
        Public Function Start(ProcessExitCallBack As ProcessExitCallBack,
                              Optional PushingData As String() = Nothing,
                              Optional _DISP_DEBUG_INFO As Boolean = False) As Integer
            AddHandler Me.ProcessExit, Sub(exitCode As Integer, exitTime As String) Call ProcessExitCallBack(exitCode, exitTime)
            Return Start(WaitForExit:=False,
                         PushingData:=PushingData,
                         _DISP_DEBUG_INFO:=_DISP_DEBUG_INFO)
        End Function

        Public Sub WriteLine(s As String) Implements I_ConsoleDeviceHandle.WriteLine
            Call _inputDevice.WriteLine(s)
            Call _inputDevice.Flush()
        End Sub

        ''' <summary>
        ''' 输出目标子进程的标准输出设备的内容
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub __listenSTDOUT()
            Call $"{MethodBase.GetCurrentMethod.ToString} threading start!  ProcessStateExit = {_processStateRunning.ToString}".__DEBUG_ECHO

            Do While _processStateRunning = True
                Call __tryGetSTDOUT()
                Call Thread.Sleep(10)
            Loop

            Call $"{MethodBase.GetCurrentMethod.ToString}(""{Me.ProcessInfo.StartInfo.FileName.ToFileURL}"") threading exit!".__DEBUG_ECHO
        End Sub

        Private Function __tryGetSTDOUT() As String
            Dim readBuffer As String = _consoleDevice.ReadToEnd   '进程退出去了之后会卡在这里？？？

            If Len(readBuffer) = 0 Then
                Call Thread.Sleep(10)
                Return ""
            End If

            Call __STDOUT_BUFFER.Append(readBuffer)
            If _DispInvokedSTDOUT Then Call Console.WriteLine(readBuffer)

            RaiseEvent DataArrival(readBuffer)

            Return readBuffer
        End Function

        ''' <summary>
        ''' 检测目标子进程是否已经结束
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub __detectProcessExit()
            Call $"{MethodBase.GetCurrentMethod.ToString} threading start!".__DEBUG_ECHO

            Do While Not ProcessInfo.HasExited
                Call Thread.Sleep(10)
            Loop

            Call "Invoked process has been exit...".__DEBUG_ECHO
            _processStateRunning = False
            RaiseEvent ProcessExit(ProcessInfo.ExitCode, ProcessInfo.ExitTime.ToString)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} {1}", ProcessInfo.StartInfo.FileName, ProcessInfo.StartInfo.Arguments)
        End Function

        ''' <summary>
        ''' 在进行隐士转换的时候，假若可执行文件的文件路径之中含有空格，则这个时候应该要特别的小心
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        Public Shared Widening Operator CType(CLI As String) As IORedirect
#If DEBUG Then
            Call CLI.__DEBUG_ECHO
#End If
            Dim Tokens As String() = Regex.Split(CLI, SPLIT_REGX_EXPRESSION)
            Dim EXE As String = Tokens.First
            Dim args As String = Mid$(CLI, Len(EXE) + 1)

            Return New IORedirect(EXE, args)
        End Operator

        ReadOnly _IORedirect As Boolean

        ''' <summary>
        ''' Creates a <see cref="System.Diagnostics.Process"/> wrapper for the CLI program operations.
        ''' (在服务器上面可能会有一些线程方面的兼容性BUG的问题，不太清楚为什么会导致这样)
        ''' </summary>
        ''' <param name="Exe">The file path of the executable file.</param>
        ''' <param name="args">The CLI arguments for the folked program.</param>
        ''' <param name="envir">Set up the environment variable for the target invoked child process.</param>
        ''' <param name="_disp_debug"></param>
        ''' <param name="disp_STDOUT">是否显示目标被调用的外部程序的标准输出</param>
        ''' <remarks></remarks>
        Public Sub New(Exe As String,
                       Optional args As String = "",
                       Optional envir As IEnumerable(Of KeyValuePair(Of String, String)) = Nothing,
                       Optional IORedirect As Boolean = True,
                       Optional disp_STDOUT As Boolean = True,
                       Optional _disp_debug As Boolean = False)

            Dim pInfo As New ProcessStartInfo(Exe.GetString(""""c), args)

            _IORedirect = IORedirect
            pInfo.UseShellExecute = False
            If IORedirect Then  '只是重定向输出设备流
                pInfo.RedirectStandardOutput = True
                pInfo.RedirectStandardError = True
            End If
            pInfo.RedirectStandardInput = True
            pInfo.ErrorDialog = False
            pInfo.WindowStyle = ProcessWindowStyle.Hidden
            pInfo.CreateNoWindow = True

            If Not envir Is Nothing Then
                For Each para As KeyValuePair(Of String, String) In envir
                    Call pInfo.EnvironmentVariables.Add(para.Key, para.Value)
                Next
            End If

            _DispInvokedSTDOUT = disp_STDOUT
            _ProcessInfo = New Process
            _ProcessInfo.EnableRaisingEvents = True
            _ProcessInfo.StartInfo = pInfo
            _processStateRunning = False

            If _disp_debug Then Call $"""{Exe}"" {args}".__DEBUG_ECHO
            If _disp_debug Then Call Console.WriteLine(If(disp_STDOUT, "disp_STDOUT  -> Yes!", "disp_STDOUT   -> NO!"))
        End Sub

        Public Shared Function Shell(CommandLine As String) As IORedirect
            Return CType(CommandLine, IORedirect)
        End Function

        Public Function Read() As Integer Implements I_ConsoleDeviceHandle.Read
            Return _consoleDevice.Read
        End Function

        Public Function ReadLine() As String Implements I_ConsoleDeviceHandle.ReadLine
            Return _consoleDevice.ReadLine
        End Function

        Public Sub WriteLine(s As String, ParamArray args() As String) Implements I_ConsoleDeviceHandle.WriteLine
            Call Me._inputDevice.WriteLine(String.Format(s, args))
            Call Me._inputDevice.Flush()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call ProcessInfo.Close()
                    Call ProcessInfo.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        ''' <summary>
        ''' Gets the value that the associated process specified when it terminated.
        ''' </summary>
        ''' <param name="WaitForExit"></param>
        ''' <returns>The code that the associated process specified when it terminated.</returns>
        Public Function Start(Optional WaitForExit As Boolean = False) As Integer Implements IIORedirectAbstract.Start
            Return Start(WaitForExit, Nothing, True)
        End Function

        ''' <summary>
        ''' 线程会被阻塞在这里，直到外部应用程序执行完毕
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As Integer Implements IIORedirectAbstract.Run
            Return Start(WaitForExit:=True)
        End Function
    End Class
End Namespace
