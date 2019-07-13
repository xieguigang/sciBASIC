#Region "Microsoft.VisualBasic::0df0be03512a14520799e88652c775f4, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\Filter.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Filter
    ' 
    '         Properties: Composites, Floods, GaussianBlurs, height, Merges
    '                     Morphologys, Offsets, width, x, y
    ' 
    '     Class feMorphology
    ' 
    '         Properties: [in], [operator], radius, result
    ' 
    '     Class feGaussianBlur
    ' 
    '         Properties: [in], result, stdDeviation
    ' 
    '     Class feFlood
    ' 
    '         Properties: floodColor, result
    ' 
    '     Class feComposite
    ' 
    '         Properties: [in], [operator], in2, result
    ' 
    '     Class feOffset
    ' 
    '         Properties: dx, dy
    ' 
    '     Class feMergeNode
    ' 
    '         Properties: [in]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    ''' <summary>
    ''' 图层滤镜
    ''' </summary>
    ''' <remarks>
    ''' 请注意：filter里面的元素是有执行顺序的
    ''' </remarks>
    Public Class Filter : Inherits Node

        <XmlElement("feMorphology")>
        Public Property Morphologys As feMorphology()

        <XmlElement("feGaussianBlur")>
        Public Property GaussianBlurs As feGaussianBlur()
        <XmlElement("feFlood")>
        Public Property Floods As feFlood()
        <XmlElement("feComposite")>
        Public Property Composites As feComposite()
        <XmlElement("feOffset")>
        Public Property Offsets As feOffset()
        <XmlArray("feMerge")>
        Public Property Merges As feMergeNode()

        <XmlAttribute> Public Property x As String
        <XmlAttribute> Public Property y As String
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String

    End Class

    Public Class feMorphology
        <XmlAttribute> Public Property [operator] As String
        <XmlAttribute> Public Property radius As String
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feGaussianBlur
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property stdDeviation As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feFlood
        <XmlAttribute("flood-color")> Public Property floodColor As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feComposite
        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property in2 As String
        <XmlAttribute> Public Property [operator] As String
        <XmlAttribute> Public Property result As String
    End Class

    Public Class feOffset
        <XmlAttribute> Public Property dx As String
        <XmlAttribute> Public Property dy As String
    End Class

    Public Class feMergeNode
        <XmlAttribute> Public Property [in] As String
    End Class
End Namespace
