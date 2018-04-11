Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports htmlNode = Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta.Node

Namespace SVG.CSS

    Public Class marker : Inherits htmlNode

        <XmlAttribute> Public Property viewBox As String
        <XmlAttribute> Public Property refX As String
        <XmlAttribute> Public Property refY As String
        <XmlAttribute> Public Property markerWidth As String
        <XmlAttribute> Public Property markerHeight As String
        <XmlAttribute> Public Property orient As String

        <XmlElement("path")>
        Public Property pathList As path()

    End Class
End Namespace