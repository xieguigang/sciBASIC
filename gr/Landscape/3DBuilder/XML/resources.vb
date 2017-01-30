Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class base

    <XmlAttribute> Public Property name As String
    <XmlAttribute> Public Property displaycolor As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class resources

    Public Property basematerials As basematerials
    <XmlElement("object")>
    Public Property objects As [object]()

End Class

Public Interface Iobject
    <XmlAttribute> Property id As Integer
End Interface

Public Class [object] : Implements Iobject

    <XmlAttribute("id")>
    Public Property id As Integer Implements Iobject.id
    <XmlAttribute> Public Property type As String
    <XmlAttribute> Public Property pid As String
    <XmlAttribute> Public Property pindex As Integer

    Public Property components As component()
    Public Property mesh As mesh

End Class

Public Class basematerials
    Implements Iobject

    <XmlAttribute("id")>
    Public Property id As Integer Implements Iobject.id
    <XmlElement("base")>
    Public Property basematerials As base()

    Public Overrides Function ToString() As String
        Return basematerials.GetJson
    End Function
End Class