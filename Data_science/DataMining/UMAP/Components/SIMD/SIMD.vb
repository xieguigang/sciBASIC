#Region "Microsoft.VisualBasic::b9439be994afbd361f930e0938efa094, sciBASIC#\Data_science\DataMining\UMAP\Components\SIMD\SIMD.vb"

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

    '   Total Lines: 239
    '    Code Lines: 189
    ' Comment Lines: 0
    '   Blank Lines: 50
    '     File Size: 7.76 KB


    ' Module SIMD
    ' 
    '     Function: DotProduct, Euclidean, Magnitude
    ' 
    '     Sub: Add, Multiply
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Friend Module SIMD

    Const Count As Integer = 8

    Private ReadOnly _vs1 As Integer = Count    ' Vector(Of Double).Count
    Private ReadOnly _vs2 As Integer = 2 * _vs1 ' Vector(Of Double).Count
    Private ReadOnly _vs3 As Integer = 3 * _vs1 ' Vector(Of Double).Count
    Private ReadOnly _vs4 As Integer = 4 * _vs1 ' Vector(Of Double).Count

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Magnitude(ByRef vec As Double()) As Double
        Return stdNum.Sqrt(SIMD.DotProduct(vec, vec))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Euclidean(ByRef lhs As Double(), ByRef rhs As Double()) As Double
        Dim result = 0F
        Dim count = lhs.Length
        Dim offset = 0
        Dim diff As Vector
        Dim d As Double

        While count >= SIMD._vs4
            diff = New Vector(lhs, offset) - New Vector(rhs, offset)
            result += diff.DotProduct(diff)
            diff = New Vector(lhs, offset + SIMD._vs1) - New Vector(rhs, offset + SIMD._vs1)
            result += diff.DotProduct(diff)
            diff = New Vector(lhs, offset + SIMD._vs2) - New Vector(rhs, offset + SIMD._vs2)
            result += diff.DotProduct(diff)
            diff = New Vector(lhs, offset + SIMD._vs3) - New Vector(rhs, offset + SIMD._vs3)
            result += diff.DotProduct(diff)

            If count = SIMD._vs4 Then
                Return result
            End If

            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
            diff = New Vector(lhs, offset) - New Vector(rhs, offset)
            result += diff.DotProduct(diff)
            diff = New Vector(lhs, offset + SIMD._vs1) - New Vector(rhs, offset + SIMD._vs1)
            result += diff.DotProduct(diff)

            If count = SIMD._vs2 Then
                Return result
            End If

            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
            diff = New Vector(lhs, offset) - New Vector(rhs, offset)
            result += diff.DotProduct(diff)

            If count = SIMD._vs1 Then
                Return result
            End If

            count -= SIMD._vs1
            offset += SIMD._vs1
        End If

        If count > 0 Then
            While count > 0
                d = lhs(offset) - rhs(offset)
                result += d * d
                offset += 1
                count -= 1
            End While
        End If

        Return result
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(ByRef lhs As Double(), f As Double)
        Dim count = lhs.Length
        Dim offset = 0
        Dim v = New Vector(f)

        While count >= SIMD._vs4
            Call (New Vector(lhs, offset) + v).CopyTo(lhs, offset)
            Call (New Vector(lhs, offset + SIMD._vs1) + v).CopyTo(lhs, offset + SIMD._vs1)
            Call (New Vector(lhs, offset + SIMD._vs2) + v).CopyTo(lhs, offset + SIMD._vs2)
            Call (New Vector(lhs, offset + SIMD._vs3) + v).CopyTo(lhs, offset + SIMD._vs3)

            If count = SIMD._vs4 Then
                Return
            End If

            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
            Call (New Vector(lhs, offset) + v).CopyTo(lhs, offset)
            Call (New Vector(lhs, CInt(offset + SIMD._vs1)) + v).CopyTo(lhs, offset + SIMD._vs1)

            If count = SIMD._vs2 Then
                Return
            End If

            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
            Call (New Vector(lhs, offset) + v).CopyTo(lhs, offset)

            If count = SIMD._vs1 Then
                Return
            End If

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
    Public Sub Multiply(ByRef lhs As Double(), f As Double)
        Dim count = lhs.Length
        Dim offset = 0

        While count >= SIMD._vs4
            Call (New Vector(lhs, offset) * f).CopyTo(lhs, offset)
            Call (New Vector(lhs, CInt(offset + SIMD._vs1)) * f).CopyTo(lhs, offset + SIMD._vs1)
            Call (New Vector(lhs, CInt(offset + SIMD._vs2)) * f).CopyTo(lhs, offset + SIMD._vs2)
            Call (New Vector(lhs, CInt(offset + SIMD._vs3)) * f).CopyTo(lhs, offset + SIMD._vs3)

            If count = SIMD._vs4 Then
                Return
            End If

            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
            Call (New Vector(lhs, offset) * f).CopyTo(lhs, offset)
            Call (New Vector(lhs, CInt(offset + SIMD._vs1)) * f).CopyTo(lhs, offset + SIMD._vs1)

            If count = SIMD._vs2 Then
                Return
            End If

            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
            Call (New Vector(lhs, offset) * f).CopyTo(lhs, offset)

            If count = SIMD._vs1 Then
                Return
            End If

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
    Public Function DotProduct(ByRef lhs As Double(), ByRef rhs As Double()) As Double
        Dim result = 0F
        Dim count = lhs.Length
        Dim offset = 0

        While count >= SIMD._vs4
            result += New Vector(lhs, offset).DotProduct(New Vector(rhs, offset))
            result += New Vector(lhs, offset + SIMD._vs1).DotProduct(New Vector(rhs, offset + SIMD._vs1))
            result += New Vector(lhs, offset + SIMD._vs2).DotProduct(New Vector(rhs, offset + SIMD._vs2))
            result += New Vector(lhs, offset + SIMD._vs3).DotProduct(New Vector(rhs, offset + SIMD._vs3))

            If count = SIMD._vs4 Then
                Return result
            End If

            count -= SIMD._vs4
            offset += SIMD._vs4
        End While

        If count >= SIMD._vs2 Then
            result += New Vector(lhs, offset).DotProduct(New Vector(rhs, offset))
            result += New Vector(lhs, offset + SIMD._vs1).DotProduct(New Vector(rhs, offset + SIMD._vs1))

            If count = SIMD._vs2 Then
                Return result
            End If

            count -= SIMD._vs2
            offset += SIMD._vs2
        End If

        If count >= SIMD._vs1 Then
            result += New Vector(lhs, offset).DotProduct(New Vector(rhs, offset))

            If count = SIMD._vs1 Then
                Return result
            End If

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
