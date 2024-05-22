#Region "Microsoft.VisualBasic::79be4bf930733f918300519c34d6f175, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\PlateRectangle.vb"

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

    '   Total Lines: 120
    '    Code Lines: 71 (59.17%)
    ' Comment Lines: 35 (29.17%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (11.67%)
    '     File Size: 4.29 KB


    '     Class PlateRectangle
    ' 
    '         Properties: Rectangle
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: contains_point, contains_rectangle_top, covers_rectangle, isEqual, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports np = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.Numpy

Namespace Drawing2D.Text.Nudge

    ''' <summary>
    ''' class of the geometrical figure of the plate rectangle, i.e
	''' the sides Of the rectangle are collinear With the canonical 
    ''' basis vectors (1,0) And (0,1).
    ''' </summary>
    Public Class PlateRectangle

        Friend ReadOnly x1 As Double()
        Friend ReadOnly l As Double
        Friend ReadOnly h As Double
        Friend ReadOnly x2 As Double()
        Friend ReadOnly y1 As Double()
        Friend ReadOnly y2 As Double()

        Public ReadOnly Property Rectangle As RectangleF
            Get
                Dim topLeft As New PointF(x1(0), x1(1) + h)
                Dim size As New SizeF(l, h)

                Return New RectangleF(topLeft, size)
            End Get
        End Property

        ''' <summary>
        ''' This is a "plate" rectangle
        ''' </summary>
        ''' <param name="bottomLeft">``x1`` for the coordinates of the bottom left point.</param>
        ''' <param name="l">l for the length of the rectangle to get the bottom right point</param>
        ''' <param name="h">h for the height of the rectangle</param>
        Sub New(bottomLeft As Double(), l As Double, h As Double)
            Dim x1 = np.array(bottomLeft)

            Me.x1 = x1
            Me.l = l
            Me.h = h
            Me.x2 = x1 + {l, 0}
            Me.y1 = x1 + {0, h}
            Me.y2 = x1 + {l, h}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(rect As RectangleF)
            Call Me.New(
                bottomLeft:=New Double() {rect.Left, rect.Top + rect.Height},
                l:=rect.Width,
                h:=rect.Height
            )
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{x1(Scan0).ToString("F2")}, {x1(1).ToString("F2")}] text_width={l}, text_height={h}"
        End Function

        ''' <summary>
        ''' return True if self rectangle equals passed argument rectangle
        ''' </summary>
        ''' <param name="r"></param>
        ''' <returns></returns>
        Public Function isEqual(r As PlateRectangle) As Boolean
            If x1.SequenceEqual(r.x1) AndAlso l = r.l AndAlso h = r.h Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' method that evaluates if self rectangle contains a point
		''' Or Not. 
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        Private Function contains_point(p As Double()) As Boolean
            If p(0) >= x1(0) AndAlso p(0) <= x2(0) AndAlso p(1) >= x1(1) AndAlso p(1) <= y1(1) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' method that evaluates if self rectangle contains at least
		''' one of another's rectangle tops or not. 
        ''' </summary>
        ''' <param name="r">object of class Rectangle</param>
        ''' <returns></returns>
        Public Function contains_rectangle_top(r As PlateRectangle) As Boolean
            If contains_point(r.x1) Or
                contains_point(r.x2) Or
                contains_point(r.y1) Or
                contains_point(r.y2) Then

                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' method that evaluates if self rectangle covers another
		''' rectangle Or Not. Return true if self And rectangle doesn't contains
		''' each other one of theirs top.
        ''' </summary>
        ''' <param name="r">object of class Rectangle</param>
        ''' <returns></returns>
        Public Function covers_rectangle(r As PlateRectangle) As Boolean
            If contains_rectangle_top(r) OrElse r.contains_rectangle_top(Me) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
