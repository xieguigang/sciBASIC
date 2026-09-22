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

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.NuGet
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

        ''' <summary>
        ''' 脚本头部预处理指令(#package/#author/#title/#version)所声明的程序集元数据
        ''' </summary>
        Public Property Metadata As ScriptMetadata

        ''' <summary>
        ''' #include所引用的外部程序集路径列表(统一为绝对路径)。
        ''' nuget 包解析出的资产与脚本引用转发的依赖也一并汇入本列表。
        ''' </summary>
        Public Property [Imports] As List(Of String)

        ''' <summary>被 #include 引入的其它脚本(递归展开后的有序列表)</summary>
        Public Property ScriptIncludes As List(Of IncludedScript)

        ''' <summary>#include 引入的 nuget 包及其解析出的资产(根包 + 全部传递依赖)</summary>
        Public Property NuGetPackages As List(Of NuGetPackage)

        ''' <summary>
        ''' 送入 Roslyn <c>MetadataReference</c> 与运行期 ALC 探测的全部 dll 绝对路径
        ''' (dll include + nuget 资产 + 脚本转发依赖), 等价于 <see cref="ScriptParseResult.Imports"/>。
        ''' </summary>
        Public ReadOnly Property ResolvedAssemblies As List(Of String)
            Get
                Return [Imports]
            End Get
        End Property

        ''' <summary>#include 解析过程中的告警信息(未解析的目标等)</summary>
        Public Property IncludeWarnings As List(Of String)

        ''' <summary>与 #include 一致的相对路径搜索目录(按优先级排列)</summary>
        Public Property SearchRoots As String()

        ''' <summary>
        ''' 本次解析是否启用了向量化改写(命令行 <c>--no-vectorize</c> 或脚本头部的
        ''' <c>#no-vectorize</c> 都会使其为 <c>False</c>)。
        ''' </summary>
        Public Property VectorizeEnabled As Boolean

        ''' <summary>
        ''' 预处理阶段是否**确实**发生了向量化改写。
        ''' 生成代码只有在为 <c>True</c> 时才需要注入 SIMD 的 Imports。
        ''' </summary>
        Public Property Vectorized As Boolean

        ''' <summary>
        ''' <c>@</c> 数组投影运算符被展开的次数(<c>@</c> 是语法糖, 与 <see cref="VectorizeEnabled"/> 无关)
        ''' </summary>
        Public Property Projections As Integer

        ''' <summary>
        ''' 文本预处理(移除 #include 行、展开 ?args / let / 元组分解 / 向量化)之后的脚本代码。
        ''' </summary>
        Public Property PreprocessedCode As String

        ''' <summary>重构之后的可以直接被Roslyn编译的完整VB.NET源代码</summary>
        Public Property GeneratedCode As String
    End Class
End Namespace
