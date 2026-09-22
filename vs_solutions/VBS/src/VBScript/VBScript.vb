#Region "Microsoft.VisualBasic::5527fb4b67413a41604d4816255a3148, vs_solutions\VBS\src\VBScript\VBScript.vb"

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

    '   Total Lines: 155
    '    Code Lines: 109 (70.32%)
    ' Comment Lines: 18 (11.61%)
    '    - Xml Docs: 61.11%
    ' 
    '   Blank Lines: 28 (18.06%)
    '     File Size: 7.61 KB


    '     Module VBScript
    ' 
    '         Function: ParseScript
    ' 
    '         Sub: PrintDefaultParameters, PrintVectorization
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Script

    Module VBScript

        ' =========================================================================
        ' 函数1: 脚本源代码文件解析
        ' =========================================================================

        ''' <summary>
        ''' 对VB.NET脚本源代码文件进行解析处理
        ''' </summary>
        ''' <param name="scriptFile">.vb脚本源代码文件路径</param>
        ''' <param name="verbose">是否输出 #include 解析与代码重构的细节</param>
        ''' <param name="vectorize">
        ''' 是否启用向量化改写(命令行 <c>--no-vectorize</c> 传入 False);
        ''' 脚本头部的 <c>#no-vectorize</c> 指令优先级更高
        ''' </param>
        Public Function ParseScript(scriptFile As String,
                                    Optional verbose As Boolean = False,
                                    Optional vectorize As Boolean = True) As ScriptParseResult
            If Not File.Exists(scriptFile) Then
                Throw New FileNotFoundException("脚本源文件不存在: " & scriptFile, scriptFile)
            End If

            Dim fullScript As String = Path.GetFullPath(scriptFile)
            Dim source As String = fullScript.ReadAllText
            Dim baseDir As String = Path.GetDirectoryName(fullScript)

            If verbose Then
                Call Console.WriteLine("----- #include resolve -----")
            End If

            ' ---- Step1: 解析 #include 指令, 得到程序集 / 其它脚本 / nuget 包三类引用 ----
            Dim root0 As String = App.HOME
            Dim root1 As String = App.HOME & "/libs"             ' bin/libs/
            Dim root2 As String = App.HOME.ParentPath & "/libs"  ' ./bin/ ./libs/
            Dim searchRoots As String() = {baseDir, root0, root1, root2}

            Dim resolver As New IncludeResolver(searchRoots, verbose)
            Dim includes As IncludeSet = resolver.Resolve(source, fullScript)

            For Each warning As String In includes.Warnings
                Call Console.WriteLine("WARNING: " & warning)
            Next

            ' ---- Step2: 解析程序集元数据指令(#package/#author/#title/#version) ----
            Dim metadata As ScriptMetadata = ScriptMetadata.Parse(source)

            ' ---- Step3: 代码结构重构 ----
            ' 魔法方法与 #include 采用一致的相对路径搜索顺序(nuget 包目录追加在末尾)
            Dim magicRoots As String() = includes.MergeSearchRoots(searchRoots).ToArray()
            Dim magicSnippets As IEnumerable(Of String) = Magics.Build(fullScript, metadata, includes.Assemblies, magicRoots)
            Dim vectorReport As New VectorizationReport()
            Dim defaultReport As New DefaultParameterReport()
            Dim preprocessed As String = ScriptRefactor.PreprocessText(source,
                                                                      vectorize:=vectorize,
                                                                      report:=vectorReport,
                                                                      extraTypeBlocks:=includes.TypeBlocks,
                                                                      defaultReport:=defaultReport)
            Dim hasVectorCode As Boolean = vectorReport.Rewritten > 0
            Dim code As String = New ScriptRefactor(metadata, magicSnippets, includes) _
                .RefactorPreprocessed(preprocessed, withSimd:=hasVectorCode)

            If verbose Then
                Call PrintVectorization(vectorReport, vectorize)
                Call PrintDefaultParameters(defaultReport)
                Call Console.WriteLine("----- generated code -----")
                Call Console.WriteLine(code)
            End If

            Return New ScriptParseResult With {
                .ScriptFile = fullScript,
                .CommandLine = Nothing,
                .Metadata = metadata,
                .Imports = New List(Of String)(includes.Assemblies),
                .ScriptIncludes = includes.Scripts,
                .NuGetPackages = includes.NuGetPackages,
                .IncludeWarnings = includes.Warnings,
                .SearchRoots = magicRoots,
                .VectorizeEnabled = vectorize,
                .Vectorized = hasVectorCode,
                .Projections = vectorReport.Projections,
                .PreprocessedCode = preprocessed,
                .GeneratedCode = code
            }
        End Function

        ''' <summary>在 verbose 模式下输出向量化改写统计与未能改写的可疑行</summary>
        Private Sub PrintVectorization(report As VectorizationReport, enabled As Boolean)
            Dim vectors As String() = report.Vectors.Distinct().ToArray()

            If enabled Then
                Call Console.WriteLine($"----- vectorization: {report.Rewritten} 处 SIMD 改写, " &
                                       $"{report.Projections} 处 @ 投影, {vectors.Length} 个向量变量 -----")
            Else
                Call Console.WriteLine("----- vectorization: disabled " &
                                       $"(@ 投影仍然生效, {report.Projections} 处) -----")
            End If

            If vectors.Length > 0 Then
                Call Console.WriteLine("    vectors: " & String.Join(", ", vectors))
            End If

            Dim objectTypes As String() = report.ObjectTypes.Distinct().ToArray()

            If objectTypes.Length > 0 Then
                Call Console.WriteLine("    object types: " & String.Join(", ", objectTypes))
            End If

            If report.Skipped.Count > 0 Then
                Call Console.WriteLine("    skipped:")

                For Each line As String In report.Skipped.Distinct()
                    Call Console.WriteLine("      ! " & line)
                Next
            End If
        End Sub

        ''' <summary>在 verbose 模式下输出默认参数表达式的改写统计</summary>
        Private Sub PrintDefaultParameters(report As DefaultParameterReport)
            If report.Rewritten = 0 AndAlso report.Skipped.Count = 0 Then
                Return
            End If

            Call Console.WriteLine($"----- default parameters: {report.Rewritten} 处调用点改写, " &
                                   $"{report.Bridges.Count} 个桥接函数, {report.InlineSubs.Count} 个 Sub 就地展开 -----")

            If report.InlineSubs.Count > 0 Then
                Call Console.WriteLine("    inline subs: " & String.Join(", ", report.InlineSubs.Distinct()))
            End If

            If report.Bridges.Count > 0 Then
                Call Console.WriteLine("    bridges: " & String.Join(", ", report.Bridges.Distinct()))
            End If

            If report.Required.Count > 0 Then
                Call Console.WriteLine("    required (存在无法覆盖的调用点, 已退化为必填):")

                For Each name As String In report.Required.Distinct()
                    Call Console.WriteLine("      ! " & name)
                Next
            End If

            If report.Skipped.Count > 0 Then
                Call Console.WriteLine("    skipped:")

                For Each name As String In report.Skipped.Distinct()
                    Call Console.WriteLine("      ! " & name)
                Next
            End If
        End Sub
    End Module
End Namespace
