#Region "Microsoft.VisualBasic::fd62aa3ae26ed0a05e7cef88390a4572, gr\Microsoft.VisualBasic.Imaging\SVG\SVGDataCache.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class SVGDataCache
    ' 
    '         Function: GetSVG, innerDefaultHeight, innerDefaultWidth, (+2 Overloads) updateLayerIndex
    ' 
    '         Sub: (+8 Overloads) Add
    ' 
    '         Operators: (+2 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

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

        Protected Friend layers As New List(Of g)
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
            layers += New g With {
                .texts = {text},
                .zIndex = ++zlayer
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(rect As rect)
            layers += New g With {
                .rect = {rect},
                .zIndex = ++zlayer
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(line As line)
            layers += New g With {
                .lines = {line},
                .zIndex = ++zlayer
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(circle As circle)
            layers += New g With {
                .circles = {circle},
                .zIndex = ++zlayer
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(path As path)
            layers += New g With {
                .path = {path},
                .zIndex = ++zlayer
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(polygon As polygon)
            layers += New g With {
                .polygon = {polygon},
                .zIndex = ++zlayer
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(image As XML.Image)
            layers += New g With {
                .images = {image},
                .zIndex = ++zlayer
            }
        End Sub

        Public Sub Add(data As SVGDataCache)
            For Each layer In data.layers
                layer.zIndex = ++zlayer
                layers += layer
            Next
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
        Public Function GetSVG(Optional size As Size = Nothing, Optional xmlComment$ = Nothing) As SVGXml
            Dim SVG As New SVGXml() With {
                .Layers = layers,
                .width = size.Width Or innerDefaultWidth(),
                .height = size.Height Or innerDefaultHeight(),
                .XmlComment = xmlComment
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
                .layers = data.layers.Select(Function(l) l + offset).AsList
            }
        End Operator
    End Class
End Namespace
