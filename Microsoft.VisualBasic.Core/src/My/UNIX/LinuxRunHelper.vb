#Region "Microsoft.VisualBasic::5c50d7f3ca5aeab5758a7db1516de394, Microsoft.VisualBasic.Core\src\My\UNIX\LinuxRunHelper.vb"

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

'     Module LinuxRunHelper
' 
'         Function: BashRun, BashShell, GetLocationHelper, getRunnerBash, MonoRun
'                   Shell
' 
' 
' /********************************************************************************/

#End Region

#If netcore5 = 1 Then
Imports System.Buffers
Imports Microsoft.VisualBasic.CommandLine.Reflection
#End If

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Text

Namespace My.UNIX

    ''' <summary>
    ''' mono shortcuts
    ''' </summary>
    Public Module LinuxRunHelper

        Public Function GetLocationHelper() As String
            Return Encodings.UTF8WithoutBOM.CodePage.GetString(My.Resources.bashRunner)
        End Function

        Private Function getRunnerBash() As Byte()
#If netcore5 = 1 Then
            Return My.Resources.runNet5
#Else
            Return My.Resources.runMono
#End If
        End Function

        ''' <summary>
        ''' Run from bash shell
        ''' </summary>
        ''' <returns></returns>
        Public Function BashRun() As String
            Dim utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage
            Dim appName As String = App.AssemblyName
            Dim locationHelper As String = utf8.GetString(My.Resources.bashRunner)
            Dim bash As String = utf8.GetString(getRunnerBash) _
                .Replace("{appName}", appName) _
                .LineTokens _
                .JoinBy(ASCII.LF)

            Return locationHelper & vbLf & bash
        End Function

        ''' <summary>
        ''' 这里比perl脚本掉调用有一个缺点，在运行前还需要使用命令修改为可执行权限
        ''' 
        ''' ```
        ''' 'sudo chmod 777 cmd.sh'
        ''' ```
        ''' </summary>
        ''' <returns></returns>
        Public Function BashShell() As Integer
            Dim path As String = App.ExecutablePath.TrimSuffix
            Dim bash As String = BashRun().LineTokens.JoinBy(ASCII.LF)
            Dim dir As String = path.ParentPath
            Dim bashfile As String = dir & "/help"

            Console.WriteLine("Bash script save at:")
            Console.WriteLine(path)

            ' 在这里写入的bash脚本都是没有文件拓展名的
            '
            ' 同时写入man命令帮助脚本
            Call My.Resources.help.FlushStream(bashfile)
            ' bash run script of current application
            Call BashRun _
                .LineTokens _
                .JoinBy(ASCII.LF) _
                .SaveTo(path, Encodings.UTF8WithoutBOM.CodePage)

            Return 0
        End Function

        Public Function MonoRun(app As String, CLI As String) As ProcessEx
            Dim proc As New ProcessEx With {
                .Bin = "mono",
                .CLIArguments = app.GetFullPath.CLIPath & " " & CLI
            }
            Return proc
        End Function

        ''' <summary>
        ''' Run linux command via ``/bin/bash``
        ''' </summary>
        ''' <param name="command"></param>
        ''' <param name="args"></param>
        ''' <param name="verbose"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <see cref="CommandLine.Call"/>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Shell(command As String, args As String, Optional verbose As Boolean = False, Optional stdin$ = Nothing) As String
            If command = "docker" Or command = "/usr/bin/docker" Then
                Dim bash As New StringBuilder("#!/bin/bash" & vbLf)
                Dim script As String = $"/tmp/{App.GetNextUniqueName("docker_")}_{App.PID.ToHexString}.sh"
                bash.AppendLine()
                bash.AppendLine($"{command} {args}")
                bash.SaveTo(script)

                'Call Console.WriteLine(bash.ToString)
                'Call Console.WriteLine(script)

                Return CommandLine.Call(script)
            Else
                Dim cmdl As String

                If args.StringEmpty Then
                    cmdl = command
                Else
                    cmdl = $"{command} {args}"
                End If

                If verbose Then
                    Call Console.WriteLine("run commandline:")
                    Call Console.WriteLine($"/bin/bash -c ""{cmdl}""")
                End If

                Return CommandLine.Call("/bin/bash", $"-c ""{cmdl}""", [in]:=stdin)
            End If
        End Function
    End Module
End Namespace
