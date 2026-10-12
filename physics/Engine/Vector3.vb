#Region "Microsoft.VisualBasic::ef4d1f6e85b2b0d426bc86d0b4464259, gr\physics\Particles\Vector3.vb"

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

    '   Total Lines: 179
    '    Code Lines: 129 (72.07%)
    ' Comment Lines: 27 (15.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (12.85%)
    '     File Size: 5.82 KB


    ' Class Vector3
    ' 
    '     Properties: Magnitude, One, Zero
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: Clone, Cross, Dot, Normalize, Random
    '               ToString
    '     Operators: (+2 Overloads) -, (+3 Overloads) *, /, +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

''' <summary>
''' A self-contained double precision 3D vector used by the 3D fluid
''' simulation engine <see cref="FluidEngine3D"/>. This type mirrors the
''' shape of the 2D <see cref="Vector2"/> so that the SPH pipeline can be
''' ported into three dimensions with minimal changes.
''' </summary>
Public Class Vector3

    Public x As Double
    Public y As Double
    Public z As Double

    Public Shared ReadOnly Property Zero As Vector3
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New Vector3(0, 0, 0)
        End Get
    End Property

    Public Shared ReadOnly Property One As Vector3
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New Vector3(1, 1, 1)
        End Get
    End Property

    ''' <summary>
    ''' The euclidean length (magnitude) of this vector.
    ''' </summary>
    Public ReadOnly Property Magnitude As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Sqrt(x * x + y * y + z * z)
        End Get
    End Property

    ''' <summary>
    ''' index based access to the three vector components: 0=x, 1=y, 2=z.
    ''' </summary>
    Default Public Property Field(offset As Integer) As Double
        Get
            Select Case offset
                Case 0 : Return x
                Case 1 : Return y
                Case 2 : Return z
                Case Else
                    Throw New InvalidProgramException(offset.ToString)
            End Select
        End Get
        Set(value As Double)
            Select Case offset
                Case 0 : x = value
                Case 1 : y = value
                Case 2 : z = value
                Case Else
                    Throw New InvalidProgramException(offset.ToString)
            End Select
        End Set
    End Property

    Sub New()
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(x As Double, y As Double, z As Double)
        Me.x = x
        Me.y = y
        Me.z = z
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(x As Single, y As Single, z As Single)
        Me.x = x
        Me.y = y
        Me.z = z
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{x:F3}, {y:F3}, {z:F3}]"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Clone() As Vector3
        Return New Vector3(x, y, z)
    End Function

    ''' <summary>
    ''' Returns the unit vector, a zero length vector returns the zero vector.
    ''' </summary>
    Public Function Normalize() As Vector3
        Dim mag As Double = Magnitude
        If mag = 0.0 Then
            Return New Vector3(0, 0, 0)
        Else
            Return New Vector3(x / mag, y / mag, z / mag)
        End If
    End Function

    ''' <summary>
    ''' The dot product of two 3d vectors.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Dot(a As Vector3, b As Vector3) As Double
        Return a.x * b.x + a.y * b.y + a.z * b.z
    End Function

    ''' <summary>
    ''' The cross product of two 3d vectors.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Cross(a As Vector3, b As Vector3) As Vector3
        Return New Vector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        )
    End Function

    ''' <summary>
    ''' Generate a random point that located inside the given box volume.
    ''' </summary>
    Public Shared Function Random(box As Vector3) As Vector3
        Return New Vector3(
            randf.NextDouble(0, box.x),
            randf.NextDouble(0, box.y),
            randf.NextDouble(0, box.z)
        )
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator +(a As Vector3, b As Vector3) As Vector3
        Return New Vector3(a.x + b.x, a.y + b.y, a.z + b.z)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator -(a As Vector3, b As Vector3) As Vector3
        Return New Vector3(a.x - b.x, a.y - b.y, a.z - b.z)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator -(v As Vector3) As Vector3
        If v Is Nothing Then
            Return New Vector3
        End If
        Return New Vector3(-v.x, -v.y, -v.z)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator *(v As Vector3, a As Double) As Vector3
        Return New Vector3(v.x * a, v.y * a, v.z * a)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator *(a As Double, v As Vector3) As Vector3
        Return New Vector3(v.x * a, v.y * a, v.z * a)
    End Operator

    ''' <summary>
    ''' component-wise multiplication of two vectors.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator *(a As Vector3, b As Vector3) As Vector3
        Return New Vector3(a.x * b.x, a.y * b.y, a.z * b.z)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator /(v As Vector3, n As Double) As Vector3
        If n = 0.0 Then
            Return Vector3.Zero
        Else
            Return New Vector3(v.x / n, v.y / n, v.z / n)
        End If
    End Operator

End Class
