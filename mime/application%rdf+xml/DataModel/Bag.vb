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