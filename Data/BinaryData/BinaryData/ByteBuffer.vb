#Region "Microsoft.VisualBasic::8b96da5e7643a2f1028e70f7635a2572, Data\BinaryData\BinaryData\ByteBuffer.vb"

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

    '   Total Lines: 358
    '    Code Lines: 283
    ' Comment Lines: 35
    '   Blank Lines: 40
    '     File Size: 11.56 KB


    ' Class ByteBuffer
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) [get], allocate, allocateDirect, capacity, clear
    '               compact, Equals, flip, (+2 Overloads) getChar, (+2 Overloads) getDouble
    '               (+2 Overloads) getFloat, (+2 Overloads) getInt, (+2 Overloads) getLong, (+2 Overloads) getShort, hasRemaining
    '               limit, order, (+2 Overloads) position, (+2 Overloads) put, (+2 Overloads) putChar
    '               (+2 Overloads) putDouble, (+2 Overloads) putFloat, (+2 Overloads) putInt, (+2 Overloads) putLong, (+2 Overloads) putShort
    '               remaining, rewind, (+2 Overloads) wrap
    ' 
    '     Sub: (+4 Overloads) [get], Finalize
    ' 
    ' /********************************************************************************/

#End Region

'-------------------------------------------------------------------------------------------
'	Copyright ©  - 2017 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class is used to replicate the java.nio.ByteBuffer class in C#.
'
'	Instances are only obtainable via the static 'allocate' method.
'
'	Some methods are not available:
'		All methods which create shared views of the buffer such as: array,
'		asCharBuffer, asDoubleBuffer, asFloatBuffer, asIntBuffer, asLongBuffer,
'		asReadOnlyBuffer, asShortBuffer, duplicate, slice, & wrap.
'
'		Other methods such as: mark, reset, isReadOnly, order, compareTo,
'		arrayOffset, & the limit setter method.
'-------------------------------------------------------------------------------------------


Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.My.JavaScript

