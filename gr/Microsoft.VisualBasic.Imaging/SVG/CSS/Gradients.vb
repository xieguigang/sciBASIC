Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    ' https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Gradients

    Public Class [stop] : Inherits Node

        <XmlAttribute("offset")> Public Property offset As String
        <XmlAttribute("stop-color")>
        Public Property stopColor As String
        <XmlAttribute("stop-opacity")>
        Public Property stopOpacity As String

        Public Overrides Function ToString() As String
            Return Me.GetXml
        End Function
    End Class

    Public Class Gradient : Inherits Node
        <XmlElement("stop")>
        Public Property stops As [stop]()
    End Class

    Public Class linearGradient : Inherits Gradient

        <XmlAttribute> Public Property x1 As String
        <XmlAttribute> Public Property x2 As String
        <XmlAttribute> Public Property y1 As String
        <XmlAttribute> Public Property y2 As String

    End Class

    Public Class radialGradient : Inherits Gradient

        <XmlAttribute> Public Property cx As String
        <XmlAttribute> Public Property cy As String
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property fx As String
        <XmlAttribute> Public Property fy As String
        <XmlAttribute> Public Property spreadMethod As String
        <XmlAttribute> Public Property gradientUnits As String

    End Class
End Namespace