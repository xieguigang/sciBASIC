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
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Scripting

Namespace SVG.XML

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

        Public Shared Operator +(c As circle, offset As PointF) As circle
            c = DirectCast(c.MemberwiseClone, circle)
            c.cx += offset.X
            c.cy += offset.Y
            Return c
        End Operator
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
            Get
                Return cache
            End Get
            Set(value As String())
                cache = value
                data = value.Select(AddressOf FloatPointParser).ToArray
            End Set
        End Property

        Dim data As PointF()
        Dim cache$()

        Sub New()
        End Sub

        Sub New(pts As IEnumerable(Of PointF))
            data = pts.ToArray
            cache = data.Select(Function(pt) $"{pt.X},{pt.Y}").ToArray
        End Sub

        Public Shared Operator +(polygon As polygon, offset As PointF) As polygon
            Dim points As PointF() = polygon _
                .data _
                .Select(Function(pt) New PointF(pt.X + offset.X, pt.Y + offset.Y)) _
                .ToArray
            Return New polygon(points) With {
                .style = polygon.style,
                .id = polygon.id,
                .class = polygon.class
            }
        End Operator
    End Class

    ''' <summary>
    ''' 矩形对象
    ''' </summary>
    Public Class rect : Inherits node

        <XmlAttribute> Public Property height As String
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property y As String
        <XmlAttribute> Public Property x As String

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

        Public Shared Operator +(rect As rect, offset As PointF) As rect
            rect = DirectCast(rect.MemberwiseClone(), rect)
            rect.x += offset.X
            rect.y += offset.Y
            Return rect
        End Operator
    End Class

    ''' <summary>
    ''' ``&lt;path>`` 标签用来定义路径。
    ''' </summary>
    Public Class path : Inherits node

        ''' <summary>
        ''' 下面的命令可用于路径数据：
        ''' 
        ''' M = moveto
        ''' L = lineto
        ''' H = horizontal lineto
        ''' V = vertical lineto
        ''' C = curveto
        ''' S = smooth curveto
        ''' Q = quadratic Belzier curve
        ''' T = smooth quadratic Belzier curveto
        ''' A = elliptical Arc
        ''' Z = closepath
        ''' 
        ''' 注释：以上所有命令均允许小写字母。大写表示绝对定位，小写表示相对定位。
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property d As String

        Sub New()
        End Sub

        Sub New(path As GraphicsPath)
            Dim points = path.PathData.Points.Select(Function(pt) $"{pt.X} {pt.Y}")
            Dim sb As New StringBuilder
            Call sb.Append("M" & points.First)
            For Each pt In points.Skip(1)
                Call sb.Append(" ")
                Call sb.Append("L" & pt)
            Next
            Call sb.Append("Z")
            d = sb.ToString
        End Sub

        Public Shared Operator +(path As path, offset As PointF) As path
            Dim data = path.d.Split
            path = DirectCast(path.MemberwiseClone, path)

            ' 这里该如何进行偏移？

            Return path
        End Operator
    End Class

    ''' <summary>
    ''' 一个线段对象
    ''' </summary>
    Public Class line : Inherits node

        <XmlAttribute> Public Property y2 As Single
        <XmlAttribute> Public Property x2 As Single
        <XmlAttribute> Public Property y1 As Single
        <XmlAttribute> Public Property x1 As Single

        Public Shared Operator +(line As line, offset As PointF) As line
            line = DirectCast(line.MemberwiseClone, line)

            With line
                .x1 += offset.X
                .x2 += offset.X
                .y1 += offset.Y
                .y2 += offset.Y

                Return line
            End With
        End Operator
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

        Public Shared Operator +(text As text, offset As PointF) As text
            text = DirectCast(text.MemberwiseClone, text)
            text.x += offset.X
            text.y += offset.Y
            Return text
        End Operator
    End Class

    ''' <summary>
    ''' 在这个SVG对象之中所定义的CSS样式数据
    ''' </summary>
    Public Class CSSStyles
        <XmlElement("style")> Public Property styles As XmlMeta.CSS()
        <XmlAttribute> Public Property id As String
    End Class
End Namespace
