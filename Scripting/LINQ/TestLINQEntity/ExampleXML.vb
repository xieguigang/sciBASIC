Imports LINQ.Extensions

<LINQ.Framework.Reflection.LINQEntity("member")>
<Xml.Serialization.XmlType("doc")> Public Class ExampleXMLCollection
    Implements LINQ.Framework.ILINQCollection

    Public Property members As List(Of member)

    Public Function GetCollection(Path As String) As Object() Implements LINQ.Framework.ILINQCollection.GetCollection
        Dim xml = Path.LoadXml(Of ExampleXMLCollection)()
        Me.members = xml.members
        Return members.ToArray
    End Function

    Public Function GetEntityType() As Type Implements LINQ.Framework.ILINQCollection.GetEntityType
        Return GetType(member)
    End Function
End Class

Public Class member
    <Xml.Serialization.XmlAttribute> Public Property name As String
    <Xml.Serialization.XmlElement> Public Property summary As String
End Class
