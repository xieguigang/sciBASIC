#Region "Microsoft.VisualBasic::81cf1477165f2fcafd76324ce2b58abf, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Image\Math\Models\Vector2D.vb"

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

    '   Total Lines: 80
    '    Code Lines: 47
    ' Comment Lines: 20
    '   Blank Lines: 13
    '     File Size: 2.30 KB


    '     Class Vector2D
    ' 
    '         Properties: Length, x, y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    '         Operators: (+2 Overloads) -, (+2 Overloads) *, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' <see cref="Drawing.PointF"/>
    ''' </summary>
    Public Class Vector2D : Implements Layout2D

        Public Property x As Double Implements Layout2D.X
        Public Property y As Double Implements Layout2D.Y

        Public ReadOnly Property Length As Double
            Get
                Return stdNum.Sqrt(x ^ 2 + y ^ 2)
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
    End Class
End Namespace
