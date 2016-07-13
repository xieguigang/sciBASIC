#Region "Microsoft.VisualBasic::66ccfb92eba7c3f69add5dea3acc4d18, ..\VisualBasic_AppFramework\DocumentFormats\RDF\RDF\Serializer\CodeDOMCreator.vb"

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

Namespace Framework.DynamicCode.VBC

    Public NotInheritable Class CodeDOMCreator

        Public Const ROOT_NAMESPACE As String = "Microsoft.VisualBasic.DocumentFormat.RDF.DynamicCodeCompiled"

        Dim CreatedTypeList As Dictionary(Of String, CodeDom.CodeTypeDeclaration) = New Dictionary(Of String, CodeDom.CodeTypeDeclaration)

        Public Function DeclareAssembly(Schema As Schema) As CodeDom.CodeCompileUnit
            Dim Assembly As CodeDom.CodeCompileUnit = New CodeDom.CodeCompileUnit
            Dim DynamicCodeNameSpace As CodeDom.CodeNamespace = New CodeDom.CodeNamespace(ROOT_NAMESPACE)
            Call Assembly.Namespaces.Add(DynamicCodeNameSpace)

            Dim RootType = DeclareTypeDefine(Schema._bindType, Schema._bindType.TypeName)
            Dim XmlAttributeType As System.Type = GetType(Xml.Serialization.XmlAttributeAttribute)

            For Each ImportedNamespace In Schema.ImportsNamespaces
                Dim NameId As String = "xmlns_" & ImportedNamespace.Type
                Call VBC.CodeDOMCreator.AddProperty(RootType, NameId, "System.String",
                                                    New KeyValuePair(Of String, System.Type)() {
                                                        New KeyValuePair(Of String, System.Type)(NameId, XmlAttributeType)})
            Next

            Call DynamicCodeNameSpace.Types.AddRange(CreatedTypeList.Values.ToArray)
            Call DynamicCodeNameSpace.Types.Add(RootType)
            Call DynamicCodeNameSpace.Imports.AddRange(ImportsNamespace)
#If DEBUG Then
            Console.WriteLine(VBC.DynamicCompiler.GenerateCode(DynamicCodeNameSpace))
