#Region "Microsoft.VisualBasic::78cfef942b525fc299507f1f46786b88, vs_solutions\VBS\src\VBScript\IncludeDirective\IncludeSet.vb"

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

    '   Total Lines: 61
    '    Code Lines: 32 (52.46%)
    ' Comment Lines: 14 (22.95%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (24.59%)
    '     File Size: 2.46 KB


    '     Class IncludeSet
    ' 
    '         Function: MergeSearchRoots
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.NuGet

Namespace Script

    ''' <summary>
    ''' 一次脚本解析中全部 <c>#include</c> 的解析结果汇总。
    ''' </summary>
    Public Class IncludeSet

        ''' <summary>全部可用于编译引用与运行期依赖探测的程序集绝对路径(去重)</summary>
        Public ReadOnly Assemblies As New List(Of String)

        ''' <summary>被引入的其它脚本(按展开顺序)</summary>
        Public ReadOnly Scripts As New List(Of IncludedScript)

        ''' <summary>被引入的 nuget 包(根包与全部传递依赖)</summary>
        Public ReadOnly NuGetPackages As New List(Of NuGetPackage)

        ''' <summary>被引入脚本贡献的头部 Imports/Option 语句</summary>
        Public ReadOnly HeaderImports As New List(Of String)

        ''' <summary>被引入脚本贡献的类型定义块</summary>
        Public ReadOnly TypeBlocks As New List(Of String)

        ''' <summary>解析过程中的告警信息(未找到的目标等)</summary>
        Public ReadOnly Warnings As New List(Of String)

        ''' <summary>未被解析的目标原文</summary>
        Public ReadOnly Unresolved As New List(Of String)

        ''' <summary>
        ''' 在基础搜索目录之后追加全部 nuget 包的解压目录, 得到完整的路径搜索列表。
        ''' 魔法方法 <c>Locate()</c> 与工程转换共用该列表。
        ''' </summary>
        Public Function MergeSearchRoots(baseRoots As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim roots As New List(Of String)

            If baseRoots IsNot Nothing Then
                For Each dir As String In baseRoots
                    If Not String.IsNullOrEmpty(dir) Then
                        Call roots.Add(dir)
                    End If
                Next
            End If

            For Each package As NuGetPackage In NuGetPackages
                Dim folder As String = package.PackageFolder

                If String.IsNullOrEmpty(folder) Then
                    Continue For
                End If

                If Not roots.Any(Function(p) String.Equals(p, folder, StringComparison.OrdinalIgnoreCase)) Then
                    Call roots.Add(folder)
                End If
            Next

            Return roots
        End Function
    End Class
End Namespace

