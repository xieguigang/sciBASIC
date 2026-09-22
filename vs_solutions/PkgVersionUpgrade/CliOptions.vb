#Region "Microsoft.VisualBasic::b347080c5f3019b22cf261edea978dfd, vs_solutions\PkgVersionUpgrade\CliOptions.vb"

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

    '   Total Lines: 26
    '    Code Lines: 12 (46.15%)
    ' Comment Lines: 13 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (3.85%)
    '     File Size: 1.72 KB


    ' Class CliOptions
    ' 
    '     Properties: [Error], DryRun, FixOutputPath, MakeClean, NamespacePrefix
    '                 OutputDir, ShowHelp, Slnx, Version
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection

''' <summary>命令行参数解析结果</summary>
Public Class CliOptions
    ''' <summary>用户显式指定的 nuget 程序包版本号，未指定时为空</summary>
    <Opt("-v", "--version")> Public Property Version As String
    ''' <summary>待处理的 slnx 解决方案文件路径。工具会解析该文件并枚举其中的 vbproj。</summary>
    <Opt("-s", "--slnx")> Public Property Slnx As String
    ''' <summary>命名空间前缀，用于过滤 slnx 中枚举到的 vbproj（按 RootNamespace 大小写不敏感匹配）。</summary>
    <Opt("-p", "--prefix", "--namespace")> Public Property NamespacePrefix As String
    ''' <summary>只打印将要发生的改动，不写盘</summary>
    <Opt("-n", "--dry-run")> Public Property DryRun As Boolean
    ''' <summary>只更新版本号，跳过过时编译配置的清理</summary>
    <Opt("--clean")> Public Property MakeClean As Boolean = False
    ''' <summary>是否修正 nuget_release|x64 配置的产物输出路径。开启时 --output 必填。</summary>
    <Opt("--fix-output-path")> Public Property FixOutputPath As Boolean
    ''' <summary>
    ''' 编译产物输出文件夹，仅当 --fix-output-path 开启时必填。
    ''' 会被展开为绝对路径并化为相对于每个目标 vbproj 的相对路径写进 OutputPath。
    ''' </summary>
    <Opt("-o", "--output")> Public Property OutputDir As String
    ''' <summary>是否请求打印用法说明</summary>
    <Opt("-h", "--help", "/?", "-?")> Public Property ShowHelp As Boolean
    ''' <summary>解析过程中出现的错误描述</summary>
    Public Property [Error] As String
End Class
