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
        Protected Friend Size As Size

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

        Public Sub Add(data As SVGDataCache)
            With data
                Call Me.texts.AddRange(.texts)
                Call Me.circles.AddRange(.circles)
                Call Me.images.AddRange(.images)
                Call Me.lines.AddRange(.lines)
                Call Me.paths.AddRange(.paths)
                Call Me.polygons.AddRange(.polygons)
                Call Me.rects.AddRange(.rects)
            End With
        End Sub

        Public Function GetSVG(size As Size) As SVGXml
            Dim SVG As New SVGXml With {
                .gs = {
                    New g With {
                        .circles = circles,
                        .polygon = polygons,
                        .rect = rects,
                        .path = paths,
                        .texts = texts,
                        .lines = lines
                    }
                },
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

        ''' <summary>
        ''' 所有的节点元素都需要进行位置位移
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Shared Operator +(data As SVGDataCache, offset As Point) As SVGDataCache
            Return data + offset.PointF
        End Operator

        Public Shared Operator +(data As SVGDataCache, offset As PointF) As SVGDataCache
            Return New SVGDataCache With {
                .bg = data.bg,
                .circles = data.circles.Select(Function(c) c + offset).ToList,
                .polygons = data.polygons.Select(Function(pl) pl + offset).ToList,
                .rects = data.rects.Select(Function(rt) rt + offset).ToList,
                .lines = data.lines.Select(Function(l) l + offset).ToList,
                .images = data.images.Select(Function(img) img + offset).ToList,
                .paths = data.paths.Select(Function(d) d + offset).ToList,
                .texts = data.texts.Select(Function(t) t + offset).ToList
            }
        End Operator
    End Class
End Namespace