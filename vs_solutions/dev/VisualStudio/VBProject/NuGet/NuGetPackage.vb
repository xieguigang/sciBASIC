#Region "Microsoft.VisualBasic::3ebd541c1f73e169c0732f90c77464da, vs_solutions\dev\VisualStudio\VBProject\NuGet\NuGetPackage.vb"

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

    '   Total Lines: 124
    '    Code Lines: 62 (50.00%)
    ' Comment Lines: 33 (26.61%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 29 (23.39%)
    '     File Size: 4.81 KB


    '     Class NuGetDependency
    ' 
    '         Properties: Id, Range, TargetFramework
    ' 
    '         Function: ToString
    ' 
    '     Class NuGetDependencyGroup
    ' 
    '         Properties: Dependencies, TargetFramework
    ' 
    '     Class NuGetPackage
    ' 
    '         Properties: Assemblies, Dependencies, Id, IsRoot, NativeAssets
    '                     NuspecFile, PackageFolder, TargetFramework, Version
    ' 
    '         Function: ToString
    ' 
    '     Class NuGetResolveResult
    ' 
    '         Properties: Packages, Root
    ' 
    '         Function: AllAssemblies, AllNativeAssets, PackageFolders
    ' 
    '     Class NuGetException
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Linq

Namespace VBProj.NuGet

    ''' <summary>
    ''' 一条 nuget 依赖声明(来自目标包的 <c>.nuspec</c>)。
    ''' </summary>
    Public Class NuGetDependency

        ''' <summary>依赖包的 id</summary>
        Public Property Id As String

        ''' <summary>依赖的版本范围</summary>
        Public Property Range As VersionRange

        ''' <summary>声明该依赖的目标框架(空字符串表示兜底依赖组)</summary>
        Public Property TargetFramework As String

        Public Overrides Function ToString() As String
            Return $"{Id} {Range}"
        End Function
    End Class

    ''' <summary>
    ''' 一个 nuspec 的依赖组(<c>&lt;group targetFramework="..."&gt;</c>)。
    ''' </summary>
    Public Class NuGetDependencyGroup

        ''' <summary>目标框架 moniker; 空字符串表示 nuspec 中的兜底(无 targetFramework 属性)依赖组</summary>
        Public Property TargetFramework As String

        Public ReadOnly Property Dependencies As New List(Of NuGetDependency)
    End Class

    ''' <summary>
    ''' 一个已经落到本地缓存目录的 nuget 包: 包含解析出的依赖与资产(托管 dll / 原生资产目录)。
    ''' </summary>
    Public Class NuGetPackage

        ''' <summary>包 id</summary>
        Public Property Id As String

        ''' <summary>实际选定的版本</summary>
        Public Property Version As NuGetVersion

        ''' <summary>包解压后的目录(<c>~/.nuget/packages/&lt;id&gt;/&lt;version&gt;</c>)</summary>
        Public Property PackageFolder As String

        ''' <summary>包描述文件(.nuspec)路径</summary>
        Public Property NuspecFile As String

        ''' <summary>资产选择所依据的目标框架</summary>
        Public Property TargetFramework As String

        ''' <summary>是否为本次 <c>#include</c> 直接引用的根包</summary>
        Public Property IsRoot As Boolean

        ''' <summary>该包在当前目标框架下的托管程序集绝对路径</summary>
        Public ReadOnly Property Assemblies As New List(Of String)

        ''' <summary>该包在当前运行平台下的原生资产目录</summary>
        Public ReadOnly Property NativeAssets As New List(Of String)

        ''' <summary>该包在当前目标框架下的直接依赖</summary>
        Public ReadOnly Property Dependencies As New List(Of NuGetDependency)

        Public Overrides Function ToString() As String
            Return $"{Id}@{Version}"
        End Function
    End Class

    ''' <summary>
    ''' 一次 nuget 依赖解析的完整结果: 根包 + 全部传递依赖。
    ''' </summary>
    Public Class NuGetResolveResult

        ''' <summary><c>#include</c> 直接引用的根包</summary>
        Public Property Root As NuGetPackage

        ''' <summary>根包与其全部传递依赖(按 id 去重, 每个 id 只保留最终选定的版本)</summary>
        Public ReadOnly Property Packages As New List(Of NuGetPackage)

        ''' <summary>解析出的全部托管程序集(去重, 保持稳定顺序)</summary>
        Public Function AllAssemblies() As String()
            Return Packages _
                .SelectMany(Function(p) p.Assemblies) _
                .Distinct(StringComparer.OrdinalIgnoreCase) _
                .OrderBy(Function(path) path, StringComparer.OrdinalIgnoreCase) _
                .ToArray()
        End Function

        ''' <summary>解析出的全部原生资产目录(去重)</summary>
        Public Function AllNativeAssets() As String()
            Return Packages _
                .SelectMany(Function(p) p.NativeAssets) _
                .Distinct(StringComparer.OrdinalIgnoreCase) _
                .ToArray()
        End Function

        ''' <summary>全部包的解压目录(用于运行期依赖探测)</summary>
        Public Function PackageFolders() As String()
            Return Packages _
                .Select(Function(p) p.PackageFolder) _
                .Where(Function(dir) Not String.IsNullOrEmpty(dir)) _
                .Distinct(StringComparer.OrdinalIgnoreCase) _
                .ToArray()
        End Function
    End Class

    ''' <summary>
    ''' nuget 包解析过程中出现的可预期错误(包不存在、版本不存在、网络不可达等)。
    ''' </summary>
    Public Class NuGetException : Inherits Exception

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(message As String, innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
    End Class
End Namespace

