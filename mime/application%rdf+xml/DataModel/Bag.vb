#Region "Microsoft.VisualBasic::debfddc8e0d4d125c72b8b71eca7698f, ..\sciBASIC#\mime\application%rdf+xml\DataModel\Bag.vb"

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

Imports System.Xml.Serialization

''' <summary>
''' ``&lt;Bag>``、``&lt;Seq>`` 以及 ``&lt;Alt>``
''' 
''' + ``&lt;rdf:Bag>`` 元素用于描述一个规定为无序的值的列表。元素可包含重复的值。
''' + ``&lt;rdf:Seq>`` 元素用于描述一个规定为有序的值的列表（比如一个字母顺序的排序）。
''' + ``&lt;rdf:Alt>`` 元素用于一个可替换的值的列表（用户仅可选择这些值的其中之一）。
''' </summary>
<XmlType(NameOf(Array), [Namespace]:=RDF.Namespace)>
Public Class Array

    <XmlNamespaceDeclarations()>
    Public xmlns As XmlSerializerNamespaces

    Sub New()
        xmlns.Add("rdf", RDF.Namespace)
    End Sub

    <XmlElement("li", [Namespace]:=RDF.Namespace)>
    Public Property list As String()
End Class
