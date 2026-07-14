#Region "Microsoft.VisualBasic::c31f17fb3cb69623ceb75b1a4dbb857f, gr\physics\Vector2Math.vb"

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

    '   Total Lines: 25
    '    Code Lines: 18 (72.00%)
    ' Comment Lines: 3 (12.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (16.00%)
    '     File Size: 772 B


    ' Module Vector2Math
    ' 
    '     Function: Abs, Dot, saturate
    ' 

    ' /********************************************************************************/

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' unity math + 2D rigid body helpers
''' </summary>
Public Module Vector2Math

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Abs(v As Vector2) As Vector2
        Return New Vector2(std.Abs(v.x), std.Abs(v.y))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Dot(lhs As Vector2, rhs As Vector2) As Double
        Return Vector.dot({lhs.x, lhs.y}, {rhs.x, rhs.y})
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function saturate(x As Single) As Single
        Return Max(0, Min(1, x))
    End Function

    ' ---- 2D rigid body math (Randy Gaul style) ----

    ''' <summary>Scalar (2D) cross product a × b.</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Cross(a As Vector2, b As Vector2) As Double
        Return a.x * b.y - a.y * b.x
    End Function

    ''' <summary>Cross of a vector with a scalar: a × s = (s·a.y, -s·a.x).</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Cross(a As Vector2, s As Double) As Vector2
        Return New Vector2(s * a.y, -s * a.x)
    End Function

    ''' <summary>Cross of a scalar with a vector: s × a = (-s·a.y, s·a.x).</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Cross(s As Double, a As Vector2) As Vector2
        Return New Vector2(-s * a.y, s * a.x)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function LengthSquared(v As Vector2) As Double
        Return v.x * v.x + v.y * v.y
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Length(v As Vector2) As Double
        Return std.Sqrt(v.x * v.x + v.y * v.y)
    End Function

    ''' <summary>Normalize; returns Zero when the vector is zero-length.</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Normalize(v As Vector2) As Vector2
        Dim len = Length(v)
        If len < 1.0e-12 Then Return Vector2.Zero
        Return v / len
    End Function

    ''' <summary>Perpendicular (rotated +90°): (-y, x).</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Perpendicular(v As Vector2) As Vector2
        Return New Vector2(-v.y, v.x)
    End Function

    ''' <summary>Rotate vector by angle (radians) counter-clockwise.</summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Rotate(v As Vector2, angle As Double) As Vector2
        Dim c = std.Cos(angle), s = std.Sin(angle)
        Return New Vector2(v.x * c - v.y * s, v.x * s + v.y * c)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Distance(a As Vector2, b As Vector2) As Double
        Return Length(a - b)
    End Function
End Module
