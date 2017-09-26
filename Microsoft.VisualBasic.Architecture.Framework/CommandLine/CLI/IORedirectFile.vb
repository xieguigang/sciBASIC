#Region "Microsoft.VisualBasic::d86c3b8226066e52265905fc287b7d85, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\CLI\IORedirectFile.vb"

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

Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.ApplicationServices

Namespace CommandLine

    ''' <summary>
    ''' Using this class object rather than <see cref="IORedirect"/> is more encouraged.
    ''' (假若所建立的子进程并不需要进行终端交互，相较于<see cref="IORedirect"/>对象，更加推荐使用本对象类型来执行。
    ''' 似乎<see cref="IORedirect"/>对象在创建一个子进程的时候的对象IO重定向的句柄的处理有问题，所以在这里构建一个更加简单的类型对象，
    ''' 这个IO重定向对象不具备终端交互功能)
    ''' </summary>
    ''' <remarks>先重定向到一个临时文件之中，然后再返回临时文件给用户代码</remarks>
    Public Class IORedirectFile
        Implements IDisposable, IIORedirectAbstract

#Region "Temp File"

        ''' <summary>
        ''' 重定向的临时文件
        ''' </summary>
        ''' <remarks>当使用.tmp拓展名的时候会由于APP框架里面的GC线程里面的自动临时文件清理而产生冲突，所以这里需要其他的文件拓展名来避免这个冲突</remarks>
        Protected ReadOnly _TempRedirect As String = App.GetAppSysTempFile(".proc_IO_STDOUT", App.Process.Id)

        ''' <summary>
        ''' shell文件接口
        ''' </summary>
        Dim ProcessBAT As String
