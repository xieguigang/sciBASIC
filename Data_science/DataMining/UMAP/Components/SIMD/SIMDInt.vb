Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

Friend Module SIMDint
    Private ReadOnly _vs1 As Integer = Vector(Of Integer).Count
    Private ReadOnly _vs2 As Integer = 2 * Vector(Of Integer).Count
    Private ReadOnly _vs3 As Integer = 3 * Vector(Of Integer).Count
    Private ReadOnly _vs4 As Integer = 4 * Vector(Of Integer).Count

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Zero(ByRef lhs As Integer())
        Dim count = lhs.Length
        Dim offset = 0

        While count >= SIMDint._vs4
            Vector(Of Integer).Zero.CopyTo(lhs, offset)
            Vector(Of Integer).Zero.CopyTo(lhs, offset + SIMDint._vs1)
            Vector(Of Integer).Zero.CopyTo(lhs, offset + SIMDint._vs2)
            Vector(Of Integer).Zero.CopyTo(lhs, offset + SIMDint._vs3)
            If count = SIMDint._vs4 Then Return
            count -= SIMDint._vs4
            offset += SIMDint._vs4
        End While

        If count >= SIMDint._vs2 Then
            Vector(Of Integer).Zero.CopyTo(lhs, offset)
            Vector(Of Integer).Zero.CopyTo(lhs, offset + SIMDint._vs1)
            If count = SIMDint._vs2 Then Return
            count -= SIMDint._vs2
            offset += SIMDint._vs2
        End If

        If count >= SIMDint._vs1 Then
            Vector(Of Integer).Zero.CopyTo(lhs, offset)
            If count = SIMDint._vs1 Then Return
            count -= SIMDint._vs1
            offset += SIMDint._vs1
        End If

        If count > 0 Then
            While count > 0
                lhs(offset) = 0
                offset += 1
                count -= 1
            End While
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Uniform(ByRef data As Single(), a As Single, random As IProvideRandomValues)
        Dim a2 = 2 * a
        Dim an = -a
        random.NextFloats(data)
        SIMD.Multiply(data, a2)
        SIMD.Add(data, an)
    End Sub
End Module
