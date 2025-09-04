#Region "Microsoft.VisualBasic::3ab1f48a3d6e685ca15d403a7154cecd, Microsoft.VisualBasic.Core\src\CommandLine\CLI\IORedirectFile.vb"

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

    '   Total Lines: 291
    '    Code Lines: 156 (53.61%)
    ' Comment Lines: 95 (32.65%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 40 (13.75%)
    '     File Size: 14.33 KB


    '     Class IORedirectFile
    ' 
    '         Properties: Bin, CLIArguments, redirectDevice, StandardOutput
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CopyRedirect, Run, (+2 Overloads) Start, ToString, writeScript
    ' 
    '         Sub: __processExitHandle, (+2 Overloads) Dispose, Start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Text
Imports ValueTuple = System.Collections.Generic.KeyValuePair(Of String, String)

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
        Protected ReadOnly _TempRedirect As String = TempFileSystem.GetAppSysTempFile(".proc_IO_std.out", App.PID)
        Protected ReadOnly _win_os As Boolean

        ''' <summary>
        ''' shell文件接口
        ''' </summary>
        Dim shellScript As String
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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New IO.StreamReader(_TempRedirect).ReadToEnd
            End Get
        End Property

        Public ReadOnly Property Bin As String Implements IIORedirectAbstract.Bin
        Public ReadOnly Property CLIArguments As String Implements IIORedirectAbstract.CLIArguments

        Public ReadOnly Property redirectDevice As String
            Get
                Return _TempRedirect
            End Get
        End Property

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
        ''' Using this class object rather than <see cref="IORedirect"/> is more 
        ''' encouraged if there is no console interactive with your folked 
        ''' process.
        ''' </summary>
        ''' <param name="file">
        ''' The program file.
        ''' (请注意检查路径参数，假若路径之中包含有%这个符号的话，在调用cmd的时候会失败)
        ''' </param>
        ''' <param name="argv">
        ''' The program commandline arguments.
        ''' (请注意检查路径参数，假若路径之中包含有%这个符号的话，在调用cmd的时候会失败)
        ''' </param>
        ''' <param name="environment">Temporary environment variable</param>
        ''' <param name="folkNew">Folk the process on a new console window if this parameter value is TRUE</param>
        ''' <param name="stdRedirect">If not want to redirect the std out to your file, just leave this value blank.</param>
        Sub New(file$,
                Optional argv$ = "",
                Optional environment As IEnumerable(Of ValueTuple) = Nothing,
                Optional folkNew As Boolean = False,
                Optional stdRedirect$ = "",
                Optional stdin$ = Nothing,
                Optional debug As Boolean = True,
                Optional isShellCommand As Boolean = False,
                Optional win_os As Boolean? = Nothing)

            If Not String.IsNullOrEmpty(stdRedirect) Then
                _TempRedirect = stdRedirect.CLIPath
            End If

            ' 没有小数点，说明可能只是一个命令，而不是具体的可执行程序文件名
            If InStr(file, ".") = 0 Then
                ' do nothing
            ElseIf Not isShellCommand Then
                ' 对于具体的程序文件的调用，在这里获取其完整路径
                Try
                    file = FileIO.FileSystem.GetFileInfo(file).FullName
                Catch ex As Exception
                    ex = New Exception(file, ex)
                    Throw ex
                End Try
            End If

            Dim app_argv As String = argv.TrimNewLine(" ")

            Bin = file
            argv = $"{app_argv} > {_TempRedirect}"
            CLIArguments = argv
            _win_os = If(win_os Is Nothing, App.IsMicrosoftPlatform, CBool(win_os))

            ' 系统可能不会自动创建文件夹，则需要在这里使用这个方法来手工创建，
            ' 避免出现无法找到文件的问题
            Call _TempRedirect.ParentPath.MakeDir
            ' 在Unix平台上面这个文件不会被自动创建？？？
            Call "".SaveTo(_TempRedirect)

            If _win_os Then
                shellScript = ScriptingExtensions.Cmd(file, argv, environment, folkNew, stdin, isShellCommand)
            Else
                shellScript = ScriptingExtensions.Bash(file, argv, environment, folkNew, stdin, isShellCommand)
            End If

            If debug Then
                If isShellCommand Then
                    Call $"""{file}"" {app_argv}".debug
                Else
                    Call $"""{file.ToFileURL}"" {app_argv}".debug
                End If

                Call $"stdout_temp: {_TempRedirect}".debug
            End If
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
            Dim path$ = writeScript()
            Dim exitCode As Integer

