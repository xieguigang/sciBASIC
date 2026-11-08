#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd validation"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Xml
Imports System.Xml.Schema

''' <summary>
''' A single diagnostic produced while validating an XML document against an XSD.
''' </summary>
Public Class XsdValidationError

    ''' <summary>Warning or error.</summary>
    Public Property Severity As XmlSeverityType

    ''' <summary>The validation message.</summary>
    Public Property Message As String

    ''' <summary>1-based line number in the XML document, or 0 when unknown.</summary>
    Public Property LineNumber As Integer

    ''' <summary>1-based line position in the XML document, or 0 when unknown.</summary>
    Public Property LinePosition As Integer

    ''' <inheritdoc/>
    Public Overrides Function ToString() As String
        Return $"{Severity} at line {LineNumber}, position {LinePosition}: {Message}"
    End Function
End Class

''' <summary>
''' The structured result of validating one XML document against one XSD.
''' </summary>
Public Class XsdValidationResult

    Private ReadOnly m_diagnostics As IReadOnlyList(Of XsdValidationError)

    ''' <summary>True when no validation error was found.</summary>
    Public ReadOnly Property IsValid As Boolean
        Get
            Return ErrorCount = 0
        End Get
    End Property

    ''' <summary>All collected diagnostics (warnings included).</summary>
    Public ReadOnly Property Errors As IReadOnlyList(Of XsdValidationError)
        Get
            Return m_diagnostics
        End Get
    End Property

    ''' <summary>Number of diagnostics with an error severity.</summary>
    Public ReadOnly Property ErrorCount As Integer
        Get
            Return Enumerable.Count(m_diagnostics, Function(item) item.Severity = XmlSeverityType.Error)
        End Get
    End Property

    ''' <summary>Diagnostics with a warning severity.</summary>
    Public ReadOnly Property Warnings As IReadOnlyList(Of XsdValidationError)
        Get
            Return m_diagnostics.Where(Function(item) item.Severity = XmlSeverityType.Warning).ToArray()
        End Get
    End Property

    ''' <summary>True when the diagnostic buffer reached its limit and further messages were dropped.</summary>
    Public ReadOnly Property IsTruncated As Boolean

    ''' <param name="diagnostics">Collected validation diagnostics.</param>
    ''' <param name="truncated">Whether diagnostics were dropped because of the limit.</param>
    Public Sub New(diagnostics As IEnumerable(Of XsdValidationError), Optional truncated As Boolean = False)
        If diagnostics Is Nothing Then
            m_diagnostics = Array.Empty(Of XsdValidationError)()
        Else
            m_diagnostics = diagnostics.ToArray()
        End If
        IsTruncated = truncated
    End Sub

    ''' <summary>Throw <see cref="XmlSchemaValidationException"/> when the document is invalid.</summary>
    Public Sub ThrowIfInvalid()
        If IsValid Then
            Return
        End If

        Dim message As New StringBuilder($"XML validation failed with {ErrorCount} error(s):")
        For Each item As XsdValidationError In m_diagnostics
            If item.Severity = XmlSeverityType.Error Then
                message.AppendLine()
                message.Append("  ").Append(item.ToString())
            End If
        Next
        If IsTruncated Then
            message.AppendLine()
            message.Append("  Additional diagnostics were dropped because the limit was reached.")
        End If

        Throw New XmlSchemaValidationException(message.ToString())
    End Sub

    ''' <inheritdoc/>
    Public Overrides Function ToString() As String
        If IsValid Then
            Return $"XML is valid against the schema ({Warnings.Count} warning(s))."
        End If

        Dim message As New StringBuilder($"XML is invalid: {ErrorCount} error(s).")
        For Each item As XsdValidationError In m_diagnostics
            If item.Severity = XmlSeverityType.Error Then
                message.AppendLine()
                message.Append("  ").Append(item.ToString())
            End If
        Next
        Return message.ToString()
    End Function
End Class

