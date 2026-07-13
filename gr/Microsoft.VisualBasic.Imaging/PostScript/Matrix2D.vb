#Region "Microsoft.VisualBasic::00000000000000000000000000000000, gr\Microsoft.VisualBasic.Imaging\PostScript\Matrix2D.vb"

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

    '     Class Matrix2D
    ' 
    '         a light weight 2D affine transformation matrix that works on both
    '         windows and linux (netcore) platforms. the system.Drawing.Drawing2D.Matrix
    '         is not available on the linux netcore build, so this class is used
    '         to bake the current transform (CTM) into the element coordinates of
    '         the postscript graphics.
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports std = System.Math

Namespace PostScript

    ''' <summary>
    ''' a light weight 2D affine transformation matrix
    ''' </summary>
    ''' <remarks>
    ''' the matrix is stored in the gdi+ convention:
    ''' 
    ''' x' = a * x + c * y + e
    ''' y' = b * x + d * y + f
    ''' 
    ''' the transform operations follow the gdi+ "System.Drawing.Drawing2D.Matrix"
    ''' semantics: a positive rotation angle is clockwise (because the gdi+ coordinate
    ''' system has its y axis pointing down). the postscript y flip is applied later by
    ''' the <see cref="Writer"/>, so this matrix only ever works in the gdi+ space.
    ''' </remarks>
    Public Class Matrix2D

        ''' <summary>
        ''' x' = a*x + c*y + e
        ''' </summary>
        Public a As Double = 1
        ''' <summary>
        ''' y' = b*x + d*y + f
        ''' </summary>
        Public b As Double = 0
        Public c As Double = 0
        Public d As Double = 1
        Public e As Double = 0
        Public f As Double = 0

        Sub New()
            ' identity matrix
            a = 1
            b = 0
            c = 0
            d = 1
            e = 0
            f = 0
        End Sub

        Sub New(a!, b!, c!, d!, e!, f!)
            Me.a = a
            Me.b = b
            Me.c = c
            Me.d = d
            Me.e = e
            Me.f = f
        End Sub

        ''' <summary>
        ''' reset this matrix back to the identity matrix
        ''' </summary>
        Public Sub Reset()
            a = 1 : b = 0 : c = 0 : d = 1 : e = 0 : f = 0
        End Sub

        ''' <summary>
        ''' transform a single point through this affine matrix (gdi+ space)
        ''' </summary>
        Public Function TransformPoint(p As PointF) As PointF
            Return New PointF(CSng(a * p.X + c * p.Y + e), CSng(b * p.X + d * p.Y + f))
        End Function

        ''' <summary>
        ''' prepend the given matrix: this = m * this
        ''' </summary>
        Private Sub Prepend(m As Matrix2D)
            Dim na = m.a * Me.a + m.c * Me.b
            Dim nb = m.b * Me.a + m.d * Me.b
            Dim nc = m.a * Me.c + m.c * Me.d
            Dim nd = m.b * Me.c + m.d * Me.d
            Dim ne = m.a * Me.e + m.c * Me.f + m.e
            Dim nf = m.b * Me.e + m.d * Me.f + m.f

            Me.a = na : Me.b = nb : Me.c = nc : Me.d = nd : Me.e = ne : Me.f = nf
        End Sub

        ''' <summary>
        ''' prepend a translation
        ''' </summary>
        Public Sub Translate(tx!, ty!)
            Call Prepend(New Matrix2D(1, 0, 0, 1, tx, ty))
        End Sub

        ''' <summary>
        ''' prepend a scaling
        ''' </summary>
        Public Sub Scale(sx!, sy!)
            Call Prepend(New Matrix2D(sx, 0, 0, sy, 0, 0))
        End Sub

        ''' <summary>
        ''' prepend a clockwise rotation by the given angle (in degree)
        ''' </summary>
        Public Sub Rotate(angle!)
            Dim r = angle * (std.PI / 180)
            Dim cos = std.Cos(r)
            Dim sin = std.Sin(r)

            ' gdi+ rotation matrix (clockwise because y points down)
            Call Prepend(New Matrix2D(cos, sin, -sin, cos, 0, 0))
        End Sub

        ''' <summary>
        ''' the linear scale factors of this matrix
        ''' </summary>
        Public Function ScaleFactors() As SizeF
            Dim sx = std.Sqrt(a * a + b * b)
            Dim sy = std.Sqrt(c * c + d * d)

            Return New SizeF(CSng(sx), CSng(sy))
        End Function

        Public Overrides Function ToString() As String
            Return $"[{a}, {b}, {c}, {d}, {e}, {f}]"
        End Function
    End Class
End Namespace
