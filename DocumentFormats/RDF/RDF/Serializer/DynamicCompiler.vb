#Region "Microsoft.VisualBasic::c6d09d601e039377a2115d8aa8d1672a, ..\visualbasic_App\DocumentFormats\RDF\RDF\Serializer\DynamicCompiler.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Text
Imports System.CodeDom.Compiler

Namespace Framework.DynamicCode.VBC

    ''' <summary>
    ''' 编译整个LINQ语句的动态代码编译器
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DynamicCompiler : Implements System.IDisposable

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Dim DotNETReferenceAssembliesDir As String = "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1"
        Dim ObjectModel As CodeDom.CodeNamespace

        Public ReadOnly Property CompiledCode As String
            Get
                Return GenerateCode(ObjectModel)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="SDK">.NET Framework Reference Assembly文件夹的位置</param>
        ''' <remarks></remarks>
        Sub New(SDK As String)
            Me.DotNETReferenceAssembliesDir = SDK
        End Sub

        ''' <summary>
        ''' Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)
        ''' </summary>
        ''' <param name="ObjectModel">CodeDom dynamic code object model.(目标动态代码的对象模型)</param>
        ''' <param name="Reference">Reference assemby file path collection.(用户代码的引用DLL文件列表)</param>
        ''' <param name="CodeStyle">VisualBasic, C#</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Compile(ObjectModel As CodeDom.CodeCompileUnit, Reference As String(), Optional CodeStyle As String = "VisualBasic") As System.Reflection.Assembly
            Dim CodeDomProvider As CodeDom.Compiler.CodeDomProvider = CodeDom.Compiler.CodeDomProvider.CreateProvider(CodeStyle)
            Dim Options As CodeDom.Compiler.CompilerParameters = New CodeDom.Compiler.CompilerParameters

            Options.GenerateInMemory = True
            Options.IncludeDebugInformation = False
            Options.GenerateExecutable = False

            If Not Reference.IsNullOrEmpty Then
                Call Options.ReferencedAssemblies.AddRange(Reference)
            End If

            Call Options.ReferencedAssemblies.AddRange(New String() {
                   DotNETReferenceAssembliesDir & "\System.dll",
                   DotNETReferenceAssembliesDir & "\System.Core.dll",
                   DotNETReferenceAssembliesDir & "\System.Data.dll",
                   DotNETReferenceAssembliesDir & "\System.Data.DataSetExtensions.dll",
                   DotNETReferenceAssembliesDir & "\System.Xml.dll",
                   DotNETReferenceAssembliesDir & "\System.Xml.Linq.dll"})

            Dim Compiled = CodeDomProvider.CompileAssemblyFromDom(Options, ObjectModel)
#If DEBUG Then
            Console.WriteLine(GetDebugInformation(Compiled))
#End If
            Return Compiled.CompiledAssembly
        End Function

        Public Shared Function GetDebugInformation(CompiledResult As CodeDom.Compiler.CompilerResults) As String
            Dim sBuilder As StringBuilder = New StringBuilder
            sBuilder.AppendLine("Error Information:")
            For Each [Error] In CompiledResult.Errors
                sBuilder.AppendLine([Error].ToString)
            Next
            sBuilder.AppendLine(vbCrLf & "Compiler Output:")
            For Each Line As String In CompiledResult.Output
                sBuilder.AppendLine(Line)
            Next

            Call sBuilder.ToString.SaveTo(".\CodeDom.log")

            Return sBuilder.ToString
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
        Public Shared Function GenerateCode([NameSpace] As CodeDom.CodeNamespace, Optional CodeStyle As String = "VisualBasic") As String
            Dim sBuilder As StringBuilder = New StringBuilder()

            Using sWriter As IO.StringWriter = New System.IO.StringWriter(sBuilder)
                Dim Options As New CodeGeneratorOptions() With {
                    .IndentString = "  ", .ElseOnClosing = True, .BlankLinesBetweenMembers = True}
                CodeDomProvider.GetCompilerInfo(CodeStyle).CreateProvider().GenerateCodeFromNamespace([NameSpace], sWriter, Options)
                Return sBuilder.ToString()
            End Using
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO:  释放托管状态(托管对象)。
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose( disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose( disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
