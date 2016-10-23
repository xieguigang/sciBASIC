#Region "Microsoft.VisualBasic::de7977b8dfb24660a64877445af4bf85, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\SVG\Xml.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node

        ''' <summary>
        ''' CSS style definition.(请注意，假若是SVG对象则赋值这个属性无效)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property style As String
        ''' <summary>
        ''' node class id, just like the id in HTML, you can also using this attribute to tweaks on the style by CSS.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property [class] As String
        <XmlAttribute> Public Property id As String

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

    Public Class CSSStyles
        <XmlElement("style")> Public Property styles As XmlMeta.CSS()
        <XmlAttribute> Public Property id As String
    End Class
End Namespace
