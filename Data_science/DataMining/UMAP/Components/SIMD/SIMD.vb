#Region "Microsoft.VisualBasic::2e4eb50e6af5394e1a0ac190b9e62cdb, Data_science\DataMining\UMAP\Components\SIMD\SIMD.vb"

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

    ' Module SIMD
    ' 
    '     Function: DotProduct, Euclidean, Magnitude
    ' 
    '     Sub: Add, Multiply
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Friend Module SIMD
    Private ReadOnly _vs1 As Integer = Vector(Of Single).Count
    Private ReadOnly _vs2 As Integer = 2 * Vector(Of Single).Count
    Private ReadOnly _vs3 As Integer = 3 * Vector(Of Single).Count
    Private ReadOnly _vs4 As Integer = 4 * Vector(Of Single).Count

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Magnitude(ByRef vec As Single()) As Single
        Return stdNum.Sqrt(SIMD.DotProduct(vec, vec))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Euclidean(ByRef lhs As Single(), ByRef rhs As Single()) As Single
        Dim result = 0F
        Dim count = lhs.Length
        Dim offset = 0
        Dim diff As Vector(Of Single)

        While count >= SIMD._vs4
            diff = New Vector(Of Single)(lhs, offset) - New Vector(Of Single)(rhs, offset)
            result += Vector.Dot(diff, diff)
            diff = New Vector(Of Single)(lhs, offset + SIMD._vs1) - New Vector(Of Single)(rhs, offset + SIMD._vs1)
            result += Vector.Dot(diff, diff)
            diff = New Vector(Of Single)(lhs, offset + SIMD._vs2) - New Vector(Of Single)(rhs, offset + SIMD._vs2)
            result += Vector.Dot(diff, diff)
            diff = New Vector(Of Single)(lhs, offset + SIMD._vs3) - New Vector(Of Single)(rhs, offset + SIMD._vs3)
            result += Vector.Dot(diff, diff)
            If count = SIMD._vs4 Then Return result
            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
            diff = New Vector(Of Single)(lhs, offset) - New Vector(Of Single)(rhs, offset)
            result += Vector.Dot(diff, diff)
            diff = New Vector(Of Single)(lhs, offset + SIMD._vs1) - New Vector(Of Single)(rhs, offset + SIMD._vs1)
            result += Vector.Dot(diff, diff)
            If count = SIMD._vs2 Then Return result
            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
            diff = New Vector(Of Single)(lhs, offset) - New Vector(Of Single)(rhs, offset)
            result += Vector.Dot(diff, diff)
            If count = SIMD._vs1 Then Return result
            count -= SIMD._vs1
            offset += SIMD._vs1
        End If

        If count > 0 Then
            While count > 0
                Dim d = lhs(offset) - rhs(offset)
                result += d * d
                offset += 1
                count -= 1
            End While
        End If

        Return result
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(ByRef lhs As Single(), f As Single)
        Dim count = lhs.Length
        Dim offset = 0
        Dim v = New Vector(Of Single)(f)

        While count >= SIMD._vs4
                (New Vector(Of Single)(lhs, offset) + v).CopyTo(lhs, offset)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs1)) + v).CopyTo(lhs, offset + SIMD._vs1)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs2)) + v).CopyTo(lhs, offset + SIMD._vs2)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs3)) + v).CopyTo(lhs, offset + SIMD._vs3)
                If count = SIMD._vs4 Then Return
            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
                (New Vector(Of Single)(lhs, offset) + v).CopyTo(lhs, offset)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs1)) + v).CopyTo(lhs, offset + SIMD._vs1)
                If count = SIMD._vs2 Then Return
            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
                (New Vector(Of Single)(lhs, offset) + v).CopyTo(lhs, offset)
                If count = SIMD._vs1 Then Return
            count -= SIMD._vs1
            offset += SIMD._vs1
        End If

        If count > 0 Then
            While count > 0
                lhs(offset) += f
                offset += 1
                count -= 1
            End While
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Multiply(ByRef lhs As Single(), f As Single)
        Dim count = lhs.Length
        Dim offset = 0

        While count >= SIMD._vs4
                (New Vector(Of Single)(lhs, offset) * f).CopyTo(lhs, offset)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs1)) * f).CopyTo(lhs, offset + SIMD._vs1)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs2)) * f).CopyTo(lhs, offset + SIMD._vs2)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs3)) * f).CopyTo(lhs, offset + SIMD._vs3)
                If count = SIMD._vs4 Then Return
            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
                (New Vector(Of Single)(lhs, offset) * f).CopyTo(lhs, offset)
                (New Vector(Of Single)(CType(lhs, Single()), CInt(offset + SIMD._vs1)) * f).CopyTo(lhs, offset + SIMD._vs1)
                If count = SIMD._vs2 Then Return
            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
                (New Vector(Of Single)(lhs, offset) * f).CopyTo(lhs, offset)
                If count = SIMD._vs1 Then Return
            count -= SIMD._vs1
            offset += SIMD._vs1
        End If

        If count > 0 Then
            While count > 0
                lhs(offset) *= f
                offset += 1
                count -= 1
            End While
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function DotProduct(ByRef lhs As Single(), ByRef rhs As Single()) As Single
        Dim result = 0F
        Dim count = lhs.Length
        Dim offset = 0

        While count >= SIMD._vs4
            result += Vector.Dot(New Vector(Of Single)(lhs, offset), New Vector(Of Single)(rhs, offset))
            result += Vector.Dot(New Vector(Of Single)(lhs, offset + SIMD._vs1), New Vector(Of Single)(rhs, offset + SIMD._vs1))
            result += Vector.Dot(New Vector(Of Single)(lhs, offset + SIMD._vs2), New Vector(Of Single)(rhs, offset + SIMD._vs2))
            result += Vector.Dot(New Vector(Of Single)(lhs, offset + SIMD._vs3), New Vector(Of Single)(rhs, offset + SIMD._vs3))
            If count = SIMD._vs4 Then Return result
            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
            result += Vector.Dot(New Vector(Of Single)(lhs, offset), New Vector(Of Single)(rhs, offset))
            result += Vector.Dot(New Vector(Of Single)(lhs, offset + SIMD._vs1), New Vector(Of Single)(rhs, offset + SIMD._vs1))
            If count = SIMD._vs2 Then Return result
            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
            result += Vector.Dot(New Vector(Of Single)(lhs, offset), New Vector(Of Single)(rhs, offset))
            If count = SIMD._vs1 Then Return result
            count -= SIMD._vs1
            offset += SIMD._vs1
        End If

        If count > 0 Then
            While count > 0
                result += lhs(offset) * rhs(offset)
                offset += 1
                count -= 1
            End While
        End If

        Return result
    End Function
End Module

