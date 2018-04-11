Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    ''' <summary>
    ''' 图层滤镜
    ''' </summary>
    Public Class Filter : Inherits Node

        <XmlElement("feGaussianBlur")>
        Public Property GaussianBlurs As feGaussianBlur()
        <XmlElement("feOffset")>
        Public Property Offsets As feOffset()
        <XmlArray("feMerge")>
        Public Property Merges As feMergeNode()
    End Class

    Public Class feGaussianBlur
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property stdDeviation As String
    End Class

    Public Class feOffset
        <XmlAttribute> Public Property dx As String
        <XmlAttribute> Public Property dy As String
    End Class

    Public Class feMergeNode
        <XmlAttribute> Public Property [in] As String
    End Class
End Namespace