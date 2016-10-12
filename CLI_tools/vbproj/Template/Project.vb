Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Project

    <XmlAttribute> Public Property ToolsVersion As String
    <XmlAttribute> Public Property DefaultTargets As String

    <XmlElement("Import")>
    Public Property [Imports] As Import()

    <XmlElement("PropertyGroup")>
    Public Property PropertyGroups As PropertyGroup()
    <XmlElement("ItemGroup")>
    Public Property ItemGroups As ItemGroup()
    <XmlElement("Target")>
    Public Property Targets As Target()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class Target
    <XmlAttribute> Public Property Name As String
End Class

Public Class Import

    <XmlAttribute> Public Property Project As String
    <XmlAttribute> Public Property Condition As String

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