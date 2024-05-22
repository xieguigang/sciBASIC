#Region "Microsoft.VisualBasic::a431a5efd7f81c7354f3f2f0fb331334, Data_science\Mathematica\Math\Math\Numerics\HalfHelper.vb"

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

    '   Total Lines: 202
    '    Code Lines: 144 (71.29%)
    ' Comment Lines: 34 (16.83%)
    '    - Xml Docs: 38.24%
    ' 
    '   Blank Lines: 24 (11.88%)
    '     File Size: 8.25 KB


    '     Class HalfHelper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Abs, ConvertMantissa, GenerateBaseTable, GenerateExponentTable, GenerateMantissaTable
    '                   GenerateOffsetTable, GenerateShiftTable, HalfToSingle, IsInfinity, IsNaN
    '                   IsNegativeInfinity, IsPositiveInfinity, Negate, SingleToHalf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Numerics

    ''' <summary>
    ''' Helper class for Half conversions and some low level operations.
    ''' This class is internally used in the Half class.
    ''' </summary>
    ''' <remarks>
    ''' References:
    '''     - Fast Half Float Conversions, Jeroen van der Zijp, link: http://www.fox-toolkit.org/ftp/fasthalffloatconversion.pdf
    ''' </remarks>
    <ComVisible(False)>
    Friend NotInheritable Class HalfHelper

        Private Sub New()
        End Sub

        Private Shared ReadOnly MantissaTable As UInteger() = GenerateMantissaTable()
        Private Shared ReadOnly ExponentTable As UInteger() = GenerateExponentTable()
        Private Shared ReadOnly OffsetTable As UShort() = GenerateOffsetTable()
        Private Shared ReadOnly BaseTable As UShort() = GenerateBaseTable()
        Private Shared ReadOnly ShiftTable As SByte() = GenerateShiftTable()

        ''' <summary>
        ''' Transforms the subnormal representation to a normalized one. 
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Private Shared Function ConvertMantissa(i As Integer) As UInteger
            Dim m = CUInt(i << 13)
            ' Zero pad mantissa bits
            Dim e As UInteger = 0
            ' Zero exponent
            ' While not normalized
            While (m And &H800000) = 0
                e -= &H800000
                ' Decrement exponent (1<<23)
                ' Shift mantissa                
                m <<= 1
            End While
            m = m And CUInt(Not &H800000)
            ' m = m And CType(Not &H800000, UncheckedInteger).UncheckUInt32
            ' Clear leading 1 bit
            e += &H38800000
            ' Adjust bias ((127-14)<<23)
            ' Return combined number
            Return m Or e
        End Function

        Private Shared Function GenerateMantissaTable() As UInteger()
            Dim mantissaTable = New UInteger(2047) {}
            mantissaTable(0) = 0
            For i As Integer = 1 To 1023
                mantissaTable(i) = ConvertMantissa(i)
            Next
            For i As Integer = 1024 To 2047
                mantissaTable(i) = CUInt(&H38000000 + ((i - 1024) << 13))
            Next

            Return mantissaTable
        End Function

        Private Shared Function GenerateExponentTable() As UInteger()
            Dim exponentTable = New UInteger(63) {}
            exponentTable(0) = 0
            For i As Integer = 1 To 30
                exponentTable(i) = CUInt(i << 23)
            Next
            exponentTable(31) = &H47800000
            exponentTable(32) = &H80000000UI
            For i As Integer = 33 To 62
                exponentTable(i) = CUInt(&H80000000UI + ((i - 32) << 23))
            Next
            exponentTable(63) = &HC7800000UI

            Return exponentTable
        End Function

        Private Shared Function GenerateOffsetTable() As UShort()
            Dim offsetTable = New UShort(63) {}
            offsetTable(0) = 0
            For i As Integer = 1 To 31
                offsetTable(i) = 1024
            Next
            offsetTable(32) = 0
            For i As Integer = 33 To 63
                offsetTable(i) = 1024
            Next

            Return offsetTable
        End Function

        Private Shared Function GenerateBaseTable() As UShort()
            Dim baseTable = New UShort(511) {}
            For i As Integer = 0 To 255
                Dim e = CSByte(127 - i)
                If e > 24 Then
                    ' Very small numbers map to zero
                    baseTable(i Or &H0) = &H0
                    baseTable(i Or &H100) = &H8000
                ElseIf e > 14 Then
                    ' Small numbers map to denorms
                    baseTable(i Or &H0) = CUShort(&H400 >> (18 + e))
                    baseTable(i Or &H100) = CUShort((&H400 >> (18 + e)) Or &H8000)
                ElseIf e >= -15 Then
                    ' Normal numbers just lose precision
                    baseTable(i Or &H0) = CUShort((15 - e) << 10)
                    baseTable(i Or &H100) = CUShort(((15 - e) << 10) Or &H8000)
                ElseIf e > -128 Then
                    ' Large numbers map to Infinity
                    baseTable(i Or &H0) = &H7C00
                    baseTable(i Or &H100) = &HFC00
                Else
                    ' Infinity and NaN's stay Infinity and NaN's
                    baseTable(i Or &H0) = &H7C00
                    baseTable(i Or &H100) = &HFC00
                End If
            Next

            Return baseTable
        End Function

        Private Shared Function GenerateShiftTable() As SByte()
            Dim shiftTable = New SByte(511) {}
            For i As Integer = 0 To 255
                Dim e = CSByte(127 - i)
                If e > 24 Then
                    ' Very small numbers map to zero
                    shiftTable(i Or &H0) = 24
                    shiftTable(i Or &H100) = 24
                ElseIf e > 14 Then
                    ' Small numbers map to denorms
                    shiftTable(i Or &H0) = CSByte(e - 1)
                    shiftTable(i Or &H100) = CSByte(e - 1)
                ElseIf e >= -15 Then
                    ' Normal numbers just lose precision
                    shiftTable(i Or &H0) = 13
                    shiftTable(i Or &H100) = 13
                ElseIf e > -128 Then
                    ' Large numbers map to Infinity
                    shiftTable(i Or &H0) = 24
                    shiftTable(i Or &H100) = 24
                Else
                    ' Infinity and NaN's stay Infinity and NaN's
                    shiftTable(i Or &H0) = 13
                    shiftTable(i Or &H100) = 13
                End If
            Next

            Return shiftTable
        End Function

        Public Shared Function HalfToSingle(half As Half) As Single
            Dim result = MantissaTable(OffsetTable(half.Value >> 10) + (half.Value And &H3FF)) + ExponentTable(half.Value >> 10)
            Dim bytes = BitConverter.GetBytes(result)
            Dim sng As Single = BitConverter.ToSingle(bytes, Scan0)

            ' Return CType(New Pointer(Of UInteger)(result), Pointer(Of Single)).Target
            Return sng
        End Function

        Public Shared Function SingleToHalf([single] As Single) As Half
            ' Dim value = CType(New Pointer(Of Single)([single]), Pointer(Of UInteger)).Target
            Dim bytes = BitConverter.GetBytes([single])
            Dim value = BitConverter.ToUInt32(bytes, Scan0)
            Dim result As UShort = CUShort(BaseTable((value >> 23) And &H1FF) + ((value And &H7FFFFF) >> ShiftTable(value >> 23)))
            Return Half.ToHalf(result)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Negate(half As Half) As Half
            Return Half.ToHalf(CUShort(half.Value Xor &H8000))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Abs(half As Half) As Half
            Return Half.ToHalf(CUShort(half.Value And &H7FFF))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsNaN(half As Half) As Boolean
            Return (half.Value And &H7FFF) > &H7C00
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsInfinity(half As Half) As Boolean
            Return (half.Value And &H7FFF) = &H7C00
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsPositiveInfinity(half As Half) As Boolean
            Return half.Value = &H7C00
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsNegativeInfinity(half As Half) As Boolean
            Return half.Value = &HFC00
        End Function
    End Class
End Namespace
