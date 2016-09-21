#Region "Microsoft.VisualBasic::503dc0d2f43af44b4103b2ea7eb767b7, ..\visualbasic_App\DocumentFormats\RDF\RDF\Serializer\Serializer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Text
Imports Microsoft.VisualBasic.MIME.RDF.Serialization

Public Class Schema
    Public Property ImportsNamespaces As Serialization.RDFNamespaceImports()
    Protected Friend _bindType As Serialization.RDFType

    Public Overrides Function ToString() As String
        Return _bindType.ToString
    End Function

    ''' <summary>
    ''' 没有添加RDF自定义属性的属性对象将会被忽略掉
    ''' </summary>
    ''' <param name="RDFRootType"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Friend Overloads Shared Function [GetType](RDFRootType As Serialization.RDFType) As Serialization.RDFType
        Dim RootTypeInfo = RDFRootType._BindTypeInfo

        If RootTypeInfo.IsArray Then
            Dim ElementType = RootTypeInfo.GetElementType
            Dim RDFType = Serialization.RDFType.GetTypeDefine(ElementType)

            If Not ElementType.IsValueType Then
                RDFRootType._BindElementTypeInfo = [GetType](RDFType)
            Else
                RDFRootType._BindElementTypeInfo = RDFType
            End If
        ElseIf Not RootTypeInfo.IsValueType Then
            Dim PropertyCollection = (From [Property] As System.Reflection.PropertyInfo In RootTypeInfo.GetProperties(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Instance)
                                      Where False = IsIgnoredProperty([Property])
                                      Let RDFAttribute = GetRDFPropertyAttribute([Property])
                                      Select RDFAttribute).ToArray

            For Each [Property] In PropertyCollection '如果不是基本数据类型则进行向下递归操作
                Dim PropertyTypeInfo = [Property]._bindProperty.PropertyType
                Dim RDFType = Serialization.RDFType.GetTypeDefine(PropertyTypeInfo)

                If PropertyTypeInfo.IsValueType OrElse PropertyTypeInfo = GetType(String) Then
                    [Property]._valueType = PropertyTypeInfo
                Else
                    [Property]._rdfType = [GetType](RDFType)
                End If
            Next

            RDFRootType.PropertyCollection = PropertyCollection.ToArray
        End If

        Return RDFRootType
    End Function

    Protected Friend Shared Function IsIgnoredProperty(PropertyInfo As System.Reflection.PropertyInfo) As Boolean
        Dim f = [PropertyInfo].GetCustomAttributes(Serialization.RDFIgnore.TypeInfo, True).IsNullOrEmpty
        Dim r = [PropertyInfo].GetCustomAttributes(XmlIgnore, True).IsNullOrEmpty
        Dim result = f AndAlso r
        Return Not result
    End Function

    Protected Friend Shared Function GetRDFPropertyAttribute(PropertyInfo As System.Reflection.PropertyInfo) As Serialization.PropertyAttribute
        Dim attrs As Object() = [PropertyInfo].GetCustomAttributes(Serialization.RDFElement.TypeInfo, True)

        If attrs.IsNullOrEmpty Then
            attrs = [PropertyInfo].GetCustomAttributes(Serialization.RDFAttribute.TypeInfo, True)
            If attrs.IsNullOrEmpty Then '完全没有的话默认新构造RDFElement属性
                Return New Serialization.RDFElement([PropertyInfo].Name).Initlaize([PropertyInfo])
            Else
                Dim attr = DirectCast(attrs.First, Serialization.RDFAttribute)
                If String.IsNullOrEmpty(attr.Name) Then
                    attr.Name = [PropertyInfo].Name
                End If
                Return attr.Initlaize([PropertyInfo])
            End If
        Else
            Dim attr = DirectCast(attrs.First, Serialization.RDFElement)
            If String.IsNullOrEmpty(attr.Name) Then
                attr.Name = [PropertyInfo].Name
            End If
            Return attr.Initlaize([PropertyInfo])
        End If
    End Function

    Shared ReadOnly XmlIgnore As System.Type = GetType(Xml.Serialization.XmlIgnoreAttribute)
