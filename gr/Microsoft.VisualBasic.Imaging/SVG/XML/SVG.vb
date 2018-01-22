#Region "Microsoft.VisualBasic::f98574e4e474168483666af1988a95bc, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\XML\SVG.vb"

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
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Text

Namespace SVG.XML

    ' 2018-1-22
    ' XmlType / XmlRoot是不一样的？
    ' 如果要修改根节点的xmlns的话，则必须要使用XmlRoot来进行修饰

    ''' <summary>
    ''' The svg vector graphics in Xml document format.
    ''' </summary>
    <XmlRoot("svg", [Namespace]:=SVGWriter.Xmlns)> Public Class SVGXml
        Implements ISaveHandle
        Implements ICanvas

#Region "xml root property"
        <XmlNamespaceDeclarations()>
        Public xmlns As New XmlSerializerNamespaces

        Public Sub New()
            xmlns.Add("xlink", SVGWriter.Xlink)
        End Sub

        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property viewBox As String()
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
        ''' SVG对象也会在这里面定义CSS
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("style")> Public Shadows Property style As XmlMeta.CSS
        <XmlElement("image")> Public Property images As Image()

        <XmlAttribute> Public Property transform As String Implements ICanvas.transform
        <XmlElement("text")> Public Property texts As text() Implements ICanvas.texts
        <XmlElement("g")> Public Property gs As g() Implements ICanvas.gs
        <XmlElement> Public Property path As path() Implements ICanvas.path
        <XmlElement> Public Property rect As rect() Implements ICanvas.rect
        <XmlElement> Public Property polygon As polygon() Implements ICanvas.polygon
        <XmlElement("line")> Public Property lines As line() Implements ICanvas.lines
        <XmlElement("circle")> Public Property circles As circle() Implements ICanvas.circles
        <XmlElement> Public Property title As String Implements ICanvas.title
        <XmlElement> Public Property polyline As polyline() Implements ICanvas.polyline

        Public Sub SetSize(size As Size)
            width = size.Width & "px"
            height = size.Height & "px"
        End Sub

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
        Private Function SaveAsXml(Optional path$ = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
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
        Public Function SaveAsXml(Optional path$ = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(path, encoding.CodePage)
        End Function
    End Class
End Namespace
