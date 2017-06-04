#Region "Microsoft.VisualBasic::2d5508eb97ee2937cbb6c3c1835a6d7b, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\SVGDataCache.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
                .circles = data.circles.Select(Function(c) c + offset).AsList,
                .polygons = data.polygons.Select(Function(pl) pl + offset).AsList,
                .rects = data.rects.Select(Function(rt) rt + offset).AsList,
                .lines = data.lines.Select(Function(l) l + offset).AsList,
                .images = data.images.Select(Function(img) img + offset).AsList,
                .paths = data.paths.Select(Function(d) d + offset).AsList,
                .texts = data.texts.Select(Function(t) t + offset).AsList
            }
        End Operator
    End Class
End Namespace
