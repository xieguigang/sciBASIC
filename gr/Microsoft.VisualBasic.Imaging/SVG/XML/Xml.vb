#Region "Microsoft.VisualBasic::fad91c5af7002e6a4cf0d392e5549d98, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\Xml.vb"

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
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVG

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node

        ''' <summary>
        ''' CSS style definition <see cref="ICSSValue"/>.(请注意，假若是SVG对象则赋值这个属性无效)
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
        <XmlAttribute> Public Property cy As Single
        <XmlAttribute> Public Property cx As Single
        <XmlAttribute> Public Property r As Single
        Public Property title As title
    End Class

    ''' <summary>
    ''' 不规则的多边形对象
    ''' </summary>
    Public Class polygon : Inherits node
        ''' <summary>
        ''' 定点坐标列表
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property points As String()
    End Class

    ''' <summary>
    ''' 矩形对象
    ''' </summary>
    Public Class rect : Inherits node

        Public Property height As String
        Public Property width As String
        Public Property y As String
        Public Property x As String

        Sub New()
        End Sub

        Sub New(rect As Rectangle)
            With Me
                .width = rect.Width
                .height = rect.Height
                .x = rect.X
                .y = rect.Y
            End With
        End Sub
    End Class

    Public Class path : Inherits node
        <XmlAttribute> Public Property d As String
    End Class

    ''' <summary>
    ''' 一个线段对象
    ''' </summary>
    Public Class line : Inherits node
        <XmlAttribute> Public Property y2 As Single
        <XmlAttribute> Public Property x2 As Single
        <XmlAttribute> Public Property y1 As Single
        <XmlAttribute> Public Property x1 As Single
    End Class

    ''' <summary>
    ''' SVG之中的文本对象
    ''' </summary>
    Public Class text : Inherits node
        <XmlAttribute> Public Property transform As String
        <XmlAttribute> Public Property dy As String
        ''' <summary>
        ''' 文本字符串
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property value As String
        <XmlAttribute("text-anchor")> Public Property anchor As String
        ''' <summary>
        ''' 文本对象在画布上面的坐标位置
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property y As String
        ''' <summary>
        ''' 文本对象在画布上面的坐标位置
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property x As String
    End Class

    ''' <summary>
    ''' 在这个SVG对象之中所定义的CSS样式数据
    ''' </summary>
    Public Class CSSStyles
        <XmlElement("style")> Public Property styles As XmlMeta.CSS()
        <XmlAttribute> Public Property id As String
    End Class
End Namespace
