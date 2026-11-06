#Region "Microsoft.VisualBasic::8344eeb7bce18e036596875d4d60ede6, mime\application%xml\CodeGenerator.vb"

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

    '   Total Lines: 6
    '    Code Lines: 2 (33.33%)
    ' Comment Lines: 3 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (16.67%)
    '     File Size: 134 B


    ' Class CodeGenerator
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Text
Imports System.Xml.Schema

''' <summary>
''' Generate the VB.NET class code based on the xsd definition
''' </summary>
''' <remarks>
''' The generated source contains <c>XmlSerializer</c> friendly classes: global
''' complexTypes become classes, enumerating simpleTypes become enums, global
''' elements become <c>XmlRoot</c> classes, and inheritance / substitution groups
''' are expressed with <c>Inherits</c> and <c>XmlInclude</c>.
''' </remarks>
Public Class CodeGenerator

    ''' <summary>
    ''' Generate VB.NET class source code from an XSD file.
    ''' </summary>
    ''' <param name="xsdPath">Path of the XSD file.</param>
    ''' <param name="codeNamespace">
    ''' VB namespace of the generated classes. When omitted, a namespace name is
    ''' derived from the XSD targetNamespace (or <c>XsdGenerated</c>).
    ''' </param>
    ''' <returns>The complete text of a <c>.vb</c> source file.</returns>
    Public Shared Function GenerateCode(xsdPath As String,
                                        Optional codeNamespace As String = Nothing) As String
        Dim schemas As XmlSchemaSet = SchemaLoader.Load(xsdPath)
        Return GenerateCode(schemas, codeNamespace)
    End Function

    ''' <summary>
    ''' Generate VB.NET class source code from XSD document text.
    ''' </summary>
    ''' <param name="xsdText">The XSD document text.</param>
    ''' <param name="baseDirectory">
    ''' Base directory used to resolve relative xs:include / xs:import locations.
    ''' </param>
    ''' <param name="codeNamespace">VB namespace of the generated classes.</param>
    Public Shared Function GenerateCodeFromText(xsdText As String,
                                                Optional baseDirectory As String = Nothing,
                                                Optional codeNamespace As String = Nothing) As String
        Dim schemas As XmlSchemaSet = SchemaLoader.LoadFromText(xsdText, baseDirectory)
        Return GenerateCode(schemas, codeNamespace)
    End Function

    ''' <summary>
    ''' Generate VB.NET class source code from a compiled schema set.
    ''' </summary>
    Public Shared Function GenerateCode(schemas As XmlSchemaSet,
                                        Optional codeNamespace As String = Nothing) As String
        If schemas Is Nothing Then
            Throw New ArgumentNullException(NameOf(schemas))
        End If
        If codeNamespace Is Nothing Then
            codeNamespace = XsdTypeMap.DeriveCodeNamespace(schemas)
        Else
            codeNamespace = XsdTypeMap.ToIdentifier(codeNamespace)
            If XsdTypeMap.IsKeyword(codeNamespace) Then
                codeNamespace &= "Schema"
            End If
        End If

        Return New SchemaModelBuilder(schemas, codeNamespace).Build()
    End Function

    ''' <summary>
    ''' Generate VB.NET class definitions from an XSD file and write them to a
    ''' <c>.vb</c> file, ready to be used by <c>XmlSerializer</c>.
    ''' </summary>
    ''' <param name="xsdPath">Path of the XSD file.</param>
    ''' <param name="vbFilePath">Destination path of the generated source file.</param>
    ''' <param name="targetNamespace">VB namespace of the generated classes.</param>
    ''' <returns>The absolute path of the written source file.</returns>
    Public Shared Function GenerateClassFile(xsdPath As String,
                                             vbFilePath As String,
                                             Optional targetNamespace As String = Nothing) As String
        If String.IsNullOrWhiteSpace(vbFilePath) Then
            Throw New ArgumentException("The output vb source file path can not be empty.", NameOf(vbFilePath))
        End If

        Dim code As String = GenerateCode(xsdPath, targetNamespace)
        WriteAllText(vbFilePath, code)
        Return System.IO.Path.GetFullPath(vbFilePath)
    End Function

    ''' <summary>
    ''' Generate VB.NET class definitions from XSD document text and write them
    ''' to a <c>.vb</c> file.
    ''' </summary>
    Public Shared Function GenerateClassFileFromText(xsdText As String,
                                                     vbFilePath As String,
                                                     Optional baseDirectory As String = Nothing,
                                                     Optional targetNamespace As String = Nothing) As String
        Dim code As String = GenerateCodeFromText(xsdText, baseDirectory, targetNamespace)
        WriteAllText(vbFilePath, code)
        Return System.IO.Path.GetFullPath(vbFilePath)
    End Function

    Private Shared Sub WriteAllText(filePath As String, text As String)
        Dim full As String = System.IO.Path.GetFullPath(filePath)
        Dim dir As String = System.IO.Path.GetDirectoryName(full)

        If Not String.IsNullOrEmpty(dir) AndAlso Not System.IO.Directory.Exists(dir) Then
            System.IO.Directory.CreateDirectory(dir)
        End If

        System.IO.File.WriteAllText(full, text, New UTF8Encoding(False))
    End Sub
End Class