''' <summary>
''' Validates XML documents against an XSD schema set.
''' </summary>
''' <remarks>
''' Validation is streaming (forward-only <see cref="XmlReader"/>), so memory
''' usage does not grow with the document size. DTD processing is prohibited and
''' the XML reader has no resolver, which prevents XXE / external entity loading.
''' </remarks>
Public Module XmlValidator

    ''' <summary>Default upper bound of collected diagnostics.</summary>
    Public Const DefaultMaxErrors As Integer = 1000

    ''' <summary>
    ''' Validate an XML file against an XSD file.
    ''' </summary>
    ''' <param name="xsdPath">Path of the XSD file (include/import are resolved).</param>
    ''' <param name="xmlPath">Path of the XML file to validate.</param>
    ''' <param name="maxErrors">Maximum number of diagnostics to collect.</param>
    Public Function Validate(xsdPath As String,
                             xmlPath As String,
                             Optional maxErrors As Integer = DefaultMaxErrors) As XsdValidationResult
        If String.IsNullOrWhiteSpace(xmlPath) Then
            Throw New ArgumentException("The xml file path can not be empty.", NameOf(xmlPath))
        End If
        If Not File.Exists(xmlPath) Then
            Throw New FileNotFoundException($"Can not find the xml file: {xmlPath}", xmlPath)
        End If

        Using stream As New FileStream(Path.GetFullPath(xmlPath), FileMode.Open,
                                       FileAccess.Read, FileShare.Read)
            Return ValidateStream(xsdPath, stream, maxErrors, Path.GetFullPath(xmlPath))
        End Using
    End Function

    ''' <summary>
    ''' Validate in-memory XML text against an XSD file.
    ''' </summary>
    Public Function ValidateText(xsdPath As String,
                                 xmlText As String,
                                 Optional maxErrors As Integer = DefaultMaxErrors) As XsdValidationResult
        If xmlText Is Nothing Then
            Throw New ArgumentNullException(NameOf(xmlText))
        End If

        Using buffer As New MemoryStream(New UTF8Encoding(False).GetBytes(xmlText))
            Return ValidateStream(xsdPath, buffer, maxErrors, "in-memory xml")
        End Using
    End Function

    ''' <summary>
    ''' Validate an XML stream against an XSD file.
    ''' </summary>
    Public Function ValidateStream(xsdPath As String,
                                   xmlStream As Stream,
                                   Optional maxErrors As Integer = DefaultMaxErrors,
                                   Optional sourceName As String = "xml stream") As XsdValidationResult
        If xmlStream Is Nothing Then
            Throw New ArgumentNullException(NameOf(xmlStream))
        End If
        If maxErrors < 1 Then
            maxErrors = 1
        End If

        Dim schemas As XmlSchemaSet = SchemaLoader.Load(xsdPath)
        Dim diagnostics As New List(Of XsdValidationError)()
        Dim truncated As Boolean = False

        Dim settings As New XmlReaderSettings With {
            .ValidationType = ValidationType.Schema,
            .Schemas = schemas,
            .DtdProcessing = DtdProcessing.Prohibit,
            .XmlResolver = Nothing,
            .CloseInput = False,
            .ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints Or
                               XmlSchemaValidationFlags.ReportValidationWarnings
        }

        AddHandler settings.ValidationEventHandler,
            Sub(sender As Object, e As ValidationEventArgs)
                If diagnostics.Count >= maxErrors Then
                    truncated = True
                    Return
                End If

                Dim severity As XmlSeverityType = e.Severity
                Dim message As String = e.Message

                ' A root element with no matching declaration only raises a warning.
                ' Treat that as an error, otherwise an unvalidated document would
                ' be reported as valid.
                If severity = XmlSeverityType.Warning AndAlso
                   message.IndexOf("could not find schema information", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    severity = XmlSeverityType.Error
                    message = "No matching XSD declaration was found. " & message
                End If

                diagnostics.Add(New XsdValidationError With {
                    .Severity = severity,
                    .Message = message,
                    .LineNumber = If(e.Exception Is Nothing, 0, e.Exception.LineNumber),
                    .LinePosition = If(e.Exception Is Nothing, 0, e.Exception.LinePosition)
                })
            End Sub

        Try
            Using reader As XmlReader = XmlReader.Create(xmlStream, settings)
                While reader.Read()
                    ' Reading to the end forces the reader to validate every node.
                End While
            End Using
        Catch ex As XmlException
            diagnostics.Add(New XsdValidationError With {
                .Severity = XmlSeverityType.Error,
                .Message = $"Malformed XML in '{sourceName}': {ex.Message}",
                .LineNumber = ex.LineNumber,
                .LinePosition = ex.LinePosition
            })
        Catch ex As XmlSchemaException
            diagnostics.Add(New XsdValidationError With {
                .Severity = XmlSeverityType.Error,
                .Message = $"Schema validation aborted: {ex.Message}",
                .LineNumber = ex.LineNumber,
                .LinePosition = ex.LinePosition
            })
        End Try

        Return New XsdValidationResult(diagnostics, truncated)
    End Function

    ''' <summary>
    ''' Convenience shortcut: True when the XML file has no validation error.
    ''' </summary>
    Public Function IsValid(xsdPath As String, xmlPath As String) As Boolean
        Return Validate(xsdPath, xmlPath).IsValid
    End Function
End Module
