#Region "Microsoft.VisualBasic::da740d880b88bb05280cc95e95a8fe72, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\Label.vb"

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

    '     Class Label
    ' 
    '         Properties: height, location, Rectangle, text, width
    '                     X, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace d3js.Layout

    Public Class Label

        ''' <summary>
        ''' the x-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
            Get
                Return Rectangle.X
            End Get
            Set(value As Double)
                _Rectangle = New RectangleF(value, Y, width, height)
            End Set
        End Property

        ''' <summary>
        ''' the y-coordinate of the label.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
            Get
                Return Rectangle.Y
            End Get
            Set(value As Double)
                _Rectangle = New RectangleF(X, value, width, height)
            End Set
        End Property

        ''' <summary>
        ''' the width of the label (approximating the label as a rectangle).
        ''' </summary>
        ''' <returns></returns>
        Public Property width As Double
            Get
                Return Rectangle.Width
            End Get
            Set(value As Double)
                _Rectangle = New RectangleF(X, Y, value, height)
            End Set
        End Property

        ''' <summary>
        ''' the height of the label (same approximation).
        ''' </summary>
        ''' <returns></returns>
        Public Property height As Double
            Get
                Return Rectangle.Height
            End Get
            Set(value As Double)
                _Rectangle = New RectangleF(X, Y, width, value)
            End Set
        End Property

        ''' <summary>
        ''' the label text.
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        ''' <summary>
        ''' 当前的这个文本标签对象所处的位置以及所占据的大小等数据
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Rectangle As RectangleF

        Public ReadOnly Property location As PointF
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New PointF(X, Y)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(label$, pos As PointF, size As SizeF)
            Me.text = label
            Me.Rectangle = New RectangleF(pos, size)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(label$, pos As Point, size As SizeF)
            Call Me.New(label, pos.PointF, size)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{text}@({X.ToString("F2")},{Y.ToString("F2")})"
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
