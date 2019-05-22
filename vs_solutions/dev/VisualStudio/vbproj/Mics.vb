
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Target
    <XmlAttribute>
    Public Property Name As String
End Class

Public Class Import

    <XmlAttribute> Public Property Project As String
    <XmlAttribute> Public Property Condition As String
    <XmlAttribute> Public Property Label As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class ConditionValue

    <XmlAttribute>
    Public Property Condition As String
    <XmlText>
    Public Property value As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
