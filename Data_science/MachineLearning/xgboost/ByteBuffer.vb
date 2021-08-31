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
Public Class ByteBuffer
    ''Mode' is only used to determine whether to return data length or capacity from the 'limit' method:
    Private Enum Mode
        Read
        Write
    End Enum

    Private modeField As Mode
    Private stream As MemoryStream
    Private reader As BinaryReader
    Private writer As BinaryWriter

    Private Sub New()
        stream = New MemoryStream()
        reader = New BinaryReader(stream)
        writer = New BinaryWriter(stream)
    End Sub

    Protected Overrides Sub Finalize()
        reader.Close()
        writer.Close()
        stream.Close()
        stream.Dispose()
    End Sub

    Public Shared Function allocate(ByVal capacity As Integer) As ByteBuffer
        Dim buffer As ByteBuffer = New ByteBuffer()
        buffer.stream.Capacity = capacity
        buffer.modeField = Mode.Write
        Return buffer
    End Function

    Public Shared Function allocateDirect(ByVal capacity As Integer) As ByteBuffer
        'this wrapper class makes no distinction between 'allocate' & 'allocateDirect'
        Return allocate(capacity)
    End Function

    Public Function capacity() As Integer
        Return stream.Capacity
    End Function

    Public Function flip() As ByteBuffer
        modeField = Mode.Read
        stream.SetLength(stream.Position)
        stream.Position = 0
        Return Me
    End Function

    Public Function clear() As ByteBuffer
        modeField = Mode.Write
        stream.Position = 0
        Return Me
    End Function

    Public Function compact() As ByteBuffer
        modeField = Mode.Write
        Dim newStream As MemoryStream = New MemoryStream(stream.Capacity)
        stream.CopyTo(newStream)
        stream = newStream
        Return Me
    End Function

    Public Function rewind() As ByteBuffer
        stream.Position = 0
        Return Me
    End Function

    Public Function limit() As Long
        If modeField = Mode.Write Then
            Return stream.Capacity
        Else
            Return stream.Length
        End If
    End Function

    Public Function position() As Long
        Return stream.Position
    End Function

    Public Function position(ByVal newPosition As Long) As ByteBuffer
        stream.Position = newPosition
        Return Me
    End Function

    Public Function remaining() As Long
        Return limit() - position()
    End Function

    Public Function hasRemaining() As Boolean
        Return remaining() > 0
    End Function

    Public Function [get]() As Integer
        Return stream.ReadByte()
    End Function

    Public Function [get](ByVal dst As Byte(), ByVal offset As Integer, ByVal length As Integer) As ByteBuffer
        stream.Read(dst, offset, length)
        Return Me
    End Function

    Public Function put(ByVal b As Byte) As ByteBuffer
        stream.WriteByte(b)
        Return Me
    End Function

    Public Function put(ByVal src As Byte(), ByVal offset As Integer, ByVal length As Integer) As ByteBuffer
        stream.Write(src, offset, length)
        Return Me
    End Function

    Public Function Equals(ByVal other As ByteBuffer) As Boolean
        If other IsNot Nothing AndAlso remaining() = other.remaining() Then
            Dim thisOriginalPosition As Long = position()
            Dim otherOriginalPosition As Long = other.position()
            Dim differenceFound = False

            While stream.Position < stream.Length

                If [get]() <> other.get() Then
                    differenceFound = True
                    Exit While
                End If
            End While

            position(thisOriginalPosition)
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

    Public Function getChar(ByVal index As Integer) As Char
        Dim originalPosition = stream.Position
        stream.Position = index
        Dim value As Char = reader.ReadChar()
        stream.Position = originalPosition
        Return value
    End Function

    Public Function getDouble() As Double
        Return reader.ReadDouble()
    End Function

    Public Function getDouble(ByVal index As Integer) As Double
        Dim originalPosition = stream.Position
        stream.Position = index
        Dim value As Double = reader.ReadDouble()
        stream.Position = originalPosition
        Return value
    End Function

    Public Function getFloat() As Single
        Return reader.ReadSingle()
    End Function

    Public Function getFloat(ByVal index As Integer) As Single
        Dim originalPosition = stream.Position
        stream.Position = index
        Dim value As Single = reader.ReadSingle()
        stream.Position = originalPosition
        Return value
    End Function

    Public Function getInt() As Integer
        Return reader.ReadInt32()
    End Function

    Public Function getInt(ByVal index As Integer) As Integer
        Dim originalPosition = stream.Position
        stream.Position = index
        Dim value As Integer = reader.ReadInt32()
        stream.Position = originalPosition
        Return value
    End Function

    Public Function getLong() As Long
        Return reader.ReadInt64()
    End Function

    Public Function getLong(ByVal index As Integer) As Long
        Dim originalPosition = stream.Position
        stream.Position = index
        Dim value As Long = reader.ReadInt64()
        stream.Position = originalPosition
        Return value
    End Function

    Public Function getShort() As Short
        Return reader.ReadInt16()
    End Function

    Public Function getShort(ByVal index As Integer) As Short
        Dim originalPosition = stream.Position
        stream.Position = index
        Dim value As Short = reader.ReadInt16()
        stream.Position = originalPosition
        Return value
    End Function

    'methods using the internal BinaryWriter:
    Public Function putChar(ByVal value As Char) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function

    Public Function putChar(ByVal index As Integer, ByVal value As Char) As ByteBuffer
        Dim originalPosition = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function

    Public Function putDouble(ByVal value As Double) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function

    Public Function putDouble(ByVal index As Integer, ByVal value As Double) As ByteBuffer
        Dim originalPosition = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function

    Public Function putFloat(ByVal value As Single) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function

    Public Function putFloat(ByVal index As Integer, ByVal value As Single) As ByteBuffer
        Dim originalPosition = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function

    Public Function putInt(ByVal value As Integer) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function

    Public Function putInt(ByVal index As Integer, ByVal value As Integer) As ByteBuffer
        Dim originalPosition = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function

    Public Function putLong(ByVal value As Long) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function

    Public Function putLong(ByVal index As Integer, ByVal value As Long) As ByteBuffer
        Dim originalPosition = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function

    Public Function putShort(ByVal value As Short) As ByteBuffer
        writer.Write(value)
        Return Me
    End Function

    Public Function putShort(ByVal index As Integer, ByVal value As Short) As ByteBuffer
        Dim originalPosition = stream.Position
        stream.Position = index
        writer.Write(value)
        stream.Position = originalPosition
        Return Me
    End Function
End Class
