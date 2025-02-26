#Region "Microsoft.VisualBasic::f1d5e00721720af9b546353f3621d708, Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\NetworkByteOrderBuffer.vb"

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

    '   Total Lines: 408
    '    Code Lines: 288 (70.59%)
    ' Comment Lines: 26 (6.37%)
    '    - Xml Docs: 88.46%
    ' 
    '   Blank Lines: 94 (23.04%)
    '     File Size: 14.55 KB


    '     Class NetworkByteOrderBuffer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Enum Compression
    ' 
    '             gzip, none, zlib
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: (+4 Overloads) Base64String, defaultDecoder, defaultDecoder32, defaultDecoderi16, defaultDecoderi32
    '               (+4 Overloads) defaultEncoder, (+6 Overloads) GetBytes, networkByteOrderDecoder, networkByteOrderDecoder32, networkByteOrderDecoderi16
    '               networkByteOrderDecoderi32, (+4 Overloads) networkByteOrderEncoder, (+2 Overloads) ParseDouble, ParseInteger, ToDouble
    '               ToFloat
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http

Namespace Serialization.BinaryDumping

    ''' <summary>
    ''' ensure that the number is converted as bytes always in network byte order outputs.
    ''' </summary>
    Public Class NetworkByteOrderBuffer

        Public ReadOnly encode As Func(Of Double(), Byte())
        Public ReadOnly decode As Func(Of Byte(), Double())

        Public ReadOnly encode32 As Func(Of Single(), Byte())
        Public ReadOnly decode32 As Func(Of Byte(), Single())

        Public ReadOnly encodei32 As Func(Of Integer(), Byte())
        Public ReadOnly decodei32 As Func(Of Byte(), Integer())

        Public ReadOnly encodei16 As Func(Of Short(), Byte())
        Public ReadOnly decodei16 As Func(Of Byte(), Short())

        Sub New()
            If BitConverter.IsLittleEndian Then
                ' reverse bytes
                encode = AddressOf networkByteOrderEncoder
                decode = AddressOf networkByteOrderDecoder
                encode32 = AddressOf networkByteOrderEncoder
                decode32 = AddressOf networkByteOrderDecoder32
                encodei32 = AddressOf networkByteOrderEncoder
                decodei32 = AddressOf networkByteOrderDecoderi32
                encodei16 = AddressOf networkByteOrderEncoder
                decodei16 = AddressOf networkByteOrderDecoderi16

                ' Call VBDebugger.EchoLine("system byte order is little endian.")
            Else
                ' no bytes sequence reverse
                encode = AddressOf defaultEncoder
                decode = AddressOf defaultDecoder
                encode32 = AddressOf defaultEncoder
                decode32 = AddressOf defaultDecoder32
                encodei32 = AddressOf defaultEncoder
                decodei32 = AddressOf defaultDecoderi32
                encodei16 = AddressOf defaultEncoder
                decodei16 = AddressOf defaultDecoderi16
            End If
        End Sub

        Public Enum Compression
            none
            gzip
            zlib
        End Enum

        ''' <summary>
        ''' parse the given base64 string as the numeric vector
        ''' </summary>
        ''' <param name="base64"></param>
        ''' <param name="zip">does the given base64 string is gzip compressed data?</param>
        ''' <param name="noMagic">does the zip compression data has two byte of magic number, default is false which means it has the magic number</param>
        ''' <returns></returns>
        Public Function ParseDouble(base64 As String,
                                    Optional zip As Compression = Compression.none,
                                    Optional noMagic As Boolean = False) As Double()

            Dim raw As Byte() = Base64Codec.Base64RawBytes(base64)
            Dim data As Byte() = raw

            If zip <> Compression.none Then
                If zip = Compression.gzip Then
                    data = raw.UnGzipStream.ToArray
                Else
                    data = raw.UnZipStream(noMagic).ToArray
                End If
            End If

            Dim vals As Double() = decode(data)

            Return vals
        End Function

        Public Function Base64String(data As IEnumerable(Of Single), Optional gzip As Boolean = False) As String
            Dim raw As Byte() = GetBytes(data)
            Dim str As String

            If gzip Then
                str = raw.GZipAsBase64(noMagic:=False)
            Else
                str = Base64Codec.ToBase64String(raw)
            End If

            Return str
        End Function

        Public Function Base64String(data As IEnumerable(Of Integer), Optional gzip As Boolean = False) As String
            Dim raw As Byte() = GetBytes(data)
            Dim str As String

            If gzip Then
                str = raw.GZipAsBase64(noMagic:=False)
            Else
                str = Base64Codec.ToBase64String(raw)
            End If

            Return str
        End Function

        Public Function Base64String(data As IEnumerable(Of Short), Optional gzip As Boolean = False) As String
            Dim raw As Byte() = GetBytes(data)
            Dim str As String

            If gzip Then
                str = raw.GZipAsBase64(noMagic:=False)
            Else
                str = Base64Codec.ToBase64String(raw)
            End If

            Return str
        End Function

        ''' <summary>
        ''' encode the given numeric data vector in network byte order and then returns the base64 encode string
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Function Base64String(data As IEnumerable(Of Double), Optional gzip As Boolean = False) As String
            Dim raw As Byte() = GetBytes(data)
            Dim str As String

            If gzip Then
                str = raw.GZipAsBase64(noMagic:=False)
            Else
                str = Base64Codec.ToBase64String(raw)
            End If

            Return str
        End Function

        Public Function ParseDouble(raw As Byte()) As Double()
            Return decode(raw)
        End Function

        Public Function ParseInteger(raw As Byte()) As Integer()
            Return decodei32(raw)
        End Function

        Public Function GetBytes(i As IEnumerable(Of Single)) As Byte()
            Return encode32(i.SafeQuery.ToArray)
        End Function


        Public Function GetBytes(i As IEnumerable(Of Integer)) As Byte()
            Return encodei32(i.SafeQuery.ToArray)
        End Function

        Public Function GetBytes(i As IEnumerable(Of Short)) As Byte()
            Return encodei16(i.SafeQuery.ToArray)
        End Function

        ''' <summary>
        ''' make binary data encoding of a given numeric vector
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' a wrapper function of the <see cref="encode"/>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetBytes(d As IEnumerable(Of Double)) As Byte()
            Return encode(d.SafeQuery.ToArray)
        End Function

        Public Function GetBytes(f As Single) As Byte()
            If BitConverter.IsLittleEndian Then
                Dim bytes As Byte() = BitConverter.GetBytes(f)
                Array.Reverse(bytes)
                Return bytes
            Else
                Return BitConverter.GetBytes(f)
            End If
        End Function

        Public Function GetBytes(d As Double) As Byte()
            If BitConverter.IsLittleEndian Then
                Dim bytes As Byte() = BitConverter.GetBytes(d)
                Array.Reverse(bytes)
                Return bytes
            Else
                Return BitConverter.GetBytes(d)
            End If
        End Function

        Public Function ToFloat(bytes As Byte()) As Single
            If BitConverter.IsLittleEndian Then
                Dim fltBytes As Byte() = New Byte(RawStream.SingleFloat - 1) {}
                Array.ConstrainedCopy(bytes, Scan0, fltBytes, Scan0, RawStream.SingleFloat)
                Return BitConverter.ToSingle(fltBytes, Scan0)
            Else
                Return BitConverter.ToSingle(bytes, Scan0)
            End If
        End Function

        Public Function ToDouble(bytes As Byte()) As Double
            If BitConverter.IsLittleEndian Then
                Dim dblBytes As Byte() = New Byte(RawStream.DblFloat - 1) {}
                Array.ConstrainedCopy(bytes, Scan0, dblBytes, Scan0, RawStream.DblFloat)
                Return BitConverter.ToDouble(dblBytes, Scan0)
            Else
                Return BitConverter.ToDouble(bytes, Scan0)
            End If
        End Function

        Private Shared Function defaultDecoder(buffer As Byte()) As Double()
            Dim nums As Double() = New Double(buffer.Length / 8 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                nums(i) = BitConverter.ToDouble(buffer, i * 8)
            Next

            Return nums
        End Function

        Private Shared Function defaultDecoderi32(buffer As Byte()) As Integer()
            Dim nums As Integer() = New Integer(buffer.Length / 4 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                nums(i) = BitConverter.ToInt32(buffer, i * 4)
            Next

            Return nums
        End Function

        Private Shared Function defaultDecoderi16(buffer As Byte()) As Short()
            Dim nums As Short() = New Short(buffer.Length / 2 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                nums(i) = BitConverter.ToInt16(buffer, i * 2)
            Next

            Return nums
        End Function

        Private Shared Function defaultEncoder(nums As Integer()) As Byte()
            Dim bytes As New List(Of Byte)

            For Each d As Integer In nums
                Call bytes.AddRange(BitConverter.GetBytes(d))
            Next

            Return bytes.ToArray
        End Function


        Private Shared Function defaultDecoder32(buffer As Byte()) As Single()
            Dim nums As Single() = New Single(buffer.Length / 4 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                nums(i) = BitConverter.ToSingle(buffer, i * 4)
            Next

            Return nums
        End Function

        Private Shared Function defaultEncoder(nums As Single()) As Byte()
            Dim bytes As New List(Of Byte)

            For Each d As Single In nums
                Call bytes.AddRange(BitConverter.GetBytes(d))
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function defaultEncoder(nums As Double()) As Byte()
            Dim bytes As New List(Of Byte)

            For Each d As Double In nums
                Call bytes.AddRange(BitConverter.GetBytes(d))
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function defaultEncoder(nums As Short()) As Byte()
            Dim bytes As New List(Of Byte)

            For Each d As Short In nums
                Call bytes.AddRange(BitConverter.GetBytes(d))
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function networkByteOrderDecoderi32(buffer As Byte()) As Integer()
            Dim nums As Integer() = New Integer(buffer.Length / 4 - 1) {}
            Dim bytes As Byte() = New Byte(4 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                Call Array.ConstrainedCopy(buffer, i * 4, bytes, Scan0, bytes.Length)
                Call Array.Reverse(bytes)

                nums(i) = BitConverter.ToInt32(bytes, Scan0)
            Next

            Return nums
        End Function

        Private Shared Function networkByteOrderDecoderi16(buffer As Byte()) As Short()
            Dim nums As Int16() = New Short(buffer.Length / 2 - 1) {}
            Dim bytes As Byte() = New Byte(2 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                Call Array.ConstrainedCopy(buffer, i * 2, bytes, Scan0, bytes.Length)
                Call Array.Reverse(bytes)

                nums(i) = BitConverter.ToInt16(bytes, Scan0)
            Next

            Return nums
        End Function

        Private Shared Function networkByteOrderDecoder32(buffer As Byte()) As Single()
            Dim nums As Single() = New Single(buffer.Length / 4 - 1) {}
            Dim bytes As Byte() = New Byte(4 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                Call Array.ConstrainedCopy(buffer, i * 4, bytes, Scan0, bytes.Length)
                Call Array.Reverse(bytes)

                nums(i) = BitConverter.ToSingle(bytes, Scan0)
            Next

            Return nums
        End Function

        Private Shared Function networkByteOrderDecoder(buffer As Byte()) As Double()
            Dim nums As Double() = New Double(buffer.Length / 8 - 1) {}
            Dim bytes As Byte() = New Byte(8 - 1) {}

            For i As Integer = 0 To nums.Length - 1
                Call Array.ConstrainedCopy(buffer, i * 8, bytes, Scan0, bytes.Length)
                Call Array.Reverse(bytes)

                nums(i) = BitConverter.ToDouble(bytes, Scan0)
            Next

            Return nums
        End Function

        Private Shared Function networkByteOrderEncoder(nums As Integer()) As Byte()
            Dim bytes As New List(Of Byte)
            Dim buffer As Byte()

            For Each d As Integer In nums
                buffer = BitConverter.GetBytes(d)

                Call Array.Reverse(buffer)
                Call bytes.AddRange(buffer)
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function networkByteOrderEncoder(nums As Short()) As Byte()
            Dim bytes As New List(Of Byte)
            Dim buffer As Byte()

            For Each d As Short In nums
                buffer = BitConverter.GetBytes(d)

                Call Array.Reverse(buffer)
                Call bytes.AddRange(buffer)
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function networkByteOrderEncoder(nums As Single()) As Byte()
            Dim bytes As New List(Of Byte)
            Dim buffer As Byte()

            For Each d As Single In nums
                buffer = BitConverter.GetBytes(d)

                Call Array.Reverse(buffer)
                Call bytes.AddRange(buffer)
            Next

            Return bytes.ToArray
        End Function

        Private Shared Function networkByteOrderEncoder(nums As Double()) As Byte()
            Dim bytes As New List(Of Byte)
            Dim buffer As Byte()

            For Each d As Double In nums
                buffer = BitConverter.GetBytes(d)

                Call Array.Reverse(buffer)
                Call bytes.AddRange(buffer)
            Next

            Return bytes.ToArray
        End Function
    End Class
End Namespace
