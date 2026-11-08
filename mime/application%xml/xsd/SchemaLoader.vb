#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd schema loader"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Xml
Imports System.Xml.Schema

''' <summary>
''' Loads and compiles an XSD document into a <see cref="XmlSchemaSet"/>.
''' </summary>
''' <remarks>
''' .NET 10 does not resolve xs:include/xs:import unless an explicit resolver is
''' assigned to <see cref="XmlSchemaSet.XmlResolver"/>. The resolver here only
''' permits local files located inside the directory of the main schema file, so
''' a schema cannot trigger arbitrary network or out-of-tree file access.
''' </remarks>
Friend Module SchemaLoader

    ''' <summary>
    ''' Load the schema from a file and compile it.
    ''' </summary>
    ''' <param name="xsdPath">Path of the main XSD file.</param>
    ''' <param name="handler">Optional receiver for schema compile diagnostics.</param>
    ''' <returns>A compiled schema set; include/import/group/inheritance are resolved.</returns>
    Public Function Load(xsdPath As String, Optional handler As ValidationEventHandler = Nothing) As XmlSchemaSet
        If String.IsNullOrWhiteSpace(xsdPath) Then
            Throw New ArgumentException("The xsd file path can not be empty.", NameOf(xsdPath))
        End If
        If Not System.IO.File.Exists(xsdPath) Then
            Throw New FileNotFoundException($"Can not find the xsd file: {xsdPath}", xsdPath)
        End If

        Dim fullPath As String = System.IO.Path.GetFullPath(xsdPath)
        Dim settings As New XmlReaderSettings With {
            .DtdProcessing = DtdProcessing.Prohibit,
            .XmlResolver = Nothing,
            .IgnoreComments = True,
            .IgnoreWhitespace = True
        }

        Using reader As XmlReader = XmlReader.Create(fullPath, settings)
            Return Compile(reader, System.IO.Path.GetDirectoryName(fullPath), handler, fullPath)
        End Using
    End Function

    ''' <summary>
    ''' Load the schema from an in-memory XSD document.
    ''' </summary>
    ''' <param name="xsdText">The XSD document text.</param>
    ''' <param name="baseDirectory">
    ''' Directory used as the base URI of the document, so relative
    ''' xs:include/xs:import locations can still be resolved.
    ''' </param>
    ''' <param name="handler">Optional receiver for schema compile diagnostics.</param>
    Public Function LoadFromText(xsdText As String,
                                 Optional baseDirectory As String = Nothing,
                                 Optional handler As ValidationEventHandler = Nothing) As XmlSchemaSet
        If String.IsNullOrWhiteSpace(xsdText) Then
            Throw New ArgumentException("The xsd document text can not be empty.", NameOf(xsdText))
        End If

        Dim root As String = If(String.IsNullOrWhiteSpace(baseDirectory),
                                System.IO.Directory.GetCurrentDirectory(),
                                System.IO.Path.GetFullPath(baseDirectory))
        If Not root.EndsWith(System.IO.Path.DirectorySeparatorChar) Then
            root &= System.IO.Path.DirectorySeparatorChar
        End If

        Dim settings As New XmlReaderSettings With {
            .DtdProcessing = DtdProcessing.Prohibit,
            .XmlResolver = Nothing,
            .IgnoreComments = True,
            .IgnoreWhitespace = True,
            .CloseInput = True
        }
        Dim baseUri As New Uri(New Uri(root), "inline.xsd")

        Using text As New StringReader(xsdText)
            Using reader As XmlReader = XmlReader.Create(text, settings, baseUri.AbsoluteUri)
                Return Compile(reader, root, handler, "inline xsd")
            End Using
        End Using
    End Function

    Private Function Compile(reader As XmlReader,
                             root As String,
                             handler As ValidationEventHandler,
                             source As String) As XmlSchemaSet
        Dim errors As New List(Of String)()
        Dim schemas As New XmlSchemaSet()

        schemas.XmlResolver = New LocalXsdResolver(root)
        AddHandler schemas.ValidationEventHandler,
            Sub(sender As Object, e As ValidationEventArgs)
                errors.Add($"{e.Severity}: {e.Message}")
                If handler IsNot Nothing Then
                    handler(sender, e)
                End If
            End Sub

        Try
            schemas.Add(Nothing, reader)
            schemas.Compile()
        Catch ex As XmlException
            Throw New XmlSchemaException($"Unable to read the xsd document '{source}': {ex.Message}", ex)
        Catch ex As XmlSchemaException
            Throw New XmlSchemaException($"Invalid xsd document '{source}': {ex.Message}", ex)
        End Try

        If errors.Count > 0 Then
            Throw New XmlSchemaException($"Invalid xsd document '{source}': {String.Join("; ", errors)}")
        End If
        If Not schemas.IsCompiled Then
            Throw New XmlSchemaException($"Unable to compile the xsd document '{source}'.")
        End If

        Return schemas
    End Function

    ''' <summary>
    ''' A resolver that only serves local schema files below a fixed directory.
    ''' </summary>
    Friend NotInheritable Class LocalXsdResolver
        Inherits XmlResolver

        Private ReadOnly root As String

        Public Sub New(root As String)
            Me.root = System.IO.Path.GetFullPath(root)
            If Not Me.root.EndsWith(System.IO.Path.DirectorySeparatorChar) Then
                Me.root &= System.IO.Path.DirectorySeparatorChar
            End If
        End Sub

        ''' <summary>
        ''' Credentials are intentionally ignored: remote access is never allowed.
        ''' </summary>
        Public Overrides WriteOnly Property Credentials As ICredentials
            Set(value As ICredentials)
                ' no-op
            End Set
        End Property

        Public Overrides Function ResolveUri(baseUri As Uri, relativeUri As String) As Uri
            Return MyBase.ResolveUri(baseUri, relativeUri)
        End Function

        Public Overrides Function GetEntity(absoluteUri As Uri, role As String, ofObjectToReturn As Type) As Object
            If absoluteUri Is Nothing Then
                Throw New ArgumentNullException(NameOf(absoluteUri))
            End If
            If Not absoluteUri.IsFile Then
                Throw New XmlException($"Remote schema references are not allowed: {absoluteUri}")
            End If

            Dim schemaPath As String = System.IO.Path.GetFullPath(absoluteUri.LocalPath)
            If Not schemaPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) Then
                Throw New XmlException($"Schema file is outside of the allowed xsd directory: {schemaPath}")
            End If
            If Not System.IO.File.Exists(schemaPath) Then
                Throw New FileNotFoundException($"The included/imported xsd file is missing: {schemaPath}", schemaPath)
            End If

            Return New FileStream(schemaPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        End Function
    End Class
End Module
