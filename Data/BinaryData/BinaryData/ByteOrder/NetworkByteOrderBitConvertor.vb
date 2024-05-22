#Region "Microsoft.VisualBasic::47a633f9b05b9dd77dc6ec5069aee966, Data\BinaryData\BinaryData\ByteOrder\NetworkByteOrderBitConvertor.vb"

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

    '   Total Lines: 282
    '    Code Lines: 174 (61.70%)
    ' Comment Lines: 80 (28.37%)
    '    - Xml Docs: 86.25%
    ' 
    '   Blank Lines: 28 (9.93%)
    '     File Size: 10.94 KB


    ' Module NetworkByteOrderBitConvertor
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: f32Bits, f64Bits, (+5 Overloads) GetBytes, getf32, getf64
    '               geti16, geti32, geti64, i16Bits, i32Bits
    '               i64Bits, (+2 Overloads) ToDouble, ToInt16, ToInt32, ToInt64
    '               ToSingle
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public Module NetworkByteOrderBitConvertor

    Dim f64Bytes As Func(Of Double, Byte())
    Dim f64 As Func(Of Byte(), Integer, Double)

    Dim f32Bytes As Func(Of Single, Byte())
    Dim f32 As Func(Of Byte(), Integer, Single)

    Dim i64Bytes As Func(Of Long, Byte())
    Dim i64 As Func(Of Byte(), Integer, Long)

    Dim i32Bytes As Func(Of Integer, Byte())
    Dim i32 As Func(Of Byte(), Integer, Integer)

    Dim i16Bytes As Func(Of Short, Byte())
    Dim i16 As Func(Of Byte(), Integer, Short)

    Sub New()
        f64 = getf64()
        f64Bytes = f64Bits()
        f32 = getf32()
        f32Bytes = f32Bits()
        i32Bytes = i32Bits()
        i32 = geti32()
        i64Bytes = i64Bits()
        i64 = geti64()
        i16Bytes = i16Bits()
        i16 = geti16()
    End Sub

    Private Function getf32() As Func(Of Byte(), Integer, Single)
        If BitConverter.IsLittleEndian Then
            Return Function(bytes, offset)
                       Call Array.Reverse(bytes)
                       Return BitConverter.ToSingle(bytes, offset)
                   End Function
        Else
            Return AddressOf BitConverter.ToSingle
        End If
    End Function

    Private Function f32Bits() As Func(Of Single, Byte())
        If BitConverter.IsLittleEndian Then
            Return Function(d)
                       Dim chunk As Byte() = BitConverter.GetBytes(d)
                       Call Array.Reverse(chunk)
                       Return chunk
                   End Function
        Else
            Return AddressOf BitConverter.GetBytes
        End If
    End Function

    Private Function getf64() As Func(Of Byte(), Integer, Double)
        If BitConverter.IsLittleEndian Then
            Return Function(bytes, offset)
                       Call Array.Reverse(bytes)
                       Return BitConverter.ToDouble(bytes, offset)
                   End Function
        Else
            Return AddressOf BitConverter.ToDouble
        End If
    End Function

    Private Function f64Bits() As Func(Of Double, Byte())
        If BitConverter.IsLittleEndian Then
            Return Function(d)
                       Dim chunk As Byte() = BitConverter.GetBytes(d)
                       Call Array.Reverse(chunk)
                       Return chunk
                   End Function
        Else
            Return AddressOf BitConverter.GetBytes
        End If
    End Function

    Private Function geti32() As Func(Of Byte(), Integer, Integer)
        If BitConverter.IsLittleEndian Then
            Return Function(bytes, offset)
                       Call Array.Reverse(bytes)
                       Return BitConverter.ToInt32(bytes, offset)
                   End Function
        Else
            Return AddressOf BitConverter.ToInt32
        End If
    End Function

    Private Function i32Bits() As Func(Of Integer, Byte())
        If BitConverter.IsLittleEndian Then
            Return Function(d)
                       Dim chunk As Byte() = BitConverter.GetBytes(d)
                       Call Array.Reverse(chunk)
                       Return chunk
                   End Function
        Else
            Return AddressOf BitConverter.GetBytes
        End If
    End Function

    Private Function geti16() As Func(Of Byte(), Integer, Short)
        If BitConverter.IsLittleEndian Then
            Return Function(bytes, offset)
                       Call Array.Reverse(bytes)
                       Return BitConverter.ToInt16(bytes, offset)
                   End Function
        Else
            Return AddressOf BitConverter.ToInt16
        End If
    End Function

    Private Function i16Bits() As Func(Of Short, Byte())
        If BitConverter.IsLittleEndian Then
            Return Function(d)
                       Dim chunk As Byte() = BitConverter.GetBytes(d)
                       Call Array.Reverse(chunk)
                       Return chunk
                   End Function
        Else
            Return AddressOf BitConverter.GetBytes
        End If
    End Function

    Private Function geti64() As Func(Of Byte(), Integer, Long)
        If BitConverter.IsLittleEndian Then
            Return Function(bytes, offset)
                       Call Array.Reverse(bytes)
                       Return BitConverter.ToInt64(bytes, offset)
                   End Function
        Else
            Return AddressOf BitConverter.ToInt64
        End If
    End Function

    Private Function i64Bits() As Func(Of Long, Byte())
        If BitConverter.IsLittleEndian Then
            Return Function(d)
                       Dim chunk As Byte() = BitConverter.GetBytes(d)
                       Call Array.Reverse(chunk)
                       Return chunk
                   End Function
        Else
            Return AddressOf BitConverter.GetBytes
        End If
    End Function

    ''' <summary>
    ''' Returns a 16-bit signed integer converted from two bytes at a specified position
    ''' in a byte array.
    ''' </summary>
    ''' <param name="value">An array of bytes that includes the two bytes to convert.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <returns>A 16-bit signed integer formed by two bytes beginning at startIndex.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToInt16(value() As Byte, startIndex As Integer) As Short
        Return i16(value, startIndex)
    End Function

    ''' <summary>
    ''' Returns the specified 16-bit signed integer value as an array of bytes.
    ''' </summary>
    ''' <param name="value">The number to convert.</param>
    ''' <returns>An array of bytes with length 2.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBytes(value As Short) As Byte()
        Return i16Bytes(value)
    End Function

    ''' <summary>
    ''' Returns a 64-bit signed integer converted from eight bytes at a specified position
    ''' in a byte array.
    ''' </summary>
    ''' <param name="value">An array of bytes that includes the eight bytes to convert.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <returns>A 64-bit signed integer formed by eight bytes beginning at startIndex.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToInt64(value() As Byte, startIndex As Integer) As Long
        Return i64(value, startIndex)
    End Function

    ''' <summary>
    ''' Returns the specified 64-bit signed integer value as an array of bytes.
    ''' </summary>
    ''' <param name="value">The number to convert.</param>
    ''' <returns>An array of bytes with length 8.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBytes(value As Long) As Byte()
        Return i64Bytes(value)
    End Function

    ''' <summary>
    ''' Returns the specified double-precision floating-point value as an array of bytes.
    ''' </summary>
    ''' <param name="value">The number to convert.</param>
    ''' <returns>An array of bytes with length 8.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBytes(value As Double) As Byte()
        Return f64Bytes(value)
    End Function

    ''' <summary>
    ''' Returns a double-precision floating point number converted from eight bytes at
    ''' a specified position in a byte array.
    ''' </summary>
    ''' <param name="value">An array of bytes that includes the eight bytes to convert.</param>
    ''' <returns>A double-precision floating point number formed by eight bytes beginning at startIndex.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToDouble(value() As Byte) As Double
        Return f64(value, Scan0)
    End Function

    ''' <summary>
    ''' Returns a double-precision floating point number converted from eight bytes at
    ''' a specified position in a byte array.
    ''' </summary>
    ''' <param name="value">An array of bytes that includes the eight bytes to convert.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <returns>A double-precision floating point number formed by eight bytes beginning at startIndex.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToDouble(value() As Byte, startIndex As Integer) As Double
        Return f64(value, startIndex)
    End Function

    ''' <summary>
    ''' Returns a 32-bit signed integer converted from four bytes at a specified position
    ''' in a byte array.
    ''' </summary>
    ''' <param name="value">An array of bytes that includes the four bytes to convert.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <returns>A 32-bit signed integer formed by four bytes beginning at startIndex.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToInt32(value() As Byte, startIndex As Integer) As Integer
        Return i32(value, startIndex)
    End Function

    ''' <summary>
    ''' Returns the specified 32-bit signed integer value as an array of bytes.
    ''' </summary>
    ''' <param name="value">The number to convert.</param>
    ''' <returns>An array of bytes with length 4.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBytes(value As Integer) As Byte()
        Return i32Bytes(value)
    End Function

    ''' <summary>
    ''' Returns a single-precision floating point number converted from four bytes at
    ''' a specified position in a byte array.
    ''' </summary>
    ''' <param name="value">An array of bytes.</param>
    ''' <param name="startIndex">The starting position within value.</param>
    ''' <returns>
    ''' A single-precision floating point number formed by four bytes beginning at 
    ''' startIndex.
    ''' </returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToSingle(value() As Byte, startIndex As Integer) As Single
        Return f32(value, startIndex)
    End Function

    ''' <summary>
    ''' Returns the specified single-precision floating point value as an array of bytes.
    ''' </summary>
    ''' <param name="value">The number to convert.</param>
    ''' <returns>An array of bytes with length 4.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBytes(value As Single) As Byte()
        Return f32Bytes(value)
    End Function
End Module
