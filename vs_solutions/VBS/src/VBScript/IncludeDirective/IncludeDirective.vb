#Region "Microsoft.VisualBasic::2db15e09e03f60733c6ae7ba8d010dc9, vs_solutions\VBS\src\VBScript\IncludeDirective\IncludeDirective.vb"

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

    '   Total Lines: 35
    '    Code Lines: 18 (51.43%)
    ' Comment Lines: 9 (25.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (22.86%)
    '     File Size: 1.19 KB


    '     Class IncludeDirective
    ' 
    '         Properties: DeclaredIn, Kind, PackageId, Raw, ResolvedPath
    '                     Version
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Script

    ''' <summary>
    ''' 一条 <c>#include</c> 预编译指令。
    ''' </summary>
    Public Class IncludeDirective

        ''' <summary>指令原文(引号内的目标文本)</summary>
        Public Property Raw As String

        ''' <summary>本地文件绝对路径(仅 Assembly / Script)</summary>
        Public Property ResolvedPath As String

        ''' <summary>引用目标类型</summary>
        Public Property Kind As IncludeKind

        ''' <summary>nuget 包 id(仅 NuGet)</summary>
        Public Property PackageId As String

        ''' <summary>nuget 版本或版本范围(仅 NuGet); Nothing 表示取最新稳定版</summary>
        Public Property Version As String

        ''' <summary>声明该指令的脚本文件绝对路径</summary>
        Public Property DeclaredIn As String

        Public Overrides Function ToString() As String
            Select Case Kind
                Case IncludeKind.NuGet
                    Return $"{PackageId}@{Version}"
                Case Else
                    Return If(ResolvedPath, Raw)
            End Select
        End Function
    End Class
End Namespace
