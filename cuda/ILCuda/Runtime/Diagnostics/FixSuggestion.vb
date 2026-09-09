#Region "Microsoft.VisualBasic::5a2ea8ca5b4a3a8e6221590b9ed31602, cuda\ILCuda\Runtime\Diagnostics\FixSuggestion.vb"

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

    '   Total Lines: 55
    '    Code Lines: 35 (63.64%)
    ' Comment Lines: 14 (25.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (10.91%)
    '     File Size: 2.07 KB


    '     Enum FixKind
    ' 
    '         EnvironmentError, InstallMatchingToolkit, NoDevice, NoKernelSource, OfflineCompileCubin
    '         SpecifyNvrtc, UpgradeDriver
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class FixSuggestion
    ' 
    '         Properties: Commands, Detail, HasCommands, Kind, Title
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>修复建议的分类</summary>
    Public Enum FixKind
        ''' <summary>环境探测本身失败（驱动不可用等）</summary>
        EnvironmentError
        ''' <summary>没有可用的 CUDA 设备</summary>
        NoDevice
        ''' <summary>没有任何参与编译的内核源码</summary>
        NoKernelSource
        ''' <summary>缺少 NVRTC 或版本与驱动不匹配</summary>
        InstallMatchingToolkit
        ''' <summary>需要升级显卡驱动</summary>
        UpgradeDriver
        ''' <summary>改用 nvcc 离线编译 cubin</summary>
        OfflineCompileCubin
        ''' <summary>显式指定 nvrtc64_*.dll 路径</summary>
        SpecifyNvrtc
    End Enum

    ''' <summary>
    ''' 一条可操作的修复建议（纯数据，由调用方决定如何呈现）
    ''' </summary>
    Public Class FixSuggestion
        Public Property Kind As FixKind
        ''' <summary>一句话标题</summary>
        Public Property Title As String
        ''' <summary>详细说明</summary>
        Public Property Detail As String
        ''' <summary>可以直接执行的命令 / 操作步骤</summary>
        Public Property Commands As IReadOnlyList(Of String)

        Public Sub New()
            Commands = Array.Empty(Of String)()
        End Sub

        Public Sub New(kind As FixKind, title As String, detail As String,
                       Optional commands As IEnumerable(Of String) = Nothing)
            Me.Kind = kind
            Me.Title = title
            Me.Detail = detail
            Me.Commands = If(commands Is Nothing, Array.Empty(Of String)(), commands.ToList())
        End Sub

        Public ReadOnly Property HasCommands As Boolean
            Get
                Return Commands IsNot Nothing AndAlso Commands.Count > 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Kind}] {Title}"
        End Function
    End Class
End Namespace

