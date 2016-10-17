Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Public Class Project : Implements ISaveHandle

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

    Public Function GetProfile(condition As String) As PropertyGroup
        Return LinqAPI.DefaultFirst(Of PropertyGroup) <=
 _
            From x As PropertyGroup
            In PropertyGroups
            Where String.Equals(
                condition,
                x.Condition,
                StringComparison.OrdinalIgnoreCase)
            Select x

    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function RemoveNamespace(xml As String) As String
        Dim doc As New XmlDoc(xml)
        doc.xmlns.xmlns = Nothing
        xml = doc.ToString
        Return xml
    End Function

    Const xmlns$ = "http://schemas.microsoft.com/developer/msbuild/2003"

    Public Function Save(Optional path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
        Dim xml As New XmlDoc(GetXml)
        xml.xmlns.xmlns = Project.xmlns
        xml.xmlns.xsd = ""
        xml.xmlns.xsi = ""
        xml.encoding = XmlEncodings.UTF8
        Return xml.ToString.SaveTo(path, encoding)
    End Function

    Public Function Save(Optional path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return Save(path, encoding.GetEncodings)
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