Public Class ByteBuffer : Inherits DataView

    Dim mode As IOWorkModes
    Dim reader As BinaryDataReader
    Dim writer As BinaryDataWriter

    Private Sub New()
        Call Me.New(New MemoryStream)
    End Sub

    Sub New(stream As Stream)
        Call MyBase.New(stream)

        If stream.CanRead Then
            Me.reader = New BinaryDataReader(stream)
        End If
        If stream.CanWrite Then
            Me.writer = New BinaryDataWriter(stream)
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Try
            reader.Close()
            writer.Close()
            stream.Close()
            stream.Dispose()
        Finally
            MyBase.Finalize()
        End Try
    End Sub

    Public Function order(byteOrder As ByteOrder) As ByteBuffer
        If reader IsNot Nothing Then reader.ByteOrder = byteOrder
        If writer IsNot Nothing Then writer.ByteOrder = byteOrder
        Return Me
    End Function

    Public Shared Function allocate(capacity As Integer) As ByteBuffer
        Dim buffer As New ByteBuffer()

        If TypeOf buffer.stream Is MemoryStream Then
            DirectCast(buffer.stream, MemoryStream).Capacity = capacity
        End If

        buffer.mode = IOWorkModes.Write
        Return buffer
    End Function

    Public Shared Function allocateDirect(capacity As Integer) As ByteBuffer
        'this wrapper class makes no distinction between 'allocate' & 'allocateDirect'
        Return allocate(capacity)
    End Function

    Public Function capacity() As Integer
        If TypeOf stream Is MemoryStream Then
            Return DirectCast(stream, MemoryStream).Capacity
        Else
            Return stream.Length
        End If
    End Function

    Public Function flip() As ByteBuffer
        mode = IOWorkModes.Read
        stream.SetLength(stream.Position)
        stream.Position = 0
        Return Me
    End Function

    Public Function clear() As ByteBuffer
        mode = IOWorkModes.Write
        stream.Position = 0
        Return Me
    End Function

    Public Function compact() As ByteBuffer
        Dim newStream As New MemoryStream(capacity)
        mode = IOWorkModes.Write
        stream.CopyTo(newStream)
        stream = newStream
        Return Me
    End Function

    Public Function rewind() As ByteBuffer
        stream.Position = 0
        Return Me
    End Function

    Public Function limit() As Long
        If mode = IOWorkModes.Write Then
            Return capacity()
        Else
            Return stream.Length
        End If
    End Function

    Public Function position() As Long
        Return stream.Position
    End Function

    Public Function position(newPosition As Long) As ByteBuffer
        stream.Position = newPosition
        Return Me
    End Function

    Public Function remaining() As Long
        Return Me.limit() - Me.position()
    End Function

    Public Function hasRemaining() As Boolean
        Return Me.remaining() > 0
    End Function

    Public Function [get]() As Integer
        Return stream.ReadByte()
    End Function

    ''' <summary>
    ''' asDoubleBuffer
    ''' </summary>
    ''' <param name="dst"></param>
    Public Sub [get](dst() As Double)
        For i As Integer = 0 To dst.Length - 1
            dst(i) = getDouble()
        Next
    End Sub

    ''' <summary>
    ''' asLongBuffer
    ''' </summary>
    ''' <param name="dst"></param>
    Public Sub [get](dst As Long())
        For i As Integer = 0 To dst.Length - 1
            dst(i) = getLong()
        Next
    End Sub

    ''' <summary>
    ''' asIntBuffer
    ''' </summary>
    ''' <param name="dst"></param>
    Public Sub [get](dst As Integer())
        For i As Integer = 0 To dst.Length - 1
            dst(i) = getInt()
        Next
    End Sub

    ''' <summary>
    ''' asShortBuffer
    ''' </summary>
    ''' <param name="dst"></param>
    Public Sub [get](dst As Short())
        For i As Integer = 0 To dst.Length - 1
            dst(i) = getShort()
        Next
    End Sub

    Public Function [get](dst As Byte(), Optional offset As Integer = Scan0, Optional length As Integer = -1) As ByteBuffer
        If length <= 0 Then
            length = dst.Length
        End If

        stream.Read(dst, offset, length)
        Return Me
    End Function

    Public Function put(b As Byte) As ByteBuffer
        stream.WriteByte(b)
        Return Me
    End Function

    Public Function put(src As Byte(), offset As Integer, length As Integer) As ByteBuffer
        stream.Write(src, offset, length)
        Return Me
    End Function

    Public Overloads Function Equals(other As ByteBuffer) As Boolean
        If other IsNot Nothing AndAlso Me.remaining() = other.remaining() Then
            Dim thisOriginalPosition As Long = Me.position()
            Dim otherOriginalPosition As Long = other.position()

            Dim differenceFound As Boolean = False
            While stream.Position < stream.Length
                If Me.[get]() <> other.[get]() Then
                    differenceFound = True
                    Exit While
                End If
            End While

            Me.position(thisOriginalPosition)
            other.position(otherOriginalPosition)

            Return Not differenceFound
        Else
            Return False
        End If
    End Function

    'methods using the internal BinaryReader:
    Public Function getChar() As Char
        Return reader.ReadChar()
    End Function
    Public Function getChar(index As Integer) As Char
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        Dim value As Char = reader.ReadChar()
        stream.Position = originalPosition
        Return value
    End Function
    Public Function getDouble() As Double
        Return reader.ReadDouble()
    End Function
    Public Function getDouble(index As Integer) As Double
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        Dim value As Double = reader.ReadDouble()
        stream.Position = originalPosition
        Return value
    End Function
    Public Function getFloat() As Single
        Return reader.ReadSingle()
    End Function
    Public Function getFloat(index As Integer) As Single
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        Dim value As Single = reader.ReadSingle()
        stream.Position = originalPosition
        Return value
    End Function
    Public Function getInt() As Integer
        Return reader.ReadInt32()
    End Function
    Public Function getInt(index As Integer) As Integer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        Dim value As Integer = reader.ReadInt32()
        stream.Position = originalPosition
        Return value
    End Function
    Public Function getLong() As Long
        Return reader.ReadInt64()
    End Function
    Public Function getLong(index As Integer) As Long
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        Dim value As Long = reader.ReadInt64()
        stream.Position = originalPosition
        Return value
    End Function
    Public Function getShort() As Short
        Return reader.ReadInt16()
    End Function
    Public Function getShort(index As Integer) As Short
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        Dim value As Short = reader.ReadInt16()
        stream.Position = originalPosition
        Return value
    End Function

    'methods using the internal BinaryWriter:
    Public Function putChar(value As Char) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function
    Public Function putChar(index As Integer, value As Char) As ByteBuffer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function
    Public Function putDouble(value As Double) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function
    Public Function putDouble(index As Integer, value As Double) As ByteBuffer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function
    Public Function putFloat(value As Single) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function
    Public Function putFloat(index As Integer, value As Single) As ByteBuffer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function
    Public Function putInt(value As Integer) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function
    Public Function putInt(index As Integer, value As Integer) As ByteBuffer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function
    Public Function putLong(value As Long) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function
    Public Function putLong(index As Integer, value As Long) As ByteBuffer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function
    Public Function putShort(value As Short) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function
    Public Function putShort(index As Integer, value As Short) As ByteBuffer
        Dim originalPosition As Long = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function

    Public Shared Function wrap(bytes As Byte()) As ByteBuffer
        Return New ByteBuffer(New MemoryStream(bytes))
    End Function

    Public Shared Function wrap(bytes As SByte()) As ByteBuffer
        Return New ByteBuffer(New MemoryStream(CType(CObj(bytes), Byte())))
    End Function
End Class
