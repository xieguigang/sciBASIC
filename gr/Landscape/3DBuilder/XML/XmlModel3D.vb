Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``3D/3dmodel.model`` xml file.
''' </summary>
''' 
<XmlRoot("model")> Public Class XmlModel3D

    <XmlAttribute>
    Public Property unit As String
    Public Property resources As resources
    Public Property build As build

End Class

Public Class build

    <XmlElement("item")> Public Property items As item()

    Public Overrides Function ToString() As String
        Return items.GetJson
    End Function
End Class

Public Class item

    <XmlAttribute> Public Property objectid As Integer
    <XmlAttribute> Public Property transform As Double()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class