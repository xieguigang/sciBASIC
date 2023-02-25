#Region "Microsoft.VisualBasic::289bd077cc0adcd7e3371ebc6f810714, sciBASIC#\mime\application%rdf+xml\RDFEntity.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 92
    '    Code Lines: 35
    ' Comment Lines: 42
    '   Blank Lines: 15
    '     File Size: 3.03 KB


    ' Class RDFEntity
    ' 
    '     Properties: about, comment, Properties, range, RDFId
    ' 
    '     Function: ToString
    ' 
    ' Class RDFProperty
    ' 
    ' 
    ' 
    ' Class EntityProperty
    ' 
    '     Properties: dataType, resource, value
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

''' <summary>
''' 在rdf之中被描述的对象实体
''' </summary>
''' <remarks>
''' ID,about
''' </remarks>
<XmlType("Description", [Namespace]:=RDFEntity.XmlnsNamespace)>
Public MustInherit Class RDFEntity : Inherits RDFProperty
    Implements INamedValue, IReadOnlyId

    ''' <summary>
    ''' rdf:XXX
    ''' </summary>
    Public Const XmlnsNamespace$ = "http://www.w3.org/1999/02/22-rdf-syntax-ns#"

    Public Property range As RDFProperty
    Public Property comment As RDFProperty

    ''' <summary>
    ''' rdf:ID
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("ID", [Namespace]:=RDFEntity.XmlnsNamespace)> Public Property RDFId As String

    ''' <summary>
    ''' [资源] 是可拥有 URI 的任何事物
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("about", [Namespace]:=RDFEntity.XmlnsNamespace)> Public Property about As String Implements INamedValue.Key, IReadOnlyId.Identity
    ''' <summary>
    ''' [属性]   是拥有名称的资源
    ''' [属性值] 是某个属性的值，(请注意一个属性值可以是另外一个<see cref="Resource"/>）
    ''' xml文档在rdf反序列化之后，原有的类型定义之中除了自有的属性被保留下来了之外，具备指向其他资源的属性都被保存在了这个属性字典之中
    ''' </summary>
    ''' <returns></returns>
    <XmlIgnore>
    Public Overloads Property Properties As Dictionary(Of String, RDFEntity)

    Public Overrides Function ToString() As String
        Return RDFId & "  // " & about
    End Function
End Class

Public Class RDFProperty : Inherits EntityProperty

End Class

''' <summary>
''' 
''' </summary>
Public MustInherit Class EntityProperty

    ''' <summary>
    ''' rdf:datatype
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("datatype", [Namespace]:=RDFEntity.XmlnsNamespace)> Public Property dataType As String
    ''' <summary>
    ''' rdf:resource
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute("resource", [Namespace]:=RDFEntity.XmlnsNamespace)> Public Property resource As String

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' ###### 20191102
    ''' 
    ''' Base type '<see cref="RDFProperty"/>' has simpleContent and can only be extended by adding <see cref="XmlAttributeAttribute"/> elements. 
    ''' Please consider changing <see cref="XmlTextAttribute"/> member of the base class to string array.
    ''' </remarks>
    <XmlText>
    Public Property value As String()

    Sub New()
    End Sub

    Protected Sub New(dt As String)
        dataType = dt
    End Sub

    Protected Sub New(type As Type)
        Call Me.New(type.SchemaDataType)
    End Sub

    Public Overrides Function ToString() As String
        Return $"({Me.SchemaDataType.ToString}) {value}; resource: {resource}"
    End Function
End Class
