#Region "Microsoft.VisualBasic::3abb3bba0115a1cd978c2dce87391631, gr\Microsoft.VisualBasic.Imaging\SVG\XML\SVG.vb"

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

    '     Class SVGXml
    ' 
    '         Properties: circles, defs, desc, enable_background, height
    '                     images, Layers, lines, overflow, path
    '                     polygon, polyline, preserveAspectRatio, rect, space
    '                     styleCSS, texts, title, transform, version
    '                     viewBox, width, WriterComment
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: AddLayer, GetSVGXml, (+2 Overloads) SaveAsXml, Size, TryLoad
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Imaging.SVG.CSS
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.Text
Imports htmlNode = Microsoft.VisualBasic.MIME.Html.XmlMeta.Node

Namespace SVG.XML

    ' 2018-1-22
    ' XmlType / XmlRoot是不一样的？
    ' 如果要修改根节点的xmlns的话，则必须要使用XmlRoot来进行修饰

    ''' <summary>
    ''' The svg vector graphics in Xml document format.
    ''' </summary>
    <XmlRoot("svg", [Namespace]:=SVGWriter.Xmlns)> Public Class SVGXml : Inherits htmlNode
        Implements ISaveHandle
        Implements ICanvas

#Region "xml root property"
        <XmlNamespaceDeclarations()>
        Public xmlns As New XmlSerializerNamespaces

        ''' <summary>
        ''' 这个字段只是用于存储作者注释数据所使用的，并不会被显示在XML文档之中，在这里忽略掉
        ''' </summary>
        <XmlIgnore>
        Public XmlComment$

        Public Sub New()
            xmlns.Add("xlink", SVGWriter.Xlink)
        End Sub

        Sub New(width%, height%)
            Call Me.New
            Call Me.Size(New Size(width, height))
        End Sub

        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property version As String
        ''' <summary>
        ''' [offsetX offset Y width height]
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property viewBox As String()
        <XmlAttribute> Public Property preserveAspectRatio As String = "xMaxYMax"
        <XmlAttribute> Public Property overflow As String

        <XmlAttribute("enable-background")>
        Public Property enable_background As String

        <XmlAttribute("space", [Namespace]:=SVGWriter.Xmlns)>
        Public Property space As String
#End Region

        ''' <summary>
        ''' Style definition of the xml node in this svg document. 
        ''' you can define the style by using css and set the class 
        ''' attribute for the specific node to controls the 
        ''' visualize style.
        ''' </summary>
        ''' <returns></returns>
        Public Property defs As CSSStyles
        ''' <summary>
        ''' All SVG documents should have a description
        ''' </summary>
        ''' <returns></returns>
        Public Property desc As String

        ''' <summary>
        ''' SVG对象也会在这里面定义CSS
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个属性会和<see cref="htmlNode.style"/>在进行XML序列化的时候产生冲突？
        ''' </remarks>
        <XmlElement("style")>
        Public Property styleCSS As XmlMeta.CSS

        <XmlElement("image")>
        Public Property images As Image() Implements ICanvas.images

        ''' <summary>
        ''' Xml comment for <see cref="Layers"/>
        ''' </summary>
        ''' <returns></returns>
        <XmlAnyElement("gComment")>
        Public Property WriterComment As XmlComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (Me.GetType.DoCall(AddressOf XmlDataModel.GetTypeReferenceComment) & vbCrLf & vbCrLf & XmlComment).CreateComment
            End Get
            Set
            End Set
        End Property

        Dim _layers As List(Of g)

        ''' <summary>
        ''' Graphic layers
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' SVG无法通过调整``z-index``来设置图层位置，在这里需要使用一个List动态列表
        ''' 调整呈现SVG里面的图层元素
        ''' </remarks>
        <XmlElement("g")>
        Public Property Layers As g() Implements ICanvas.Layers
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _layers.ToArray
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As g())
                _layers = New List(Of g)(value)
            End Set
        End Property

        <XmlAttribute> Public Property transform As String Implements ICanvas.transform
        <XmlElement("text")> Public Property texts As text() Implements ICanvas.texts
        <XmlElement> Public Property path As path() Implements ICanvas.path
        <XmlElement> Public Property rect As rect() Implements ICanvas.rect
        <XmlElement> Public Property polygon As polygon() Implements ICanvas.polygon
        <XmlElement("line")> Public Property lines As line() Implements ICanvas.lines
        <XmlElement("circle")> Public Property circles As circle() Implements ICanvas.circles
        <XmlElement> Public Property title As String Implements ICanvas.title
        <XmlElement> Public Property polyline As polyline() Implements ICanvas.polyline

        Public Function Size(sz As Size) As SVGXml
            width = sz.Width & "px"
            height = sz.Height & "px"
            viewBox = {0, 0, sz.Width, sz.Height}
            Return Me
        End Function

        Public Function AddLayer(layer As g) As SVGXml
            _layers.Add(item:=layer)
            Return Me
        End Function

        ''' <summary>
        ''' Load SVG object from a specific xml file path or xml file text content.
        ''' </summary>
        ''' <param name="xml"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function TryLoad(xml As String) As SVGXml
            Return xml.SolveStream.LoadFromXml(Of SVGXml)(throwEx:=True)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function SaveAsXml(path$, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Return GetSVGXml.SaveTo(path, encoding)
        End Function

        ''' <summary>
        ''' 将当前的这个SVG对象序列化为XML字符串文本
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSVGXml() As String
            Return GetXml
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveAsXml(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(path, encoding.CodePage)
        End Function
    End Class
End Namespace
