#Region "Microsoft.VisualBasic::7e64711257ee115713a4cc6daa5b7478, cuda\ILCuda\Runtime\CudaEngine\EngineOptions.vb"

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

    '   Total Lines: 18
    '    Code Lines: 10 (55.56%)
    ' Comment Lines: 7 (38.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (5.56%)
    '     File Size: 926 B


    '     Class EngineOptions
    ' 
    '         Properties: DeviceOrdinal, Diagnostics, ErrorMessage, ForceImage, ImagePath
    '                     NvrtcPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>引擎创建参数</summary>
    Public Class EngineOptions
        ''' <summary>使用第几块 GPU（默认 0）</summary>
        Public Property DeviceOrdinal As Integer = 0
        ''' <summary>--nvrtc 显式指定的 NVRTC 动态库路径</summary>
        Public Property NvrtcPath As String
        ''' <summary>--cubin 显式指定的预编译镜像路径</summary>
        Public Property ImagePath As String
        ''' <summary>--force-image：跳过镜像与驱动版本的兼容性预检（可能导致驱动崩溃，仅供调试）</summary>
        Public Property ForceImage As Boolean = False
        ''' <summary>内核获取过程的诊断日志</summary>
        Public Property Diagnostics As List(Of String) = New List(Of String)()
        ''' <summary>失败原因</summary>
        Public Property ErrorMessage As String
    End Class
End Namespace
