#Region "Microsoft.VisualBasic::6f221e7edd25114b7e698edb6488e9c7, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\d3js\labeler\Anchor.vb"

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
Imports sys = System.Math

Namespace d3js.Layout

    Public Class Anchor

        ''' <summary>
        ''' the x-coordinate of the anchor.
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Double
        ''' <summary>
        ''' the y-coordinate of the anchor.
        ''' </summary>
        ''' <returns></returns>
        Public Property y As Double
        ''' <summary>
        ''' the anchor radius (assuming anchor is a circle).
        ''' </summary>
        ''' <returns></returns>
        Public Property r As Double

        Sub New()
        End Sub

        ''' <summary>
        ''' 目标节点的绘图模型
        ''' </summary>
        ''' <param name="circle">假设anchor是一个圆，画圆的时候是依据矩形框来建模的</param>
        Sub New(circle As Rectangle)
            r = sys.Min(circle.Width, circle.Height) / 2
            x = circle.Left + r
            y = circle.Top + r
        End Sub

        Public Shared Widening Operator CType(anchor As Anchor) As Point
            With anchor
                Return New Point(.x, .y)
            End With
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As PointF
            With anchor
                Return New PointF(.x, .y)
            End With
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As RectangleF
            Dim r# = anchor.r

            Return New RectangleF With {
                .Location = anchor,
                .Size = New SizeF(r, r)
            }
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As Rectangle
            With CType(anchor, RectangleF)
                Return New Rectangle(.Location.ToPoint, .Size.ToSize)
            End With
        End Operator
    End Class
End Namespace