#If NET48 Then
            exitCode = Interaction.Shell(
                path,
                Style:=AppWinStyle.Hide,
                Wait:=True
            )
#Else
#If UNIX Then
            ' xdg-open: file '/tmp/gut_16s/15201/tmp00003.sh' does not exist
            With New Process() With {
                .StartInfo = New ProcessStartInfo(path) With {
                    .CreateNoWindow = False
                }
            }
                Call .Start()
                Call .WaitForExit()

                exitCode = .ExitCode
            End With
#Else
            ' [ERROR 12/10/2017 4:57:27 AM] <Print>::System.Exception: Print ---> System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation. ---> System.DllNotFoundException: kernel32
            '   at (wrapper managed-to-native) Microsoft.VisualBasic.CompilerServices.NativeMethods:GetStartupInfo (Microsoft.VisualBasic.CompilerServices.NativeTypes/STARTUPINFO)
            '   at Microsoft.VisualBasic.Interaction.Shell (System.String PathName, Microsoft.VisualBasic.AppWinStyle Style, System.Boolean Wait, System.Int32 Timeout) [0x00077] in <828807dda9f14f24a7db780c6c644162>:0
            '   at Microsoft.VisualBasic.CommandLine.IORedirectFile.Run () [0x00011] in <d9cf6734998c48a092e8a1528ac0142f>:0
            '   at SMRUCC.genomics.Analysis.Metagenome.Mothur.RunMothur (System.String args) [0x00014] in <d8095d5f77564ae4af334ce9b17144fb>:0
            '   at SMRUCC.genomics.Analysis.Metagenome.Mothur.Make_contigs (System.String file, System.Int32 processors) [0x00013] in <d8095d5f77564ae4af334ce9b17144fb>:0
            '   at SMRUCC.genomics.Analysis.Metagenome.MothurContigsOTU.ClusterOTUByMothur (System.String left, System.String right, System.String silva, System.String workspace, System.Int32 processor) [0x00060] in <d8095d5f77564ae4af334ce9b17144fb>:0
            '   at meta_community.CLI.ClusterOTU (Microsoft.VisualBasic.CommandLine.CommandLine args) [0x0004b] in <58a80ce28e644b22a21c332e0f3bd1f5>:0
            '   at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (System.Reflection.MonoMethod,object,object[],System.Exception&)
            '   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00032] in <902ab9e386384bec9c07fa19aa938869>:0
            '    --- End of inner exception stack trace ---
            '   at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00048] in <902ab9e386384bec9c07fa19aa938869>:0
            '   at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) [0x00000] in <902ab9e386384bec9c07fa19aa938869>:0
            '   at Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints.APIEntryPoint.__directInvoke (System.Object[] callParameters, System.Object target, System.Boolean Throw) [0x0000c] in <d9cf6734998c48a092e8a1528ac0142f>:0
            '    --- End of inner exception stack trace ---
            [Call](path, "", "")
#End If
#End If
            Call path.debug

            Return exitCode
        End Function

        Private Function writeScript() As String
            Dim ext$ = If(_win_os, ".bat", ".sh")
            Dim path$ = TempFileSystem.GetAppSysTempFile(ext, App.PID)
            Call shellScript.SaveTo(path, Encodings.UTF8WithoutBOM.CodePage)
            Return path
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
            Return shellScript
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
