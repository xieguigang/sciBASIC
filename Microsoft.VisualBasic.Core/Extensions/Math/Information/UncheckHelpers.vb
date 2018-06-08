Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Language

Namespace Math.Information

    Public Module UncheckHelpers

        ReadOnly sizeOfInt64% = Marshal.SizeOf(Long.MaxValue)
        ReadOnly sizeOfInt32% = Marshal.SizeOf(Integer.MaxValue)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToTruncateInt64(bi As BigInteger) As Long
            With bi.ToByteArray
                If .Length < sizeOfInt64 Then
                    Return CType(bi, Long)
                Else
                    Return BitConverter.ToInt64(.ByRef, Scan0)
                End If
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToTruncateInt32(bi As BigInteger) As Integer
            With bi.ToByteArray
                If .Length < sizeOfInt32 Then
                    Return CType(bi, Long)
                Else
                    Return BitConverter.ToInt32(.ByRef, Scan0)
                End If
            End With
        End Function
    End Module
End Namespace