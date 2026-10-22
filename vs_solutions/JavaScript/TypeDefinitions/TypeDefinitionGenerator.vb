#Region "Microsoft.VisualBasic::8e0b5d39a2c74f16b9d3e8a05c1f7462, vs_solutions\JavaScript\TypeDefinitions\TypeDefinitionGenerator.vb"

    ' Author:
    ' 
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
    ' GNU General Public LICENSE for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:

    '     Class GenerationResult
    ' 
    '         Properties: MemberCount, Text, TypeCount, XmlDocLoaded, XmlDocPath

    '     Module TypeDefinitionGenerator
    ' 
    '         Function: Generate, GenerateFrom, Write

    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Text

Namespace TypeDefinitions

    ''' <summary>Statistics of one generation run.</summary>
    Public Class GenerationResult

        ''' <summary>The generated ``.d.ts`` text.</summary>
        Public Property Text As String
        ''' <summary>Number of exported types found.</summary>
        Public Property TypeCount As Integer
        ''' <summary>Total number of emitted members.</summary>
        Public Property MemberCount As Integer
        ''' <summary>The XML documentation file that was resolved, Nothing when none.</summary>
        Public Property XmlDocPath As String
        ''' <summary>True when the XML documentation was loaded (JSDoc emitted).</summary>
        Public Property XmlDocLoaded As Boolean

    End Class

    ''' <summary>
    ''' Facade of the TypeScript definition generator: reflects one CLR assembly and
    ''' emits a single ``.d.ts`` whose member visibility and type mapping are taken
    ''' from the same <see cref="Runtime.ClrMembers"/> / <see cref="Runtime.ClrConvert"/>
    ''' rules the script bridge uses, so IntelliSense never contradicts runtime
    ''' behaviour.
    ''' <para>
    ''' The assembly is loaded into the default load context with
    ''' <see cref="Assembly.LoadFrom"/> — fine for a one-shot CLI run; a long living
    ''' host should call <see cref="GenerateFrom"/> with its own assembly instance.
    ''' </para>
    ''' </summary>
    Public Module TypeDefinitionGenerator

        ''' <summary>Generate the ``.d.ts`` text for one assembly.</summary>
        Public Function Generate(assemblyPath As String, Optional options As TypeDefinitionOptions = Nothing) As String
            Return Core(assemblyPath, options).Text
        End Function

        ''' <summary>
        ''' Generate and save to a file (UTF-8 without BOM). Returns the statistics of
        ''' the run so a caller can report them.
        ''' </summary>
        Public Function Write(assemblyPath As String, outputPath As String,
                              Optional options As TypeDefinitionOptions = Nothing) As GenerationResult

            If String.IsNullOrEmpty(outputPath) Then
                Throw New ArgumentException("output path is empty", NameOf(outputPath))
            End If

            Dim result As GenerationResult = Core(assemblyPath, options)

            File.WriteAllText(outputPath, result.Text, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return result
        End Function

        ''' <summary>Generate from an already loaded assembly (no <see cref="Assembly.LoadFrom"/>).</summary>
        Friend Function GenerateFrom(asm As Assembly, assemblyPath As String,
                                     options As TypeDefinitionOptions) As GenerationResult

            If asm Is Nothing Then Throw New ArgumentNullException(NameOf(asm))
            If options Is Nothing Then options = New TypeDefinitionOptions

            Dim scan As ScanResult = AssemblyScanner.Scan(asm, options)
            Dim xmlPath As String = ResolveXmlDoc(assemblyPath, options)
            Dim docs As XmlDocIndex = Nothing

            If xmlPath IsNot Nothing Then docs = XmlDocIndex.TryLoad(xmlPath)

            Dim meta As New EmitMeta With {
                .AssemblyName = asm.FullName,
                .RuntimeVersion = asm.ImageRuntimeVersion,
                .Globals = options.Globals,
                .Documents = docs IsNot Nothing,
                .DocumentPath = xmlPath
            }

            Dim text As String = TsEmitter.Emit(scan.Types, scan.ExportNames, docs, meta)
            Dim members As Integer = 0

            For Each model As TsType In scan.Types
                members += model.MemberCount
            Next

            Return New GenerationResult With {
                .Text = text,
                .TypeCount = scan.Types.Count,
                .MemberCount = members,
                .XmlDocPath = xmlPath,
                .XmlDocLoaded = docs IsNot Nothing
            }
        End Function

        ' ======================================================== helpers

        Private Function Core(assemblyPath As String, options As TypeDefinitionOptions) As GenerationResult
            If String.IsNullOrEmpty(assemblyPath) Then
                Throw New ArgumentException("assembly path is empty", NameOf(assemblyPath))
            End If

            If Not File.Exists(assemblyPath) Then
                Throw New FileNotFoundException("assembly file not found: " & assemblyPath, assemblyPath)
            End If

            Dim asm As Assembly

            Try
                asm = Assembly.LoadFrom(assemblyPath)
            Catch ex As Exception
                Throw New ArgumentException("not a loadable .NET assembly: " & assemblyPath, NameOf(assemblyPath), ex)
            End Try

            Return GenerateFrom(asm, assemblyPath, options)
        End Function

        ''' <summary>
        ''' Explicit option wins; otherwise look for ``&lt;assembly file name&gt;.xml``
        ''' next to the dll and return Nothing when it does not exist.
        ''' </summary>
        Private Function ResolveXmlDoc(assemblyPath As String, options As TypeDefinitionOptions) As String
            If Not String.IsNullOrEmpty(options.XmlDocPath) Then Return options.XmlDocPath

            Dim candidate As String = Path.ChangeExtension(assemblyPath, ".xml")

            If File.Exists(candidate) Then Return candidate

            Return Nothing
        End Function

    End Module

End Namespace
