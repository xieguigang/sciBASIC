#Region "Microsoft.VisualBasic::59b88f692c6293c9bfc2b7e7af2d6e63, Microsoft.VisualBasic.Core\src\CommandLine\CLI\Scripting.vb"

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

    '   Total Lines: 86
    '    Code Lines: 63 (73.26%)
    ' Comment Lines: 8 (9.30%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (17.44%)
    '     File Size: 3.49 KB


    '     Module ScriptingExtensions
    ' 
    '         Function: Bash, Cmd
    ' 
    '         Sub: start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports ValueTuple = System.Collections.Generic.KeyValuePair(Of String, String)

Namespace CommandLine

    Module ScriptingExtensions

        Public Function Cmd(program$, argv$, environment As IEnumerable(Of ValueTuple), folkNew As Boolean, stdin$, isShellCommand As Boolean) As String
            Dim bat As New StringBuilder(1024)

            ' 切换工作目录至当前的进程所处的文件夹
            Dim Drive As String = FileIO.FileSystem _
                .GetDirectoryInfo(App.CurrentDirectory) _
                .Root _
                .Name _
                .Replace("\", "") _
                .Replace("/", "")

            Call bat.AppendLine("@echo off")
            Call bat.AppendLine(Drive)
            Call bat.AppendLine("CD " & App.CurrentDirectory.CLIPath)

            If Not environment Is Nothing Then
                ' 写入临时的环境变量
                For Each para As ValueTuple In environment
                    Call bat.AppendLine($"set {para.Key}={para.Value}")
                Next
            End If

            Call bat.AppendLine()

            If folkNew Then
                ' 在新的窗口打开
                Call bat.AppendLine($"start ""{program}"" {argv}")
            Else
                ' 生成IO重定向的命令行
                ' https://stackoverflow.com/questions/3680977/can-a-batch-file-capture-the-exit-codes-of-the-commands-it-is-invoking
                Call bat.AppendLine("set errorlevel=")
                Call bat.start(program, argv, stdin, isShellCommand)
                Call bat.AppendLine("exit /b %errorlevel%")
            End If

            Return bat.ToString
        End Function

        <Extension>
        Private Sub start(script As StringBuilder, program$, argv$, stdin$, isShellCommand As Boolean)
            If stdin.StringEmpty Then
                If isShellCommand Then
                    Call script.AppendLine($"{program} {argv}")
                Else
                    Call script.AppendLine($"""{program}"" {argv}")
                End If
            Else
                If isShellCommand Then
                    Call script.AppendLine($"echo {stdin} | {program} {argv}")
                Else
                    Call script.AppendLine($"echo {stdin} | ""{program}"" {argv}")
                End If
            End If
        End Sub

        Public Function Bash(program$, argv$, environment As IEnumerable(Of ValueTuple), folkNew As Boolean, stdin$, isShellCommand As Boolean) As String
            Dim shell As New StringBuilder("#!/bin/bash")

            Call shell.AppendLine()
            Call shell.AppendLine()

            If Not environment Is Nothing Then
                For Each param As ValueTuple In environment
                    ' To set it for current shell And all processes started from current shell
                    ' shorter, less portable version
                    ' export VARNAME="my value"      
                    Call shell.AppendLine($"export {param.Key}=""{param.Value}""")
                Next
            End If

            Call shell.AppendLine($"cd {App.CurrentDirectory.CLIPath}")
            Call shell.start(program, argv, stdin, isShellCommand)

            Return shell.ToString
        End Function
    End Module
End Namespace
