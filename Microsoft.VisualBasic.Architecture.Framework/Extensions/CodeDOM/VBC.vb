#Region "Microsoft.VisualBasic::6a4cea0c4a4405c57f8f4736162a4d26, ..\Microsoft.VisualBasic.Architecture.Framework\Extensions\CodeDOM\VBC.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace CodeDOM_VBC

    Public Module VBC

        ''' <summary>
        ''' Construct of the vbc.exe compiler parameters.
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
        ''' <returns></returns>
        Public Function CompileCode(code As String, output As String, Optional ByRef errInfo As String = "") As Assembly
            Dim params As New CompilerParameters()

            'Make sure we generate an EXE, not a DLL
            params.GenerateExecutable = True
            params.OutputAssembly = output
            Return VBC.CompileCode(code, params, errInfo)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="code">VisualBasic源代码</param>
        ''' <returns></returns>
        Public Function CompileCode(code As String, args As CompilerParameters, Optional ByRef errInfo As String = "") As Assembly
            Dim codeProvider As New VBCodeProvider()
#Disable Warning
            Dim icc As ICodeCompiler = codeProvider.CreateCompiler
#Enable Warning
            Dim results As CompilerResults = icc.CompileAssemblyFromSource(args, code)

            If results.Errors.Count > 0 Then            'There were compiler errors
                Dim Err As StringBuilder = New StringBuilder("There were compiler errors:")
                Call Err.AppendLine()
                Call Err.AppendLine()

                For Each CompErr As CompilerError In results.Errors
                    Dim errDetail As String = "Line number " & CompErr.Line &
                ", Error Number: " & CompErr.ErrorNumber &
                ", '" & CompErr.ErrorText & ";"
                    Call Err.AppendLine(errDetail)
                    Call Err.AppendLine()
                Next

                errInfo = Err.ToString

                Return Nothing
            Else
                'Successful Compile
                Return results.CompiledAssembly
            End If
        End Function
    End Module
End Namespace
