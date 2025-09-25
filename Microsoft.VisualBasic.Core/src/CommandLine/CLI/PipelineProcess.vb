#Region "Microsoft.VisualBasic::7b9bfc2821619b9f5d1c355c431163d1, Microsoft.VisualBasic.Core\src\CommandLine\CLI\PipelineProcess.vb"

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

    '   Total Lines: 347
    '    Code Lines: 186 (53.60%)
    ' Comment Lines: 110 (31.70%)
    '    - Xml Docs: 84.55%
    ' 
    '   Blank Lines: 51 (14.70%)
    '     File Size: 13.49 KB


    '     Module PipelineProcess
    ' 
    '         Function: (+2 Overloads) [Call], CallDotNetCorePipeline, CheckProcessStreamOpen, CreatePipeline, (+2 Overloads) ExecSub
    '                   FindProc, (+2 Overloads) GetProc, handleRunStream
    ' 
    '         Sub: ReadLines
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Language
Imports ConsoleApp = Microsoft.VisualBasic.CommandLine.InteropService.InteropService
Imports Proc = System.Diagnostics.Process

Namespace CommandLine

    ''' <summary>
    ''' How to found the process by CLI
    ''' </summary>
    ''' 
    Public Module PipelineProcess

        ''' <summary>
        ''' <see cref="Process.GetProcessById"/>
        ''' </summary>
        ''' <param name="pid"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetProc(pid As Integer) As Process
            Return Process.GetProcessById(pid)
        End Function

        ''' <summary>
        ''' Get process by command line parameter.(按照命令行参数来获取进程实例)
        ''' </summary>
        ''' <param name="cli"></param>
        ''' <returns></returns>
        <Extension> Public Function GetProc(cli As String) As Process
            Dim CLICompared As CommandLine = CommandLine.op_Implicit(cli)
            Dim listProc As Process() = Proc.GetProcesses
            Dim process = LinqAPI.DefaultFirst(Of Process) _
                                                           _
                () <= From proc As Process
                      In listProc
                      Let args = Parsers.TryParse(proc.StartInfo.Arguments)
                      Where CLITools.Equals(CLICompared, args)  ' 由于参数的顺序可能会有些不一样，所以不可以直接按照字符串比较来获取
                      Select proc

            Return process
        End Function

        ''' <summary>
        ''' 这个主要是为了<see cref="IORedirectFile"/>对象进行相关进程的查找而设置的，
        ''' 对于<see cref="IORedirect"/>而言则直接可以从其属性<see cref="IORedirect.ProcessInfo"/>之中获取相关的进程信息
        ''' </summary>
        ''' <param name="IO"></param>
        ''' <returns></returns>
        Public Function FindProc(IO As IIORedirectAbstract) As Process
            Dim proc As Process = IO.CLIArguments.GetProc

            If proc Is Nothing Then '空值说明进程还没有启动或者已经终止了，所以查找将不会查找到进程的信息
                Dim msg As String = String.Format(NoProcessFound, IO.ToString)
                Call VBDebugger.Warning(msg)
            End If

            Return proc
        End Function

        Const NoProcessFound As String = "Unable to found associated process {0}, it maybe haven't been started or already terminated."

        ''' <summary>
        ''' 执行CMD命令
        ''' 
        ''' Example:
        ''' 
        ''' ```vbnet
        ''' Call excuteCommand("ipconfig", "/all", AddressOf PrintMessage)
        ''' ```
        ''' </summary>
        ''' <param name="app">命令</param>
        ''' <param name="args">参数</param>
        ''' <param name="onReadLine">行信息（委托）</param>
        ''' <remarks>https://github.com/lishewen/LSWFramework/blob/master/LSWClassLib/CMD/CMDHelper.vb</remarks>
        Public Function ExecSub(app$, args$, onReadLine As Action(Of String),
                                Optional in$ = "",
                                Optional ByRef stdErr As String = Nothing,
                                Optional workdir As String = Nothing,
                                Optional shell As Boolean = False,
                                Optional setProcess As Action(Of Process) = Nothing) As Integer
            ' check for shell flag
            Dim check_shell = app.ExtensionSuffix("sh", "cmd", "bat") OrElse
                shell OrElse
                Not app.FileExists
            Dim p As Process = CreatePipeline(
                appPath:=app,
                args:=args,
                it:=Not check_shell,
                workdir:=workdir
            )

            ' 20241224 there is a bug about access the standard output stream:
            ' thread needs to sleep for a while
            ' or the file access error will happends when access the standard output stream
            Call Thread.Sleep(500)

            If p.StartInfo.RedirectStandardOutput Then
                stdErr = handleRunStream(p, [in], onReadLine, async:=False)
            End If
            If Not setProcess Is Nothing Then
                setProcess(p)
            End If

            Call p.WaitForExit()

            Return p.ExitCode
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="in">
        ''' the standard input
        ''' </param>
        ''' <param name="onReadLine">
        ''' populate the standard output lines
        ''' </param>
        ''' <returns></returns>
        Friend Function handleRunStream(p As Process, in$, onReadLine As Action(Of String), async As Boolean) As String
            Dim errReader As StreamReader = p.StandardError

            If Not String.IsNullOrEmpty([in]) Then
                Dim writer As StreamWriter = p.StandardInput

                Call writer.WriteLine([in])
                Call writer.WriteLine()
                Call writer.Flush()
                Call writer.Close()
            End If

            If Not async Then
                Call ReadLines(p, onReadLine)
                Return errReader.ReadToEnd
            Else
                Call Task.Run(Sub() Call ReadLines(p, onReadLine))
                Return Nothing
            End If
        End Function

        Private Sub ReadLines(p As Process, onReadLine As Action(Of String))
            Dim reader As StreamReader = p.StandardOutput

            While CheckProcessStreamOpen(p, reader)
                Call onReadLine(reader.ReadLine)
            End While

            For Each line As String In reader.ReadToEnd.LineTokens
                Call onReadLine(line)
            Next
        End Sub

        ''' <summary>
        ''' A common wrapper for check of the sub-process stdout stream is avaiable?
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        Public Function CheckProcessStreamOpen(ByRef p As Process, ByRef reader As StreamReader) As Boolean
            ' current program has flag exited
            ' the loop thread should break for exit 
            If Not App.Running Then
                Return False
            End If

            If reader.EndOfStream Then
                Return False
            End If

            Return Not p.HasExited
        End Function

        ''' <summary>
        ''' Create a new process
        ''' </summary>
        ''' <param name="appPath"></param>
        ''' <param name="args"></param>
        ''' <param name="it">
        ''' this option will affects the UseShellExecute:
        ''' 
        ''' ```
        ''' docker run -it XXX
        ''' ```
        ''' 
        ''' parameter value set to TRUE means not UseShellExecute
        ''' </param>
        ''' <returns>
        ''' the target process object is already has been 
        ''' started in this function.
        ''' </returns>
        Public Function CreatePipeline(appPath As String,
                                       args As String,
                                       Optional it As Boolean = True,
                                       Optional workdir As String = Nothing) As Process
            Dim p As New Process

            ' force shell exec when call a dotnet app
            If appPath = "dotnet" Then
                it = False
                p.StartInfo.UseShellExecute = True
            End If

            p.StartInfo = New ProcessStartInfo
            p.StartInfo.FileName = appPath
            p.StartInfo.Arguments = args.TrimNewLine(replacement:=" ")

            If it Then
                ' io redirect
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                p.StartInfo.RedirectStandardOutput = True
                p.StartInfo.RedirectStandardInput = True
                p.StartInfo.RedirectStandardError = True
                p.StartInfo.UseShellExecute = False
                p.StartInfo.CreateNoWindow = True
            End If

            If Not workdir.StringEmpty Then
                If Not workdir.DirectoryExists Then
                    Call $"mising work directory: {workdir}!".Warning
                    Call workdir.MakeDir
                End If

                p.StartInfo.WorkingDirectory = workdir.GetDirectoryFullPath
            End If

            Call p.Start()

            Return p
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="app$"></param>
        ''' <param name="args$"></param>
        ''' <param name="in$"></param>
        ''' <returns>The standard output of the target <paramref name="app"/>, the 
        ''' data inside this stream object may contains text or image or other
        ''' binary data.</returns>
        Public Function ExecSub(app$, args$, Optional in$ = "") As MemoryStream
            Dim p As Process = CreatePipeline(app, args)
            Dim reader As Stream = p.StandardOutput.BaseStream
            Dim buffer As New MemoryStream

            If Not String.IsNullOrEmpty([in]) Then
                Dim writer As StreamWriter = p.StandardInput

                Call writer.WriteLine([in])
                Call writer.Flush()
            End If

            Dim chunk As Byte() = New Byte(1024 - 1) {}
            Dim nbytes As Integer

            Do While Microsoft.VisualBasic.App.Running
                nbytes = reader.Read(chunk, Scan0, chunk.Length)

                If nbytes = 0 Then
                    Exit Do
                Else
                    Call buffer.Write(chunk, Scan0, nbytes)
                End If
            Loop

            Erase chunk

            Call p.WaitForExit()
            Call buffer.Flush()
            Call buffer.Seek(Scan0, SeekOrigin.Begin)

            Return buffer
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="app">
        ''' the ``*.dll/*.exe`` program file path, which is going to running under the ``dotnet`` command.
        ''' </param>
        ''' <param name="args"></param>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function CallDotNetCorePipeline(app As ConsoleApp, Optional args As String = "", Optional [in] As String = "") As MemoryStream
            Dim dll As String = app.Path.TrimSuffix & ".dll"
            Dim cli As String = $"{dll.CLIPath} {args}"

            ' run on UNIX .net 5 
            Return ExecSub("dotnet", cli, [in])
        End Function

        ''' <summary>
        ''' Run process and then gets the ``std_out`` of the child process
        ''' </summary>
        ''' <param name="app">The file path of the application to be called by its parent process.</param>
        ''' <param name="args">CLI arguments</param>
        ''' <returns></returns>
        <Extension>
        Public Function [Call](app As String,
                               Optional args As String = Nothing,
                               Optional [in] As String = "",
                               Optional debug As Boolean = False,
                               Optional ByRef stdErr As String = Nothing,
                               Optional ByRef exitCode As Integer = 0) As String

            Dim stdout As New List(Of String)
            Dim readLine As Action(Of String)

            If debug Then
                readLine = Sub(line)
                               Call stdout.Add(line)
                               Call Console.WriteLine(line)
                           End Sub
            Else
                readLine = AddressOf stdout.Add
            End If

            exitCode = ExecSub(app, args, readLine, [in], stdErr)

            Return stdout.JoinBy(vbCrLf)
        End Function

        ''' <summary>
        ''' Run process and then gets the ``std_out`` of the child process
        ''' </summary>
        ''' <param name="app">The file path of the application to be called by its parent process.</param>
        ''' <param name="args">CLI arguments</param>
        ''' <param name="dotnet">
        ''' Run a .NET core console application?
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function [Call](app As ConsoleApp,
                               Optional args As String = "",
                               Optional [in] As String = "",
                               Optional dotnet As Boolean = False,
                               Optional debug As Boolean = False) As String

            If dotnet Then
                Dim dll As String = app.Path.TrimSuffix & ".dll"
                Dim cli As String = $"{dll.CLIPath} {args}"

                ' run on UNIX .net 5 
                Return [Call]("dotnet", cli, [in], debug:=debug)
            Else
                Return [Call](app.Path, args, [in], debug:=debug)
            End If
        End Function
    End Module
End Namespace
