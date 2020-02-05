#Region "Microsoft.VisualBasic::84fb6ca8f13bde80212a3ec5cecc61c8, gr\Microsoft.VisualBasic.Imaging\SVG\SVGDataLayers.vb"

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

    '     Class SVGDataLayers
    ' 
    '         Function: (+7 Overloads) Add, ApplyFilter, GetSVG, innerDefaultHeight, innerDefaultWidth
    '                   updateLayerIndex
    ' 
    '         Sub: Add, ApplyFilter
    ' 
    '         Operators: (+2 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging.SVG.CSS
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace SVG

    ''' <summary>
    ''' 使用<see cref="g"/>图层的方式构建出一个完整的SVG模型
    ''' </summary>
    Public Class SVGDataLayers

        Protected Friend layers As New HashList(Of g)
        Protected Friend bg$
        Protected Friend Size As Size

        ''' <summary>
        ''' <see cref="Filter.id"/>为字典的键名
        ''' </summary>
        Protected Friend filters As Dictionary(Of String, Filter)

        ''' <summary>
        ''' Generates the <see cref="CSSLayer"/> index order value.
        ''' </summary>
        Friend zlayer As i32 = 0

        Default Public ReadOnly Property GetLayer(zindex As Integer) As g
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return layers(zindex)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="zindex%">图层的编号</param>
        ''' <param name="filter$">filter的id编号</param>
        Public Sub ApplyFilter(zindex%, filter$)
            layers(zindex).filter = $"url(#filter)"
        End Sub

        ''' <summary>
        ''' 通过元素选择器来设置滤镜，函数返回所有<paramref name="selector"/>查找成功的图层的编号
        ''' </summary>
        ''' <param name="selector$"></param>
        ''' <param name="filter$"></param>
        Public Iterator Function ApplyFilter(selector$, filter$) As IEnumerable(Of Integer)

        End Function

        Private Iterator Function updateLayerIndex(Of T As CSSLayer)(nodes As IEnumerable(Of T)) As IEnumerable(Of T)
            For Each node As T In nodes
                node.zIndex = ++zlayer
                Yield node
            Next
        End Function

#Region "Add svg shape element"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(text As XML.text) As Integer
            layers += New g With {
                .texts = {text},
                .zIndex = ++zlayer
            }
            Return zlayer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(rect As rect) As Integer
            layers += New g With {
                .rect = {rect},
                .zIndex = ++zlayer
            }
            Return zlayer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(line As line) As Integer
            layers += New g With {
                .lines = {line},
                .zIndex = ++zlayer
            }
            Return zlayer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(circle As circle) As Integer
            layers += New g With {
                .circles = {circle},
                .zIndex = ++zlayer
            }
            Return zlayer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(path As path) As Integer
            layers += New g With {
                .path = {path},
                .zIndex = ++zlayer
            }
            Return zlayer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(polygon As polygon, layerComment$) As Integer
            layers += New g With {
                .polygon = {polygon},
                .zIndex = ++zlayer,
                .XmlCommentValue = layerComment
            }
            Return zlayer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(image As XML.Image) As Integer
            layers += New g With {
                .images = {image},
                .zIndex = ++zlayer
            }
            Return zlayer
        End Function

        Public Sub Add(data As SVGDataLayers)
            For Each layer In data.layers
                layer.zIndex = ++zlayer
                layers += layer
            Next
        End Sub
#End Region

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function innerDefaultWidth() As [Default](Of  Integer)
            Return Size.Width.AsDefault(Function(n) CType(n, Integer) = 0)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function innerDefaultHeight() As [Default](Of  Integer)
            Return Size.Height.AsDefault(Function(n) CType(n, Integer) = 0)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size">
        ''' If this argument is ignored, then the default internal <see cref="Size"/> value will be used.
        ''' </param>
        ''' <returns></returns>
        Public Function GetSVG(Optional size As Size = Nothing,
                               Optional xmlComment$ = Nothing,
                               Optional desc$ = Nothing,
                               Optional title$ = Nothing) As SVGXml

            Dim SVG As New SVGXml() With {
                .Layers = layers,
                .width = size.Width Or innerDefaultWidth(),
                .height = size.Height Or innerDefaultHeight(),
                .XmlComment = xmlComment,
                .desc = desc,
                .title = title
            }

            If Not bg.StringEmpty Then
                SVG.styleCSS = New XmlMeta.CSS With {
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
        Public Shared Operator +(data As SVGDataLayers, offset As Point) As SVGDataLayers
            Return data + offset.PointF
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(data As SVGDataLayers, offset As PointF) As SVGDataLayers
            Return New SVGDataLayers With {
                .bg = data.bg,
                .layers = data.layers _
                              .Select(Function(l) l + offset) _
                              .AsHashList
            }
        End Operator
    End Class
End Namespace
