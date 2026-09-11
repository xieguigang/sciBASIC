#Region "Microsoft.VisualBasic::afe8bcddee047599489e248c6732b957, vs_solutions\VBS\src\VBScript\VBScript.vb"

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

    '   Total Lines: 81
    '    Code Lines: 46 (56.79%)
    ' Comment Lines: 21 (25.93%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 14 (17.28%)
    '     File Size: 3.52 KB


    '     Module VBScript
    ' 
    '         Function: ParseScript, RefactorScript
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine

Namespace Script

    Module VBScript

        ' =========================================================================
        ' 函数1: 脚本源代码文件解析
        ' =========================================================================

        ''' <summary>
        ''' 对VB.NET脚本源代码文件进行解析处理
        ''' </summary>
        ''' <param name="scriptFile">.vb脚本源代码文件路径</param>
        Public Function ParseScript(scriptFile As String, Optional verbose As Boolean = False) As ScriptParseResult
            If Not File.Exists(scriptFile) Then
                Throw New FileNotFoundException("脚本源文件不存在: " & scriptFile, scriptFile)
            End If

            Dim source As String = scriptFile.ReadAllText
            Dim baseDir As String = Path.GetDirectoryName(Path.GetFullPath(scriptFile))

            ' ---- Step1: 解析 #include 元数据, 得到引用的外部程序集文件路径 ----
            Dim [imports] As New List(Of String)
            Dim root0 As String = App.HOME
            Dim root1 As String = App.HOME & "/libs"             ' bin/libs/
            Dim root2 As String = App.HOME.ParentPath & "/libs"  ' ./bin/ ./libs/

            For Each m As Match In Regex.Matches(source, "#include\s+""(?<dll>[^""]+)""", RegexOptions.IgnoreCase)
                Dim dll As String = m.Groups("dll").Value

                ' 相对路径统一解析为相对于脚本文件所在文件夹的绝对路径
                If Path.IsPathRooted(dll) Then
                    Call [imports].Add(dll)
                Else
                    For Each dir As String In {baseDir, root0, root1, root2}
                        Dim dllfile As String = Path.GetFullPath(Path.Combine(dir, dll))

                        If dllfile.FileExists Then
                            Call [imports].Add(dllfile)
                            Exit For
                        End If
                    Next
                End If
            Next

            ' ---- Step2: 代码结构重构 ----
            Dim code As String = RefactorScript(source)

            If verbose Then
                Call Console.WriteLine("----- generated code -----")
                Call Console.WriteLine(code)
            End If

            Return New ScriptParseResult With {
                .ScriptFile = scriptFile,
                .CommandLine = Nothing,
                .Imports = [imports],
                .GeneratedCode = code
            }
        End Function

        ' =========================================================================
        ' 函数2: 脚本代码结构重构
        ' =========================================================================

        ''' <summary>
        ''' 对脚本源代码进行重构处理, 生成最终的完整可编译代码。
        ''' </summary>
        ''' <remarks>
        ''' 重构的具体过程(文本预处理, 逐行块扫描, 顶层函数落位, 代码组装)由
        ''' <see cref="ScriptRefactor"/> 负责, 这里只是一个薄封装。
        ''' </remarks>
        ''' <param name="source">脚本源代码文本</param>
        Private Function RefactorScript(source As String) As String
            Return New ScriptRefactor().Refactor(source)
        End Function
    End Module
End Namespace

