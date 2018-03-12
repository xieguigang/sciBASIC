Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Public Class Repository(Of K, V)

    <XmlAttribute>
    Public Property Root As Integer
    <XmlElement>
    Public Property Index As BinaryTree(Of K, V)()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Index(Root).ToString
    End Function
End Class
