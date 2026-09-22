#Region "Microsoft.VisualBasic::364376ce9e69a8404e526421aa9b5e88, vs_solutions\VBS\src\VBScript\IncludeDirective\IncludeKind.vb"

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

    '   Total Lines: 22
    '    Code Lines: 9 (40.91%)
    ' Comment Lines: 7 (31.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (27.27%)
    '     File Size: 720 B


    '     Enum IncludeKind
    ' 
    '         [Assembly], NuGet, Script, Unresolved
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.NuGet

Namespace Script

    ''' <summary>
    ''' <c>#include</c> 指令所引用的目标类型。
    ''' </summary>
    Public Enum IncludeKind

        ''' <summary>外部 CLR 程序集(.dll)</summary>
        [Assembly]

        ''' <summary>其它 VB.NET 脚本(.vb) —— 其代码会被复制到主脚本的顶层命名空间</summary>
        Script

        ''' <summary>nuget 程序包(由 <see cref="NuGetResolver"/> 解析并缓存到本地)</summary>
        NuGet

        ''' <summary>无法解析的目标(保持历史宽松语义: 仅告警)</summary>
        Unresolved
    End Enum
End Namespace