End Class

''' <summary>
''' 
''' </summary>
''' <typeparam name="T"></typeparam>
''' <remarks>
''' 工作流程
''' 
'''  -- 解析类型定义
'''  -- CodeDOM创建新类型
'''  -- RDF文件格式化
'''  -- 使用新类型进行反序列化
'''  -- 属性映射
'''  -- 返回原来的目标对象实例
''' </remarks>
Public Class Serializer(Of T As Class)

    Dim SchemaInfo As Schema

    Sub New()
        Me.SchemaInfo = InitlaizeSchema()
    End Sub

    ''' <summary>
    ''' 第一步，解析类型定义
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Friend Shared Function InitlaizeSchema() As Schema
        Dim TypeInfo As System.Type = GetType(T)
        Dim Namespaces = (From attr As Object
                          In TypeInfo.GetCustomAttributes(RDFNamespaceImports.TypeInfo, True)
                          Select DirectCast(attr, Serialization.RDFNamespaceImports)).ToArray
        Dim RDFRootType = Serialization.RDFType.GetTypeDefine(TypeInfo)
        RDFRootType.TypeName = "rdf_RDF"
        Dim Schema As Schema = New Schema With {
            .ImportsNamespaces = Namespaces,
            ._bindType = Schema.[GetType](RDFRootType)
        }

        Return Schema
    End Function

    Public Function DeSerialization(ObjectStream As IO.StreamReader) As T
        Dim Stream As StringBuilder = New StringBuilder(ObjectStream.ReadToEnd)
        Return DeSerialization(Stream)
    End Function

    ''' <summary>
    ''' 第二步，CodeDOM创建新类型
    ''' </summary>
    ''' <param name="SchemaInfo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Friend Shared Function CreateDynamicType(SchemaInfo As Schema) As System.Type
        Dim DynamicAssembly = New Framework.DynamicCode.VBC.CodeDOMCreator().DeclareAssembly(SchemaInfo)
        Dim DynamicTypeId As String = Framework.DynamicCode.VBC.CodeDOMCreator.ROOT_NAMESPACE & "." & SchemaInfo._bindType.TypeName
        Dim DynamicType = New Framework.DynamicCode.VBC.DynamicCompiler("").Compile(DynamicAssembly, New String() {}).GetType(DynamicTypeId)

        Return DynamicType
    End Function

    Protected Friend Function DeSerialization(ObjectStream As StringBuilder) As T
        Call Normalization(ObjectStream)

        Dim DynamicType = CreateDynamicType(SchemaInfo)
        Dim DynamicInstance = ObjectStream.CreateObjectFromXml(DynamicType) '第四步，XML反序列化操作
        Dim DataInstance As T = Copy(DynamicInstance)

        Return DataInstance
    End Function

    ''' <summary>
    ''' 第三部，RDF文件格式化
    ''' </summary>
    ''' <param name="ObjectStream"></param>
    ''' <remarks></remarks>
    Protected Friend Sub Normalization(ObjectStream As StringBuilder)
        Call ObjectStream.Replace("rdf:RDF", "rdf_RDF")
        Call ObjectStream.Replace("xmlns:rdf", "xmlns_rdf")

        For Each ns In Me.SchemaInfo.ImportsNamespaces
            Dim ns_Name As String = ns.Type
            Call ObjectStream.Replace("xmlns:" & ns_Name, "xmlns_" & ns_Name)
        Next
    End Sub

    ''' <summary>
    ''' 第五步，属性映射
    ''' </summary>
    ''' <param name="DynamicInstance"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Friend Function Copy(DynamicInstance As Object) As T
        Try
            Dim DataInstance As T = Activator.CreateInstance(Of T)()

            Return DataInstance
        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Function DeSerialization(RDFFile As String) As T
        Dim Stream As StringBuilder = New StringBuilder(IO.File.ReadAllText(RDFFile))
        Return DeSerialization(Stream)
    End Function
End Class
