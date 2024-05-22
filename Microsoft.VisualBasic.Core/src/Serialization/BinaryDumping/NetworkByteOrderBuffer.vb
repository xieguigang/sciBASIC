#Region "Microsoft.VisualBasic::ae7369042a99744ed2b9403a23dd42c2, Microsoft.VisualBasic.Core\src\Serialization\BinaryDumping\NetworkByteOrderBuffer.vb"

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

    '   Total Lines: 188
    '    Code Lines: 141 (75.00%)
    ' Comment Lines: 6 (3.19%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 41 (21.81%)
    '     File Size: 6.79 KB


    '     Class NetworkByteOrderBuffer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: defaultDecoder, defaultDecoder32, (+2 Overloads) defaultEncoder, (+3 Overloads) GetBytes, networkByteOrderDecoder
    '                   networkByteOrderDecoder32, (+2 Overloads) networkByteOrderEncoder, (+2 Overloads) ParseDouble, ToDouble, ToFloat
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

        Sub New()
            If BitConverter.IsLittleEndian Then
                ' reverse bytes
                encode = AddressOf networkByteOrderEncoder
                decode = AddressOf networkByteOrderDecoder
                encode32 = AddressOf networkByteOrderEncoder
                decode32 = AddressOf networkByteOrderDecoder32

                ' Call VBDebugger.EchoLine("system byte order is little endian.")
            Else
                ' no bytes sequence reverse
                encode = AddressOf defaultEncoder
                decode = AddressOf defaultDecoder
                encode32 = AddressOf defaultEncoder
                decode32 = AddressOf defaultDecoder32
            End If
        End Sub

        Public Function ParseDouble(base64 As String) As Double()
            Dim raw As Byte() = Base64Codec.Base64RawBytes(base64)
            Dim vals As Double() = decode(raw)
            Return vals
        End Function

        Public Function ParseDouble(raw As Byte()) As Double()
            Return decode(raw)
        End Function

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
