#Region "Microsoft.VisualBasic::cedbd375d2122f475d2c3d66cc02a28a, mime\application%rdf+xml\DataModel\Bag.vb"

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

    '   Total Lines: 41
    '    Code Lines: 26 (63.41%)
    ' Comment Lines: 7 (17.07%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 8 (19.51%)
    '     File Size: 1.35 KB


    ' Class Array
    ' 
    '     Properties: list
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' Class li
    ' 
    '     Properties: resource
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

''' <summary>
''' ``&lt;Bag>``、``&lt;Seq>`` 以及 ``&lt;Alt>``
''' 
''' + ``&lt;rdf:Bag>`` 元素用于描述一个规定为无序的值的列表。元素可包含重复的值。
''' + ``&lt;rdf:Seq>`` 元素用于描述一个规定为有序的值的列表（比如一个字母顺序的排序）。
''' + ``&lt;rdf:Alt>`` 元素用于一个可替换的值的列表（用户仅可选择这些值的其中之一）。
''' </summary>
<XmlType(NameOf(Array), [Namespace]:=RDFEntity.XmlnsNamespace)>
Public Class Array

    <XmlNamespaceDeclarations()>
    Public xmlns As New XmlSerializerNamespaces

    <XmlElement("li", [Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property list As li()

    Sub New()
        xmlns.Add("rdf", RDFEntity.XmlnsNamespace)
    End Sub

    Public Overrides Function ToString() As String
        Return $"listof {list.Count} elements: {list.Take(3).JoinBy(", ")}..."
    End Function
End Class

<XmlType("item", [Namespace]:="NA")>
Public Class li

    <XmlAttribute([Namespace]:=RDFEntity.XmlnsNamespace)>
    Public Property resource As String

    Public Overrides Function ToString() As String
        If resource Is Nothing Then
            Return "null"
        Else
            Return resource
        End If
    End Function
End Class
