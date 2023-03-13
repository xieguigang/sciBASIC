#Region "Microsoft.VisualBasic::606cb4661e24c98e7886253b80d9a3c1, sciBASIC#\Microsoft.VisualBasic.Core\src\CommandLine\InteropService\Pipeline\RunSlavePipeline.vb"

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

    '   Total Lines: 111
    '    Code Lines: 77
    ' Comment Lines: 11
    '   Blank Lines: 23
    '     File Size: 3.82 KB


    '     Class RunSlavePipeline
    ' 
    '         Properties: Arguments, CommandLine, Process, Shell
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Run, Start, ToString
    ' 
    '         Sub: HookProgress, ProcessMessage, SendMessage, SendProgress
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.InteropService.Pipeline

    Public Class RunSlavePipeline

        Public Event SetProgress(percentage As Integer, details As String)
        Public Event SetMessage(message As String)
        Public Event Finish(exitCode As Integer)

        ReadOnly app As String
        ReadOnly workdir As String

        Public ReadOnly Property Process As Process

        Public ReadOnly Property CommandLine As String
            Get
                Return $"{app} {Arguments}"
            End Get
        End Property

        Public ReadOnly Property Arguments As String
        Public Property Shell As Boolean = False

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
                setProcess:=Sub(p) _Process = p
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

        Public Overrides Function ToString() As String
            Return CommandLine
        End Function

        Private Sub ProcessMessage(line As String)
            If line.StringEmpty Then
                Return
            End If

            If line.StartsWith("[SET_MESSAGE]") Then
                ' [SET_MESSAGE] message text
                RaiseEvent SetMessage(line.GetTagValue(" ", trim:=True).Value)
            ElseIf line.StartsWith("[SET_PROGRESS]") Then
                ' [SET_PROGRESS] percentage message text
                Dim data = line.GetTagValue(" ", trim:=True).Value.GetTagValue(" ", trim:=True)
                Dim percentage As Double = Val(data.Name)
                Dim message As String = data.Value

                RaiseEvent SetProgress(percentage, message)
            End If
        End Sub

        Public Shared Sub SendMessage(message As String)
            ' Call VBDebugger.WaitOutput()
            Call VBDebugger.EchoLine($"[SET_MESSAGE] {message}")
        End Sub

        Shared m_hookProgress As SetProgressEventHandler

        Public Shared Sub HookProgress(progress As SetProgressEventHandler)
            m_hookProgress = progress
        End Sub

        Public Shared Sub SendProgress(percentage As Double, message As String)
            ' Call VBDebugger.WaitOutput()
            Call VBDebugger.EchoLine($"[SET_PROGRESS] {percentage} {message}")

            If Not m_hookProgress Is Nothing Then
                Call m_hookProgress(percentage, message)
            End If
        End Sub

    End Class
End Namespace
