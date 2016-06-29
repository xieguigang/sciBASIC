#Region "Microsoft.VisualBasic::a99c07429a8a1556047ad0ce4fd7b670, ..\RDF\RDF\Serializer\RDF.vb"

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


Namespace Serialization

    ''' <summary>
    ''' Custom attribute class base type that using on a property target.(适用于Property对象类型上的自定义属性)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, allowmultiple:=False, inherited:=True)>
    Public MustInherit Class PropertyAttribute : Inherits Attribute
        Public Property Name As String

        Protected Friend Property _bindProperty As System.Reflection.PropertyInfo
        Protected Friend Property _rdfType As Serialization.RDFType
        Protected Friend Property _valueType As System.Type

        Protected Friend MustOverride Function GetXmlSerializationCustomAttribute() As KeyValuePair(Of String, Type)

        Protected Friend Function Initlaize(BindProperty As System.Reflection.PropertyInfo) As PropertyAttribute
            Me._bindProperty = BindProperty
            Return Me
        End Function

        Sub New(Name As String)
            Me.Name = Name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Protected Friend ReadOnly Property IsValueType As Boolean
            Get
                Return _bindProperty.PropertyType.IsValueType OrElse _bindProperty.PropertyType = GetType(String)
            End Get
        End Property

        Protected Friend ReadOnly Property IsArrayType As Boolean
            Get
                Return _bindProperty.PropertyType.IsArray
            End Get
        End Property
    End Class

    ''' <summary>
    ''' This custom attribute indicated that the bind target property will not be parsed by the rdf serializer.
    ''' (本自定义属性指明所绑定的目标属性对象将不会被解析)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, allowmultiple:=False, inherited:=True)>
    Public Class RDFIgnore : Inherits Attribute
        Public Overrides Function ToString() As String
            Return "This custom attribute indicated that the bind target property will not be parsed by the rdf serializer." &
                "(本自定义属性指明所绑定的目标属性对象将不会被解析)"
        End Function

        Protected Friend Shared ReadOnly TypeInfo As System.Type = GetType(RDFIgnore)
    End Class

    <AttributeUsage(AttributeTargets.Class, allowmultiple:=False, inherited:=True)>
    Public Class RDFDescription : Inherits Attribute
        Public Property About As String

    End Class

    <AttributeUsage(AttributeTargets.Property, allowmultiple:=False, inherited:=True)>
    Public Class RDFElement : Inherits PropertyAttribute

        Sub New(Optional attributeName As String = "")
            Call MyBase.New(attributeName)
        End Sub

        Protected Friend Overrides Function GetXmlSerializationCustomAttribute() As KeyValuePair(Of String, Type)
            Return New KeyValuePair(Of String, Type)(MyBase.Name, GetType(Xml.Serialization.XmlElementAttribute))
        End Function

        ''' <summary>
        ''' Get the <see cref="System.Type"></see> type information of the class type <see cref="Serialization.RDFElement"></see>
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend Shared ReadOnly TypeInfo As System.Type = GetType(RDFElement)
    End Class

    <AttributeUsage(AttributeTargets.Property, allowmultiple:=False, inherited:=True)>
    Public Class RDFAttribute : Inherits PropertyAttribute

        Sub New(Optional attributeName As String = "")
            Call MyBase.New(attributeName)
        End Sub

        Protected Friend Overrides Function GetXmlSerializationCustomAttribute() As KeyValuePair(Of String, Type)
            Return New KeyValuePair(Of String, Type)(MyBase.Name, GetType(Xml.Serialization.XmlAttributeAttribute))
        End Function

        ''' <summary>
        ''' Get the <see cref="System.Type"></see> type information of the class type <see cref="Serialization.RDFAttribute"></see>
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend Shared ReadOnly TypeInfo As System.Type = GetType(RDFAttribute)
    End Class

    ''' <summary>
    ''' 在申明RDF对象的时候所申明的Schema中的目标类型
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class, allowmultiple:=True, inherited:=True)>
    Public Class RDFType : Inherits Attribute
        Public Property TypeName As String

        Protected Friend Property PropertyCollection As PropertyAttribute()
        ''' <summary>
        ''' 自己本身的类型属性
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Property _BindTypeInfo As System.Type
        ''' <summary>
        ''' 假若目标类型为一个数组类型，则本属性则为目标数组的元素的类型，但是不是的话，则本属性为空值
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Property _BindElementTypeInfo As Serialization.RDFType

        Sub New(Name As String)
            TypeName = Name
        End Sub

        Public ReadOnly Property IsArrayType As Boolean
            Get
                Return _BindTypeInfo.IsArray
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return TypeName
        End Function

        Protected Friend Shared ReadOnly TypeInfo As System.Type = GetType(RDFType)

        Protected Friend Function GetXmlSerializationCustomAttribute() As KeyValuePair(Of String, Type)
            Return New KeyValuePair(Of String, System.Type)(TypeName, GetType(Xml.Serialization.XmlTypeAttribute))
        End Function

        Public Shared Function GetTypeDefine(TypeInfo As System.Type) As Serialization.RDFType
            Dim LQuery = (From attrs As Object In TypeInfo.GetCustomAttributes(RDFType.TypeInfo, True) Select DirectCast(attrs, RDFType)).ToArray
            If Not LQuery.IsNullOrEmpty Then
                Dim retVal = LQuery.First
                retVal._BindTypeInfo = TypeInfo
                Return retVal
            Else
                Return CreateTypeDefine(TypeInfo)
            End If
        End Function

        ''' <summary>
        ''' 当目标类型不存在RDFType自定义属性的时候，进行创建的方法
        ''' </summary>
        ''' <param name="TypeInfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateTypeDefine(TypeInfo As System.Type) As Serialization.RDFType
            Dim RDFType As RDFType = New Serialization.RDFType(TypeInfo.Name)
            RDFType._BindTypeInfo = TypeInfo
            Return RDFType
        End Function
    End Class

    ''' <summary>
    ''' RDF命名空间的Schema导入来源
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class, allowmultiple:=True, inherited:=True)>
    Public Class RDFNamespaceImports : Inherits Xml.Serialization.XmlTypeAttribute
        ''' <summary>
        ''' Type name of the target imports namespace
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Type As String
            Get
                Return TypeName
            End Get
        End Property
        Public ReadOnly Property SchemaUrl As String
            Get
                Return [Namespace]
            End Get
        End Property

        Sub New(Type As String, SchemaUrl As String)
            MyBase.TypeName = Type
            MyBase.Namespace = SchemaUrl
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("<rdf:RDF xmlns:{0}=""{1}"" >", Type, SchemaUrl)
        End Function

        ''' <summary>
        ''' Get the <see cref="System.Type"></see> type information of the class type <see cref="Serialization.RDFNamespaceImports"></see>
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend Shared ReadOnly TypeInfo As System.Type = GetType(RDFNamespaceImports)
    End Class
End Namespace
