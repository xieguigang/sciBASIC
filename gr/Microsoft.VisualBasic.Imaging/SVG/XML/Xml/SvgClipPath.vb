Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgClipPath
        Inherits SvgContainer
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgClipPath
            Dim element = parent.OwnerDocument.CreateElement("clipPath")
            parent.AppendChild(element)
            Return New SvgClipPath(element)
        End Function

        Public Property ClipPathUnits As SvgClipPathUnits
            Get
                Return Element.GetAttribute(Of SvgClipPathUnits)("clipPathUnits", SvgClipPathUnits.UserSpaceOnUse)
            End Get
            Set(value As SvgClipPathUnits)
                Element.SetAttribute("clipPathUnits", value)
            End Set
        End Property
    End Class
End Namespace
