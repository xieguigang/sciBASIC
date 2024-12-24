#Region "Microsoft.VisualBasic::f71422c8a03c76380218111e6e2db3db, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\Pipeline\RunSlavePipeline.vb"

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

    '   Total Lines: 179
    '    Code Lines: 106 (59.22%)
    ' Comment Lines: 43 (24.02%)
    '    - Xml Docs: 83.72%
    ' 
    '   Blank Lines: 30 (16.76%)
    '     File Size: 6.69 KB


    '     Class RunSlavePipeline
    ' 
    '         Properties: Arguments, CommandLine, Process, Shell, std_input
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Bind, Run, Start, ToString
    ' 
    '         Sub: HookProgress, (+2 Overloads) ProcessMessage, SendMessage, SendProgress
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace CommandLine.InteropService.Pipeline

    ''' <summary>
    ''' wrapper for run a background task process
    ''' </summary>
    Public Class RunSlavePipeline

        Public Event SetProgress(percentage As Integer, details As String)

        ''' <summary>
        ''' action of the string message
        ''' </summary>
        ''' <param name="message"></param>
        ''' <remarks><see cref="Action(Of String)"/></remarks>
        Public Event SetMessage(message As String)
        Public Event Finish(exitCode As Integer)

        ''' <summary>
        ''' the file full path to the target executable application file
        ''' </summary>
        ReadOnly app As String
        ReadOnly workdir As String

        Public ReadOnly Property Process As Process

        Public ReadOnly Property CommandLine As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return $"{app} {Arguments}"
            End Get
        End Property

        ''' <summary>
        ''' the commandline argument of the target background task
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Arguments As String
        Public Property Shell As Boolean = False
        Public Property std_input As String = Nothing

        ''' <summary>
        ''' create a new commandline pipeline task
        ''' </summary>
        ''' <param name="app">the commandline application</param>
        ''' <param name="arguments">
        ''' the commandline argument string, value of this string
        ''' parameter could be in multiple line
        ''' </param>
        ''' <param name="workdir"></param>
        ''' <remarks>
        ''' the commandline <paramref name="arguments"/> could be in multiple lines
        ''' </remarks>
        Sub New(app$, arguments$, Optional workdir As String = Nothing)
            Me.app = app
            Me.Arguments = arguments
            Me.workdir = workdir
        End Sub

        Public Function Run() As Integer
            Dim code As Integer = PipelineProcess.ExecSub(
                app:=app,
                args:=Arguments,
                onReadLine:=AddressOf ProcessMessage,
                workdir:=workdir,
                shell:=Shell,
                setProcess:=Sub(p) _Process = p,
                [in]:=std_input
            )

            RaiseEvent Finish(code)

            Return code
        End Function

        Public Function Start() As Process
            _Process = CreatePipeline(
                appPath:=app,
                args:=Arguments,
                it:=(Not app.ExtensionSuffix("sh")) OrElse (Not Shell) OrElse app.FileExists,
                workdir:=workdir
            )

            If Process.StartInfo.RedirectStandardOutput Then
                Call handleRunStream(Process, "", onReadLine:=AddressOf ProcessMessage, async:=True)
            End If

            Return Process
        End Function

        Public Shared Function Bind(proc As Process) As RunSlavePipeline
            Dim info = proc.StartInfo
            Dim pip As New RunSlavePipeline(info.FileName, info.Arguments, info.WorkingDirectory)
            pip._Process = proc
            If info.RedirectStandardOutput Then
                Call handleRunStream(proc, "", onReadLine:=AddressOf pip.ProcessMessage, async:=True)
            End If
            Return pip
        End Function

        ''' <summary>
        ''' get commandline of current background task, in string format like 
        ''' example as: /path/to/exe cli_arguments
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return CommandLine
        End Function

        Private Sub ProcessMessage(line As String)
            If line.StringEmpty Then
                Return
            End If

            If line.StartsWith(message_header) Then
                ' [SET_MESSAGE] message text
                RaiseEvent SetMessage(line.GetTagValue(" ", trim:=True).Value)
            ElseIf line.StartsWith(progress_header) Then
                ' [SET_PROGRESS] percentage message text
                Dim data = line.GetTagValue(" ", trim:=True).Value.GetTagValue(" ", trim:=True)
                Dim percentage As Double = Val(data.Name)
                Dim message As String = data.Value

                RaiseEvent SetProgress(percentage, message)
            Else
                ' other standard output text treated as message output?
                RaiseEvent SetMessage(line)
            End If
        End Sub

        Const message_header = "[SET_MESSAGE]"
        Const progress_header = "[SET_PROGRESS]"

        Public Shared Sub ProcessMessage(line As String, println As Action(Of String), progress As Action(Of Double, String))
            If line.StringEmpty Then
                Return
            End If

            If line.StartsWith(message_header) Then
                ' [SET_MESSAGE] message text
                println(line.GetTagValue(" ", trim:=True).Value)
            ElseIf line.StartsWith(progress_header) Then
                ' [SET_PROGRESS] percentage message text
                Dim data = line.GetTagValue(" ", trim:=True).Value.GetTagValue(" ", trim:=True)
                Dim percentage As Double = Val(data.Name)
                Dim message As String = data.Value

                progress(percentage, message)
            End If
        End Sub

        ''' <summary>
        ''' pop out a taged message that tells the parent process to capture this message and echo to the text logger
        ''' </summary>
        ''' <param name="message"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub SendMessage(message As String)
            ' Call VBDebugger.WaitOutput()
            Call VBDebugger.EchoLine($"{message_header} {message}")
        End Sub

        Shared m_hookProgress As SetProgressEventHandler

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub HookProgress(progress As SetProgressEventHandler)
            m_hookProgress = progress
        End Sub

        Public Shared Sub SendProgress(percentage As Double, message As String)
            ' Call VBDebugger.WaitOutput()
            Call VBDebugger.EchoLine($"{progress_header} {percentage} {message}")

            If Not m_hookProgress Is Nothing Then
                Call m_hookProgress(percentage, message)
            End If
        End Sub

    End Class
End Namespace
