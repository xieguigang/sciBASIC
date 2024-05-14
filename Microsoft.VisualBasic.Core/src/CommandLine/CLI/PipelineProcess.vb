#Region "Microsoft.VisualBasic::519dd6d8f4a97ef1c5636124a805c763, Microsoft.VisualBasic.Core\src\CommandLine\CLI\PipelineProcess.vb"

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

    '   Total Lines: 316
    '    Code Lines: 171
    ' Comment Lines: 99
    '   Blank Lines: 46
    '     File Size: 12.26 KB


    '     Module PipelineProcess
    ' 
    '         Function: (+2 Overloads) [Call], CallDotNetCorePipeline, CreatePipeline, (+2 Overloads) ExecSub, FindProc
    '                   (+2 Overloads) GetProc, handleRunStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
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

            Dim p As Process = CreatePipeline(
                appPath:=app,
                args:=args,
                it:=(Not app.ExtensionSuffix("sh")) OrElse (Not shell) OrElse app.FileExists,
                workdir:=workdir
            )

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
            Dim reader As StreamReader = p.StandardOutput
            Dim errReader As StreamReader = p.StandardError

            If Not String.IsNullOrEmpty([in]) Then
                Dim writer As StreamWriter = p.StandardInput

                Call writer.WriteLine([in])
                Call writer.Flush()
            End If

            If Not async Then
                While Not reader.EndOfStream
                    Call onReadLine(reader.ReadLine)
                End While

                Return errReader.ReadToEnd
            Else
                Call Task.Run(
                    Sub()
                        While Not reader.EndOfStream
                            Call onReadLine(reader.ReadLine)
                        End While
                    End Sub)

                Return Nothing
            End If
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
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            p.StartInfo.RedirectStandardOutput = it
            p.StartInfo.RedirectStandardInput = it
            p.StartInfo.RedirectStandardError = it
            p.StartInfo.UseShellExecute = Not it
            p.StartInfo.CreateNoWindow = App.IsMicrosoftPlatform

            If Not workdir.StringEmpty Then
                If Not workdir.DirectoryExists Then
                    Call $"mising work directory: {workdir}!".Warning
                    Call workdir.MakeDir
                End If

                p.StartInfo.WorkingDirectory = workdir.GetDirectoryFullPath
            End If

            p.Start()

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

            Do While True
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
