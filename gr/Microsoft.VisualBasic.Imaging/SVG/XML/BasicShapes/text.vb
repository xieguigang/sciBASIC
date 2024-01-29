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
Imports System.Xml.Serialization

Namespace SVG.XML

    ''' <summary>
    ''' SVG之中的文本对象
    ''' </summary>
    Public Class text : Inherits node

        <XmlAttribute> Public Property dy As String
        ''' <summary>
        ''' 文本字符串
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property value As String
        <XmlAttribute("text-anchor")> Public Property anchor As String
        <XmlAttribute("font-family")> Public Property fontFamily As String
        <XmlAttribute("font-size")> Public Property fontSize As String
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

        Sub New()
        End Sub

        Sub New(text As String)
            Me.value = text
        End Sub

        Sub New(x As Double, y As Double, text As String)
            Me.x = x
            Me.y = y
            Me.value = text
        End Sub

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
