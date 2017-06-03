#Region "Microsoft.VisualBasic::f98574e4e474168483666af1988a95bc, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\XML\SVG.vb"

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
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The svg vector graphics in Xml document format.
    ''' </summary>
    <XmlType("svg")> Public Class SVGXml
        Implements ISaveHandle
        Implements ICanvas

#Region "xml root property"

        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property viewBox As String()
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

        Public Sub SetSize(size As Size)
            width = size.Width & "px"
            height = size.Height & "px"
        End Sub

        Public Shared Function TryLoad(xml As String) As SVGXml
            Dim xmlDoc As New XmlDoc(xml)
            xmlDoc.xmlns.xmlns = ""
            Dim sb As New StringBuilder(xmlDoc.ToString)
            Call sb.Replace("xlink:href=""", "image.data=""")
            Return sb.ToString.LoadFromXml(Of SVGXml)(throwEx:=True)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Private Function SaveAsXml(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            Return GetSVGXml.SaveTo(Path, encoding)
        End Function

        ''' <summary>
        ''' 将当前的这个SVG对象序列化为XML字符串文本
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSVGXml() As String
            Dim sb As New StringBuilder(Me.GetXml)
            Call sb.Replace("image.data=""", "xlink:href=""")

            Dim xml As New XmlDoc(sb.ToString)
            xml.encoding = XmlEncodings.UTF8
            xml.standalone = False
            xml.xmlns.Set("xlink", "http://www.w3.org/1999/xlink")
            xml.xmlns.xmlns = "http://www.w3.org/2000/svg"

            Return xml.ToString
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Function SaveAsXml(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(Path, encoding.CodePage)
        End Function
    End Class
End Namespace
