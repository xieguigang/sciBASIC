#Region "Microsoft.VisualBasic.MIME.application.xml :: reflection to xsd"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Xml
Imports System.Xml.Schema
Imports System.Xml.Serialization

''' <summary>
''' Exports XSD documents from compiled VB.NET types by reflection.
''' </summary>
''' <remarks>
''' Each <see cref="XmlSchema"/> is written with its own <see cref="XmlWriter"/>
''' and its own stream. Reusing one writer/stream for several schemas (as the
''' Core helper <c>XmlExtensions.GenerateXsdFromType</c> does) produces a text
''' with several XML declarations, which is not a valid XSD document.
''' </remarks>
Public Module SchemaExporter

    ''' <summary>
    ''' Reflect a type and return the primary XSD document as text.
    ''' </summary>
    ''' <param name="type">The type to map; it must be usable by <see cref="XmlSerializer"/>.</param>
    ''' <param name="defaultNamespace">Default XML namespace of the exported schema.</param>
    Public Function Export(type As Type, Optional defaultNamespace As String = Nothing) As String
        Dim schemas As List(Of XmlSchema) = ExportSchemas(type, defaultNamespace)

        Using buffer As New System.IO.MemoryStream()
            WriteTo(schemas(0), buffer)
            Return New UTF8Encoding(False).GetString(buffer.ToArray())
        End Using
    End Function

    ''' <summary>
    ''' Reflect a type and return every XSD document produced by the exporter.
    ''' More than one schema is produced when the object graph references several
    ''' XML namespaces.
    ''' </summary>
    Public Function ExportSchemas(type As Type,
                                  Optional defaultNamespace As String = Nothing) As List(Of XmlSchema)
        If type Is Nothing Then
            Throw New ArgumentNullException(NameOf(type))
        End If

        Dim mapping As XmlTypeMapping
        Try
            Dim importer As New XmlReflectionImporter()
            If String.IsNullOrEmpty(defaultNamespace) Then
                mapping = importer.ImportTypeMapping(type)
            Else
                mapping = importer.ImportTypeMapping(type, defaultNamespace)
            End If
        Catch ex As Exception
            Throw New InvalidOperationException(
                $"Type '{type.FullName}' can not be mapped to an XML schema: {ex.Message}", ex)
        End Try

        Dim schemas As New XmlSchemas()
        Try
            Dim exporter As New XmlSchemaExporter(schemas)
            exporter.ExportTypeMapping(mapping)
            schemas.Compile(Nothing, True)
        Catch ex As Exception
            Throw New InvalidOperationException(
                $"Failed to export the XML schema of type '{type.FullName}': {ex.Message}", ex)
        End Try

        Dim result As New List(Of XmlSchema)()
        For Each schema As XmlSchema In schemas
            result.Add(schema)
        Next
        If result.Count = 0 Then
            Throw New InvalidOperationException($"No XML schema was produced for type '{type.FullName}'.")
        End If

        If Not String.IsNullOrEmpty(defaultNamespace) Then
            Dim primary As Integer = result.FindIndex(
                Function(s) String.Equals(s.TargetNamespace, defaultNamespace, StringComparison.Ordinal))
            If primary > 0 Then
                Dim selected As XmlSchema = result(primary)
                result.RemoveAt(primary)
                result.Insert(0, selected)
            End If
        End If

        Return result
    End Function

    ''' <summary>
    ''' Reflect a type and write its XSD document(s) to disk.
    ''' </summary>
    ''' <param name="type">The type to map.</param>
    ''' <param name="xsdFilePath">Path of the primary <c>.xsd</c> file.</param>
    ''' <param name="defaultNamespace">Default XML namespace of the exported schema.</param>
    ''' <returns>
    ''' All written file paths; the first one is always <paramref name="xsdFilePath"/>.
    ''' Extra namespaces are written next to it with a numeric suffix.
    ''' </returns>
    Public Function ExportToFile(type As Type,
                                 xsdFilePath As String,
                                 Optional defaultNamespace As String = Nothing) As String()
        Return ExportToFiles(ExportSchemas(type, defaultNamespace), xsdFilePath)
    End Function

    ''' <summary>
    ''' Reflect <typeparamref name="T"/> and write its XSD document(s) to disk.
    ''' </summary>
    Public Function ExportToFile(Of T)(xsdFilePath As String,
                                       Optional defaultNamespace As String = Nothing) As String()
        Return ExportToFile(GetType(T), xsdFilePath, defaultNamespace)
    End Function

    ''' <summary>
    ''' Reflect every serializable public type of an assembly and write one XSD
    ''' document per type into <paramref name="outputDir"/>.
    ''' </summary>
    ''' <param name="assemblyPath">Path of the assembly (<c>.dll</c>/<c>.exe</c>).</param>
    ''' <param name="outputDir">Directory that receives the generated <c>.xsd</c> files.</param>
    ''' <returns>All written file paths.</returns>
    Public Function ExportAssembly(assemblyPath As String, outputDir As String) As String()
        If String.IsNullOrWhiteSpace(assemblyPath) Then
            Throw New ArgumentException("The assembly path can not be empty.", NameOf(assemblyPath))
        End If
        If Not System.IO.File.Exists(assemblyPath) Then
            Throw New System.IO.FileNotFoundException($"Can not find the assembly: {assemblyPath}", assemblyPath)
        End If
        If String.IsNullOrWhiteSpace(outputDir) Then
            Throw New ArgumentException("The output directory can not be empty.", NameOf(outputDir))
        End If

        Dim fullAssemblyPath As String = System.IO.Path.GetFullPath(assemblyPath)
        Dim fullOutputDir As String = System.IO.Path.GetFullPath(outputDir)
        If Not System.IO.Directory.Exists(fullOutputDir) Then
            System.IO.Directory.CreateDirectory(fullOutputDir)
        End If

        Dim assembly As Assembly = Assembly.LoadFrom(fullAssemblyPath)
        Dim written As New List(Of String)()
        Dim usedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Dim types As Type() = assembly.GetExportedTypes().
            OrderBy(Function(t) t.FullName, StringComparer.Ordinal).
            ToArray()

        For Each type As Type In types
            If Not IsSerializableModel(type) Then
                Continue For
            End If

            Dim documentName As String = SafeFileName(type.Name)
            Dim index As Integer = 1
            While Not usedNames.Add(documentName)
                index += 1
                documentName = SafeFileName(type.Name) & "_" & index.ToString()
            End While

            Try
                Dim schemas As List(Of XmlSchema) = ExportSchemas(type)
                Dim target As String = System.IO.Path.Combine(fullOutputDir, documentName & ".xsd")
                written.AddRange(ExportToFiles(schemas, target))
            Catch ex As Exception
                Trace.TraceWarning($"Skip XSD export for '{type.FullName}': {ex.Message}")
            End Try
        Next

        Return written.ToArray()
    End Function

    ''' <summary>True when the type can be handled by <see cref="XmlSerializer"/>.</summary>
    Public Function IsSerializableModel(type As Type) As Boolean
        If type Is Nothing OrElse type.IsInterface OrElse type.IsEnum OrElse
           type.IsAbstract OrElse type.IsPointer OrElse type.IsByRef OrElse
           type.ContainsGenericParameters OrElse type.IsGenericTypeDefinition Then
            Return False
        End If
        If Not type.IsValueType AndAlso type.GetConstructor(Type.EmptyTypes) Is Nothing Then
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Write every schema to its own file. The first schema always uses the
    ''' requested path; additional ones get a numeric suffix.
    ''' </summary>
    Private Function ExportToFiles(schemas As List(Of XmlSchema), xsdFilePath As String) As String()
        If String.IsNullOrWhiteSpace(xsdFilePath) Then
            Throw New ArgumentException("The xsd file path can not be empty.", NameOf(xsdFilePath))
        End If

        Dim primary As String = System.IO.Path.GetFullPath(xsdFilePath)
        Dim dir As String = System.IO.Path.GetDirectoryName(primary)
        If Not String.IsNullOrEmpty(dir) AndAlso Not System.IO.Directory.Exists(dir) Then
            System.IO.Directory.CreateDirectory(dir)
        End If

        Dim baseName As String = System.IO.Path.GetFileNameWithoutExtension(primary)
        Dim extension As String = System.IO.Path.GetExtension(primary)
        If String.IsNullOrEmpty(extension) Then
            extension = ".xsd"
            primary &= extension
        End If

        Dim written As New List(Of String)()
        For i As Integer = 0 To schemas.Count - 1
            Dim target As String = If(i = 0, primary,
                                      System.IO.Path.Combine(dir, $"{baseName}_{i}{extension}"))
            Using stream As New System.IO.FileStream(target, System.IO.FileMode.Create,
                                                     System.IO.FileAccess.Write, System.IO.FileShare.Read)
                WriteTo(schemas(i), stream)
            End Using
            written.Add(target)
        Next

        Return written.ToArray()
    End Function

    ''' <summary>
    ''' Write one schema into one stream with a single UTF-8 XML declaration.
    ''' </summary>
    Private Sub WriteTo(schema As XmlSchema, stream As System.IO.Stream)
        Dim settings As New XmlWriterSettings With {
            .Indent = True,
            .IndentChars = "  ",
            .Encoding = New UTF8Encoding(False),
            .OmitXmlDeclaration = False,
            .CloseOutput = False
        }

        Using writer As XmlWriter = XmlWriter.Create(stream, settings)
            schema.Write(writer)
        End Using
    End Sub

    Private Function SafeFileName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then
            Return "Type"
        End If

        Dim sb As New StringBuilder(name.Length)
        For Each ch As Char In name
            If System.IO.Path.GetInvalidFileNameChars().Contains(ch) Then
                sb.Append("_"c)
            Else
                sb.Append(ch)
            End If
        Next

        Dim result As String = sb.ToString().Trim()
        Return If(result.Length = 0, "Type", result)
    End Function
End Module
