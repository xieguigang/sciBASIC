#Region "Microsoft.VisualBasic::62b2a35b73b3dd4a1501c4431731dd3e, vs_solutions\VBS\src\VBScript\Magics.vb"

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

    '   Total Lines: 212
    '    Code Lines: 156 (73.58%)
    ' Comment Lines: 32 (15.09%)
    '    - Xml Docs: 71.88%
    ' 
    '   Blank Lines: 24 (11.32%)
    '     File Size: 10.26 KB


    '     Module Magics
    ' 
    '         Function: Build, Lines, StringArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Script

    ''' <summary>
    ''' 脚本引擎"魔法方法"源码生成器。
    ''' </summary>
    ''' <remarks>
    ''' 这些方法只有在脚本引擎的预处理阶段才能被注入: 它们依赖脚本自身的上下文
    ''' (脚本文件路径、<c>#package</c> 等元数据指令、<c>#include</c> 依赖与搜索目录),
    ''' 由引擎在解析期把这些信息以常量/字面量的形式烘焙进生成代码, 之后统一放入
    ''' <c>DynamicDll.VBScriptHostMagics</c> 模块之中供脚本直接调用, 脚本无需任何 import。
    ''' </remarks>
    Public Module Magics

        ''' <summary>
        ''' 生成全部魔法方法的源代码片段(每个元素为一段可以整体输出的多行代码)。
        ''' </summary>
        ''' <param name="scriptFile">脚本文件的绝对路径</param>
        ''' <param name="metadata">从脚本头部指令解析得到的程序集元数据</param>
        ''' <param name="imports">
        ''' #include 所引用的全部程序集绝对路径 —— 包括本地 dll、nuget 包解析出的资产
        ''' 以及被引入脚本转发的依赖
        ''' </param>
        ''' <param name="searchRoots">
        ''' 与 #include 一致的相对路径搜索目录(按优先级排列), nuget 包的解压目录追加在末尾
        ''' </param>
        Public Function Build(scriptFile As String,
                              metadata As ScriptMetadata,
                              [imports] As IEnumerable(Of String),
                              searchRoots As IEnumerable(Of String)) As IEnumerable(Of String)

            Dim file As String = Path.GetFullPath(scriptFile)
            Dim dir As String = Path.GetDirectoryName(file)
            Dim pkg As String = If(metadata Is Nothing, "", metadata.Package)
            Dim author As String = If(metadata Is Nothing, "", metadata.Author)
            Dim title As String = If(metadata Is Nothing, "", metadata.Title)
            Dim ver As String = If(metadata Is Nothing, "", metadata.Version)
            Dim snippets As New List(Of String)

            ' =============================================================
            ' 脚本自身上下文: 常量与内部辅助函数
            ' =============================================================
            Call snippets.Add(Lines(
                "' ---- 脚本自身上下文(由脚本引擎在预处理阶段烘焙) ----",
                $"Public ReadOnly __magicScriptFile As String = {ScriptMetadata.Quote(file)}",
                $"Public ReadOnly __magicScriptDir As String = {ScriptMetadata.Quote(dir)}",
                "",
                "Private Function __magicResolve(relpath As String) As String",
                "    If String.IsNullOrEmpty(relpath) Then",
                "        Return __magicScriptDir",
                "    End If",
                "",
                "    If System.IO.Path.IsPathRooted(relpath) Then",
                "        Return System.IO.Path.GetFullPath(relpath)",
                "    End If",
                "",
                "    Return System.IO.Path.GetFullPath(System.IO.Path.Combine(__magicScriptDir, relpath))",
                "End Function"))

            Call snippets.Add(Lines(
                "' 将相对路径解析为相对于脚本文件所在文件夹的绝对路径",
                "Public Function Here(relpath As String) As String",
                "    Return __magicResolve(relpath)",
                "End Function"))

            Call snippets.Add(Lines(
                "' 脚本文件所在的文件夹",
                "Public Function ScriptDir() As String",
                "    Return __magicScriptDir",
                "End Function"))

            Call snippets.Add(Lines(
                "' 脚本文件的绝对路径",
                "Public Function ScriptFile() As String",
                "    Return __magicScriptFile",
                "End Function"))

            Call snippets.Add(Lines(
                "' 脚本文件名; withExtension=False 时返回不带扩展名的文件名",
                "Public Function ScriptName(Optional withExtension As Boolean = True) As String",
                "    If withExtension Then",
                "        Return System.IO.Path.GetFileName(__magicScriptFile)",
                "    End If",
                "",
                "    Return System.IO.Path.GetFileNameWithoutExtension(__magicScriptFile)",
                "End Function"))

            Call snippets.Add(Lines(
                "' 读取脚本自身的源代码文本",
                "Public Function ScriptText() As String",
                "    Return System.IO.File.ReadAllText(__magicScriptFile)",
                "End Function"))

            Call snippets.Add(Lines(
                "' 读取脚本自身的源代码, 按行返回",
                "Public Function ScriptLines() As String()",
                "    Return System.IO.File.ReadAllLines(__magicScriptFile)",
                "End Function"))

            Call snippets.Add(Lines(
                "' 脚本自身所编译得到的 assembly",
                "Public Function Self() As System.Reflection.Assembly",
                "    Return System.Reflection.Assembly.GetExecutingAssembly()",
                "End Function"))

            ' =============================================================
            ' 预处理指令元数据反射
            ' =============================================================
            Call snippets.Add(Lines(
                "' ---- 预处理指令元数据反射 ----",
                "' #package 指令的值(assembly 名称)",
                "Public Function Package() As String",
                $"    Return {ScriptMetadata.Quote(pkg)}",
                "End Function"))

            Call snippets.Add(Lines(
                "' #author 指令的值",
                "Public Function Author() As String",
                $"    Return {ScriptMetadata.Quote(author)}",
                "End Function"))

            Call snippets.Add(Lines(
                "' #title 指令的值",
                "Public Function Title() As String",
                $"    Return {ScriptMetadata.Quote(title)}",
                "End Function"))

            Call snippets.Add(Lines(
                "' #version 指令的值",
                "Public Function Version() As String",
                $"    Return {ScriptMetadata.Quote(ver)}",
                "End Function"))

            Call snippets.Add(Lines(
                "' 按名称读取元数据指令的值(package/author/title/version), 未知名称返回 Nothing",
                "Public Function Meta(key As String) As String",
                "    If String.IsNullOrEmpty(key) Then",
                "        Return Nothing",
                "    End If",
                "",
                "    Select Case key.Trim().ToLower()",
                "        Case ""package"" : Return Package()",
                "        Case ""author"" : Return Author()",
                "        Case ""title"" : Return Title()",
                "        Case ""version"" : Return Version()",
                "        Case Else : Return Nothing",
                "    End Select",
                "End Function"))

            ' =============================================================
            ' 依赖与路径定位
            ' =============================================================
            Call snippets.Add(Lines(
                "' ---- #include 依赖程序集列表 ----",
                "Public Function Includes() As String()",
                $"    Return {StringArray([imports])}",
                "End Function"))

            Call snippets.Add(Lines(
                "' ---- 按 #include 的搜索顺序定位文件 ----",
                "' 搜索顺序: 脚本目录, App.HOME, App.HOME/libs, App.HOME上级/libs",
                "Public Function Locate(name As String) As String",
                "    If String.IsNullOrEmpty(name) Then",
                "        Return Nothing",
                "    End If",
                "",
                "    If System.IO.Path.IsPathRooted(name) Then",
                "        If System.IO.File.Exists(name) Then",
                "            Return System.IO.Path.GetFullPath(name)",
                "        End If",
                "",
                "        Return Nothing",
                "    End If",
                "",
                $"    Dim __magicRoots As String() = {StringArray(searchRoots)}",
                "",
                "    For Each __magicDir As String In __magicRoots",
                "        Dim __magicFile As String = System.IO.Path.GetFullPath(System.IO.Path.Combine(__magicDir, name))",
                "",
                "        If System.IO.File.Exists(__magicFile) Then",
                "            Return __magicFile",
                "        End If",
                "    Next",
                "",
                "    Return Nothing",
                "End Function"))

            Return snippets
        End Function

        ''' <summary>把若干代码行拼接为一段可直接输出的代码</summary>
        Private Function Lines(ParamArray parts() As String) As String
            Return String.Join(Environment.NewLine, parts)
        End Function

        ''' <summary>把路径集合渲染为 VB 字符串数组字面量</summary>
        Private Function StringArray(items As IEnumerable(Of String)) As String
            Dim values As String() = items _
                .Where(Function(x) Not String.IsNullOrEmpty(x)) _
                .Distinct(StringComparer.OrdinalIgnoreCase) _
                .Select(Function(x) ScriptMetadata.Quote(x)) _
                .ToArray()

            If values.Length = 0 Then
                Return "New String() {}"
            End If

            Return "New String() {" & String.Join(", ", values) & "}"
        End Function
    End Module
End Namespace
