#Region "Microsoft.VisualBasic::a2add56866dd9673a729a29f120119ee, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\CodeDOM\VBC.vb"

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

Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace Emit.CodeDOM_VBC

    ''' <summary>
    ''' Extension wrappers for VisualBasic compiler
    ''' </summary>
    Public Module VBC

        ''' <summary>
        ''' Construct of the ``vbc.exe`` compiler parameters <see cref="CompilerParameters"/>.
        ''' </summary>
        ''' <param name="ref"></param>
        ''' <param name="SDK"></param>
        ''' <param name="dll"></param>
        ''' <returns></returns>
        Public Function CreateParameters(ref As IEnumerable(Of String), SDK As String, Optional dll As Boolean = True) As CompilerParameters
            Dim args As CompilerParameters = If(dll, DllProfile, ExecutableProfile)
            Dim libs As New List(Of String)

            libs += From path As String
                    In ref
                    Where Array.IndexOf(DotNETFramework, IO.Path.GetFileNameWithoutExtension(path)) = -1
                    Select path '
            libs += {
                SDK & "\System.dll",
                SDK & "\System.Core.dll",
                SDK & "\System.Data.dll",
                SDK & "\System.Data.DataSetExtensions.dll",
                SDK & "\System.Xml.dll",
                SDK & "\System.Xml.Linq.dll"
            }
            Call args.ReferencedAssemblies.AddRange(libs)
            Return args
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="code">VisualBasic源代码</param>
        ''' <param name="output">The output ``*.exe`` file.</param>
        ''' <returns></returns>
        Public Function CompileCode(code As String, output As String, Optional ByRef errInfo As String = "") As Assembly
            Dim params As New CompilerParameters()

            'Make sure we generate an EXE, not a DLL
            params.GenerateExecutable = True
            params.OutputAssembly = output
            Return VBC.CompileCode(code, params, errInfo)
        End Function

        ''' <summary>
        ''' If the code compile failure, then this function will returns nothing.
        ''' </summary>
        ''' <param name="code">VisualBasic源代码</param>
        ''' <returns><see cref="Assembly"/> from the source <paramref name="code"/></returns>
        Public Function CompileCode(code As String, args As CompilerParameters, Optional ByRef errInfo As String = "") As Assembly
            Dim codeProvider As New VBCodeProvider()
#Disable Warning
            Dim icc As ICodeCompiler = codeProvider.CreateCompiler
#Enable Warning
            Dim results As CompilerResults = icc.CompileAssemblyFromSource(args, code)

            If results.Errors.Count > 0 Then   ' There were compiler errors
                Dim err As New StringBuilder("There were compiler errors:")
                Call err.AppendLine()
                Call err.AppendLine()

                For Each CompErr As CompilerError In results.Errors
                    Dim errDetail As String = "Line number " & CompErr.Line &
                ", Error Number: " & CompErr.ErrorNumber &
                ", '" & CompErr.ErrorText & ";"
                    Call err.AppendLine(errDetail)
                    Call err.AppendLine()
                Next

                errInfo = err.ToString

                Return Nothing
            Else
                ' Successful Compile
                Return results.CompiledAssembly
            End If
        End Function
    End Module
End Namespace
