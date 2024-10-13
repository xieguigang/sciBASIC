#Region "Microsoft.VisualBasic::c0234e585d330d78da76bae1ab1d0765, Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Models\Vector2D.vb"

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

    '   Total Lines: 107
    '    Code Lines: 56 (52.34%)
    ' Comment Lines: 35 (32.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (14.95%)
    '     File Size: 3.20 KB


    '     Class Vector2D
    ' 
    '         Properties: Length, x, y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetDistance, ToString
    '         Operators: (+2 Overloads) -, (+2 Overloads) *, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' <see cref="Drawing.PointF"/>, basic model for physical simulator
    ''' </summary>
    ''' <remarks>
    ''' this vector model could be cast to gdi+ <see cref="Drawing.PointF"/> object directly.
    ''' </remarks>
    Public Class Vector2D : Implements Layout2D

        ''' <summary>
        ''' position x
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Double Implements Layout2D.X

        ''' <summary>
        ''' position y
        ''' </summary>
        ''' <returns></returns>
        Public Property y As Double Implements Layout2D.Y

        ''' <summary>
        ''' distance to zero [0,0]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Double
            Get
                Return std.Sqrt(x ^ 2 + y ^ 2)
            End Get
        End Property

        Public Sub New()
            Me.New(0.0, 0.0)
        End Sub

        Public Sub New(x As Double, y As Double)
            Me.x = x
            Me.y = y
        End Sub

        Public Sub New(x As Integer, y As Integer)
            Me.x = x
            Me.y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{x}, {y}]"
        End Function

        Public Shared Operator +(a As Vector2D, b As Layout2D) As Vector2D
            Return New Vector2D(a.x + b.X, a.y + b.Y)
        End Operator

        ''' <summary>
        ''' reverse
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Operator -(v As Vector2D) As Vector2D
            With v
                Return New Vector2D(- .x, - .y)
            End With
        End Operator

        Public Shared Operator -(a As Vector2D, b As Layout2D) As Vector2D
            Return New Vector2D(a.x - b.X, a.y - b.Y)
        End Operator

        ''' <summary>
        ''' multiple
        ''' </summary>
        ''' <param name="scale#"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Operator *(scale#, v As Vector2D) As Vector2D
            With v
                Return New Vector2D(scale * .x, scale * .y)
            End With
        End Operator

        ''' <summary>
        ''' multiple
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="scale#"></param>
        ''' <returns></returns>
        Public Shared Operator *(v As Vector2D, scale#) As Vector2D
            With v
                Return New Vector2D(scale * .x, scale * .y)
            End With
        End Operator

        Public Shared Narrowing Operator CType(v As Vector2D) As PointF
            Return New PointF(v.x, v.y)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDistance(v As Layout2D) As Double
            Return GeomTransform.Distance(x, y, v.X, v.Y)
        End Function
    End Class
End Namespace
