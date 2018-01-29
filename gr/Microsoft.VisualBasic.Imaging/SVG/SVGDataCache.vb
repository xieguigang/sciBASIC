#Region "Microsoft.VisualBasic::ad7e6196067ec19808b40c3ccc753e3b, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\SVGDataCache.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace SVG

    Friend Class SVGDataCache

        Protected Friend texts As New List(Of XML.text)
        Protected Friend rects As New List(Of rect)
        Protected Friend lines As New List(Of line)
        Protected Friend circles As New List(Of circle)
        Protected Friend paths As New List(Of path)
        Protected Friend polygons As New List(Of polygon)
        Protected Friend polylines As New List(Of polyline)
        Protected Friend images As New List(Of XML.Image)
        Protected Friend bg$
        Protected Friend Size As Size

        ''' <summary>
        ''' Generates the <see cref="CSSLayer"/> index order value.
        ''' </summary>
        Friend zlayer As int = 0

        Private Function updateLayerIndex(Of T As CSSLayer)(node As T) As T
            node.zIndex = ++zlayer
            Return node
        End Function

        Private Iterator Function updateLayerIndex(Of T As CSSLayer)(nodes As IEnumerable(Of T)) As IEnumerable(Of T)
            For Each node As T In nodes
                node.zIndex = ++zlayer
                Yield node
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(text As XML.text)
            texts += updateLayerIndex(text)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(rect As rect)
            rects += updateLayerIndex(rect)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(line As line)
            lines += updateLayerIndex(line)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(circle As circle)
            circles += updateLayerIndex(circle)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(path As path)
            paths += updateLayerIndex(path)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(polygon As polygon)
            polygons += updateLayerIndex(polygon)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(image As XML.Image)
            images += updateLayerIndex(image)
        End Sub

        Public Sub Add(data As SVGDataCache)
            With data
                Call Me.texts.AddRange(updateLayerIndex(.texts))
                Call Me.circles.AddRange(updateLayerIndex(.circles))
                Call Me.images.AddRange(updateLayerIndex(.images))
                Call Me.lines.AddRange(updateLayerIndex(.lines))
                Call Me.paths.AddRange(updateLayerIndex(.paths))
                Call Me.polygons.AddRange(updateLayerIndex(.polygons))
                Call Me.rects.AddRange(updateLayerIndex(.rects))
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function innerDefaultWidth() As DefaultValue(Of Integer)
            Return Size.Width.AsDefault(Function(n) CType(n, Integer) = 0)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function innerDefaultHeight() As DefaultValue(Of Integer)
            Return Size.Height.AsDefault(Function(n) CType(n, Integer) = 0)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size">
        ''' If this argument is ignored, then the default internal <see cref="Size"/> value will be used.
        ''' </param>
        ''' <returns></returns>
        Public Function GetSVG(Optional size As Size = Nothing) As SVGXml
            Dim SVG As New SVGXml() With {
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
                .width = size.Width Or innerDefaultWidth(),
                .height = size.Height Or innerDefaultHeight()
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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(data As SVGDataCache, offset As Point) As SVGDataCache
            Return data + offset.PointF
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
