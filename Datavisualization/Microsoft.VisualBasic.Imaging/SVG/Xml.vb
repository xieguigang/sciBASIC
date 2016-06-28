#Region "251707da58147b071500678b016029cd, ..\Microsoft.VisualBasic.Imaging\SVG\Xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
Imports Microsoft.VisualBasic.MarkupLanguage.HTML
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node

        ''' <summary>
        ''' CSS style definition
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property style As String
        ''' <summary>
        ''' node class id, just like the id in HTML, you can also using this attribute to tweaks on the style by CSS.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [class] As String

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class

    Public Class title : Inherits node
        <XmlText> Public Property innerHTML As String
    End Class

    Public Class circle : Inherits node
        <XmlAttribute> Public Property cy As Double
        <XmlAttribute> Public Property cx As Double
        <XmlAttribute> Public Property r As Double
        Public Property title As title
    End Class

    Public Class g : Inherits node
        <XmlAttribute> Public Property transform As String
        <XmlElement("text")> Public Property texts As text()
        <XmlElement("g")> Public Property gs As g()
        <XmlElement> Public Property path As path()
        <XmlElement> Public Property rect As rect()
        <XmlElement> Public Property polygon As polygon()
        <XmlElement("line")> Public Property lines As line()
        <XmlElement("circle")> Public Property circles As circle()
        <XmlAttribute> Public Property fill As String
    End Class

    Public Class polygon : Inherits node
        <XmlAttribute> Public Property points As String()
    End Class

    Public Class rect : Inherits node
        Public Property height As String
        Public Property width As String
        Public Property y As String
        Public Property x As String
    End Class

    Public Class path : Inherits node
        <XmlAttribute> Public Property d As String
    End Class

    Public Class line : Inherits node
        <XmlAttribute> Public Property y2 As Double
        <XmlAttribute> Public Property x2 As Double
        <XmlAttribute> Public Property y1 As Double
        <XmlAttribute> Public Property x1 As Double
    End Class

    Public Class text : Inherits node
        <XmlAttribute> Public Property transform As String
        <XmlAttribute> Public Property dy As String
        <XmlText> Public Property value As String
        <XmlAttribute("text-anchor")> Public Property anchor As String
        <XmlAttribute> Public Property y As String
        <XmlAttribute> Public Property x As String
    End Class

    ''' <summary>
    ''' The svg vector graphics in Xml document format.
    ''' </summary>
    <XmlType("svg")> Public Class SVGXml : Inherits g
        Implements ISaveHandle

#Region "xml root property"

        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property version As String
#End Region

        ''' <summary>
        ''' Style definition of the xml node in this svg document. 
        ''' you can define the style by using css and set the class 
        ''' attribute for the specific node to controls the 
        ''' visualize style.
        ''' </summary>
        ''' <returns></returns>
        Public Property defs As CSSStyles

        Public Sub SetSize(size As Size)
            width = size.Width & "px"
            height = size.Height & "px"
        End Sub

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Private Function SaveAsXml(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            Dim xml As New XmlDoc(Me.GetXml)
            xml.encoding = XmlEncodings.UTF8
            xml.standalone = False
            xml.xmlns.Set("xlink", "http://www.w3.org/1999/xlink")
            xml.xmlns.xmlns = "http://www.w3.org/2000/svg"

            Return xml.SaveTo(Path, encoding)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Function SaveAsXml(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(Path, encoding.GetEncodings)
        End Function
    End Class

    Public Class CSSStyles
        <XmlElement("style")> Public Property styles As XmlMeta.CSS()
    End Class
End Namespace
