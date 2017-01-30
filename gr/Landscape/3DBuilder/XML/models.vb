Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class mesh

    Public Property vertices As Point3D()
    Public Property triangles As triangle()
    Public Property components As component()

End Class

Public Class triangle

    <XmlAttribute> Public Property v1 As Integer
    <XmlAttribute> Public Property v2 As Integer
    <XmlAttribute> Public Property v3 As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class component

    <XmlAttribute>
    Public Property objectid As Integer
End Class