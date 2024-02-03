Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    Public NotInheritable Class SvgClipPath
        Inherits SvgContainer

        Public Property ClipPathUnits As SvgClipPathUnits
            Get
                Return Element.GetAttribute(Of SvgClipPathUnits)("clipPathUnits", SvgClipPathUnits.UserSpaceOnUse)
            End Get
            Set(value As SvgClipPathUnits)
                Element.SetAttribute("clipPathUnits", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgClipPath
            Dim element = parent.OwnerDocument.CreateElement("clipPath")
            parent.AppendChild(element)
            Return New SvgClipPath(element)
        End Function
    End Class
End Namespace
