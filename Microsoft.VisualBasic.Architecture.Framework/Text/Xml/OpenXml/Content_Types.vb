Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.OpenXml

    ''' <summary>
    ''' ``[Content_Types].xml``
    ''' </summary>
    ''' 
    <XmlRoot("Types", Namespace:="http://schemas.openxmlformats.org/package/2006/content-types")>
    Public Class Content_Types
        <XmlElement> Public Property [Default] As Type()
        <XmlElement("Override")>
        Public Property [Overrides] As Type()
    End Class

    Public Structure Type

        <XmlAttribute> Public Property Extension As String
        <XmlAttribute> Public Property ContentType As String
        <XmlAttribute> Public Property PartName As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace