#Region "Microsoft.VisualBasic::8c568dfc3f6f2d0e21e7d83b6846aa1b, gr\network-visualization\Datavisualization.Network\Layouts\Models\Vectors\FDGVector3.vb"

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

    '     Class FDGVector3
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Add, Divide, (+2 Overloads) Equals, GetHashCode, Identity
    '                   Magnitude, Multiply, Normalize, Random, SetIdentity
    '                   SetZero, Subtract, Zero
    '         Operators: -, (+2 Overloads) *, /, +, <>
    '                    =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file Vector3.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Vector3 Interface
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
'An Interface for the Vector3 Class.
'
'

Imports System.Drawing
Imports System.Math
Imports Microsoft.VisualBasic.Math

Namespace Layouts

    Public Class FDGVector3
        Inherits AbstractVector

        Public Sub New()
            MyBase.New()
            x = 0F
            y = 0F
            z = 0F
        End Sub

        Sub New(p As PointF)
            Call Me.New(p.X, p.Y, 0)
        End Sub

        Sub New(copy As AbstractVector)
            If Not copy Is Nothing Then
                x = copy.x
                y = copy.y
                z = copy.z
            End If
        End Sub

        Public Sub New(x As Double, y As Double, z As Double)
            Call MyBase.New()

            MyBase.x = x
            MyBase.y = y
            MyBase.z = z
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return CInt(Truncate(x)) Xor CInt(Truncate(y)) Xor CInt(Truncate(z))
        End Function
        Public Overrides Function Equals(obj As System.Object) As Boolean
            ' If parameter is null return false.
            If obj Is Nothing Then
                Return False
            End If

            ' If parameter cannot be cast to Point return false.
            Dim p As FDGVector3 = TryCast(obj, FDGVector3)
            If DirectCast(p, System.Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (x = p.x) AndAlso (y = p.y) AndAlso (z = p.z)
        End Function

        Public Overloads Function Equals(p As FDGVector3) As Boolean
            ' If parameter is null return false:
            If DirectCast(p, Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (x = p.x) AndAlso (y = p.y) AndAlso (z = p.z)
        End Function

        Public Overloads Shared Operator =(a As FDGVector3, b As FDGVector3) As Boolean
            ' If both are null, or both are same instance, return true.
            If System.[Object].ReferenceEquals(a, b) Then
                Return True
            End If

            ' If one is null, but not both, return false.
            If (DirectCast(a, Object) Is Nothing) OrElse (DirectCast(b, Object) Is Nothing) Then
                Return False
            End If

            ' Return true if the fields match:
            Return (a.x = b.x) AndAlso (a.y = b.y) AndAlso (a.z = b.z)
        End Operator

        Public Overloads Shared Operator <>(a As FDGVector3, b As FDGVector3) As Boolean
            Return Not (a = b)
        End Operator


        Public Overrides Function Add(v2 As AbstractVector) As AbstractVector
            Dim v32 As FDGVector3 = TryCast(v2, FDGVector3)
            x = x + v32.x
            y = y + v32.y
            z = z + v32.z
            Return Me
        End Function

        Public Overrides Function Subtract(v2 As AbstractVector) As AbstractVector
            Dim v32 As FDGVector3 = TryCast(v2, FDGVector3)
            x = x - v32.x
            y = y - v32.y
            z = z - v32.z
            Return Me
        End Function

        Public Overrides Function Multiply(n As Double) As AbstractVector
            x = x * n
            y = y * n
            z = z * n
            Return Me
        End Function

        Public Overrides Function Divide(n As Double) As AbstractVector
            If n = 0F Then
                x = 0F
                y = 0F
                z = 0F
            Else
                x = x / n
                y = y / n
                z = z / n
            End If
            Return Me
        End Function

        Public Overrides Function Magnitude() As Double
            Return CSng(Sqrt(CDbl(x * x) + CDbl(y * y) + CDbl(z * z)))
        End Function

        Public Overrides Function Normalize() As AbstractVector
            Return Me / Magnitude()
        End Function

        Public Overrides Function SetZero() As AbstractVector
            x = 0F
            y = 0F
            z = 0F
            Return Me
        End Function
        Public Overrides Function SetIdentity() As AbstractVector
            x = 1.0F
            y = 1.0F
            z = 1.0F
            Return Me
        End Function
        Public Shared Function Zero() As AbstractVector
            Return New FDGVector3(0F, 0F, 0F)
        End Function

        Public Shared Function Identity() As AbstractVector
            Return New FDGVector3(1.0F, 1.0F, 1.0F)
        End Function

        Public Shared Function Random() As AbstractVector
            Return New FDGVector3(10.0F * (RandomSingle() - 0.5F), 10.0F * (RandomSingle() - 0.5F), 10.0F * (RandomSingle() - 0.5F))
        End Function

        Public Overloads Shared Operator +(a As FDGVector3, b As FDGVector3) As FDGVector3
            Dim temp As New FDGVector3(a.x, a.y, a.z)
            temp.Add(b)
            Return temp
        End Operator
        Public Overloads Shared Operator -(a As FDGVector3, b As FDGVector3) As FDGVector3
            Dim temp As New FDGVector3(a.x, a.y, a.z)
            temp.Subtract(b)
            Return temp
        End Operator
        Public Overloads Shared Operator *(a As FDGVector3, b As Double) As FDGVector3
            Dim temp As New FDGVector3(a.x, a.y, a.z)
            temp.Multiply(b)
            Return temp
        End Operator
        Public Overloads Shared Operator *(a As Double, b As FDGVector3) As FDGVector3
            Dim temp As New FDGVector3(b.x, b.y, b.z)
            temp.Multiply(a)
            Return temp
        End Operator

        Public Overloads Shared Operator /(a As FDGVector3, b As Double) As FDGVector3
            Dim temp As New FDGVector3(a.x, a.y, a.z)
            temp.Divide(b)
            Return temp
        End Operator

    End Class
End Namespace
