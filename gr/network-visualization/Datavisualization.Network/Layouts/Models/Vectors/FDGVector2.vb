#Region "Microsoft.VisualBasic::a95ce18a1e14c8164a0fcfef057d8bb8, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Models\Vectors\FDGVector2.vb"

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

    '   Total Lines: 270
    '    Code Lines: 175
    ' Comment Lines: 58
    '   Blank Lines: 37
    '     File Size: 9.24 KB


    '     Class FDGVector2
    ' 
    '         Properties: x, y
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: Add, Divide, (+2 Overloads) Equals, GetHashCode, Identity
    '                   Magnitude, Multiply, Normal, Normalize, Normalized
    '                   Random, SetIdentity, SetZero, SquaredNorm, Subtract
    '                   Zero
    '         Operators: -, (+2 Overloads) *, (+2 Overloads) /, (+2 Overloads) +, <>
    '                    =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file FDGVector2.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief FDGVector2 Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the FDGVector2 Class.
'
'

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Layouts

    Public Class FDGVector2 : Inherits AbstractVector
        Implements Layout2D

        Public Overrides Property x As Double Implements Layout2D.X
        Public Overrides Property y As Double Implements Layout2D.Y

        Public Sub New()
            MyBase.New()
            x = 0F
            y = 0F
            z = 0F
        End Sub

        Public Sub New(iX As Double, iY As Double)
            MyBase.New()
            x = iX
            y = iY

            z = 0F
        End Sub

        Sub New(point As Vector2D)
            x = point.x
            y = point.y
            z = 0
        End Sub

        Sub New(pt As Point)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Sub New(pt As PointF)
            Call Me.New(pt.X, pt.Y)
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return CInt(Truncate(x)) Xor CInt(Truncate(y))
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            ' If parameter is null return false.
            If obj Is Nothing Then
                Return False
            End If

            ' If parameter cannot be cast to Point return false.
            Dim p As FDGVector2 = TryCast(obj, FDGVector2)
            If DirectCast(p, System.Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (x = p.x) AndAlso (y = p.y)
        End Function

        Public Overloads Function Equals(p As FDGVector2) As Boolean
            ' If parameter is null return false:
            If DirectCast(p, Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (x = p.x) AndAlso (y = p.y)
        End Function

        Public Overloads Shared Operator =(a As FDGVector2, b As FDGVector2) As Boolean
            ' If both are null, or both are same instance, return true.
            If System.[Object].ReferenceEquals(a, b) Then
                Return True
            End If

            ' If one is null, but not both, return false.
            If (DirectCast(a, Object) Is Nothing) OrElse (DirectCast(b, Object) Is Nothing) Then
                Return False
            End If

            ' Return true if the fields match:
            Return (a.x = b.x) AndAlso (a.y = b.y)
        End Operator

        Public Overloads Shared Operator <>(a As FDGVector2, b As FDGVector2) As Boolean
            Return Not (a = b)
        End Operator

        Public Overrides Function Add(v2 As AbstractVector) As AbstractVector
            Dim v22 As FDGVector2 = TryCast(v2, FDGVector2)
            x = x + v22.x
            y = y + v22.y
            Return Me
        End Function

        Public Overrides Function Subtract(v2 As AbstractVector) As AbstractVector
            Dim v22 As FDGVector2 = TryCast(v2, FDGVector2)
            x = x - v22.x
            y = y - v22.y
            Return Me
        End Function

        Public Overrides Function Multiply(n As Double) As AbstractVector
            x = x * n
            y = y * n
            Return Me
        End Function

        Public Overrides Function Divide(n As Double) As AbstractVector
            If n = 0F Then
                x = 0F
                y = 0F
            Else
                x = x / n
                y = y / n
            End If
            Return Me
        End Function

        ''' <summary>
        ''' Calculates the squared 2-norm of this instance.
        ''' </summary>
        ''' <returns>System.Double.</returns>
        Public Function SquaredNorm() As Double
            Return x * x + y * y
        End Function

        ''' <summary>
        ''' [norm] Calculates the 2-norm of this instance.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Magnitude() As Double
            Return CSng(Sqrt(CDbl(x * x) + CDbl(y * y)))
        End Function

        ''' <summary>
        ''' Returns the normalized vector.
        ''' </summary>
        ''' <param name="norm">The norm.</param>
        ''' <returns>Vector.</returns>
        Public Function Normalized(<Out> ByRef norm As Double) As FDGVector2
            If x = 0.0 AndAlso y = 0.0 Then
                norm = 0
                Return FDGVector2.Zero
            Else
                norm = Magnitude()
                Dim inverseNorm = 1.0R / norm
                Return New FDGVector2(x * inverseNorm, y * inverseNorm)
            End If
        End Function

        Public Function Normal() As AbstractVector
            Return New FDGVector2(y * -1.0F, x)
        End Function

        Public Overrides Function Normalize() As AbstractVector
            Return Me / Magnitude()
        End Function

        Public Overrides Function SetZero() As AbstractVector
            x = 0F
            y = 0F
            Return Me
        End Function
        Public Overrides Function SetIdentity() As AbstractVector
            x = 1.0F
            y = 1.0F
            Return Me
        End Function
        Public Shared Function Zero() As AbstractVector
            Return New FDGVector2(0F, 0F)
        End Function

        Public Shared Function Identity() As AbstractVector
            Return New FDGVector2(1.0F, 1.0F)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function Random(Optional width% = 1440, Optional height% = 900) As AbstractVector
            Return New FDGVector2(width * (randf.RandomSingle() - 0.5F), height * (randf.RandomSingle() - 0.5F))
        End Function

        Public Overloads Shared Operator +(a As FDGVector2, b As Double) As FDGVector2
            If a Is Nothing Then
                Return New FDGVector2(b, b)
            Else
                Return New FDGVector2(a.x + b, a.y + b)
            End If
        End Operator

        Public Overloads Shared Operator +(a As FDGVector2, b As FDGVector2) As FDGVector2
            Return New FDGVector2(a.x, a.y).With(Function(f) f.Add(b))
        End Operator

        Public Overloads Shared Operator -(a As FDGVector2, b As FDGVector2) As FDGVector2
            Dim temp As New FDGVector2(a.x, a.y)
            temp.Subtract(b)
            Return temp
        End Operator
        Public Overloads Shared Operator *(a As FDGVector2, b As Double) As FDGVector2
            Dim temp As New FDGVector2(a.x, a.y)
            temp.Multiply(b)
            Return temp
        End Operator
        Public Overloads Shared Operator *(a As Double, b As FDGVector2) As FDGVector2
            Dim temp As New FDGVector2(b.x, b.y)
            temp.Multiply(a)
            Return temp
        End Operator

        Public Overloads Shared Operator /(a As FDGVector2, b As Double) As FDGVector2
            Dim temp As New FDGVector2(a.x, a.y)
            temp.Divide(b)
            Return temp
        End Operator
        Public Overloads Shared Operator /(a As Double, b As FDGVector2) As FDGVector2
            Dim temp As New FDGVector2(b.x, b.y)
            temp.Divide(a)
            Return temp
        End Operator

    End Class
End Namespace
