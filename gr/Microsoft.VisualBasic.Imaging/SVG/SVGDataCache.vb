Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Namespace SVG

    Friend Class SVGDataCache

        Protected Friend texts As New List(Of XML.text)
        Protected Friend rects As New List(Of rect)
        Protected Friend lines As New List(Of line)
        Protected Friend circles As New List(Of circle)
        Protected Friend paths As New List(Of path)
        Protected Friend polygons As New List(Of polygon)
        Protected Friend images As New List(Of XML.Image)
        Protected Friend bg$

        Public Sub Add(text As XML.text)
            texts += text
        End Sub

        Public Sub Add(rect As rect)
            rects += rect
        End Sub

        Public Sub Add(line As line)
            lines += line
        End Sub

        Public Sub Add(circle As circle)
            circles += circle
        End Sub

        Public Sub Add(path As path)
            paths += path
        End Sub

        Public Sub Add(polygon As polygon)
            polygons += polygon
        End Sub

        Public Sub Add(image As XML.Image)
            images += image
        End Sub

        Public Function GetSVG(size As Size) As SVGXml
            Dim SVG As New SVGXml With {
                .circles = circles,
                .polygon = polygons,
                .rect = rects,
                .path = paths,
                .texts = texts,
                .lines = lines,
                .width = size.Width,
                .height = size.Height
            }

            If Not bg.StringEmpty Then
                SVG.style = New XmlMeta.CSS With {
                    .style = "svg{ background-color: " & bg & "; }"
                }
            End If

            Return SVG
        End Function
    End Class
End Namespace