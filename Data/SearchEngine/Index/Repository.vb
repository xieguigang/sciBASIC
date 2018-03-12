Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

<XmlRoot("Repository", [Namespace]:="http://schema.sciBASIC.net/xml/Data/Index/Repository.xst")>
Public Class Repository(Of K, V)

    <XmlAttribute("root")>
    Public Property Root As Integer
    <XmlElement>
    Public Property Index As BinaryTreeIndex(Of K, V)()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Index(Root).ToString
    End Function
End Class
