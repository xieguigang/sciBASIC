#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd tools facade"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Xml.Schema

''' <summary>
''' XSD toolkit: generate VB.NET classes from a schema, export a schema from
''' reflected VB.NET types, and validate XML documents against a schema.
''' </summary>
''' <remarks>
''' Typical round trip: <c>CreateXsdFromType</c> -> <c>CreateClassFromXsd</c>
''' -> <c>XmlSerializer</c> -> <c>ValidateXml</c>.
''' </remarks>
Public Module XsdTools

    ''' <summary>
    ''' Create a VB.NET class definition file from an XSD file. The generated
    ''' classes carry the <c>XmlSerializer</c> attributes needed for both
    ''' serialization and deserialization.
    ''' </summary>
    ''' <param name="xsdPath">Path of the source XSD file.</param>
    ''' <param name="vbFilePath">Path of the generated <c>.vb</c> file.</param>
    ''' <param name="targetNamespace">
    ''' VB namespace for the generated classes. When omitted, a name is derived
    ''' from the XSD targetNamespace (falling back to <c>XsdGenerated</c>).
    ''' </param>
    Public Sub CreateClassFromXsd(xsdPath As String,
                                  vbFilePath As String,
                                  Optional targetNamespace As String = Nothing)
        RequireText(xsdPath, NameOf(xsdPath))
        RequireText(vbFilePath, NameOf(vbFilePath))

        Call CodeGenerator.GenerateClassFile(xsdPath, vbFilePath, targetNamespace)
    End Sub

    ''' <summary>
    ''' Generate VB.NET class source code from an XSD file without writing a file.
    ''' </summary>
    Public Function CreateClassCodeFromXsd(xsdPath As String,
                                           Optional targetNamespace As String = Nothing) As String
        RequireText(xsdPath, NameOf(xsdPath))
        Return CodeGenerator.GenerateCode(xsdPath, targetNamespace)
    End Function

    ''' <summary>
    ''' Generate VB.NET class source code from in-memory XSD text.
    ''' </summary>
    ''' <param name="xsdText">The XSD document text.</param>
    ''' <param name="baseDirectory">Base directory for relative xs:include / xs:import.</param>
    ''' <param name="targetNamespace">VB namespace for the generated classes.</param>
    Public Function CreateClassCodeFromXsdText(xsdText As String,
                                               Optional baseDirectory As String = Nothing,
                                               Optional targetNamespace As String = Nothing) As String
        Return CodeGenerator.GenerateCodeFromText(xsdText, baseDirectory, targetNamespace)
    End Function

    ''' <summary>
    ''' Generate an XSD file from a VB.NET class definition by reflection.
    ''' </summary>
    ''' <param name="type">A type that <c>XmlSerializer</c> can process.</param>
    ''' <param name="xsdFilePath">Path of the generated <c>.xsd</c> file.</param>
    ''' <param name="defaultNamespace">Default XML namespace of the schema.</param>
    Public Sub CreateXsdFromType(type As Type,
                                 xsdFilePath As String,
                                 Optional defaultNamespace As String = Nothing)
        If type Is Nothing Then
            Throw New ArgumentNullException(NameOf(type))
        End If
        RequireText(xsdFilePath, NameOf(xsdFilePath))

        Call SchemaExporter.ExportToFile(type, xsdFilePath, defaultNamespace)
    End Sub

    ''' <summary>
    ''' Generate an XSD file from <typeparamref name="T"/> by reflection.
    ''' </summary>
    Public Sub CreateXsdFromType(Of T)(xsdFilePath As String,
                                       Optional defaultNamespace As String = Nothing)
        RequireText(xsdFilePath, NameOf(xsdFilePath))
        Call SchemaExporter.ExportToFile(Of T)(xsdFilePath, defaultNamespace)
    End Sub

    ''' <summary>
    ''' Export an XSD document as text from a reflected VB.NET type.
    ''' </summary>
    Public Function CreateXsdTextFromType(type As Type,
                                          Optional defaultNamespace As String = Nothing) As String
        If type Is Nothing Then
            Throw New ArgumentNullException(NameOf(type))
        End If
        Return SchemaExporter.Export(type, defaultNamespace)
    End Function

    ''' <summary>
    ''' Reflect every serializable public type of an assembly and write one XSD
    ''' file per type into <paramref name="outputDir"/>.
    ''' </summary>
    Public Sub CreateXsdFromAssembly(assemblyPath As String, outputDir As String)
        RequireText(assemblyPath, NameOf(assemblyPath))
        RequireText(outputDir, NameOf(outputDir))
        Call SchemaExporter.ExportAssembly(assemblyPath, outputDir)
    End Sub

    ''' <summary>
    ''' Validate an XML file (for example one produced by <c>XmlSerializer</c>)
    ''' against an XSD file.
    ''' </summary>
    ''' <returns>
    ''' A structured result carrying the severity, message and source position
    ''' of every diagnostic. Validation errors are reported, not thrown; call
    ''' <see cref="XsdValidationResult.ThrowIfInvalid"/> to throw instead.
    ''' </returns>
    Public Function ValidateXml(xsdPath As String, xmlPath As String) As XsdValidationResult
        RequireText(xsdPath, NameOf(xsdPath))
        RequireText(xmlPath, NameOf(xmlPath))
        Return XmlValidator.Validate(xsdPath, xmlPath)
    End Function

    ''' <summary>
    ''' Validate in-memory XML text against an XSD file.
    ''' </summary>
    Public Function ValidateXmlText(xsdPath As String, xmlText As String) As XsdValidationResult
        RequireText(xsdPath, NameOf(xsdPath))
        Return XmlValidator.ValidateText(xsdPath, xmlText)
    End Function

    ''' <summary>
    ''' True when the XML file contains no validation error against the XSD file.
    ''' </summary>
    Public Function IsValid(xsdPath As String, xmlPath As String) As Boolean
        Return ValidateXml(xsdPath, xmlPath).IsValid
    End Function

    ''' <summary>
    ''' Load and compile an XSD file into a schema set for advanced scenarios.
    ''' </summary>
    Public Function LoadSchemas(xsdPath As String) As XmlSchemaSet
        RequireText(xsdPath, NameOf(xsdPath))
        Return SchemaLoader.Load(xsdPath)
    End Function

    Private Sub RequireText(value As String, name As String)
        If String.IsNullOrWhiteSpace(value) Then
            Throw New ArgumentException($"The value of '{name}' can not be empty.", name)
        End If
    End Sub
End Module
