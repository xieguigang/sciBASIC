Imports System.Xml.Serialization

Namespace SVG.CSS

    ' https://developer.mozilla.org/en-US/docs/Web/SVG/Tutorial/Gradients

    Public Class [stop]
        <XmlAttribute("class")> Public Property [class] As String
        <XmlAttribute("offset")> Public Property offset As String
        <XmlAttribute("stop-color")>
        Public Property stopColor As String
        <XmlAttribute("stop-opacity")>
        Public Property stopOpacity As String

        Public Overrides Function ToString() As String
            Return Me.GetXml
        End Function
    End Class

    Public Class linearGradient

        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property x1 As String
        <XmlAttribute> Public Property x2 As String
        <XmlAttribute> Public Property y1 As String
        <XmlAttribute> Public Property y2 As String

        <XmlElement("stop")>
        Public Property stops As [stop]()
    End Class

    Public Class radialGradient

        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property cx As String
        <XmlAttribute> Public Property cy As String
        <XmlAttribute> Public Property r As String
        <XmlAttribute> Public Property fx As String
        <XmlAttribute> Public Property fy As String
        <XmlAttribute> Public Property spreadMethod As String
        <XmlAttribute> Public Property gradientUnits As String

        <XmlElement("stop")>
        Public Property stops As [stop]()
    End Class
End Namespace