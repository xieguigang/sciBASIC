
Imports System.Xml

Namespace SVG.XML

    Public Class SvgDefs : Inherits SvgContainer

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Function CreateMarker(id As String, w As Single, h As Single) As SvgMarker
            Dim marker = SvgMarker.Create(Element)
            marker.Id = id
            marker.Width = w
            marker.Height = h
            marker.ViewBox = New SvgViewBox(0, 0, 10, 10)
            marker.RefX = 5
            marker.RefY = 5
            marker.orient = "auto-start-reverse"
            marker.AddPath.D = "M 0 0 L 10 5 L 0 10 z"
            Return marker
        End Function

        Public Function GetMarkerById(id As String) As SvgMarker
            For Each element As SvgElement In GetElements()
                If TypeOf element Is SvgMarker AndAlso element.Id = id Then
                    Return element
                End If
            Next

            Return Nothing
        End Function

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgDefs
            Dim element = parent.OwnerDocument.CreateElement("defs")
            parent.AppendChild(element)
            Return New SvgDefs(element)
        End Function
    End Class
End Namespace