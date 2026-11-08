#Region "Microsoft.VisualBasic.MIME.application.xml :: xsd to VB source emitter"

' Copyright (c) sciBASIC.NET Foundation
' Licensed under the GNU General Public License v3.0 or later (GPL-3.0-or-later)
' See <http://www.gnu.org/licenses/> for the full license text.

#End Region

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Xml
Imports System.Xml.Schema

''' <summary>
''' Emits VB.NET class definitions from a compiled <see cref="XmlSchemaSet"/>.
''' </summary>
''' <remarks>
''' The compiled form is used on purpose: after <c>Compile()</c> the
''' <c>ContentTypeParticle</c> and <c>AttributeUses</c> already contain the
''' expanded xs:group / xs:attributeGroup references and the members inherited
''' through xs:extension, so include/import and inheritance need no extra parsing.
''' Constructs that <see cref="System.Xml.Serialization.XmlSerializer"/> cannot
''' express are degraded to comments instead of producing invalid code.
''' </remarks>
Friend NotInheritable Class SchemaModelBuilder

    Private ReadOnly schemas As XmlSchemaSet
    Private ReadOnly codeNamespace As String
    Private ReadOnly output As New StringBuilder()
    Private ReadOnly notes As New List(Of String)()

    Private ReadOnly globalTypes As New Dictionary(Of XmlQualifiedName, XmlSchemaType)()
    Private ReadOnly complexTypes As New Dictionary(Of XmlQualifiedName, XmlSchemaComplexType)()
    Private ReadOnly enumTypes As New Dictionary(Of XmlQualifiedName, XmlSchemaSimpleType)()
    Private ReadOnly typeNames As New Dictionary(Of XmlQualifiedName, String)()

    Private ReadOnly anonymousNames As New Dictionary(Of XmlSchemaType, String)()
    Private ReadOnly anonymousComplex As New List(Of KeyValuePair(Of String, XmlSchemaComplexType))()
    Private ReadOnly anonymousEnums As New List(Of KeyValuePair(Of String, XmlSchemaSimpleType))()

    Private ReadOnly elements As New List(Of XmlSchemaElement)()
    Private ReadOnly elementNames As New Dictionary(Of XmlQualifiedName, String)()
    Private ReadOnly xmlIncludes As New Dictionary(Of XmlQualifiedName, List(Of String))()

    Private ReadOnly usedTypeNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly enumTypeNames As New HashSet(Of String)(StringComparer.Ordinal)
    Private ReadOnly choiceEnums As New List(Of ChoiceEnumSpec)()
    Private ReadOnly emittedClasses As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <param name="schemas">A compiled schema set.</param>
    ''' <param name="codeNamespace">The VB namespace to emit, or Nothing for the global namespace.</param>
    Public Sub New(schemas As XmlSchemaSet, codeNamespace As String)
        If schemas Is Nothing Then
            Throw New ArgumentNullException(NameOf(schemas))
        End If
        If Not schemas.IsCompiled Then
            Throw New ArgumentException("The schema set must be compiled before generating code.", NameOf(schemas))
        End If

        Me.schemas = schemas
        Me.codeNamespace = codeNamespace
    End Sub

    ''' <summary>Generate the complete VB.NET source file.</summary>
    Public Function Build() As String
        PreScan()

        EmitHeader()

        Dim indent As String = If(String.IsNullOrEmpty(codeNamespace), "", "    ")
        If Not String.IsNullOrEmpty(codeNamespace) Then
            output.AppendLine($"Namespace {codeNamespace}")
            output.AppendLine()
        End If

        For Each q As XmlQualifiedName In SortedKeys(enumTypes.Keys)
            EmitEnum(enumTypes(q), q, typeNames(q), indent)
        Next
        For Each kv As KeyValuePair(Of String, XmlSchemaSimpleType) In anonymousEnums
            EmitEnum(kv.Value, Nothing, kv.Key, indent)
        Next

        For Each q As XmlQualifiedName In SortedKeys(complexTypes.Keys)
            EmitComplexType(complexTypes(q), q, typeNames(q), False, indent)
        Next
        For Each kv As KeyValuePair(Of String, XmlSchemaComplexType) In anonymousComplex
            EmitComplexType(kv.Value, Nothing, kv.Key, False, indent)
        Next

        For Each el As XmlSchemaElement In elements
            EmitGlobalElement(el, indent)
        Next

        For Each spec As ChoiceEnumSpec In choiceEnums
            EmitChoiceEnum(spec, indent)
        Next

        EmitNotes(indent)

        If Not String.IsNullOrEmpty(codeNamespace) Then
            output.AppendLine($"End Namespace")
        End If

        Return output.ToString()
    End Function

    Private Shared Function SortedKeys(source As IEnumerable(Of XmlQualifiedName)) As List(Of XmlQualifiedName)
        Dim keys As List(Of XmlQualifiedName) = source.ToList()
        keys.Sort(Function(a, b) String.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal))
        Return keys
    End Function

#Region "pre scan"

    Private Sub PreScan()
        Dim typeKeys As New List(Of XmlQualifiedName)()

        For Each de As DictionaryEntry In schemas.GlobalTypes
            Dim q As XmlQualifiedName = CType(de.Key, XmlQualifiedName)
            If q Is Nothing OrElse XsdTypeMap.IsReservedNamespace(q.Namespace) Then
                Continue For
            End If

            Dim schemaType As XmlSchemaType = TryCast(de.Value, XmlSchemaType)
            If schemaType Is Nothing Then
                Continue For
            End If

            globalTypes(q) = schemaType
            typeKeys.Add(q)
        Next

        For Each q As XmlQualifiedName In SortedKeys(typeKeys)
            Dim schemaType As XmlSchemaType = globalTypes(q)

            If TypeOf schemaType Is XmlSchemaComplexType Then
                complexTypes(q) = DirectCast(schemaType, XmlSchemaComplexType)
                typeNames(q) = AllocateName(q.Name)
            Else
                Dim simple As XmlSchemaSimpleType = TryCast(schemaType, XmlSchemaSimpleType)
                If simple IsNot Nothing AndAlso HasEnumerationFacet(simple) Then
                    enumTypes(q) = simple
                    Dim name As String = AllocateName(q.Name)
                    typeNames(q) = name
                    enumTypeNames.Add(name)
                Else
                    AddNote($"simpleType '{q}' is a restriction/list/union; its facets are not enforced by XmlSerializer and the members use the underlying CLR type.")
                End If
            End If
        Next

        For Each de As DictionaryEntry In schemas.GlobalElements
            Dim q As XmlQualifiedName = CType(de.Key, XmlQualifiedName)
            If q Is Nothing OrElse XsdTypeMap.IsReservedNamespace(q.Namespace) Then
                Continue For
            End If
            Dim el As XmlSchemaElement = TryCast(de.Value, XmlSchemaElement)
            If el IsNot Nothing Then
                elements.Add(el)
            End If
        Next
        elements.Sort(Function(a, b) String.Compare(a.QualifiedName.ToString(), b.QualifiedName.ToString(), StringComparison.Ordinal))

        For Each el As XmlSchemaElement In elements
            elementNames(el.QualifiedName) = AllocateName(el.QualifiedName.Name)
        Next

        For Each q As XmlQualifiedName In SortedKeys(complexTypes.Keys)
            ScanComplexMembers(complexTypes(q), XsdTypeMap.ToIdentifier(q.Name))
        Next
        For Each el As XmlSchemaElement In elements
            If el.SchemaType IsNot Nothing Then
                RegisterAnonymous(el.SchemaType, XsdTypeMap.ToIdentifier(el.QualifiedName.Name) & "Type")
            End If
        Next

        ScanSubstitutionGroups()
        ScanGlobalAttributes()
        ScanConstraints()
    End Sub

    Private Sub ScanSubstitutionGroups()
        For Each el As XmlSchemaElement In elements
            If el.SubstitutionGroup Is Nothing OrElse el.SubstitutionGroup.IsEmpty Then
                Continue For
            End If

            Dim head As XmlSchemaElement = Nothing
            For Each candidate As XmlSchemaElement In elements
                If candidate.QualifiedName.ToString() = el.SubstitutionGroup.ToString() Then
                    head = candidate
                    Exit For
                End If
            Next

            Dim headQ As XmlQualifiedName = If(head Is Nothing OrElse head.ElementSchemaType Is Nothing,
                                               Nothing, head.ElementSchemaType.QualifiedName)
            Dim ownQ As XmlQualifiedName = If(el.ElementSchemaType Is Nothing,
                                              Nothing, el.ElementSchemaType.QualifiedName)

            If headQ Is Nothing OrElse headQ.IsEmpty OrElse Not complexTypes.ContainsKey(headQ) OrElse
               ownQ Is Nothing OrElse ownQ.IsEmpty OrElse Not complexTypes.ContainsKey(ownQ) Then
                AddNote($"substitutionGroup '{el.SubstitutionGroup}' on element '{el.QualifiedName}' cannot be mapped to an XmlInclude because the head/substitute type is anonymous.")
                Continue For
            End If
            If Not IsDerivedFrom(el.ElementSchemaType, headQ) Then
                AddNote($"substitutionGroup '{el.SubstitutionGroup}' on element '{el.QualifiedName}' was kept as documentation because its type does not derive from the head type.")
                Continue For
            End If

            If Not xmlIncludes.ContainsKey(headQ) Then
                xmlIncludes(headQ) = New List(Of String)()
            End If
            xmlIncludes(headQ).Add(elementNames(el.QualifiedName))
        Next
    End Sub

    Private Shared Function IsDerivedFrom(schemaType As XmlSchemaType, ancestor As XmlQualifiedName) As Boolean
        Dim current As XmlSchemaType = schemaType
        Dim depth As Integer = 0

        While current IsNot Nothing AndAlso depth < 64
            If current.QualifiedName IsNot Nothing AndAlso
               current.QualifiedName.ToString() = ancestor.ToString() Then
                Return True
            End If
            If current.QualifiedName IsNot Nothing AndAlso XsdTypeMap.IsBuiltIn(current.QualifiedName) Then
                Return False
            End If
            current = current.BaseXmlSchemaType
            depth += 1
        End While

        Return False
    End Function

    Private Sub ScanGlobalAttributes()
        For Each de As DictionaryEntry In schemas.GlobalAttributes
            Dim q As XmlQualifiedName = CType(de.Key, XmlQualifiedName)
            If q Is Nothing OrElse XsdTypeMap.IsReservedNamespace(q.Namespace) Then
                Continue For
            End If
            AddNote($"global attribute '{q}' has no standalone XmlSerializer class; it is emitted as a member wherever the referencing complexType uses it.")
        Next
    End Sub

    Private Sub ScanConstraints()
        For Each el As XmlSchemaElement In elements
            If el.Constraints IsNot Nothing AndAlso el.Constraints.Count > 0 Then
                AddNote($"identity constraints (xs:key / xs:keyref / xs:unique) on element '{el.QualifiedName}' are not expressible by XmlSerializer.")
            End If
        Next
    End Sub

    ''' <summary>
    ''' Walk the (already expanded) members of a complexType and register only
    ''' the inline anonymous types found inside it.
    ''' </summary>
    Private Sub ScanComplexMembers(ct As XmlSchemaComplexType, owner As String)
        If ct Is Nothing Then
            Return
        End If

        If ct.ContentTypeParticle IsNot Nothing Then
            CollectAnonymousFromParticle(ct.ContentTypeParticle, owner)
        End If
        For Each de As DictionaryEntry In ct.AttributeUses
            Dim attr As XmlSchemaAttribute = TryCast(de.Value, XmlSchemaAttribute)
            If attr IsNot Nothing AndAlso attr.SchemaType IsNot Nothing Then
                RegisterAnonymous(attr.SchemaType, owner & "Attribute")
            End If
        Next
    End Sub

    Private Sub CollectAnonymousFromComplex(ct As XmlSchemaComplexType, suggested As String)
        If ct Is Nothing OrElse anonymousNames.ContainsKey(ct) Then
            Return
        End If

        Dim name As String = AllocateName(suggested & "Type")
        anonymousNames(ct) = name
        anonymousComplex.Add(New KeyValuePair(Of String, XmlSchemaComplexType)(name, ct))
        ScanComplexMembers(ct, name)
    End Sub

    Private Sub CollectAnonymousFromParticle(particle As XmlSchemaParticle, owner As String)
        If particle Is Nothing Then
            Return
        End If

        Dim el As XmlSchemaElement = TryCast(particle, XmlSchemaElement)
        If el IsNot Nothing Then
            If el.SchemaType IsNot Nothing Then
                RegisterAnonymous(el.SchemaType, owner & "_" & XsdTypeMap.ToIdentifier(el.QualifiedName.Name))
            End If
            Return
        End If

        Dim groupRef As XmlSchemaGroupRef = TryCast(particle, XmlSchemaGroupRef)
        If groupRef IsNot Nothing Then
            If groupRef.Particle IsNot Nothing Then
                CollectAnonymousFromParticle(groupRef.Particle, owner)
            End If
            Return
        End If

        Dim groupBase As XmlSchemaGroupBase = TryCast(particle, XmlSchemaGroupBase)
        If groupBase IsNot Nothing Then
            For Each item As XmlSchemaObject In groupBase.Items
                Dim child As XmlSchemaParticle = TryCast(item, XmlSchemaParticle)
                If child IsNot Nothing Then
                    CollectAnonymousFromParticle(child, owner)
                End If
            Next
        End If
    End Sub

    Private Sub RegisterAnonymous(schemaType As XmlSchemaType, suggested As String)
        If schemaType Is Nothing OrElse anonymousNames.ContainsKey(schemaType) Then
            Return
        End If

        Dim complexDef As XmlSchemaComplexType = TryCast(schemaType, XmlSchemaComplexType)
        If complexDef IsNot Nothing Then
            CollectAnonymousFromComplex(complexDef, suggested)
            Return
        End If

        Dim simple As XmlSchemaSimpleType = TryCast(schemaType, XmlSchemaSimpleType)
        If simple IsNot Nothing AndAlso HasEnumerationFacet(simple) Then
            Dim name As String = AllocateName(suggested)
            anonymousNames(simple) = name
            anonymousEnums.Add(New KeyValuePair(Of String, XmlSchemaSimpleType)(name, simple))
            enumTypeNames.Add(name)
            Return
        End If

        AddNote($"inline simpleType '{suggested}' restricts its base type; facets are documented but not enforced by XmlSerializer.")
    End Sub

    Private Function AllocateName(raw As String) As String
        Dim baseName As String = XsdTypeMap.ToIdentifier(raw)
        Dim candidate As String = baseName
        Dim index As Integer = 1

        While Not usedTypeNames.Add(candidate)
            index += 1
            candidate = baseName & "_" & index.ToString()
        End While

        Return XsdTypeMap.EscapeIdentifier(candidate)
    End Function

    Private Shared Function HasEnumerationFacet(simple As XmlSchemaSimpleType) As Boolean
        Dim restriction As XmlSchemaSimpleTypeRestriction = TryCast(simple.Content, XmlSchemaSimpleTypeRestriction)
        If restriction Is Nothing Then
            Return False
        End If
        For Each facet As XmlSchemaFacet In restriction.Facets
            If TypeOf facet Is XmlSchemaEnumerationFacet Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Shared Function EnumerationValues(simple As XmlSchemaSimpleType) As List(Of String)
        Dim values As New List(Of String)()
        Dim restriction As XmlSchemaSimpleTypeRestriction = TryCast(simple.Content, XmlSchemaSimpleTypeRestriction)
        If restriction Is Nothing Then
            Return values
        End If
        For Each facet As XmlSchemaFacet In restriction.Facets
            Dim enumeration As XmlSchemaEnumerationFacet = TryCast(facet, XmlSchemaEnumerationFacet)
            If enumeration IsNot Nothing Then
                values.Add(If(enumeration.Value, ""))
            End If
        Next
        Return values
    End Function

    Private Shared Function FacetSummary(simple As XmlSchemaSimpleType) As String
        Dim restriction As XmlSchemaSimpleTypeRestriction = TryCast(simple.Content, XmlSchemaSimpleTypeRestriction)
        If restriction Is Nothing OrElse restriction.Facets.Count = 0 Then
            Return Nothing
        End If

        Dim parts As New List(Of String)()
        For Each facet As XmlSchemaFacet In restriction.Facets
            If TypeOf facet Is XmlSchemaEnumerationFacet Then
                Continue For
            End If
            parts.Add($"{facet.GetType().Name.Replace("XmlSchema", "").Replace("Facet", "")}={facet.Value}")
        Next

        If parts.Count = 0 Then
            Return Nothing
        End If
        Return $"XSD facets not enforced by XmlSerializer: {String.Join(", ", parts)}."
    End Function

#End Region

#Region "emit helpers"

    Private Sub EmitHeader()
        output.AppendLine("' -----------------------------------------------------------------------------")
        output.AppendLine("' This file was generated from an XML Schema definition by")
        output.AppendLine("' Microsoft.VisualBasic.MIME.application.xml.XsdTools")
        output.AppendLine("' Regenerating the file will overwrite manual changes.")
        output.AppendLine("' -----------------------------------------------------------------------------")
        output.AppendLine("Option Explicit On")
        output.AppendLine()
        output.AppendLine("Imports System")
        output.AppendLine("Imports System.Xml")
        output.AppendLine("Imports System.Xml.Schema")
        output.AppendLine("Imports System.Xml.Serialization")
        output.AppendLine()
    End Sub

    Private Sub EmitNotes(indent As String)
        output.AppendLine()
        output.AppendLine($"{indent}' =============================================================================")
        output.AppendLine($"{indent}' XSD constructs that XmlSerializer cannot express are documented below.")
        output.AppendLine($"{indent}' They are degraded to comments so the generated code stays compilable.")
        output.AppendLine($"{indent}' =============================================================================")

        For Each note As String In notes.Distinct()
            Dim safe As String = note.
                Replace(vbCrLf, " ").
                Replace(vbLf, " ").
                Replace(vbCr, " ")
            output.AppendLine($"{indent}' - {safe}")
        Next
        output.AppendLine()
    End Sub

    Private Sub EmitDoc(indent As String, text As String, Optional remark As String = Nothing)
        If String.IsNullOrWhiteSpace(text) AndAlso String.IsNullOrWhiteSpace(remark) Then
            Return
        End If

        output.AppendLine($"{indent}''' <summary>")
        For Each line As String In XsdTypeMap.DocLines(text)
            output.AppendLine($"{indent}''' {line}")
        Next
        If Not String.IsNullOrWhiteSpace(remark) Then
            For Each line As String In XsdTypeMap.DocLines(remark)
                output.AppendLine($"{indent}''' <para>{line}</para>")
            Next
        End If
        output.AppendLine($"{indent}''' </summary>")
    End Sub

    Private Shared Function DocumentationOf(annotated As XmlSchemaAnnotated) As String
        If annotated Is Nothing OrElse annotated.Annotation Is Nothing Then
            Return Nothing
        End If

        Dim sb As New StringBuilder()
        For Each item As XmlSchemaObject In annotated.Annotation.Items
            Dim doc As XmlSchemaDocumentation = TryCast(item, XmlSchemaDocumentation)
            If doc Is Nothing OrElse doc.Markup Is Nothing Then
                Continue For
            End If
            For Each node As XmlNode In doc.Markup
                sb.Append(node.InnerText)
            Next
            sb.Append(" "c)
        Next

        Return sb.ToString().Trim()
    End Function

    Private Sub AddNote(note As String)
        If Not String.IsNullOrWhiteSpace(note) Then
            notes.Add(note)
        End If
    End Sub

    Private Shared Function UniqueMember(members As HashSet(Of String), baseName As String) As String
        Dim candidate As String = baseName
        Dim index As Integer = 1

        While Not members.Add(candidate)
            index += 1
            candidate = baseName & "_" & index.ToString()
        End While

        Return candidate
    End Function

    Private Function IsValueType(vbType As String) As Boolean
        Return XsdTypeMap.IsPrimitiveValueType(vbType) OrElse enumTypeNames.Contains(vbType)
    End Function

    Private Function IncludesFor(q As XmlQualifiedName) As List(Of String)
        If q Is Nothing OrElse q.IsEmpty Then
            Return Nothing
        End If
        Dim list As List(Of String) = Nothing
        If xmlIncludes.TryGetValue(q, list) Then
            Return list
        End If
        Return Nothing
    End Function

    Private Sub EmitIncludes(indent As String, q As XmlQualifiedName)
        Dim list As List(Of String) = IncludesFor(q)
        If list Is Nothing Then
            Return
        End If
        For Each name As String In list.Distinct()
            output.AppendLine($"{indent}<XmlInclude(GetType({name}))>")
        Next
    End Sub

