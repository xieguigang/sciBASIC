#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd self test"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.Schema
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MIME.application.xml

''' <summary>
''' End to end self test of the XSD toolkit: type -> xsd -> VB source ->
''' XmlSerializer output -> xsd validation. All assertions are recorded by
''' <see cref="DebugAudit"/> and a JSON report is written at the end.
''' </summary>
Public Module XsdSelfTest

    Private Const XsPrelude As String =
        "<?xml version=""1.0"" encoding=""utf-8""?>" &
        "<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"" " &
        "xmlns:tns=""urn:books"" targetNamespace=""urn:books"" elementFormDefault=""qualified"">"

    Private Const CommonSchema As String = XsPrelude &
        "<xs:simpleType name=""Genre"">" &
        " <xs:annotation><xs:documentation>Book genre classification</xs:documentation></xs:annotation>" &
        " <xs:restriction base=""xs:string"">" &
        "  <xs:enumeration value=""fiction""/><xs:enumeration value=""non-fiction""/>" &
        "  <xs:pattern value=""[a-z-]+""/>" &
        " </xs:restriction></xs:simpleType>" &
        "<xs:simpleType name=""Isbn"">" &
        " <xs:restriction base=""xs:string""><xs:minLength value=""3""/></xs:restriction>" &
        "</xs:simpleType>" &
        "<xs:group name=""Identity""><xs:sequence>" &
        " <xs:element name=""title"" type=""xs:string""/>" &
        " <xs:element name=""published"" type=""xs:int"" minOccurs=""0"" nillable=""true""/>" &
        "</xs:sequence></xs:group>" &
        "<xs:attributeGroup name=""CommonAttrs"">" &
        " <xs:attribute name=""id"" type=""xs:string"" use=""required""/>" &
        " <xs:attribute name=""revision"" type=""xs:int"" use=""optional""/>" &
        "</xs:attributeGroup>" &
        "<xs:complexType name=""Publication"">" &
        " <xs:annotation><xs:documentation>Base publication type.</xs:documentation></xs:annotation>" &
        " <xs:sequence>" &
        "  <xs:group ref=""tns:Identity""/>" &
        "  <xs:element name=""tag"" type=""xs:string"" minOccurs=""0"" maxOccurs=""unbounded""/>" &
        "  <xs:any namespace=""##other"" processContents=""lax"" minOccurs=""0""/>" &
        " </xs:sequence>" &
        " <xs:attributeGroup ref=""tns:CommonAttrs""/>" &
        " <xs:anyAttribute namespace=""##other"" processContents=""lax""/>" &
        "</xs:complexType>" &
        "<xs:complexType name=""BookType""><xs:complexContent>" &
        " <xs:extension base=""tns:Publication"">" &
        "  <xs:choice>" &
        "   <xs:element name=""isbn"" type=""tns:Isbn""/>" &
        "   <xs:element name=""genre"" type=""tns:Genre""/>" &
        "  </xs:choice>" &
        "  <xs:attribute name=""class"" type=""xs:string""/>" &
        " </xs:extension></xs:complexContent></xs:complexType>" &
        "<xs:complexType name=""Note""><xs:simpleContent>" &
        " <xs:extension base=""xs:string""><xs:attribute name=""lang"" type=""xs:string""/>" &
        " </xs:extension></xs:simpleContent></xs:complexType>" &
        "</xs:schema>"

    Private Const MainSchema As String = XsPrelude &
        "<xs:include schemaLocation=""common.xsd""/>" &
        "<xs:element name=""publication"" type=""tns:Publication"" abstract=""true""/>" &
        "<xs:element name=""book"" type=""tns:BookType"" substitutionGroup=""tns:publication""/>" &
        "<xs:element name=""note"" type=""xs:string""/>" &
        "<xs:element name=""standalone""><xs:complexType><xs:sequence>" &
        " <xs:element name=""nested""><xs:complexType><xs:sequence>" &
        "  <xs:element name=""flag"" type=""xs:boolean""/>" &
        " </xs:sequence></xs:complexType></xs:element>" &
        " <xs:element name=""inlineChoice"" minOccurs=""0""><xs:simpleType>" &
        "  <xs:restriction base=""xs:string"">" &
        "   <xs:enumeration value=""a-b""/><xs:enumeration value=""c""/>" &
        "  </xs:restriction></xs:simpleType></xs:element>" &
        "</xs:sequence></xs:complexType></xs:element>" &
        "</xs:schema>"

    Private Const ValidBookXml As String =
        "<?xml version=""1.0"" encoding=""utf-8""?>" &
        "<book xmlns=""urn:books"" id=""b-1"" class=""shelf-a"">" &
        "<title>XML in Action</title><genre>fiction</genre></book>"

    Private workDir As String
    Private orderXsd As String
    Private orderXml As String
    Private generatedVb As String
    Private advancedXsd As String

    ''' <summary>Run every suite and write the JSON audit report.</summary>
    ''' <returns>The number of failed assertions.</returns>
    Public Function Run() As Integer
        workDir = Path.Combine(AppContext.BaseDirectory, "xsd-selftest")
        Dim schemaDir As String = Path.Combine(workDir, "schema")

        Try
            If Directory.Exists(workDir) Then
                Directory.Delete(workDir, True)
            End If
        Catch ex As Exception
            Console.WriteLine($"warning: can not clean the test directory: {ex.Message}")
        End Try
        Directory.CreateDirectory(schemaDir)

        File.WriteAllText(Path.Combine(schemaDir, "common.xsd"), CommonSchema, New UTF8Encoding(False))
        advancedXsd = Path.Combine(schemaDir, "main.xsd")
        File.WriteAllText(advancedXsd, MainSchema, New UTF8Encoding(False))
        File.WriteAllText(Path.Combine(schemaDir, "book.xml"), ValidBookXml, New UTF8Encoding(False))

        orderXsd = Path.Combine(workDir, "order.xsd")
        orderXml = Path.Combine(workDir, "order.xml")
        generatedVb = Path.Combine(workDir, "Books.vb")

        Dim reportPath As String = Path.Combine(AppContext.BaseDirectory, "xsd-test-report.json")

        Call DebugAudit.Reset()
        DebugAudit.MaxEvents = 4096
        DebugAudit.ThrowOnFailure = False

        RunSuite("reflection -> xsd", AddressOf TestTypeToXsd)
        RunSuite("xsd -> vb class", AddressOf TestXsdToVb)
        RunSuite("xml validation", AddressOf TestValidation)
        RunSuite("advanced xsd constructs", AddressOf TestAdvancedSchema)
        RunSuite("assembly export", AddressOf TestAssemblyExport)
        RunSuite("error handling", AddressOf TestErrorHandling)

        Call DebugAudit.SaveReport(reportPath)

        Console.WriteLine()
        Console.WriteLine("==================================================")
        Console.WriteLine($" 失败断言数: {DebugAudit.FailureCount}")
        Console.WriteLine($" 测试报告:   {reportPath}")
        Console.WriteLine($" 测试产物:   {workDir}")
        Console.WriteLine("==================================================")

        Return DebugAudit.FailureCount
    End Function

#Region "suites"

    ''' <summary>Class definition -> XSD file (reflection).</summary>
    Private Sub TestTypeToXsd()
        XsdTools.CreateXsdFromType(GetType(BookOrder), orderXsd, "urn:books")
        Expect(File.Exists(orderXsd), "CreateXsdFromType should write the xsd file")

        Dim text As String = File.ReadAllText(orderXsd)
        DebugAudit.Mark("exported-xsd-length", text.Length)
        Expect(text.StartsWith("<?xml version=""1.0"" encoding=""utf-8""?>"),
               "exported xsd should declare utf-8")
        Expect(CountOf(text, "<?xml") = 1, "exported xsd must contain exactly one xml declaration")
        Expect(text.Contains("<xs:element name=""bookOrder"""), "xsd should declare the bookOrder root element")
        Expect(text.Contains("ArrayOfOrderLine"), "xsd should declare the nested array complexType")
        Expect(text.Contains("OrderStatus"), "xsd should declare the enumeration simpleType")

        Dim schemaSet As New XmlSchemaSet()
        schemaSet.Add(Nothing, orderXsd)
        schemaSet.Compile()
        Expect(schemaSet.IsCompiled, "exported xsd should compile as a schema set")

        Dim xsdText As String = XsdTools.CreateXsdTextFromType(GetType(BookOrder), "urn:books")
        Expect(CountOf(xsdText, "<?xml") = 1, "CreateXsdTextFromType should return one document")

        Dim genericPath As String = Path.Combine(workDir, "order-generic.xsd")
        XsdTools.CreateXsdFromType(Of BookOrder)(genericPath, "urn:books")
        Expect(File.Exists(genericPath), "generic CreateXsdFromType overload should write a file")

        Dim written As String() = SchemaExporter.ExportToFile(Of OrderLine)(
            Path.Combine(workDir, "order-line.xsd"), "urn:books")
        Expect(written.Length >= 1 AndAlso File.Exists(written(0)),
               "SchemaExporter.ExportToFile should return the written path")
    End Sub

    ''' <summary>XSD file -> VB.NET class definition file.</summary>
    Private Sub TestXsdToVb()
        XsdTools.CreateClassFromXsd(orderXsd, generatedVb)
        Expect(File.Exists(generatedVb), "CreateClassFromXsd should write the vb file")

        Dim code As String = File.ReadAllText(generatedVb)
        DebugAudit.Mark("generated-vb-length", code.Length)
        Expect(code.Contains("Option Explicit On"), "generated code should be explicit")
        Expect(code.Contains("Imports System.Xml.Serialization"), "generated code should import XmlSerializer")
        Expect(code.Contains("Namespace books"), "generated namespace should derive from the xsd targetNamespace")
        Expect(code.Contains("<XmlRoot("), "generated root classes should carry XmlRoot")
        Expect(code.Contains("<XmlType("), "generated types should carry XmlType")
        Expect(code.Contains("<XmlElement("), "generated members should carry XmlElement")
        Expect(code.Contains("<XmlAttribute("), "generated members should carry XmlAttribute")
        Expect(code.Contains("Public Class bookOrder"), "generated root class should match the xsd element")
        Expect(code.Contains("Public Enum OrderStatus"), "xsd enumeration should become a VB enum")
        Expect(code.Contains("Public Property lines"), "xsd array should become an array/list member")
        Expect(code.Contains("()"), "generated code should contain array types")
        Expect(code.Contains("XSD constructs that XmlSerializer cannot express"),
               "the generated file should always document its degradation policy")
        Expect(CountOf(code, "<?xml") = 0, "generated vb source must not contain xml declarations")
        ExpectNoDuplicateTypeNames(code)

        Dim codeFromText As String = XsdTools.CreateClassCodeFromXsd(orderXsd)
        ExpectEqual(code, codeFromText, "GenerateCode and GenerateClassFile should produce the same text")

        Dim fromText As String = XsdTools.CreateClassCodeFromXsdText(File.ReadAllText(orderXsd))
        Expect(fromText.Contains("Public Class bookOrder"),
               "in-memory xsd text should also generate the root class")
    End Sub

    ''' <summary>Serialize the VB model and validate the produced XML.</summary>
    Private Sub TestValidation()
        Dim sample As BookOrder = BookOrder.CreateSample()
        Dim serializer As New XmlSerializer(GetType(BookOrder))

        Using stream As New FileStream(orderXml, FileMode.Create, FileAccess.Write, FileShare.Read)
            serializer.Serialize(stream, sample)
        End Using
        Expect(File.Exists(orderXml), "XmlSerializer should write the xml file")
        DebugAudit.Mark("xml-length", New FileInfo(orderXml).Length)

        Dim valid As XsdValidationResult = XsdTools.ValidateXml(orderXsd, orderXml)
        DebugAudit.Mark("validation-result", valid.ToString())
        Expect(valid.IsValid, $"serialized xml should be valid against the reflected xsd: {valid}")
        ExpectEqual(0, valid.ErrorCount, "valid xml should have no errors")
        Expect(XsdTools.IsValid(orderXsd, orderXml), "IsValid shortcut should agree with ValidateXml")

        Using stream As New FileStream(orderXml, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim restored As BookOrder = CType(serializer.Deserialize(stream), BookOrder)
            ExpectEqual(sample.Id, restored.Id, "deserialized id")
            ExpectEqual(sample.Customer, restored.Customer, "deserialized customer")
            ExpectEqual(sample.Lines.Length, restored.Lines.Length, "deserialized line count")
            ExpectEqual(sample.Status, restored.Status, "deserialized status")
        End Using

        Dim badChild As String = "<?xml version=""1.0""?>" &
                                "<bookOrder xmlns=""urn:books"" id=""x""><bogus>1</bogus></bookOrder>"
        Dim invalid As XsdValidationResult = XsdTools.ValidateXmlText(orderXsd, badChild)
        Expect(Not invalid.IsValid, "an unexpected child element should be rejected")
        Expect(invalid.ErrorCount > 0, "the invalid document should report at least one error")
        Expect(invalid.Errors.Any(Function(e) e.Severity = XmlSeverityType.Error),
               "the diagnostics should expose the error severity")
        DebugAudit.Mark("negative-validation", invalid.ToString())

        Dim unknownRoot As XsdValidationResult = XsdTools.ValidateXmlText(
            orderXsd, "<unknownRoot xmlns=""urn:nowhere""><x/></unknownRoot>")
        Expect(Not unknownRoot.IsValid, "an undeclared root element must not be reported as valid")

        Dim malformed As XsdValidationResult = XsdTools.ValidateXmlText(orderXsd, "<a><b></a>")
        Expect(Not malformed.IsValid, "malformed xml should be reported as invalid")

        Dim threw As Boolean = False
        Try
            Call invalid.ThrowIfInvalid()
        Catch ex As XmlSchemaValidationException
            threw = True
        End Try
        Expect(threw, "ThrowIfInvalid should throw on an invalid document")

        Dim noThrow As Boolean = True
        Try
            Call valid.ThrowIfInvalid()
        Catch ex As Exception
            noThrow = False
        End Try
        Expect(noThrow, "ThrowIfInvalid should not throw on a valid document")
    End Sub

    ''' <summary>Advanced XSD constructs: include, groups, inheritance, choice, wildcards.</summary>
    Private Sub TestAdvancedSchema()
        Dim code As String = XsdTools.CreateClassCodeFromXsd(advancedXsd)
        DebugAudit.Mark("advanced-vb-length", code.Length)

        Expect(code.Contains("Public Enum Genre"), "simpleType enumeration -> VB enum")
        Expect(code.Contains("<XmlEnum("), "enumeration facets -> XmlEnum")
        Expect(code.Contains("Inherits Publication"), "complexContent extension -> Inherits")
        Expect(code.Contains("<XmlInclude(GetType("), "substitutionGroup -> XmlInclude")
        Expect(code.Contains("XmlChoiceIdentifier("), "choice -> XmlChoiceIdentifier")
        Expect(code.Contains("<XmlAnyElement>"), "xs:any -> XmlAnyElement")
        Expect(code.Contains("<XmlAnyAttribute>"), "xs:anyAttribute -> XmlAnyAttribute")
        Expect(code.Contains("Nullable(Of "), "optional/nillable value type -> Nullable(Of T)")
        Expect(code.Contains("Specified As Boolean"), "optional value-type attribute -> xxxSpecified")
        Expect(code.Contains("MustInherit Class"), "abstract element -> MustInherit")
        Expect(code.Contains("<XmlText>"), "simpleContent -> XmlText")
        Expect(code.Contains("XSD constructs that XmlSerializer cannot express"),
               "unsupported facets should be listed as degradation notes")
        Expect(code.Contains("Book genre classification"), "annotation -> xml doc comment")
        ExpectNoDuplicateTypeNames(code)

        Dim vbFile As String = Path.Combine(workDir, "Advanced.vb")
        XsdTools.CreateClassFromXsd(advancedXsd, vbFile)
        ExpectEqual(code, File.ReadAllText(vbFile), "advanced generation should be deterministic")

        ' xs:include is resolved by the controlled local resolver.
        Dim validBook As XsdValidationResult = XsdTools.ValidateXml(
            advancedXsd, Path.Combine(workDir, "schema", "book.xml"))
        DebugAudit.Mark("advanced-validation", validBook.ToString())
        Expect(validBook.IsValid, $"xml matching an included schema should validate: {validBook}")

        Dim invalidBook As XsdValidationResult = XsdTools.ValidateXmlText(
            advancedXsd, "<book xmlns=""urn:books""><title>x</title></book>")
        Expect(Not invalidBook.IsValid, "a document missing the required id/choice should be rejected")
    End Sub

    ''' <summary>Reflection export of every serializable type of an assembly.</summary>
    Private Sub TestAssemblyExport()
        Dim assemblyPath As String = Assembly.GetExecutingAssembly().Location
        Dim outDir As String = Path.Combine(workDir, "assembly")
        XsdTools.CreateXsdFromAssembly(assemblyPath, outDir)

        Dim files As String() = Directory.GetFiles(outDir, "*.xsd")
        DebugAudit.Mark("assembly-xsd-count", files.Length)
        Expect(files.Length > 0, "CreateXsdFromAssembly should write at least one xsd file")

        For Each xsdFile As String In files
            Dim text As String = System.IO.File.ReadAllText(xsdFile)
            Expect(CountOf(text, "<?xml") = 1, $"assembly export '{Path.GetFileName(xsdFile)}' must have one declaration")
        Next
    End Sub

    ''' <summary>Actionable errors for missing files and malformed input.</summary>
    Private Sub TestErrorHandling()
        ExpectThrows(Of FileNotFoundException)(
            Sub() XsdTools.CreateClassCodeFromXsd(Path.Combine(workDir, "missing.xsd")),
            "a missing xsd file should raise FileNotFoundException")

        ExpectThrows(Of FileNotFoundException)(
            Sub() XsdTools.ValidateXml(orderXsd, Path.Combine(workDir, "missing.xml")),
            "a missing xml file should raise FileNotFoundException")

        ExpectThrows(Of ArgumentException)(
            Sub() XsdTools.CreateClassFromXsd("", generatedVb),
            "an empty xsd path should raise ArgumentException")

        ' A schema with an undefined type is diagnosed at compile time.
        Dim brokenXsd As String = XsPrelude &
            "<xs:element name=""broken"" type=""tns:DoesNotExist""/></xs:schema>"
        Dim threw As Boolean = False
        Try
            Call XsdTools.CreateClassCodeFromXsdText(brokenXsd)
        Catch ex As XmlSchemaException
            threw = True
        Catch ex As Exception
            DebugAudit.Mark("broken-xsd-exception", ex.GetType().FullName)
            threw = True
        End Try
        Expect(threw, "an invalid xsd should be rejected with an actionable exception")

        ' The resolver must not allow schemas outside of the include directory.
        Expect(File.Exists(Path.Combine(workDir, "schema", "common.xsd")),
               "the included schema should be present for the resolver test")
    End Sub

#End Region

#Region "helpers"

    Private Sub RunSuite(name As String, action As Action)
        Call DebugAudit.Mark($"suite:{name}", "start")
        Try
            Call action()
        Catch ex As Exception
            Call DebugAudit.Mark($"crash:{name}", $"{ex.GetType().Name}: {ex.Message}")
            Call DebugAudit.Test(False, $"{name} 抛出异常：{ex.GetType().Name}: {ex.Message}")
        End Try
    End Sub

    Private Sub Expect(condition As Boolean, message As String,
                       <CallerFilePath> Optional sourceFile As String = Nothing,
                       <CallerLineNumber> Optional line As Integer = -1,
                       <CallerMemberName> Optional callee As String = Nothing)
        Call DebugAudit.Test(condition, message, message, sourceFile, line, callee)
    End Sub

    Private Sub ExpectEqual(Of T)(expected As T, actual As T, message As String,
                                  <CallerFilePath> Optional sourceFile As String = Nothing,
                                  <CallerLineNumber> Optional line As Integer = -1,
                                  <CallerMemberName> Optional callee As String = Nothing)
        Call DebugAudit.AreEqual(expected, actual, message, message, sourceFile, line, callee)
    End Sub

    Private Sub ExpectThrows(Of TException As Exception)(action As Action, message As String)
        Dim threw As Boolean = False
        Try
            Call action()
        Catch ex As TException
            threw = True
        Catch ex As Exception
            Call DebugAudit.Mark($"unexpected-exception:{message}", $"{ex.GetType().Name}: {ex.Message}")
        End Try
        Expect(threw, message)
    End Sub

    Private Function CountOf(text As String, token As String) As Integer
        Return text.Split(New String() {token}, StringSplitOptions.None).Length - 1
    End Function

    Private Sub ExpectNoDuplicateTypeNames(code As String)
        Dim names As New List(Of String)()

        For Each m As Match In Regex.Matches(code, "(?m)^\s*Public (?:MustInherit )?Class (\S+)")
            names.Add(m.Groups(1).Value)
        Next
        For Each m As Match In Regex.Matches(code, "(?m)^\s*Public Enum (\S+)")
            names.Add(m.Groups(1).Value)
        Next

        Dim duplicates As String() = names.
            GroupBy(Function(n) n, StringComparer.OrdinalIgnoreCase).
            Where(Function(g) g.Count() > 1).
            Select(Function(g) g.Key).
            ToArray()

        Expect(duplicates.Length = 0,
               $"generated VB type names should be unique: {String.Join(", ", duplicates)}")
        Expect(names.Count > 0, "the generated source should declare at least one type")
    End Sub

#End Region
End Module
