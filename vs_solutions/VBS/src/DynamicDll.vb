#Region "Microsoft.VisualBasic::d107dd63353241ef35ad290b6c5c24dd, vs_solutions\VBS\src\DynamicDll.vb"

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

    '   Total Lines: 187
    '    Code Lines: 106 (56.68%)
    ' Comment Lines: 44 (23.53%)
    '    - Xml Docs: 34.09%
    ' 
    '   Blank Lines: 37 (19.79%)
    '     File Size: 7.98 KB


    ' Module DynamicDll
    ' 
    '     Function: CompileScript, Run
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Emit
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports VBScriptHost.Script

Module DynamicDll

    ' Namespace DynamicDll
    '    Module Program
    '       Public Function Main(args As CommandLine) As Integer
    '          ' run vbs code 
    '       End Function
    '    End Module
    ' End Namespace

    ''' <summary>脚本代码的固定顶层命名空间</summary>
    Public Const NamespaceName As String = "DynamicDll"

    ''' <summary>脚本代码的固定容器类型(Module)名称</summary>
    Public Const ModuleName As String = "Program"

    ''' <summary>脚本入口函数的固定名称</summary>
    Public Const MainName As String = "Main"

    ' =========================================================================
    ' 函数2: 脚本代码内存编译
    ' =========================================================================

    ''' <summary>
    ''' 将解析后的脚本代码基于Roslyn在内存中编译为assembly(全程不落盘)
    ''' </summary>
    ''' <param name="script">ParseScript函数的返回结果</param>
    ''' <param name="asmName">目标assembly名称(默认依次取脚本 #package 指令、脚本文件名)</param>
    ''' <param name="extraRefs">额外的引用程序集路径</param>
    ''' <param name="debug">是否以debug模式编译</param>
    ''' 
    <Extension>
    Public Function CompileScript(script As ScriptParseResult,
                                  Optional asmName As String = Nothing,
                                  Optional extraRefs As IEnumerable(Of String) = Nothing,
                                  Optional debug As Boolean = False) As ScriptRuntime

        ' ---- Step1: 生成语法树 ----
        Dim parseOptions As New VisualBasicParseOptions(LanguageVersion.Latest)
        Dim trees As New List(Of SyntaxTree)

        Call trees.Add(VisualBasicSyntaxTree.ParseText(script.GeneratedCode, parseOptions))

        ' ---- Step2: 收集编译引用(按程序集简单名去重) ----
        Dim references As New List(Of MetadataReference)
        Dim added As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ' 已经加载到当前 AppDomain 的程序集(引擎依赖与共享框架程序集)。
        ' 当某个 #include(nuget 包中的资产尤其常见)指向的是同名程序集的另一个副本时,
        ' 优先复用已经加载的版本, 否则同一程序集的两个副本同时作为 MetadataReference
        ' 会造成重复类型定义的编译错误。
        Dim loaded As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            Dim location As String = asm.Location
            Dim simple As String = asm.GetName().Name

            If String.IsNullOrEmpty(location) OrElse String.IsNullOrEmpty(simple) Then
                Continue For
            End If

            If Not loaded.ContainsKey(simple) Then
                loaded(simple) = location
            End If
        Next

        Dim AddRef = Sub(dllPath As String)
                         If String.IsNullOrEmpty(dllPath) OrElse Not File.Exists(dllPath) Then
                             Exit Sub
                         End If

                         Dim target As String = dllPath
                         Dim simple As String = Path.GetFileNameWithoutExtension(target)
                         Dim loadedPath As String = Nothing

                         If loaded.TryGetValue(simple, loadedPath) Then
                             target = loadedPath
                             simple = Path.GetFileNameWithoutExtension(target)
                         End If

                         If added.Add(simple) Then
                             Call references.Add(MetadataReference.CreateFromFile(target))
                         End If
                     End Sub

        ' 2.1 #include 所引用的外部程序集(dll / nuget 资产 / 脚本引用转发的依赖)
        For Each dll As String In script.Imports
            If Not File.Exists(dll) Then
                Throw New FileNotFoundException($"#include所引用的程序集不存在: {dll}", dll)
            End If

            Call AddRef(dll)
        Next

        ' 2.2 引擎自身assembly(提供CommandLine类型定义)
        Call AddRef(GetType(DynamicDll).Assembly.Location)
        Call AddRef(GetType(CommandLine).Assembly.Location)
        ' 2.3 VB运行时(支持脚本使用VB内置函数)
        Call AddRef(GetType(Microsoft.VisualBasic.Strings).Assembly.Location)

        ' 2.4 额外引用
        If Not extraRefs Is Nothing Then
            For Each dll As String In extraRefs
                Call AddRef(dll)
            Next
        End If

        ' 2.5 当前AppDomain中所有已加载的BCL程序集
        For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            Call AddRef(asm.Location)
        Next

        ' ---- Step3: 编译选项 ----
        Dim options As New VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        options = options.WithOptimizationLevel(
            If(debug, OptimizationLevel.Debug, OptimizationLevel.Release))

        ' ---- Step4: 执行编译 ----
        ' assembly名称: 显式参数 > #package指令 > 脚本文件名
        Dim finalAsmName As String = asmName

        If String.IsNullOrEmpty(finalAsmName) AndAlso script.Metadata IsNot Nothing Then
            finalAsmName = script.Metadata.Package
        End If

        If String.IsNullOrEmpty(finalAsmName) Then
            finalAsmName = Path.GetFileNameWithoutExtension(script.ScriptFile)
        End If

        Dim compilation As VisualBasicCompilation = VisualBasicCompilation.Create(
            assemblyName:=finalAsmName,
            syntaxTrees:=trees,
            references:=references,
            options:=options)

        ' ---- Step5: 发射IL到内存流并加载 ----
        Using ms As New MemoryStream(), pdb As New MemoryStream()
            Dim result As EmitResult = compilation.Emit(ms, pdb)

            If Not result.Success Then
                Dim errors As String() = result.Diagnostics _
                    .Where(Function(d) d.Severity = DiagnosticSeverity.Error) _
                    .Select(Function(d) d.ToString()) _
                    .ToArray()

                Throw New InvalidOperationException("脚本代码编译失败!" & vbCrLf & String.Join(vbCrLf, errors))
            End If

            Call ms.Seek(0, SeekOrigin.Begin)

            Dim ctx As New ScriptLoadContext("script-" & Guid.NewGuid().ToString("N"), script.Imports)

            Return New ScriptRuntime(ctx, ctx.LoadFromStream(ms))
        End Using
    End Function

    ''' <summary>
    ''' Call function Main(args As CommandLine) As integer
    ''' </summary>
    ''' <param name="dynamicAsm"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function Run(dynamicAsm As Assembly, args As Object) As Integer
        ' 8. 通过反射获取目标类型和方法
        Dim targetType As Type = dynamicAsm.GetType($"{NameOf(DynamicDll)}.Program")
        Dim instance As Object = Nothing
        Dim methodInfo As MethodInfo = targetType.GetMethod("Main", BindingFlags.Public Or BindingFlags.Static)

        ' --- 调用方式 A：传统反射调用 ---
        Dim result As Object = methodInfo.Invoke(instance, New Object() {args})
        Dim i32 As Integer = CInt(result)

        Return i32
    End Function
End Module