#End Region

#Region "emit types"

    Private Sub EmitEnum(simple As XmlSchemaSimpleType, q As XmlQualifiedName, name As String, indent As String)
        Dim values As List(Of String) = EnumerationValues(simple)
        Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        EmitDoc(indent, DocumentationOf(simple), FacetSummary(simple))
        If q IsNot Nothing AndAlso Not q.IsEmpty Then
            output.AppendLine($"{indent}<XmlType(TypeName:={XsdTypeMap.StringLiteral(q.Name)}, Namespace:={XsdTypeMap.StringLiteral(q.Namespace)})>")
        End If
        output.AppendLine($"{indent}Public Enum {name}")

        For i As Integer = 0 To values.Count - 1
            Dim raw As String = values(i)
            Dim memberBase As String = XsdTypeMap.ToIdentifier(raw)
            If memberBase = "Item" AndAlso Not String.IsNullOrEmpty(raw) Then
                memberBase = "Value"
            End If
            Dim member As String = memberBase
            Dim index As Integer = 1
            While Not used.Add(member)
                index += 1
                member = memberBase & "_" & index.ToString()
            End While

            output.AppendLine($"{indent}    <XmlEnum({XsdTypeMap.StringLiteral(raw)})>")
            output.AppendLine($"{indent}    {XsdTypeMap.EscapeIdentifier(member)}")
        Next

        output.AppendLine($"{indent}End Enum")
        output.AppendLine()
    End Sub

    Private Sub EmitChoiceEnum(spec As ChoiceEnumSpec, indent As String)
        output.AppendLine($"{indent}''' <summary>Discriminator for an xs:choice compositor.</summary>")
        output.AppendLine($"{indent}Public Enum {spec.Name}")
        For Each kv As KeyValuePair(Of String, String) In spec.Values
            output.AppendLine($"{indent}    <XmlEnum({XsdTypeMap.StringLiteral(kv.Value)})>")
            output.AppendLine($"{indent}    {XsdTypeMap.EscapeIdentifier(kv.Key)}")
        Next
        output.AppendLine($"{indent}End Enum")
        output.AppendLine()
    End Sub

    Private Sub EmitComplexType(ct As XmlSchemaComplexType,
                                q As XmlQualifiedName,
                                name As String,
                                isAbstract As Boolean,
                                indent As String)
        If Not emittedClasses.Add(name) Then
            Return
        End If
        EmitClassBody(ct, q, name, isAbstract, indent)
    End Sub

    Private Sub EmitClassBody(ct As XmlSchemaComplexType,
                              q As XmlQualifiedName,
                              name As String,
                              isAbstract As Boolean,
                              indent As String)
        Dim inherit As Boolean = ShouldInherit(ct)
        Dim baseClass As String = If(inherit, typeNames(ct.BaseXmlSchemaType.QualifiedName), Nothing)
        Dim exclusions As Exclusions = ComputeExclusions(ct)
        Dim members As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        EmitDoc(indent, DocumentationOf(ct))
        If q IsNot Nothing AndAlso Not q.IsEmpty Then
            output.AppendLine($"{indent}<XmlType(TypeName:={XsdTypeMap.StringLiteral(q.Name)}, Namespace:={XsdTypeMap.StringLiteral(q.Namespace)})>")
        End If
        EmitIncludes(indent, q)

        Dim modifier As String = If(isAbstract OrElse ct.IsAbstract, "Public MustInherit Class ", "Public Class ")
        output.AppendLine($"{indent}{modifier}{name}")
        If inherit Then
            output.AppendLine($"{indent}    Inherits {baseClass}")
        End If
        output.AppendLine()

        If ct.ContentTypeParticle IsNot Nothing Then
            EmitParticle(ct.ContentTypeParticle, name, members, exclusions, indent & "    ")
        End If

        For Each attr As XmlSchemaAttribute In AttributesOf(ct, exclusions.Attributes)
            EmitAttribute(attr, members, indent & "    ")
        Next

        If ct.AttributeWildcard IsNot Nothing AndAlso Not exclusions.HasAnyAttribute Then
            EmitAnyAttribute(members, indent & "    ")
        End If

        If ct.ContentType = XmlSchemaContentType.TextOnly Then
            EmitXmlText(members, ct, indent & "    ")
        ElseIf ct.ContentType = XmlSchemaContentType.Mixed Then
            AddNote($"complexType '{If(q Is Nothing, name, q.ToString())}' has mixed content; text nodes are not represented by a member.")
        End If

        output.AppendLine($"{indent}End Class")
        output.AppendLine()
    End Sub

    Private Function ShouldInherit(ct As XmlSchemaComplexType) As Boolean
        If ct Is Nothing OrElse ct.BaseXmlSchemaType Is Nothing Then
            Return False
        End If
        If ct.DerivedBy <> XmlSchemaDerivationMethod.Extension Then
            Return False
        End If

        Dim q As XmlQualifiedName = ct.BaseXmlSchemaType.QualifiedName
        If q Is Nothing OrElse q.IsEmpty OrElse XsdTypeMap.IsBuiltIn(q) Then
            Return False
        End If

        Return complexTypes.ContainsKey(q) AndAlso typeNames.ContainsKey(q)
    End Function

    Private Function ComputeExclusions(ct As XmlSchemaComplexType) As Exclusions
        Dim result As New Exclusions()
        If Not ShouldInherit(ct) Then
            Return result
        End If

        Dim baseType As XmlSchemaComplexType = TryCast(ct.BaseXmlSchemaType, XmlSchemaComplexType)
        If baseType Is Nothing Then
            Return result
        End If

        CollectElementKeys(baseType.ContentTypeParticle, result)
        For Each de As DictionaryEntry In baseType.AttributeUses
            Dim q As XmlQualifiedName = CType(de.Key, XmlQualifiedName)
            If q IsNot Nothing Then
                result.Attributes.Add(q.ToString())
            End If
        Next
        result.HasAnyAttribute = baseType.AttributeWildcard IsNot Nothing

        Return result
    End Function

    Private Shared Sub CollectElementKeys(particle As XmlSchemaParticle, result As Exclusions)
        If particle Is Nothing Then
            Return
        End If

        Dim el As XmlSchemaElement = TryCast(particle, XmlSchemaElement)
        If el IsNot Nothing Then
            result.Elements.Add(ElementKey(el))
            Return
        End If

        If TypeOf particle Is XmlSchemaAny Then
            result.HasAnyElement = True
            Return
        End If

        Dim groupRef As XmlSchemaGroupRef = TryCast(particle, XmlSchemaGroupRef)
        If groupRef IsNot Nothing Then
            CollectElementKeys(groupRef.Particle, result)
            Return
        End If

        Dim groupBase As XmlSchemaGroupBase = TryCast(particle, XmlSchemaGroupBase)
        If groupBase IsNot Nothing Then
            For Each item As XmlSchemaObject In groupBase.Items
                CollectElementKeys(TryCast(item, XmlSchemaParticle), result)
            Next
        End If
    End Sub

    Private Shared Function ElementKey(el As XmlSchemaElement) As String
        If el.QualifiedName IsNot Nothing AndAlso Not el.QualifiedName.IsEmpty Then
            Return el.QualifiedName.ToString()
        End If
        Return If(el.Name, el.RefName.ToString())
    End Function

    Private Shared Function AttributesOf(ct As XmlSchemaComplexType,
                                         excluded As HashSet(Of String)) As List(Of XmlSchemaAttribute)
        Dim result As New List(Of XmlSchemaAttribute)()

        For Each de As DictionaryEntry In ct.AttributeUses
            Dim q As XmlQualifiedName = CType(de.Key, XmlQualifiedName)
            If q IsNot Nothing AndAlso excluded.Contains(q.ToString()) Then
                Continue For
            End If
            Dim attr As XmlSchemaAttribute = TryCast(de.Value, XmlSchemaAttribute)
            If attr IsNot Nothing AndAlso attr.Use <> XmlSchemaUse.Prohibited Then
                result.Add(attr)
            End If
        Next

        result.Sort(Function(a, b) String.Compare(a.QualifiedName.ToString(), b.QualifiedName.ToString(), StringComparison.Ordinal))
        Return result
    End Function

