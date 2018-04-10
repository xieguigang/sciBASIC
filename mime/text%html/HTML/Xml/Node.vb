Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace HTML.XmlMeta

    Public MustInherit Class Node

        <XmlAttribute> Public Property id As String
        ''' <summary>
        ''' node class id, just like the id in HTML, you can also using this attribute to tweaks on the style by CSS.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [class] As String

        ''' <summary>
        ''' CSS style definition <see cref="ICSSValue"/>.(请注意，假若是SVG对象则赋值这个属性无效)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property style As String

    End Class

    Public Class GenericNode : Inherits Node

        <XmlAttribute> Public Property Tag As String

    End Class
End Namespace