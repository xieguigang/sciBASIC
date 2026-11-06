#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd type mapping"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Xml
Imports System.Xml.Schema

''' <summary>
''' Translates XSD built-in types and XSD names into VB.NET source text.
''' </summary>
Friend Module XsdTypeMap

    ''' <summary>The W3C XML Schema namespace.</summary>
    Public Const XsdNamespace As String = "http://www.w3.org/2001/XMLSchema"

    Private Const XmlnsNamespace As String = "http://www.w3.org/XML/1998/namespace"
    Private Const XsiNamespace As String = "http://www.w3.org/2001/XMLSchema-instance"

    Private ReadOnly keywords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "AddHandler", "AddressOf", "Alias", "And", "AndAlso", "As", "Boolean", "ByRef", "Byte", "ByVal",
        "Call", "Case", "Catch", "CBool", "CByte", "CChar", "CDate", "CDbl", "CDec", "Char", "CInt",
        "Class", "CLng", "CObj", "Const", "Continue", "CSByte", "CShort", "CSng", "CStr", "CType",
        "CUInt", "CULng", "CUShort", "Date", "Decimal", "Declare", "Default", "Delegate", "Dim",
        "DirectCast", "Do", "Double", "Each", "Else", "ElseIf", "End", "EndIf", "Enum", "Erase",
        "Error", "Event", "Exit", "False", "Finally", "For", "Friend", "Function", "Get", "GetType",
        "GetXMLNamespace", "Global", "GoSub", "GoTo", "Handles", "If", "Implements", "Imports", "In",
        "Inherits", "Integer", "Interface", "Is", "IsNot", "Let", "Lib", "Like", "Long", "Loop", "Me",
        "Mod", "Module", "MustInherit", "My", "MyBase", "MyClass", "Namespace", "Narrowing", "New",
        "Next", "Not", "Nothing", "NotInheritable", "NotOverridable", "Object", "Of", "On", "Operator",
        "Option", "Optional", "Or", "OrElse", "Overloads", "Overridable", "Overrides", "ParamArray",
        "Partial", "Private", "Property", "Protected", "Public", "RaiseEvent", "ReadOnly", "ReDim",
        "REM", "RemoveHandler", "Resume", "Return", "SByte", "Select", "Set", "Shadows", "Shared",
        "Short", "Single", "Static", "Step", "Stop", "String", "Structure", "Sub", "SyncLock", "Then",
        "Throw", "To", "True", "Try", "TryCast", "TypeOf", "UInteger", "ULong", "UShort", "Using",
        "Variant", "Wend", "When", "While", "Widening", "With", "WithEvents", "WriteOnly", "Xor",
        "Aggregate", "Ansi", "Assembly", "Async", "Auto", "Await", "Binary", "Compare", "Custom",
        "Distinct", "Equals", "Explicit", "From", "Group", "Infer", "Into", "IsFalse", "IsTrue",
        "Iterator", "Join", "Key", "Mid", "NameOf", "Off", "Order", "Preserve", "Skip", "Strict",
        "Take", "Text", "Unicode", "Until", "Where", "Yield"
    }

    Private ReadOnly primitiveValueTypes As New HashSet(Of String)(StringComparer.Ordinal) From {
        "Boolean", "Byte", "SByte", "Short", "UShort", "Integer", "UInteger", "Long", "ULong",
        "Single", "Double", "Decimal", "Date", "Char"
    }

    ''' <summary>True when the QName belongs to the W3C schema namespace.</summary>
    Public Function IsBuiltIn(qname As XmlQualifiedName) As Boolean
        Return qname IsNot Nothing AndAlso qname.Namespace = XsdNamespace
    End Function

    ''' <summary>True for names owned by the XML/schema infrastructure itself.</summary>
    Public Function IsReservedNamespace(ns As String) As Boolean
        Return ns = XsdNamespace OrElse ns = XsiNamespace OrElse ns = XmlnsNamespace
    End Function

    ''' <summary>True when the name is a VB keyword (or a contextual keyword).</summary>
    Public Function IsKeyword(name As String) As Boolean
        Return name IsNot Nothing AndAlso keywords.Contains(name)
    End Function

    ''' <summary>True when the VB type name denotes a value type that supports Nullable(Of T).</summary>
    Public Function IsPrimitiveValueType(vbType As String) As Boolean
        Return vbType IsNot Nothing AndAlso primitiveValueTypes.Contains(vbType)
    End Function

    ''' <summary>
    ''' Map a compiled <see cref="XmlSchemaDatatype"/> to a VB.NET type name.
    ''' </summary>
    ''' <remarks>
    ''' Using the compiled CLR value type instead of the lexical QName also covers
    ''' user defined simpleType restrictions, lists and unions: their Datatype is
    ''' already resolved by <see cref="XmlSchemaSet.Compile"/>.
    ''' </remarks>
    Public Function VbTypeOf(datatype As XmlSchemaDatatype) As String
        If datatype Is Nothing OrElse datatype.ValueType Is Nothing Then
            Return "String"
        End If

        Select Case datatype.ValueType.FullName
            Case "System.String" : Return "String"
            Case "System.Boolean" : Return "Boolean"
            Case "System.Int32" : Return "Integer"
            Case "System.Int64" : Return "Long"
            Case "System.Int16" : Return "Short"
            Case "System.Byte" : Return "Byte"
            Case "System.SByte" : Return "SByte"
            Case "System.UInt16" : Return "UShort"
            Case "System.UInt32" : Return "UInteger"
            Case "System.UInt64" : Return "ULong"
            Case "System.Decimal" : Return "Decimal"
            Case "System.Single" : Return "Single"
            Case "System.Double" : Return "Double"
            Case "System.DateTime" : Return "Date"
            Case "System.Char" : Return "Char"
            Case "System.Byte[]" : Return "Byte()"
            Case "System.String[]" : Return "String()"
            Case "System.Object" : Return "Object"
            Case "System.Xml.XmlQualifiedName" : Return "System.Xml.XmlQualifiedName"
            Case Else
                ' xs:time / xs:duration / list / union cannot be represented
                ' faithfully by XmlSerializer, degrade them to text.
                If datatype.ValueType.IsArray Then
                    Return "String()"
                End If
                Return "String"
        End Select
    End Function

    ''' <summary>Build a legal VB identifier out of an arbitrary XML name.</summary>
    Public Function ToIdentifier(raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then
            Return "Item"
        End If

        Dim sb As New StringBuilder(raw.Length)

        For Each ch As Char In raw.Trim()
            If Char.IsLetterOrDigit(ch) OrElse ch = "_"c Then
                sb.Append(ch)
            Else
                sb.Append("_"c)
            End If
        Next

        Dim result As String = sb.ToString()
        If result.Length = 0 Then
            Return "Item"
        End If
        If Char.IsDigit(result(0)) Then
            result = "_" & result
        End If

        Return result
    End Function

    ''' <summary>Wrap a VB keyword in square brackets so it can be used as an identifier.</summary>
    Public Function EscapeIdentifier(identifier As String) As String
        If IsKeyword(identifier) Then
            Return $"[{identifier}]"
        End If
        Return identifier
    End Function

    ''' <summary>Derive a VB class/enum name from an XSD qualified name.</summary>
    Public Function ToTypeName(qname As XmlQualifiedName) As String
        If qname Is Nothing OrElse String.IsNullOrEmpty(qname.Name) Then
            Return "GeneratedType"
        End If
        Return EscapeIdentifier(ToIdentifier(qname.Name))
    End Function

    ''' <summary>
    ''' Derive a VB namespace name from the XSD targetNamespace, e.g. urn:books -> books.
    ''' </summary>
    Public Function DeriveCodeNamespace(schemas As XmlSchemaSet) As String
        Dim target As String = Nothing

        For Each schema As XmlSchema In schemas.Schemas()
            If Not String.IsNullOrEmpty(schema.TargetNamespace) Then
                target = schema.TargetNamespace
                Exit For
            End If
        Next

        If String.IsNullOrEmpty(target) Then
            Return "XsdGenerated"
        End If

        Dim parts As String() = target.Split({"/"c, ":"c, "#"c, "."c, "\"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim candidate As String = If(parts.Length = 0, target, parts(parts.Length - 1))
        Dim identifier As String = ToIdentifier(candidate)

        If IsKeyword(identifier) Then
            identifier &= "Schema"
        End If

        Return identifier
    End Function

    ''' <summary>Emit a VB string literal, escaping quotes and stripping control characters.</summary>
    Public Function StringLiteral(value As String) As String
        If value Is Nothing Then
            Return """"""
        End If

        Dim cleaned As New StringBuilder(value.Length + 2)
        For Each ch As Char In value
            If Char.IsControl(ch) Then
                cleaned.Append(" "c)
            Else
                cleaned.Append(ch)
            End If
        Next

        Return """" & cleaned.ToString().Replace("""", """""") & """"
    End Function

    ''' <summary>Emit a VB Boolean literal.</summary>
    Public Function BooleanLiteral(value As Boolean) As String
        Return If(value, "True", "False")
    End Function

    ''' <summary>
    ''' Split arbitrary XSD documentation into lines that are safe to place
    ''' inside an XML documentation comment.
    ''' </summary>
    Public Function DocLines(text As String) As IEnumerable(Of String)
        If String.IsNullOrWhiteSpace(text) Then
            Return Enumerable.Empty(Of String)()
        End If

        Dim safe As String = text.
            Replace("&", "&amp;").
            Replace("<", "&lt;").
            Replace(">", "&gt;").
            Replace("--", "- -")

        Return safe.
            Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.None).
            Select(Function(line) line.Trim()).
            Where(Function(line) line.Length > 0).
            ToArray()
    End Function
End Module
