#Region "Microsoft.VisualBasic::e99753ec5166b18cdaceb4e0fe015449, vs_solutions\dev\ApplicationServices\CodeDOM\CodeDOM.vb"

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

    '     Module CodeDOMExtension
    ' 
    '         Properties: DllProfile, DotNETFramework, ExecutableProfile
    ' 
    '         Function: (+4 Overloads) Compile, CompileDll, CompileExe, GenerateCode, GetDebugInformation
    '                   Icon, ImportsNamespace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.Collections.Specialized
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Emit.CodeDOM_VBC

    <Extension> Public Module CodeDOMExtension

        ''' <summary>
        ''' ```
        ''' C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\
        ''' ```
        ''' </summary>
        Public Const net46Default As String = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\"

        ''' <summary>
        ''' 设置所编译的应用程序的图标
        ''' </summary>
        ''' <param name="iconPath"></param>
        ''' <returns></returns>
        Public Function Icon(iconPath As String) As String
            Return $"/target:winexe /win32icon:""{iconPath}"""
        End Function

        <Extension> Public Function ImportsNamespace(refList As IEnumerable(Of String)) As CodeDom.CodeNamespaceImport()
            Dim nsArray As New List(Of CodeNamespaceImport) From {
                New CodeDom.CodeNamespaceImport("Microsoft.VisualBasic"),
                New CodeDom.CodeNamespaceImport("System"),
                New CodeDom.CodeNamespaceImport("System.Collections"),
                New CodeDom.CodeNamespaceImport("System.Collections.Generic"),
                New CodeDom.CodeNamespaceImport("System.Data"),
                New CodeDom.CodeNamespaceImport("System.Diagnostics"),
                New CodeDom.CodeNamespaceImport("System.Linq"),
                New CodeDom.CodeNamespaceImport("System.Xml.Linq"),
                New CodeDom.CodeNamespaceImport("System.Text.RegularExpressions")
            }
            Call nsArray.Add(refList.Select(Function(ns) New CodeNamespaceImport(ns)).ToArray)
            Return nsArray.ToArray
        End Function

        ''' <summary>
        ''' Generate the source code from the CodeDOM object model.(根据对象模型生成源代码以方便调试程序)
        ''' </summary>
        ''' <param name="NameSpace"></param>
        ''' <param name="CodeStyle">VisualBasic, C#</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' You can easily convert the source code between VisualBasic and C# using this function just by makes change in statement: 
        ''' CodeDomProvider.GetCompilerInfo("VisualBasic").CreateProvider().GenerateCodeFromNamespace([NameSpace], sWriter, Options)
        ''' Modify the VisualBasic in to C#
        ''' </remarks>
        <Extension> Public Function GenerateCode([NameSpace] As CodeNamespace, Optional CodeStyle As String = "VisualBasic") As String
            Dim code As New StringBuilder()

            Using sWriter As New StringWriter(code)
                Dim Options As New CodeGeneratorOptions() With {
                    .IndentString = "  ",
                    .ElseOnClosing = True,
                    .BlankLinesBetweenMembers = True
                }
                CodeDomProvider.GetCompilerInfo(CodeStyle) _
                    .CreateProvider() _
                    .GenerateCodeFromNamespace([NameSpace], sWriter, Options)

                Return code.ToString()
            End Using
        End Function

        ''' <summary>
        ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
        ''' </summary>
        ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
        ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
        ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
        ''' <param name="CodeStyle">VisualBasic, C#</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Compile(ObjectModel As CodeNamespace,
                                            Reference As String(),
                                            DotNETReferenceAssembliesDir As String,
                                            Optional CodeStyle As String = "VisualBasic") As Assembly
            Dim assembly As New CodeCompileUnit
            Call assembly.Namespaces.Add(ObjectModel)
            Return Compile(assembly, Reference, DotNETReferenceAssembliesDir, CodeStyle)
        End Function

        ''' <summary>
        ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
        ''' </summary>
        ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
        ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
        ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
        ''' <param name="CodeStyle">VisualBasic, C#</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Compile(ObjectModel As CodeNamespace,
                                            Reference As String(),
                                            DotNETReferenceAssembliesDir As String,
                                            Options As CompilerParameters,
                                            Optional CodeStyle As String = "VisualBasic") As Assembly
            With New CodeCompileUnit
                Call .Namespaces.Add(ObjectModel)
                Return .Compile(Reference, DotNETReferenceAssembliesDir, Options, CodeStyle)
            End With
        End Function

        ''' <summary>
        ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
        ''' </summary>
        ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
        ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
        ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
        ''' <param name="CodeStyle">VisualBasic, C#</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Compile(ObjectModel As CodeCompileUnit,
                                            Reference As String(),
                                            DotNETReferenceAssembliesDir As String,
                                            Optional CodeStyle As String = "VisualBasic") As Assembly

            Dim Options As New CompilerParameters With {
                .GenerateInMemory = True,
                .IncludeDebugInformation = False,
                .GenerateExecutable = False
            }
            Return Compile(ObjectModel, Reference, DotNETReferenceAssembliesDir, Options, CodeStyle)
        End Function

        ''' <summary>
        ''' .exe的编译配置文件
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExecutableProfile As CompilerParameters
            Get
                Dim Options As New CompilerParameters
                Options.GenerateInMemory = False
                Options.IncludeDebugInformation = True
                Options.GenerateExecutable = True

                Return Options
            End Get
        End Property

        ''' <summary>
        ''' .Dll的编译配置文件
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DllProfile As CompilerParameters
            Get
                Dim Options As New CompilerParameters
                Options.GenerateInMemory = False
                Options.IncludeDebugInformation = True
                Options.GenerateExecutable = False

                Return Options
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CompileExe(assm As CodeCompileUnit, ref As String(), SDK As String, Optional codeStyle As String = "VisualBasic") As Assembly
            Return Compile(assm, ref, SDK, ExecutableProfile, codeStyle)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CompileDll(assm As CodeCompileUnit,
                                   ref As IEnumerable(Of String),
                                   SDK As String,
                                   Optional codeStyle As String = "VisualBasic") As Assembly
            Return Compile(assm, ref.ToArray, SDK, DllProfile, codeStyle)
        End Function

        ''' <summary>
        ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
        ''' </summary>
        ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
        ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
        ''' <param name="DotNETReferenceAssembliesDir">.NET Framework SDK</param>
        ''' <param name="CodeStyle">VisualBasic, C#</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Compile(ObjectModel As CodeCompileUnit,
                                            Reference As String(),
                                            DotNETReferenceAssembliesDir As String,
                                            Options As CompilerParameters,
                                            Optional CodeStyle As String = "VisualBasic") As Assembly

            Dim CodeDomProvider As CodeDomProvider = CodeDomProvider.CreateProvider(CodeStyle)
            Dim refs As StringCollection = Options.ReferencedAssemblies

            If Not Reference.IsNullOrEmpty Then
                With From path As String
                     In Reference
                     Where Array.IndexOf(DotNETFramework, BaseName(path)) = -1
                     Select path

                    Call refs.AddRange(.ByRef.ToArray)
                End With
            End If

            Call refs.AddRange({
                DotNETReferenceAssembliesDir & "\System.dll",
                DotNETReferenceAssembliesDir & "\System.Core.dll",
                DotNETReferenceAssembliesDir & "\System.Data.dll",
                DotNETReferenceAssembliesDir & "\System.Data.DataSetExtensions.dll",
                DotNETReferenceAssembliesDir & "\System.Xml.dll",
                DotNETReferenceAssembliesDir & "\System.Xml.Linq.dll"
            })

            Dim Compiled = CodeDomProvider.CompileAssemblyFromDom(Options, ObjectModel)

            Call GetDebugInformation(Compiled, ObjectModel).__DEBUG_ECHO

            Return Compiled.CompiledAssembly
        End Function

        ''' <summary>
        ''' 基本的引用集合
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DotNETFramework As String() = {
            "System",
            "System.Core",
            "System.Data",
            "System.Data.DataSetExtensions",
            "System.Xml",
            "System.Xml.Linq"
        }

        ''' <summary>
        ''' Output logs
        ''' </summary>
        ''' <param name="CompiledResult"></param>
        ''' <param name="Assembly"></param>
        ''' <returns></returns>
        <Extension> Public Function GetDebugInformation(CompiledResult As CompilerResults, Assembly As CodeCompileUnit) As String
            With New StringBuilder
                Call .AppendLine(GenerateCode(Assembly.Namespaces.Item(0)) & vbCrLf)

                Call .AppendLine("Error Information: ")

                For Each [Error] In CompiledResult.Errors
                    .AppendLine([Error].ToString)
                Next

                Call .AppendLine(vbCrLf & "Compiler Output:")
                For Each Line As String In CompiledResult.Output
                    .AppendLine(Line)
                Next

                Call .ToString.SaveTo(".\CodeDom.log")
                Return .ToString
            End With
        End Function
    End Module
End Namespace
