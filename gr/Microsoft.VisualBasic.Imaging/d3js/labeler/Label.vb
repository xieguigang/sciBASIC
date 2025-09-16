#Region "Microsoft.VisualBasic::d4db37b73c74a963bb749c2f19e7ed92, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\Label.vb"

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

    '   Total Lines: 145
    '    Code Lines: 86 (59.31%)
    ' Comment Lines: 39 (26.90%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (13.79%)
    '     File Size: 4.63 KB


    '     Class Label
    ' 
    '         Properties: height, location, pinned, rectangle, text
    '                     width, X, Y
    ' 
    '         Constructor: (+6 Overloads) Sub New
    '         Function: distanceTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text.Nudge

Namespace d3js.Layout

    ''' <summary>
    ''' a text label object
    ''' </summary>
    Public Class Label

        ''' <summary>
        ''' the x-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
            Get
                Return rectangle.X
            End Get
            Set(value As Double)
                _rectangle = New RectangleF(value, Y, width, height)
            End Set
        End Property

        ''' <summary>
        ''' the y-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
            Get
                Return rectangle.Y
            End Get
            Set(value As Double)
                _rectangle = New RectangleF(X, value, width, height)
            End Set
        End Property

        ''' <summary>
        ''' the width of the label (approximating the label as a rectangle).
        ''' </summary>
        ''' <returns></returns>
        Public Property width As Double
            Get
                Return rectangle.Width
            End Get
            Set(value As Double)
                _rectangle = New RectangleF(X, Y, value, height)
            End Set
        End Property

        ''' <summary>
        ''' the height of the label (same approximation).
        ''' </summary>
        ''' <returns></returns>
        Public Property height As Double
            Get
                Return rectangle.Height
            End Get
            Set(value As Double)
                _rectangle = New RectangleF(X, Y, width, value)
            End Set
        End Property

        ''' <summary>
        ''' the label text.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 20200618 实际上这个标签文本与实际的标签没有很多关联，也可以为一个用于获取图片对象的主键
        ''' </remarks>
        Public Property text As String = Me.GetHashCode

        ''' <summary>
        ''' 在计算位置的时候，这个标签将不会被随机模拟事件选中
        ''' 即这个标签当这个属性为true的时候不会发生任何变化
        ''' </summary>
        ''' <returns></returns>
        Public Property pinned As Boolean

        ''' <summary>
        ''' 当前的这个文本标签对象所处的位置以及所占据的大小等数据
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property rectangle As RectangleF

        ''' <summary>
        ''' [x, y]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property location As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New PointF(X, Y)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(size As SizeF)
            Me.rectangle = New RectangleF(Nothing, size)
        End Sub

        Sub New(text As TextRectangle)
            Me.text = text.text
            Me.rectangle = text.rect
        End Sub

        Sub New(rect As RectangleF)
            Me.rectangle = rect
        End Sub

        Sub New(label$, pos As PointF, size As SizeF)
            Me.text = label
            Me.rectangle = New RectangleF(pos, size)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(label$, pos As Point, size As SizeF)
            Call Me.New(label, pos.PointF, size)
        End Sub

        ''' <summary>
        ''' make value copy of the text label data
        ''' </summary>
        ''' <param name="copy"></param>
        Sub New(copy As Label)
            Me.New(copy.text, copy.location, New SizeF(copy.width, copy.height))
            Me.pinned = copy.pinned
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X.ToString("F2")},{Y.ToString("F2")}] [{If(pinned, "pinned", "**unpin")}] {text}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function distanceTo(anchor As Anchor) As Double
            Return (X - anchor.x) ^ 2 + (Y - anchor.y) ^ 2
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(label As Label) As PointF
            Return New PointF With {
                .X = label.X,
                .Y = label.Y
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(label As Label) As String
            Return label.text
        End Operator
    End Class
End Namespace