#End If

            Return Assembly
        End Function

        ''' <summary>
        ''' 需要生成新类型是由于XML序列化和反序列化操作的需要
        ''' </summary>
        ''' <param name="RDFType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Function DeclareTypeDefine(RDFType As Serialization.RDFType, TypeId As String) As CodeDom.CodeTypeDeclaration
            Dim Type = New CodeDom.CodeTypeDeclaration(TypeId)
            Dim attribute = RDFType.GetXmlSerializationCustomAttribute
            Type.CustomAttributes.Add(New CodeDom.CodeAttributeDeclaration(New CodeDom.CodeTypeReference(attribute.Value), New CodeDom.CodeAttributeArgument(New CodeDom.CodePrimitiveExpression(attribute.Key))))

            For Each [Property] In RDFType.PropertyCollection
                If [Property].IsValueType Then
                    Call VBC.CodeDOMCreator.AddProperty(Type, [Property].Name, [Property]._bindProperty.PropertyType, New KeyValuePair(Of String, Type)() {[Property].GetXmlSerializationCustomAttribute})
                Else '需要创建新类型
                    If [Property].IsArrayType Then '创建ElementType的Array
                        TypeId = RDFType.TypeName & "_" & [Property]._rdfType._BindElementTypeInfo.TypeName
                        Call CreatedTypeList.Add(TypeId, DeclareTypeDefine([Property]._rdfType._BindElementTypeInfo, TypeId))

                        Dim ArrayType As New CodeDom.CodeTypeReference(New CodeDom.CodeTypeReference(TypeId), 0)

                        Call VBC.CodeDOMCreator.AddProperty(Type, [Property].Name, ArrayType, New KeyValuePair(Of String, Type)() {[Property].GetXmlSerializationCustomAttribute})
                    Else
                        TypeId = RDFType.TypeName & "_" & [Property]._rdfType.TypeName
                        Call CreatedTypeList.Add(TypeId, DeclareTypeDefine([Property]._rdfType, TypeId))

                        '创建类型属性引用
                        Call VBC.CodeDOMCreator.AddProperty(Type, [Property].Name, TypeId, New KeyValuePair(Of String, Type)() {[Property].GetXmlSerializationCustomAttribute})
                    End If
                End If
            Next
            Return Type
        End Function

        Private Shared Function ImportsNamespace() As CodeDom.CodeNamespaceImport()
            Dim Array As CodeDom.CodeNamespaceImport() = New CodeDom.CodeNamespaceImport() {
                New CodeDom.CodeNamespaceImport("Microsoft.VisualBasic"),
                New CodeDom.CodeNamespaceImport("System"),
                New CodeDom.CodeNamespaceImport("System.Collections"),
                New CodeDom.CodeNamespaceImport("System.Collections.Generic"),
                New CodeDom.CodeNamespaceImport("System.Data"),
                New CodeDom.CodeNamespaceImport("System.Diagnostics"),
                New CodeDom.CodeNamespaceImport("System.Linq"),
                New CodeDom.CodeNamespaceImport("System.Xml.Linq"),
                New CodeDom.CodeNamespaceImport("System.Text.RegularExpressions")}
            Return Array
        End Function

        Protected Friend Shared Sub AddProperty(Target As CodeDom.CodeTypeDeclaration, PropertyName As String, PropertyType As System.Type, CustomAttributes As KeyValuePair(Of String, System.Type)())
            Dim [Property] As New CodeDom.CodeMemberProperty()
            [Property].Name = PropertyName
            [Property].Type = New CodeDom.CodeTypeReference(PropertyType)
            [Property].CustomAttributes.AddRange((From attr In CustomAttributes Let attrTypeRef = New CodeDom.CodeTypeReference(attr.Value) Select New CodeDom.CodeAttributeDeclaration(attrTypeRef, New CodeDom.CodeAttributeArgument(New CodeDom.CodePrimitiveExpression(attr.Key)))).ToArray)
            [Property].Attributes = CodeDom.MemberAttributes.Public
            Call Target.Members.Add([Property])
        End Sub

        Protected Friend Shared Sub AddProperty(Target As CodeDom.CodeTypeDeclaration, PropertyName As String, PropertyTypeId As String, CustomAttributes As KeyValuePair(Of String, System.Type)())
            Dim [Property] As New CodeDom.CodeMemberProperty()
            [Property].Name = PropertyName
            [Property].Type = New CodeDom.CodeTypeReference(PropertyTypeId)
            [Property].CustomAttributes.AddRange((From attr In CustomAttributes Let attrTypeRef = New CodeDom.CodeTypeReference(attr.Value) Select New CodeDom.CodeAttributeDeclaration(attrTypeRef, New CodeDom.CodeAttributeArgument(New CodeDom.CodePrimitiveExpression(attr.Key)))).ToArray)
            [Property].Attributes = CodeDom.MemberAttributes.Public
            Call Target.Members.Add([Property])
        End Sub

        Protected Friend Shared Sub AddProperty(Target As CodeDom.CodeTypeDeclaration, PropertyName As String, TypeReference As CodeDom.CodeTypeReference, CustomAttributes As KeyValuePair(Of String, System.Type)())
            Dim [Property] As New CodeDom.CodeMemberProperty()
            [Property].Name = PropertyName
            [Property].Type = TypeReference
            [Property].CustomAttributes.AddRange((From attr In CustomAttributes Let attrTypeRef = New CodeDom.CodeTypeReference(attr.Value) Select New CodeDom.CodeAttributeDeclaration(attrTypeRef, New CodeDom.CodeAttributeArgument(New CodeDom.CodePrimitiveExpression(attr.Key)))).ToArray)
            [Property].Attributes = CodeDom.MemberAttributes.Public
            Call Target.Members.Add([Property])
        End Sub
    End Class
End Namespace
