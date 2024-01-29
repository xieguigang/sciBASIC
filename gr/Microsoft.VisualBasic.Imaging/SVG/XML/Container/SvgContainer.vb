Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' SVG之中的画布对象
    ''' </summary>
    Public MustInherit Class SvgContainer : Inherits SvgElement

        Public Property filter As String
            Get
                Return Element.GetAttribute("filter")
            End Get
            Set(value As String)
                Element.SetAttribute("filter", value)
            End Set
        End Property

        Protected Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Function AddCircle() As SvgCircle
            Return SvgCircle.Create(Element)
        End Function

        Public Function AddClipPath() As SvgClipPath
            Return SvgClipPath.Create(Element)
        End Function

        Public Function AddEllipse() As SvgEllipse
            Return SvgEllipse.Create(Element)
        End Function

        Public Function AddGroup() As SvgGroup
            Return SvgGroup.Create(Element)
        End Function

        Public Function AddImage() As SvgImage
            Return SvgImage.Create(Element)
        End Function

        Public Function AddLine() As SvgLine
            Return SvgLine.Create(Element)
        End Function

        Public Function AddMarker() As SvgMarker
            Return SvgMarker.Create(Element)
        End Function

        Public Function AddPath() As SvgPath
            Return SvgPath.Create(Element)
        End Function

        Public Function AddPolygon() As SvgPolygon
            Return SvgPolygon.Create(Element)
        End Function

        Public Function AddPolyLine() As SvgPolyLine
            Return SvgPolyLine.Create(Element)
        End Function

        Public Function AddRect() As SvgRect
            Return SvgRect.Create(Element)
        End Function

        Public Function AddText() As SvgText
            Return SvgText.Create(Element)
        End Function

        Public Iterator Function GetElements() As IEnumerable(Of SvgElement)
            For i As Integer = 0 To Element.ChildNodes.Count - 1
                Yield SvgElement.Create(DirectCast(Element.ChildNodes(i), XmlElement))
            Next
        End Function
    End Class
End Namespace
