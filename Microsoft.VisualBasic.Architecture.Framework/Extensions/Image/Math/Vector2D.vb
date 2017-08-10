#Region "Microsoft.VisualBasic::4a2e4cf3630ac6b2149653aca323d72f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Math\Vector2D.vb"

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


Namespace Imaging.Math2D

    Public Class Vector2D

        Public x As Double
        Public y As Double

        Public ReadOnly Property Length() As Double
            Get
                Return Math.Sqrt(x ^ 2 + y ^ 2)
            End Get
        End Property

        Public Sub New()
            Me.New(0.0, 0.0)
        End Sub

        Public Sub New(paramDouble1 As Double, paramDouble2 As Double)
            Me.x = paramDouble1
            Me.y = paramDouble2
        End Sub

        Public Sub New(paramInt1 As Integer, paramInt2 As Integer)
            Me.x = paramInt1
            Me.y = paramInt2
        End Sub

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

