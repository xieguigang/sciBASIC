#Region "Microsoft.VisualBasic::c6d6d0391b4f399b32cfd0534d04de3d, Microsoft.VisualBasic.Core\src\My\Framework\DoConfiguration.vb"

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

    '   Total Lines: 122
    '    Code Lines: 75 (61.48%)
    ' Comment Lines: 30 (24.59%)
    '    - Xml Docs: 40.00%
    ' 
    '   Blank Lines: 17 (13.93%)
    '     File Size: 4.80 KB


    '     Module DoConfiguration
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ConfigMemory, InternalPlatformID
    ' 
    '         Sub: ConfigFrameworkRuntime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports CLI = Microsoft.VisualBasic.CommandLine.CommandLine

Namespace My.FrameworkInternal

    ''' <summary>
    ''' Do configuration
    ''' </summary>
    Public Module DoConfiguration

        ''' <summary>
        ''' --unix
        ''' </summary>
        Public ReadOnly unix_debug_flag As Boolean

        ''' <summary>
        ''' --verbose
        ''' </summary>
        Public ReadOnly verbose_flag As Boolean

        ''' <summary>
        ''' should be vanilla code
        ''' </summary>
        Sub New()
            Dim args As String() = Environment.GetCommandLineArgs

            If Not args Is Nothing Then
                For Each s As String In args.Select(Function(si) si.ToLower)
                    Select Case s
                        Case "--unix" : unix_debug_flag = True
                        Case "--verbose" : verbose_flag = True
                    End Select
                Next
            End If
        End Sub

        Friend Function InternalPlatformID() As PlatformID
            If unix_debug_flag Then
                Return PlatformID.Unix
            Else
                Return Environment.OSVersion.Platform
            End If
        End Function

        <Extension>
        Friend Sub ConfigFrameworkRuntime(configuration As Config, args As CLI)
            Dim envir As Dictionary(Of String, String) = args.EnvironmentVariables
            Dim disableLoadOptions As Boolean = args.IsTrue("--load_options.disable")
            Dim max_stack_size As String = args.Tokens _
                .SafeQuery _
                .Where(Function(t) Strings.LCase(t).StartsWith("/stack:")) _
                .FirstOrDefault
            Dim name$

            ' initial before call app module
            'If args.GetBoolean("--unix") OrElse args.Name.TextEquals("--unix") Then
            '    unix_debug_flag = True
            'End If

            ' call app module later
            If Not max_stack_size.StringEmpty Then
                Call App.JoinVariable("max_stack_size", max_stack_size.Split(":"c).Last)
            End If

            ' load config from config file.
            For Each config In configuration.environment.SafeQuery
                ' 在加载配置文件的时候需要注意一下
                ' 用户从命令行中配置的变量的优先级应该要高于配置文件中加载的默认变量值
                ' 所以在这个for循环中
                ' 需要首先检查一下配置项是否存在
                ' 不可以直接赋值
                If App.GetVariable(config.Key) Is Nothing Then
                    ' 只写入未存在的变量就可以了
                    ' 已存在的变量是在App模块自动初始化加载的
                    Call App.JoinVariable(config.Key, config.Value)
                End If
            Next

            ' --load_options.disable 开关将会禁止所有的环境项目的设置
            ' 但是环境变量任然会进行加载设置
            If Not disableLoadOptions AndAlso Not envir.IsNullOrEmpty Then
                If envir.ContainsKey("proxy") Then
                    ' /@set proxy=http://127.0.0.1:10809
                    WebServiceUtils.Proxy = envir("proxy")
                    Call $"[Config] webUtils_proxy={WebServiceUtils.Proxy}".info
                End If
                If envir.ContainsKey("setwd") Then
                    App.CurrentDirectory = envir("setwd")
                    Call $"[Config] current_work_directory={App.CurrentDirectory}".info
                End If
                If envir.ContainsKey("buffer_size") Then
                    Call App.SetBufferSize(envir!buffer_size)
                End If
            End If

            ' config value from commandline will overrides the config value that loaded 
            ' from the config json file.

            ' /@var=name "value"
            For Each var As NamedValue(Of String) In args.ParameterList
                With var
                    If InStr(.Name, "/@var=", CompareMethod.Text) = 1 Then
                        name = .Name _
                               .GetTagValue("=") _
                               .Value

                        Call App.JoinVariable(name, .Value)
                    End If
                End With
            Next
        End Sub

        Public Function ConfigMemory(Optional load As MemoryLoads? = Nothing) As MemoryLoads
            If Not load Is Nothing Then
                App.m_memoryLoad = load
            End If

            Return App.MemoryLoad
        End Function
    End Module
End Namespace
