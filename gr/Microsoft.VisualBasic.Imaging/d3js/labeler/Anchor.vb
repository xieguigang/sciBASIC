#Region "Microsoft.VisualBasic::f48f496404007bf374422bdce0e7e607, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\Anchor.vb"

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

    '   Total Lines: 104
    '    Code Lines: 64
    ' Comment Lines: 22
    '   Blank Lines: 18
    '     File Size: 3.00 KB


    '     Class Anchor
    ' 
    '         Properties: r, x, y
    ' 
    '         Constructor: (+6 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports stdNum = System.Math

Namespace d3js.Layout

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' anchor point can be created via <see cref="GetLabelAnchors"/> function
    ''' </remarks>
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

        Sub New(location As PointF, r#)
            Me.r = r

            x = location.X
            y = location.Y
        End Sub

        Sub New(location As Point, r#)
            Me.r = r

            x = location.X
            y = location.Y
        End Sub

        Sub New(x#, y#, r#)
            Me.r = r
            Me.x = x
            Me.y = y
        End Sub

        ''' <summary>
        ''' 目标节点的绘图模型
        ''' </summary>
        ''' <param name="circle">假设anchor是一个圆，画圆的时候是依据矩形框来建模的</param>
        Sub New(circle As Rectangle)
            r = stdNum.Min(circle.Width, circle.Height) / 2
            x = circle.Left + r
            y = circle.Top + r
        End Sub

        Sub New(circle As RectangleF)
            r = stdNum.Min(circle.Width, circle.Height) / 2
            x = circle.Left + r
            y = circle.Top + r
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{x}, {y}]"
        End Function

        Public Shared Widening Operator CType(anchor As Anchor) As Point
            With anchor
                Return New Point(CInt(.x), CInt(.y))
            End With
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As PointF
            With anchor
                Return New PointF(CInt(.x), CInt(.y))
            End With
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As RectangleF
            Dim r# = anchor.r

            Return New RectangleF With {
                .Location = anchor,
                .Size = New SizeF(CSng(r), CSng(r))
            }
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As Rectangle
            With CType(anchor, RectangleF)
                Return New Rectangle(.Location.ToPoint, .Size.ToSize)
            End With
        End Operator

        Public Shared Widening Operator CType(pt As PointF) As Anchor
            Return New Anchor With {.x = pt.X, .y = pt.Y}
        End Operator
    End Class
End Namespace
