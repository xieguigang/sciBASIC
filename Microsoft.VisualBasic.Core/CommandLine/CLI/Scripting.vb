Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports ValueTuple = System.Collections.Generic.KeyValuePair(Of String, String)

Namespace CommandLine

    Module ScriptingExtensions

        Public Function Cmd(program$, argv$, environment As IEnumerable(Of ValueTuple), folkNew As Boolean) As String
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

            If Not environment.IsNullOrEmpty Then
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
                Call bat.AppendLine($"""{program}"" {argv}")
                Call bat.AppendLine("exit /b %errorlevel%")
            End If

            Return bat.ToString
        End Function

        Public Function Bash(program$, argv$, environment As IEnumerable(Of ValueTuple), folkNew As Boolean) As String
            Dim shell As New StringBuilder("#!/bin/bash")

            Call shell.AppendLine()
            Call shell.AppendLine()

            If Not environment.IsNullOrEmpty Then
                For Each param As ValueTuple In environment
                    ' To set it for current shell And all processes started from current shell
                    ' shorter, less portable version
                    ' export VARNAME="my value"      
                    Call shell.AppendLine($"export {param.Key}=""{param.Value}""")
                Next
            End If

            Call shell.AppendLine($"cd {App.CurrentDirectory.CLIPath}")
            Call shell.AppendLine($"""{program}"" {argv}")

            Return shell.ToString
        End Function
    End Module
End Namespace