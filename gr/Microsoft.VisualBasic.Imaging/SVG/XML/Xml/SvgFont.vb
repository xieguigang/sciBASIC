Imports System.Xml

Namespace SVG.XML

    Public Class SvgFont : Inherits SvgContainer

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgFont
            Dim element = parent.OwnerDocument.CreateElement("font")
            parent.AppendChild(element)
            Return New SvgFont(element)
        End Function

    End Class

    Public Class SvgFontFace : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgFontFace
            Dim element = parent.OwnerDocument.CreateElement("font-face")
            parent.AppendChild(element)
            Return New SvgFontFace(element)
        End Function

    End Class

    Public Class SvgGlyph : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgGlyph
            Dim element = parent.OwnerDocument.CreateElement("glyph")
            parent.AppendChild(element)
            Return New SvgGlyph(element)
        End Function

    End Class

    Public Class SvgMissingGlyph : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgMissingGlyph
            Dim element = parent.OwnerDocument.CreateElement("missing-glyph")
            parent.AppendChild(element)
            Return New SvgMissingGlyph(element)
        End Function

    End Class
End Namespace