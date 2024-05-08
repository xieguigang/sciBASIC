' 
'* Copyright 2014 Google Inc. All rights reserved.
'*
'* Licensed under the Apache License, Version 2.0 (the "License");
'* you may not use this file except in compliance with the License.
'* You may obtain a copy of the License at
'*
'*     http://www.apache.org/licenses/LICENSE-2.0
'*
'* Unless required by applicable law or agreed to in writing, software
'* distributed under the License is distributed on an "AS IS" BASIS,
'* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'* See the License for the specific language governing permissions and
'* limitations under the License.


' There are 2 #defines that have an impact on performance of this ByteBuffer implementation
'
'      UNSAFE_BYTEBUFFER 
'          This will use unsafe code to manipulate the underlying byte array. This
'          can yield a reasonable performance increase.
'
'      BYTEBUFFER_NO_BOUNDS_CHECK
'          This will disable the bounds check asserts to the byte array. This can
'          yield a small performance gain in normal code..
'
' Using UNSAFE_BYTEBUFFER and BYTEBUFFER_NO_BOUNDS_CHECK together can yield a
' performance gain of ~15% for some operations, however doing so is potentially 
' dangerous. Do so at your own risk!
'

Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Math.Information

Namespace FlatBuffers
    ''' <summary>
    ''' Class to mimic Java's ByteBuffer which is used heavily in Flatbuffers.
    ''' </summary>
    Friend Class ByteBuffer
        Private ReadOnly _buffer As Byte()
        Private _pos As Integer  ' Must track start of the buffer.

        Public ReadOnly Property Length As Integer
            Get
                Return _buffer.Length
            End Get
        End Property

        Public ReadOnly Property Data As Byte()
            Get
                Return _buffer
            End Get
        End Property

        Public Sub New(buffer As Byte())
            Me.New(buffer, 0)
        End Sub

        Public Sub New(buffer As Byte(), pos As Integer)
            _buffer = buffer
            _pos = pos
        End Sub

        Public Property Position As Integer
            Get
                Return _pos
            End Get
            Set(value As Integer)
                _pos = value
            End Set
        End Property

        Public Sub Reset()
            _pos = 0
        End Sub

        ' Pre-allocated helper arrays for convertion.
        Private floathelper As Single() = {0.0F}
        Private inthelper As Integer() = {0}
        Private doublehelper As Double() = {0.0}
        Private ulonghelper As ULong() = {0UL}

        Public Overrides Function ToString() As String
            Return "ByteBuffer: " & StringFormats.Lanudry(bytes:=Length)
        End Function

        ' Helper functions for the unsafe version.
        Public Shared Function ReverseBytes(input As UShort) As UShort
            Return (input And &HFFUI) << 8 Or (input And &HFF00UI) >> 8
        End Function
        Public Shared Function ReverseBytes(input As UInteger) As UInteger
            Return (input And &HFFUI) << 24 Or (input And &HFF00UI) << 8 Or (input And &HFF0000UI) >> 8 Or (input And &HFF000000UI) >> 24
        End Function
        Public Shared Function ReverseBytes(input As ULong) As ULong
            Return (input And &HFFUL) << 56 Or (input And &HFF00UL) << 40 Or (input And &HFF0000UL) << 24 Or (input And &HFF000000UL) << 8 Or (input And &HFF00000000UL) >> 8 Or (input And &HFF0000000000UL) >> 24 Or (input And &HFF000000000000UL) >> 40 Or (input And &HFF00000000000000UL) >> 56
        End Function

#If Not UNSAFE_BYTEBUFFER Then
        ' Helper functions for the safe (but slower) version.
        Protected Sub WriteLittleEndian(offset As Integer, count As Integer, data As ULong)
            If BitConverter.IsLittleEndian Then
                For i = 0 To count - 1
                    _buffer(offset + i) = CByte(data >> i * 8)
                Next
            Else
                For i = 0 To count - 1
                    _buffer(offset + count - 1 - i) = CByte(data >> i * 8)
                Next
            End If
        End Sub

        Protected Function ReadLittleEndian(offset As Integer, count As Integer) As ULong
            AssertOffsetAndLength(offset, count)
            Dim r As ULong = 0
            If BitConverter.IsLittleEndian Then
                For i = 0 To count - 1
                    r = r Or CULng(_buffer(offset + i)) << i * 8
                Next
            Else
                For i = 0 To count - 1
                    r = r Or CULng(_buffer(offset + count - 1 - i)) << i * 8
                Next
            End If
            Return r
        End Function
#End If


        Private Sub AssertOffsetAndLength(offset As Integer, length As Integer)
#If Not BYTEBUFFER_NO_BOUNDS_CHECK Then
            If offset < 0 OrElse offset > _buffer.Length - length Then Throw New ArgumentOutOfRangeException()
#End If
        End Sub

        Public Sub PutSbyte(offset As Integer, value As SByte)
            AssertOffsetAndLength(offset, HeapSizeOf.sbyte)
            _buffer(offset) = CByte(value)
        End Sub

        Public Sub PutByte(offset As Integer, value As Byte)
            AssertOffsetAndLength(offset, HeapSizeOf.byte)
            _buffer(offset) = value
        End Sub

        Public Sub PutByte(offset As Integer, value As Byte, count As Integer)
            AssertOffsetAndLength(offset, HeapSizeOf.byte * count)
            Dim i = 0

            While i < count
                _buffer(offset + i) = value
                Threading.Interlocked.Increment(i)
            End While
        End Sub

        ' this method exists in order to conform with Java ByteBuffer standards
        Public Sub Put(offset As Integer, value As Byte)
            PutByte(offset, value)
        End Sub

#If UNSAFE_BYTEBUFFER Then

#Else ' !UNSAFE_BYTEBUFFER
        ' Slower versions of Put* for when unsafe code is not allowed.
        Public Sub PutShort(offset As Integer, value As Short)
            AssertOffsetAndLength(offset, HeapSizeOf.short)
            WriteLittleEndian(offset, HeapSizeOf.short, value)
        End Sub

        Public Sub PutUshort(offset As Integer, value As UShort)
            AssertOffsetAndLength(offset, HeapSizeOf.ushort)
            WriteLittleEndian(offset, HeapSizeOf.ushort, value)
        End Sub

        Public Sub PutInt(offset As Integer, value As Integer)
            AssertOffsetAndLength(offset, HeapSizeOf.int)
            WriteLittleEndian(offset, HeapSizeOf.int, value)
        End Sub

        Public Sub PutUint(offset As Integer, value As UInteger)
            AssertOffsetAndLength(offset, HeapSizeOf.uint)
            WriteLittleEndian(offset, HeapSizeOf.uint, value)
        End Sub

        Public Sub PutLong(offset As Integer, value As Long)
            AssertOffsetAndLength(offset, HeapSizeOf.long)
            WriteLittleEndian(offset, HeapSizeOf.long, value)
        End Sub

        Public Sub PutUlong(offset As Integer, value As ULong)
            AssertOffsetAndLength(offset, HeapSizeOf.ulong)
            WriteLittleEndian(offset, HeapSizeOf.ulong, value)
        End Sub

        Public Sub PutFloat(offset As Integer, value As Single)
            AssertOffsetAndLength(offset, HeapSizeOf.float)
            floathelper(0) = value
            Buffer.BlockCopy(floathelper, 0, inthelper, 0, HeapSizeOf.float)
            WriteLittleEndian(offset, HeapSizeOf.float, inthelper(0))
        End Sub

        Public Sub PutDouble(offset As Integer, value As Double)
            AssertOffsetAndLength(offset, HeapSizeOf.double)
            doublehelper(0) = value
            Buffer.BlockCopy(doublehelper, 0, ulonghelper, 0, HeapSizeOf.double)
            WriteLittleEndian(offset, HeapSizeOf.double, ulonghelper(0))
        End Sub

#End If

        Public Function GetSbyte(index As Integer) As SByte
            AssertOffsetAndLength(index, HeapSizeOf.sbyte)
            Return _buffer(index)
        End Function

        Public Function [Get](index As Integer) As Byte
            AssertOffsetAndLength(index, HeapSizeOf.byte)
            Return _buffer(index)
        End Function

#If UNSAFE_BYTEBUFFER Then

#Else ' !UNSAFE_BYTEBUFFER
        ' Slower versions of Get* for when unsafe code is not allowed.
        Public Function GetShort(index As Integer) As Short
            Return ReadLittleEndian(index, HeapSizeOf.short)
        End Function

        Public Function GetUshort(index As Integer) As UShort
            Return ReadLittleEndian(index, HeapSizeOf.ushort)
        End Function

        Public Function GetInt(index As Integer) As Integer
            Return ReadLittleEndian(index, HeapSizeOf.int).UnsafeTruncateInteger
        End Function

        Public Function GetUint(index As Integer) As UInteger
            Return ReadLittleEndian(index, HeapSizeOf.uint)
        End Function

        Public Function GetLong(index As Integer) As Long
            Return ReadLittleEndian(index, HeapSizeOf.long)
        End Function

        Public Function GetUlong(index As Integer) As ULong
            Return ReadLittleEndian(index, HeapSizeOf.ulong)
        End Function

        Public Function GetFloat(index As Integer) As Single
            Dim i As Integer = ReadLittleEndian(index, HeapSizeOf.float)
            inthelper(0) = i
            Buffer.BlockCopy(inthelper, 0, floathelper, 0, HeapSizeOf.float)
            Return floathelper(0)
        End Function

        Public Function GetDouble(index As Integer) As Double
            Dim i = ReadLittleEndian(index, HeapSizeOf.double)
            ulonghelper(0) = i
            Buffer.BlockCopy(ulonghelper, 0, doublehelper, 0, HeapSizeOf.double)
            Return doublehelper(0)
        End Function
#End If
    End Class
End Namespace