#End Region

#Region "emit members"

    Private Sub EmitParticle(particle As XmlSchemaParticle,
                             owner As String,
                             members As HashSet(Of String),
                             exclusions As Exclusions,
                             indent As String)
        If particle Is Nothing Then
            Return
        End If

        Dim el As XmlSchemaElement = TryCast(particle, XmlSchemaElement)
        If el IsNot Nothing Then
            EmitElementProperty(el, members, exclusions, indent)
            Return
        End If

        If TypeOf particle Is XmlSchemaAny Then
            If Not exclusions.HasAnyElement Then
                EmitAnyElement(DirectCast(particle, XmlSchemaAny), members, indent)
            End If
            Return
        End If

        Dim choice As XmlSchemaChoice = TryCast(particle, XmlSchemaChoice)
        If choice IsNot Nothing Then
            EmitChoice(choice, owner, members, exclusions, indent)
            Return
        End If

        Dim groupRef As XmlSchemaGroupRef = TryCast(particle, XmlSchemaGroupRef)
        If groupRef IsNot Nothing Then
            If groupRef.Particle IsNot Nothing Then
                EmitParticle(groupRef.Particle, owner, members, exclusions, indent)
            Else
                AddNote($"xs:group ref='{groupRef.RefName}' could not be expanded by the schema compiler.")
            End If
            Return
        End If

        Dim groupBase As XmlSchemaGroupBase = TryCast(particle, XmlSchemaGroupBase)
        If groupBase IsNot Nothing Then
            For Each item As XmlSchemaObject In groupBase.Items
                EmitParticle(TryCast(item, XmlSchemaParticle), owner, members, exclusions, indent)
            Next
        End If
    End Sub

    Private Sub EmitElementProperty(el As XmlSchemaElement,
                                    members As HashSet(Of String),
                                    exclusions As Exclusions,
                                    indent As String)
        If exclusions.Elements.Contains(ElementKey(el)) Then
            Return
        End If

        Dim rawName As String = If(String.IsNullOrEmpty(el.QualifiedName.Name), el.Name, el.QualifiedName.Name)
        If String.IsNullOrEmpty(rawName) Then
            rawName = If(el.RefName.Name, "Item")
        End If

        Dim member As String = UniqueMember(members, XsdTypeMap.ToIdentifier(rawName))
        Dim vbType As String = ResolveElementType(el)
        Dim isArray As Boolean = el.MaxOccurs > 1D
        Dim needNullable As Boolean = (el.IsNillable OrElse el.MinOccurs = 0) AndAlso IsValueType(vbType)

        Dim finalType As String
        If isArray Then
            finalType = vbType & "()"
        ElseIf needNullable Then
            finalType = $"Nullable(Of {vbType})"
        Else
            finalType = vbType
        End If

        Dim remark As String = Nothing
        If Not String.IsNullOrEmpty(el.DefaultValue) Then
            remark = $"XSD default='{el.DefaultValue}' is documented only; XmlSerializer does not enforce default values."
        ElseIf Not String.IsNullOrEmpty(el.FixedValue) Then
            remark = $"XSD fixed='{el.FixedValue}' is documented only; XmlSerializer does not enforce fixed values."
        End If

        EmitDoc(indent, DocumentationOf(el), remark)

        Dim args As New List(Of String) From {$"ElementName:={XsdTypeMap.StringLiteral(rawName)}"}
        If Not String.IsNullOrEmpty(el.QualifiedName.Namespace) Then
            args.Add($"Namespace:={XsdTypeMap.StringLiteral(el.QualifiedName.Namespace)}")
        Else
            args.Add("Form:=XmlSchemaForm.Unqualified")
        End If
        If el.IsNillable Then
            args.Add("IsNullable:=True")
        End If

        output.AppendLine($"{indent}<XmlElement({String.Join(", ", args)})>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(member)} As {finalType}")
        output.AppendLine()
    End Sub

    Private Sub EmitChoice(choice As XmlSchemaChoice,
                           owner As String,
                           members As HashSet(Of String),
                           exclusions As Exclusions,
                           indent As String)
        Dim items As New List(Of XmlSchemaElement)()
        Dim nested As Boolean = False

        For Each item As XmlSchemaObject In choice.Items
            Dim el As XmlSchemaElement = TryCast(item, XmlSchemaElement)
            If el IsNot Nothing Then
                If Not exclusions.Elements.Contains(ElementKey(el)) Then
                    items.Add(el)
                End If
            Else
                nested = True
                EmitParticle(TryCast(item, XmlSchemaParticle), owner, members, exclusions, indent)
            End If
        Next

        If nested Then
            AddNote($"an xs:choice in '{owner}' contains nested compositors; its alternatives were emitted as independent members.")
            Return
        End If
        If items.Count = 0 Then
            Return
        End If
        If items.Count = 1 Then
            EmitElementProperty(items(0), members, exclusions, indent)
            Return
        End If

        Dim enumName As String = AllocateName(XsdTypeMap.ToIdentifier(owner) & "ChoiceType")
        Dim spec As New ChoiceEnumSpec With {.Name = enumName}
        Dim usedEnumMembers As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim elementAttrs As New List(Of String)()

        For Each el As XmlSchemaElement In items
            Dim rawName As String = If(String.IsNullOrEmpty(el.QualifiedName.Name), el.Name, el.QualifiedName.Name)
            Dim vbType As String = ResolveElementType(el)
            elementAttrs.Add($"XmlElement({XsdTypeMap.StringLiteral(rawName)}, GetType({vbType}))")

            Dim memberBase As String = XsdTypeMap.ToIdentifier(rawName)
            Dim member As String = memberBase
            Dim index As Integer = 1
            While Not usedEnumMembers.Add(member)
                index += 1
                member = memberBase & "_" & index.ToString()
            End While
            spec.Values.Add(New KeyValuePair(Of String, String)(member, rawName))
        Next

        Dim memberName As String = UniqueMember(members, "Item")
        Dim identifierName As String = UniqueMember(members, memberName & "ElementName")
        Dim isArray As Boolean = choice.MaxOccurs > 1D
        Dim propertyType As String = If(isArray, "Object()", "Object")
        Dim identifierType As String = If(isArray, $"{enumName}()", enumName)

        choiceEnums.Add(spec)

        EmitDoc(indent, DocumentationOf(choice), "One of the xs:choice alternatives is stored in this member; ItemElementName identifies which alternative is present.")
        elementAttrs.Add($"XmlChoiceIdentifier({XsdTypeMap.StringLiteral(identifierName)})")
        output.AppendLine($"{indent}<{String.Join(", ", elementAttrs)}>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(memberName)} As {propertyType}")
        output.AppendLine()
        output.AppendLine($"{indent}<XmlIgnore>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(identifierName)} As {identifierType}")
        output.AppendLine()
    End Sub

    Private Sub EmitAnyElement(any As XmlSchemaAny, members As HashSet(Of String), indent As String)
        Dim member As String = UniqueMember(members, "Any")

        AddNote($"xs:any namespace='{any.Namespace}' processContents='{any.ProcessContents}' is emitted as XmlAnyElement; wildcard constraints are not enforced by XmlSerializer.")
        EmitDoc(indent, DocumentationOf(any), "Wildcard element content. The XSD namespace/processContents constraints are not enforced by XmlSerializer.")
        output.AppendLine($"{indent}<XmlAnyElement>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(member)} As System.Xml.XmlElement()")
        output.AppendLine()
    End Sub

    Private Sub EmitAnyAttribute(members As HashSet(Of String), indent As String)
        Dim member As String = UniqueMember(members, "AnyAttributes")

        AddNote("xs:anyAttribute is emitted as XmlAnyAttribute; its namespace/processContents constraints are not enforced by XmlSerializer.")
        output.AppendLine($"{indent}<XmlAnyAttribute>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(member)} As System.Xml.XmlAttribute()")
        output.AppendLine()
    End Sub

    Private Sub EmitAttribute(attr As XmlSchemaAttribute,
                              members As HashSet(Of String),
                              indent As String)
        Dim rawName As String = If(String.IsNullOrEmpty(attr.QualifiedName.Name), attr.Name, attr.QualifiedName.Name)
        If String.IsNullOrEmpty(rawName) Then
            rawName = If(attr.RefName.Name, "Value")
        End If

        Dim member As String = UniqueMember(members, XsdTypeMap.ToIdentifier(rawName))
        Dim vbType As String = ResolveAttributeType(attr)
        ' XmlSerializer rejects Nullable(Of T) on XmlAttribute/XmlText. Optional
        ' value-type attributes therefore use the standard xxxSpecified pattern.
        Dim addSpecified As Boolean = attr.Use = XmlSchemaUse.Optional AndAlso IsValueType(vbType)

        Dim remark As String = Nothing
        If addSpecified Then
            remark = "This XSD attribute is optional; the companion xxxSpecified flag records whether it was present."
        ElseIf attr.Use = XmlSchemaUse.Required Then
            remark = "This attribute is required by the XSD; XmlSerializer does not enforce required attributes."
        ElseIf Not String.IsNullOrEmpty(attr.DefaultValue) Then
            remark = $"XSD default='{attr.DefaultValue}' is documented only."
        End If

        EmitDoc(indent, DocumentationOf(attr), remark)

        Dim args As New List(Of String) From {$"AttributeName:={XsdTypeMap.StringLiteral(rawName)}"}
        If Not String.IsNullOrEmpty(attr.QualifiedName.Namespace) Then
            args.Add($"Namespace:={XsdTypeMap.StringLiteral(attr.QualifiedName.Namespace)}")
        Else
            args.Add("Form:=XmlSchemaForm.Unqualified")
        End If

        output.AppendLine($"{indent}<XmlAttribute({String.Join(", ", args)})>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(member)} As {vbType}")
        output.AppendLine()

        If addSpecified Then
            Dim specified As String = UniqueMember(members, member & "Specified")
            output.AppendLine($"{indent}''' <summary>True when the optional XSD attribute '{rawName}' is present.</summary>")
            output.AppendLine($"{indent}<XmlIgnore>")
            output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(specified)} As Boolean")
            output.AppendLine()
        End If
    End Sub

    Private Sub EmitXmlText(members As HashSet(Of String), ct As XmlSchemaComplexType, indent As String)
        Dim member As String = UniqueMember(members, "Value")
        Dim vbType As String = XsdTypeMap.VbTypeOf(ct.Datatype)

        If vbType <> "String" Then
            AddNote($"simpleContent of '{ct.QualifiedName}' uses XSD datatype '{vbType}'; the XmlText member is emitted as String to stay compatible with XmlSerializer.")
        End If

        EmitDoc(indent, DocumentationOf(ct), "The text value of this element (xs:simpleContent).")
        output.AppendLine($"{indent}<XmlText>")
        output.AppendLine($"{indent}Public Property {XsdTypeMap.EscapeIdentifier(member)} As String")
        output.AppendLine()
    End Sub

    Private Function ResolveElementType(el As XmlSchemaElement) As String
        If el.SchemaType IsNot Nothing AndAlso anonymousNames.ContainsKey(el.SchemaType) Then
            Return anonymousNames(el.SchemaType)
        End If

        Dim q As XmlQualifiedName = el.SchemaTypeName
        If q IsNot Nothing AndAlso Not q.IsEmpty AndAlso typeNames.ContainsKey(q) Then
            Return typeNames(q)
        End If

        q = If(el.ElementSchemaType Is Nothing, Nothing, el.ElementSchemaType.QualifiedName)
        If q IsNot Nothing AndAlso Not q.IsEmpty AndAlso typeNames.ContainsKey(q) Then
            Return typeNames(q)
        End If

        Return XsdTypeMap.VbTypeOf(If(el.ElementSchemaType Is Nothing, Nothing, el.ElementSchemaType.Datatype))
    End Function

    Private Function ResolveAttributeType(attr As XmlSchemaAttribute) As String
        If attr.SchemaType IsNot Nothing AndAlso anonymousNames.ContainsKey(attr.SchemaType) Then
            Return anonymousNames(attr.SchemaType)
        End If

        Dim q As XmlQualifiedName = attr.SchemaTypeName
        If q IsNot Nothing AndAlso Not q.IsEmpty AndAlso typeNames.ContainsKey(q) Then
            Return typeNames(q)
        End If

        q = If(attr.AttributeSchemaType Is Nothing, Nothing, attr.AttributeSchemaType.QualifiedName)
        If q IsNot Nothing AndAlso Not q.IsEmpty AndAlso typeNames.ContainsKey(q) Then
            Return typeNames(q)
        End If

        Return XsdTypeMap.VbTypeOf(If(attr.AttributeSchemaType Is Nothing, Nothing, attr.AttributeSchemaType.Datatype))
    End Function

