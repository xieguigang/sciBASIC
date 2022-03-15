#Region "Microsoft.VisualBasic::6d6c519d9ad4a4293658d6ef668ec199, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml.vb"

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


    ' Code Statistics:

    '   Total Lines: 247
    '    Code Lines: 158
    ' Comment Lines: 50
    '   Blank Lines: 39
    '     File Size: 7.48 KB


    '     Class title
    ' 
    '         Properties: innerHTML
    ' 
    '     Class circle
    ' 
    '         Properties: cx, cy, r, title
    '         Operators: +
    ' 
    '     Class polygon
    ' 
    '         Properties: points
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Operators: +
    ' 
    '     Class polyline
    ' 
    '         Properties: markerEnd, points
    ' 
    '         Function: ToString
    '         Operators: +
    ' 
    '     Class rect
    ' 
    '         Properties: height, rx, ry, width, x
    '                     y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Operators: +
    ' 
    '     Class path
    ' 
    '         Properties: d
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Operators: +
    ' 
    '     Class line
    ' 
    '         Properties: DashArray, x1, x2, y1, y2
    '         Operators: +
    ' 
    '     Class text
    ' 
    '         Properties: anchor, dy, transform, value, x
    '                     y
    ' 
    '         Function: ToString
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace SVG.XML

    Public Class title : Inherits node
        <XmlText>
        Public Property innerHTML As String
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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return cache
            End Get
            Set(value As String())
                cache = value _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray
                data = cache _
                    .Select(AddressOf FloatPointParser) _
                    .ToArray
            End Set
        End Property

        Dim data As PointF()
        Dim cache$()

        Sub New()
        End Sub

        Sub New(pts As IEnumerable(Of PointF))
            data = pts.ToArray
            cache = data _
                .Select(Function(pt) $"{pt.X},{pt.Y}") _
                .ToArray
        End Sub

        Public Shared Operator +(polygon As polygon, offset As PointF) As polygon
            Dim points As PointF() = polygon _
                .data _
                .Select(Function(pt)
                            Return New PointF With {
                                .X = pt.X + offset.X,
                                .Y = pt.Y + offset.Y
                            }
                        End Function) _
                .ToArray
            Return New polygon(points) With {
                .style = polygon.style,
                .id = polygon.id,
                .class = polygon.class
            }
        End Operator
    End Class

    Public Class polyline : Inherits node

        <XmlAttribute> Public Property points As String()
        <XmlAttribute("marker-end")>
        Public Property markerEnd As String

        Public Overrides Function ToString() As String
            Return points.JoinBy(" ")
        End Function

        Public Shared Operator +(line As polyline, offset As PointF) As polyline
            ' Throw New NotImplementedException
            Return Nothing
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

#Region "圆角矩形"
        Public Property rx As String
        Public Property ry As String
#End Region

        Sub New()
        End Sub

        Sub New(rect As RectangleF)
            With Me
                .width = rect.Width
                .height = rect.Height
                .x = rect.X
                .y = rect.Y
            End With
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
            d = path.SVGPathData
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

        <XmlAttribute("stroke-dasharray")>
        Public Property DashArray As Integer()

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

        Public Overrides Function ToString() As String
            Return $"[{x}, {y}] {value}"
        End Function

        Public Shared Operator +(text As text, offset As PointF) As text
            text = DirectCast(text.MemberwiseClone, text)
            text.x += offset.X
            text.y += offset.Y
            Return text
        End Operator
    End Class
End Namespace
