#Region "Microsoft.VisualBasic::54f255fd3b5c0536cf00264b0806e45f, vs_solutions\VBS\src\VBScript\ScriptParseResult.vb"

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
    '     File Size: 763 B


    '     Class ScriptParseResult
    ' 
    '         Properties: [Imports], CommandLine, GeneratedCode, ScriptFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine

Namespace Script

    ''' <summary>
    ''' 脚本文件的解析结果
    ''' </summary>
    Public Class ScriptParseResult

        ''' <summary>被解析的脚本源代码文件路径</summary>
        Public Property ScriptFile As String

        ''' <summary>解析得到的虚拟命令行参数对象</summary>
        Public Property CommandLine As CommandLine

        ''' <summary>#include所引用的外部程序集路径列表(统一为绝对路径)</summary>
        Public Property [Imports] As List(Of String)

        ''' <summary>重构之后的可以直接被Roslyn编译的完整VB.NET源代码</summary>
        Public Property GeneratedCode As String
    End Class
End Namespace