#End Region

#Region "emit global elements"

    Private Sub EmitGlobalElement(el As XmlSchemaElement, indent As String)
        Dim q As XmlQualifiedName = el.QualifiedName
        Dim name As String = elementNames(q)
        Dim schemaType As XmlSchemaType = el.ElementSchemaType

        EmitDoc(indent, DocumentationOf(el))
        output.AppendLine($"{indent}<XmlRoot(ElementName:={XsdTypeMap.StringLiteral(q.Name)}, " &
                          $"Namespace:={XsdTypeMap.StringLiteral(q.Namespace)}, " &
                          $"IsNullable:={XsdTypeMap.BooleanLiteral(el.IsNillable)})>")

        Dim complexDef As XmlSchemaComplexType = TryCast(schemaType, XmlSchemaComplexType)
        Dim namedQ As XmlQualifiedName = If(complexDef Is Nothing, Nothing, complexDef.QualifiedName)
        Dim named As Boolean = namedQ IsNot Nothing AndAlso Not namedQ.IsEmpty AndAlso
                               complexTypes.ContainsKey(namedQ) AndAlso Not XsdTypeMap.IsBuiltIn(namedQ)

        Dim modifier As String = If(el.IsAbstract, "Public MustInherit Class ", "Public Class ")

        If named Then
            output.AppendLine($"{indent}{modifier}{name}")
            output.AppendLine($"{indent}    Inherits {typeNames(namedQ)}")
            output.AppendLine()
            output.AppendLine($"{indent}End Class")
            output.AppendLine()
            Return
        End If

        If complexDef IsNot Nothing Then
            If anonymousNames.ContainsKey(complexDef) Then
                output.AppendLine($"{indent}{modifier}{name}")
                output.AppendLine($"{indent}    Inherits {anonymousNames(complexDef)}")
                output.AppendLine()
                output.AppendLine($"{indent}End Class")
                output.AppendLine()
                Return
            End If
            EmitClassBody(complexDef, Nothing, name, el.IsAbstract, indent)
            Return
        End If

        Dim simpleDef As XmlSchemaSimpleType = TryCast(schemaType, XmlSchemaSimpleType)
        Dim remark As String = Nothing
        If simpleDef IsNot Nothing Then
            remark = "The XSD root element has simple content; the value is emitted as String so that XmlSerializer can serialize the root element."
            AddNote($"global element '{q}' has simple/XSD built-in content; generated root class keeps the value as String via XmlText.")
        End If

        output.AppendLine($"{indent}{modifier}{name}")
        output.AppendLine()
        EmitDoc(indent & "    ", DocumentationOf(simpleDef), remark)
        output.AppendLine($"{indent}    <XmlText>")
        output.AppendLine($"{indent}    Public Property [Value] As String")
        output.AppendLine()
        output.AppendLine($"{indent}End Class")
        output.AppendLine()
    End Sub

#End Region

    Private NotInheritable Class Exclusions
        Public ReadOnly Elements As New HashSet(Of String)(StringComparer.Ordinal)
        Public ReadOnly Attributes As New HashSet(Of String)(StringComparer.Ordinal)
        Public Property HasAnyElement As Boolean
        Public Property HasAnyAttribute As Boolean
    End Class

    Private NotInheritable Class ChoiceEnumSpec
        Public Property Name As String
        Public ReadOnly Values As New List(Of KeyValuePair(Of String, String))()
    End Class
End Class
