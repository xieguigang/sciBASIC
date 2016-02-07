
''' <summary>
''' 在rdf之中被描述的对象实体
''' </summary>
''' 
<Xml.Serialization.XmlType("Description")>
Public MustInherit Class RDFEntity

    ''' <summary>
    ''' [资源] 是可拥有 URI 的任何事物
    ''' </summary>
    ''' <returns></returns>
    <Xml.Serialization.XmlAttribute("about")> Public Property Resource As String
End Class