#End Region

        ''' <summary>
        ''' The target invoked process event has been exit with a specific return code.(目标派生子进程已经结束了运行并且返回了一个错误值)
        ''' </summary>
        ''' <param name="exitCode"></param>
        ''' <param name="exitTime"></param>
        ''' <remarks></remarks>
        Public Event ProcessExit(exitCode As Integer, exitTime As String) Implements IIORedirectAbstract.ProcessExit

        ''' <summary>
        ''' 目标子进程的终端标准输出
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StandardOutput As String Implements IIORedirectAbstract.StandardOutput
            Get
                Return New IO.StreamReader(_TempRedirect).ReadToEnd
            End Get
        End Property

        Public ReadOnly Property Bin As String Implements IIORedirectAbstract.Bin
        Public ReadOnly Property CLIArguments As String Implements IIORedirectAbstract.CLIArguments

        ''' <summary>
        ''' 将目标子进程的标准终端输出文件复制到一个新的文本文件之中
        ''' </summary>
        ''' <param name="CopyToPath"></param>
        ''' <returns></returns>
        Public Function CopyRedirect(CopyToPath As String) As Boolean
            If CopyToPath.FileExists Then
                Call FileIO.FileSystem.DeleteFile(CopyToPath)
            End If
            Try
                Call FileIO.FileSystem.CopyFile(_TempRedirect, CopyToPath)
            Catch ex As Exception
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Using this class object rather than <see cref="IORedirect"/> is more encouraged if there is no console interactive with your folked process.
        ''' </summary>
        ''' <param name="file">The program file.(请注意检查路径参数，假若路径之中包含有%这个符号的话，在调用cmd的时候会失败)</param>
        ''' <param name="argv">The program commandline arguments.(请注意检查路径参数，假若路径之中包含有%这个符号的话，在调用cmd的时候会失败)</param>
        ''' <param name="environment">Temporary environment variable</param>
        ''' <param name="FolkNew">Folk the process on a new console window if this parameter value is TRUE</param>
        ''' <param name="stdRedirect">If not want to redirect the std out to your file, just leave this value blank.</param>
        Sub New(file As String, Optional argv As String = "",
                Optional environment As KeyValuePair(Of String, String)() = Nothing,
                Optional FolkNew As Boolean = False,
                Optional stdRedirect As String = "")

            If Not String.IsNullOrEmpty(stdRedirect) Then
                _TempRedirect = stdRedirect.CLIPath
            End If

            Try
                file = FileIO.FileSystem.GetFileInfo(file).FullName
            Catch ex As Exception
                ex = New Exception(file, ex)
                Throw ex
            End Try

            argv = $"{argv} > {_TempRedirect}"

            If file.Contains(" "c) Then
                file = $"""{file}"""
            End If

            Dim BAT As New StringBuilder(1024)

            '切换工作目录至当前的进程所处的文件夹
            Dim Drive As String = FileIO.FileSystem _
                .GetDirectoryInfo(App.CurrentDirectory) _
                .Root _
                .Name _
                .Replace("\", "") _
                .Replace("/", "")

            Call BAT.AppendLine(Drive)
            Call BAT.AppendLine("CD " & App.CurrentDirectory.CLIPath)

            If Not environment.IsNullOrEmpty Then '写入临时的环境变量
                For Each para As KeyValuePair(Of String, String) In environment
                    Call BAT.AppendLine($"set {para.Key}={para.Value }")
                Next
            End If

            Call "".SaveTo(_TempRedirect)  ' 系统可能不会自动创建文件夹，则使用这个方法来创建，避免出现无法找到文件的问题

            'Process = New Process
            'Process.EnableRaisingEvents = True
            'Process.StartInfo = pInfo
            Call BAT.AppendLine()

            If FolkNew Then
                Call BAT.AppendLine($"start {file} {argv}")   '在新的窗口打开
            Else
                Call BAT.AppendLine($"{file} {argv}")  '生成IO重定向的命令行
            End If

            ProcessBAT = BAT.ToString
            Bin = file
            CLIArguments = argv

            Call $"""{file.ToFileURL}"" {argv}".__DEBUG_ECHO
        End Sub

        ''' <summary>
        ''' Start target child process and then wait for the child process exits. 
        ''' So that the thread will be stuck at here until the sub process is 
        ''' job done!
        ''' (启动目标子进程，然后等待执行完毕并返回退出代码(请注意，在进程未执行完毕
        ''' 之前，整个线程会阻塞在这里))
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As Integer Implements IIORedirectAbstract.Run
            Dim tmpBAT As String = App.GetAppSysTempFile(".bat", App.Process.Id)
            Call ProcessBAT.SaveTo(tmpBAT)
            Dim ExitCode As Integer = Interaction.Shell(tmpBAT, AppWinStyle.Hide, Wait:=True)

            Call Console.WriteLine(StandardOutput)
            Call $"Process exit with code {ExitCode}....".__DEBUG_ECHO

            Try
                Call FileIO.FileSystem.DeleteFile(tmpBAT, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
            Finally
            End Try

            Return ExitCode
        End Function

        Dim currentLength As Long

        Private Sub __readSTDOUT()
            Call Threading.Thread.Sleep(5000)

            Do While Not Me.disposedValue

                Call Threading.Thread.Sleep(1000)
                Call __readSTDOUTInvoke()

            Loop
        End Sub

        Private Sub __readSTDOUTInvoke()
            Using fs = New IO.FileStream(Me._TempRedirect, IO.FileMode.Open, access:=IO.FileAccess.Read)

                If fs.Length <= currentLength Then Return

                Dim d As Long = fs.Length - currentLength
                Dim buf As Byte() = New Byte(d - 1) {}
                Call fs.Read(buf, currentLength, buf.Length)
                Call Console.WriteLine(Encoding.Default.GetString(buf))

                currentLength = fs.Length
            End Using
        End Sub

        Public Function Start(WaitForExit As Boolean, PushingData As String(), _DISP_DEBUG_INFO As Boolean) As Integer
            Return Start(WaitForExit)
        End Function

        Public Function Start(Optional waitForExit As Boolean = False) As Integer Implements IIORedirectAbstract.Start
            If waitForExit Then
                Return Run()
            Else
                Call Start(procExitCallback:=Nothing)
            End If

            Return 0
        End Function

        ''' <summary>
        ''' 启动子进程，但是不等待执行完毕，当目标子进程退出的时候，回调<paramref name="procExitCallback"/>函数句柄
        ''' </summary>
        ''' <param name="procExitCallback"></param>
        Public Sub Start(Optional procExitCallback As Action = Nothing)
            Call New Tasks.Task(Of Action)(procExitCallback, AddressOf __processExitHandle).Start()
        End Sub

        Private Sub __processExitHandle(ProcessExitCallback As Action)
            Dim ExitCode = Run()
            RaiseEvent ProcessExit(ExitCode, Now.ToString)
            If Not ProcessExitCallback Is Nothing Then
                Call ProcessExitCallback()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return ProcessBAT
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    ' 清理临时文件
                    On Error Resume Next

                    Call FileIO.FileSystem.DeleteFile(Me._TempRedirect, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
