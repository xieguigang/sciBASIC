#Region "Microsoft.VisualBasic::9a08d2b59f74e8ac57c17b94ebd00fd4, Data\BinaryData\BinaryData\Stream\BinaryDataReader.vb"

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

    '   Total Lines: 739
    '    Code Lines: 370 (50.07%)
    ' Comment Lines: 293 (39.65%)
    '    - Xml Docs: 89.08%
    ' 
    '   Blank Lines: 76 (10.28%)
    '     File Size: 32.51 KB


    ' Class BinaryDataReader
    ' 
    '     Properties: BufferView, ByteOrder, Encoding, EndOfStream, Length
    '                 Position
    ' 
    '     Constructor: (+8 Overloads) Sub New
    ' 
    '     Function: DecimalFromBytes, getDebugView, ReadBoolean, ReadByte, ReadByteLengthPrefixString
    '               ReadBytes, ReadChar, ReadDateTime, ReadDecimal, ReadDecimals
    '               ReadDouble, ReadDoubles, ReadDwordLengthPrefixString, ReadDwordLenString, ReadInt16
    '               ReadInt16s, ReadInt32, ReadInt32s, ReadInt64, ReadInt64s
    '               ReadMultiple, ReadSByte, ReadSBytes, ReadSingle, ReadSingles
    '               (+5 Overloads) ReadString, ReadUInt16, ReadUInt16s, ReadUInt32, ReadUInt32s
    '               ReadUInt64, ReadUInt64s, ReadWordLengthPrefixString, ReadZeroTerminatedString, (+2 Overloads) Seek
    '               (+3 Overloads) TemporarySeek, ToString
    ' 
    '     Sub: Align, Mark, Reset, TemporarySeek
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Represents an extended <see cref="BinaryReader"/> supporting special file format data types.
''' </summary>
Public Class BinaryDataReader : Inherits BinaryReader
    Implements IReaderDebugAccess

    Dim _byteOrder As ByteOrder
    Dim _needsReversion As Boolean
    Dim _markedPos As Long

    ''' <summary>
    ''' [debug view]以ascii显示当前位置的附近16个字节的内容
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BufferView As String
        Get
            Return getDebugView(128)
        End Get
    End Property

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream and
    ''' using UTF-8 encoding.
    ''' </summary>
    ''' <param name="input">The input stream.</param>
    ''' <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
    ''' </exception>
    Public Sub New(input As Stream)
        Me.New(input, New UTF8Encoding(), False)
    End Sub

    ''' <summary>
    ''' Create binary data reader from the in-memory data buffer
    ''' </summary>
    ''' <param name="data"></param>
    Sub New(data As IEnumerable(Of Byte))
        Call Me.New(New MemoryStream(data.ToArray))
    End Sub

    Sub New(data As RequestStream)
        Call Me.New(New MemoryStream(data.ChunkBuffer))
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream, UTF-8
    ''' encoding and optionally leaves the stream open.
    ''' </summary>
    ''' <param name="input">The input stream.</param>
    ''' <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataReader"/> object
    ''' is disposed; otherwise <c>false</c>.</param>
    ''' <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
    ''' </exception>
    ''' <exception cref="ArgumentNullException">encoding is null.</exception>
    Public Sub New(input As Stream, leaveOpen As Boolean)
        Me.New(input, New UTF8Encoding(), leaveOpen)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream and
    ''' character encoding.
    ''' </summary>
    ''' <param name="input">The input stream.</param>
    ''' <param name="encoding">The character encoding to use.</param>
    ''' <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
    ''' </exception>
    ''' <exception cref="ArgumentNullException">encoding is null.</exception>
    Public Sub New(input As Stream, encoding As Encoding)
        Me.New(input, encoding, False)
    End Sub

    Public Sub New(input As Stream, Optional encoding As Encodings = Encodings.UTF8)
        Me.New(input, encoding.CodePage)
    End Sub

    ''' <summary>
    ''' the constructor works for the numeric stream
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="byteOrder"></param>
    Sub New(input As Stream, byteOrder As ByteOrder)
        Call Me.New(input, Encodings.UTF8)
        ' works for the numeric data
        Me.ByteOrder = byteOrder
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream and
    ''' character encoding, and optionally leaves the stream open.
    ''' </summary>
    ''' <param name="input">The input stream.</param>
    ''' <param name="encoding">The character encoding to use.</param>
    ''' <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataReader"/> object
    ''' is disposed; otherwise <c>false</c>.</param>
    ''' <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
    ''' </exception>
    ''' <exception cref="ArgumentNullException">encoding is null.</exception>
    Public Sub New(input As Stream, encoding As Encoding, leaveOpen As Boolean)
        MyBase.New(input, encoding, leaveOpen)
        Me.Encoding = encoding
        ByteOrder = ByteOrderHelper.SystemByteOrder
    End Sub

    ''' <summary>
    ''' Gets or sets the byte order used to parse binary data with.
    ''' </summary>
    Public Property ByteOrder() As ByteOrder
        Get
            Return _byteOrder
        End Get
        Set
            _byteOrder = Value
            _needsReversion = ByteOrderHelper.NeedsReversion(Value)
        End Set
    End Property

    ''' <summary>
    ''' Gets the encoding used for string related operations where no other encoding has been provided. Due to the
    ''' way the underlying <see cref="BinaryReader"/> is instantiated, it can only be specified at creation time.
    ''' </summary>
    Public Property Encoding() As Encoding

    ''' <summary>
    ''' Gets the length in bytes of the stream in bytes. This is a shortcut to the base stream Length property.
    ''' </summary>
    Public ReadOnly Property Length() As Long Implements IReaderDebugAccess.Length
        Get
            Return BaseStream.Length
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the position within the current stream. This is a shortcut to the base stream Position
    ''' property.
    ''' </summary>
    Public Property Position() As Long Implements IReaderDebugAccess.Position
        Get
            Return BaseStream.Position
        End Get
        Set
            BaseStream.Position = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets a value indicating whether the end of the stream has been reached and no more data can be read.
    ''' </summary>
    Public ReadOnly Property EndOfStream() As Boolean
        Get
            Return BaseStream.Position >= BaseStream.Length
        End Get
    End Property

    Private Function getDebugView(bufSize As Integer) As String
        Using TemporarySeek()
            Return Helpers.getDebugView(Me, bufSize)
        End Using
    End Function

    ''' <summary>
    ''' Mark current stream buffer position
    ''' </summary>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Mark()
        _markedPos = Position
    End Sub

    ''' <summary>
    ''' Move the buffer back to the position that marked by <see cref="Mark"/> method.
    ''' </summary>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Reset()
        Position = _markedPos
    End Sub

    ''' <summary>
    ''' Aligns the reader to the next given byte multiple.
    ''' </summary>
    ''' <param name="alignment">The byte multiple.</param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Align(alignment As Integer)
        Seek((-Position Mod alignment + alignment) Mod alignment)
    End Sub

#Region "Bind Base"

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Bind(TypeCode.Boolean)>
    Public Overrides Function ReadBoolean() As Boolean
        Return MyBase.ReadBoolean()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Bind(TypeCode.Char)>
    Public Overrides Function ReadChar() As Char
        Return MyBase.ReadChar()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Bind(TypeCode.Byte)>
    Public Overrides Function ReadByte() As Byte
        Return MyBase.ReadByte()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Bind(TypeCode.SByte)>
    Public Overrides Function ReadSByte() As SByte
        Return MyBase.ReadSByte()
    End Function
#End Region

    ''' <summary>
    ''' Reads a <see cref="DateTime"/> from the current stream. The <see cref="DateTime"/> is available in the
    ''' specified binary format.
    ''' </summary>
    ''' <param name="format">The binary format, in which the <see cref="DateTime"/> will be read.</param>
    ''' <returns>The <see cref="DateTime"/> read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.DateTime, BinaryDateTimeFormat.CTime)>
    Public Function ReadDateTime(format As BinaryDateTimeFormat) As DateTime
        Select Case format
            Case BinaryDateTimeFormat.CTime
                Return New DateTime(1970, 1, 1).ToLocalTime().AddSeconds(ReadUInt32())
            Case BinaryDateTimeFormat.NetTicks
                Return New DateTime(ReadInt64())
            Case Else
                Throw New ArgumentOutOfRangeException("format", "The specified binary datetime format is invalid")
        End Select
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ReadBytes(count As Integer) As Byte() Implements IReaderDebugAccess.ReadBytes
        Return MyBase.ReadBytes(count)
    End Function

    ''' <summary>
    ''' Reads an 16-byte floating point value from the current stream and advances the current position of the
    ''' stream by sixteen bytes.
    ''' </summary>
    ''' <returns>The 16-byte floating point value read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.Decimal)>
    Public Overrides Function ReadDecimal() As Decimal
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(Marshal.SizeOf(GetType(Decimal)))
            Array.Reverse(bytes)
            Return DecimalFromBytes(bytes)
        Else
            Return MyBase.ReadDecimal()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="Decimal"/> values from the current stream into a
    ''' <see cref="Decimal"/> array and advances the current position by that number of <see cref="Decimal"/> values
    ''' multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="Decimal"/> values to read.</param>
    ''' <returns>The <see cref="Decimal"/> array containing data read from the current stream. This might be less
    ''' than the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadDecimals(count As Integer) As Decimal()
        Return ReadMultiple(count, AddressOf ReadDecimal)
    End Function

    ''' <summary>
    ''' Reads an 8-byte floating point value from the current stream and advances the current position of the stream
    ''' by eight bytes.
    ''' </summary>
    ''' <returns>The 8-byte floating point value read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.Double)>
    Public Overrides Function ReadDouble() As Double
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(8)
            Array.Reverse(bytes)
            Return BitConverter.ToDouble(bytes, 0)
        Else
            Return MyBase.ReadDouble()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="Double"/> values from the current stream into a
    ''' <see cref="Double"/> array and advances the current position by that number of <see cref="Double"/> values
    ''' multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="Double"/> values to read.</param>
    ''' <returns>The <see cref="Double"/> array containing data read from the current stream. This might be less
    ''' than the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadDoubles(count As Integer) As Double()
        Return ReadMultiple(count, AddressOf ReadDouble)
    End Function

    ''' <summary>
    ''' Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two
    ''' bytes.
    ''' </summary>
    ''' <returns>The 2-byte signed integer read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.Int16)>
    Public Overrides Function ReadInt16() As Int16
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(2)
            Array.Reverse(bytes)
            Return BitConverter.ToInt16(bytes, 0)
        Else
            Return MyBase.ReadInt16()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="Int16"/> values from the current stream into a <see cref="Int16"/>
    ''' array and advances the current position by that number of <see cref="Int16"/> values multiplied with the
    ''' size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="Int16"/> values to read.</param>
    ''' <returns>The <see cref="Int16"/> array containing data read from the current stream. This might be less than
    ''' the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadInt16s(count As Integer) As Int16()
        Return ReadMultiple(count, AddressOf ReadInt16)
    End Function

    ''' <summary>
    ''' Reads a 4-byte signed integer from the current stream and advances the current position of the stream by
    ''' four bytes.
    ''' </summary>
    ''' <returns>The 4-byte signed integer read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.Int32)>
    Public Overrides Function ReadInt32() As Int32
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(4)
            Array.Reverse(bytes)
            Return BitConverter.ToInt32(bytes, 0)
        Else
            Return MyBase.ReadInt32()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="Int32"/> values from the current stream into a <see cref="Int32"/>
    ''' array and advances the current position by that number of <see cref="Int32"/> values multiplied with the
    ''' size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="Int32"/> values to read.</param>
    ''' <returns>The <see cref="Int32"/> array containing data read from the current stream. This might be less than
    ''' the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadInt32s(count As Integer) As Int32()
        Return ReadMultiple(count, AddressOf ReadInt32)
    End Function

    ''' <summary>
    ''' Reads an 8-byte signed integer from the current stream and advances the current position of the stream by
    ''' eight bytes.
    ''' </summary>
    ''' <returns>The 8-byte signed integer read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.Int64)>
    Public Overrides Function ReadInt64() As Int64
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(8)
            Array.Reverse(bytes)
            Return BitConverter.ToInt64(bytes, 0)
        Else
            Return MyBase.ReadInt64()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="Int64"/> values from the current stream into a <see cref="Int64"/>
    ''' array and advances the current position by that number of <see cref="Int64"/> values multiplied with the
    ''' size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="Int64"/> values to read.</param>
    ''' <returns>The <see cref="Int64"/> array containing data read from the current stream. This might be less than
    ''' the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadInt64s(count As Integer) As Int64()
        Return ReadMultiple(count, AddressOf ReadInt64)
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="SByte"/> values from the current stream into a <see cref="SByte"/>
    ''' array and advances the current position by that number of <see cref="SByte"/> values multiplied with the
    ''' size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="SByte"/> values to read.</param>
    ''' <returns>The <see cref="SByte"/> array containing data read from the current stream. This might be less than
    ''' the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadSBytes(count As Integer) As SByte()
        Return ReadMultiple(count, AddressOf ReadSByte)
    End Function

    ''' <summary>
    ''' Reads a 4-byte floating point value from the current stream and advances the current position of the stream
    ''' by four bytes.
    ''' </summary>
    ''' <returns>The 4-byte floating point value read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.Single)>
    Public Overrides Function ReadSingle() As Single
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(4)
            Array.Reverse(bytes)
            Return BitConverter.ToSingle(bytes, 0)
        Else
            Return MyBase.ReadSingle()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="Single"/> values from the current stream into a
    ''' <see cref="Single"/> array and advances the current position by that number of <see cref="Single"/> values
    ''' multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="Single"/> values to read.</param>
    ''' <returns>The <see cref="Single"/> array containing data read from the current stream. This might be less
    ''' than the number of bytes requested if the end of the stream is reached.</returns>
    Public Function ReadSingles(count As Integer) As Single()
        Return ReadMultiple(count, AddressOf ReadSingle)
    End Function

    ''' <summary>
    ''' Reads a string from the current stream. The string is available in the specified binary format.
    ''' </summary>
    ''' <param name="format">The binary format, in which the string will be read.</param>
    ''' <returns>The string read from the current stream.</returns>
    ''' 
    Public Overloads Function ReadString(format As BinaryStringFormat) As String
        Return ReadString(format, Encoding)
    End Function

    ''' <summary>
    ''' Reads a string from the current stream. The string is available in the specified binary format and encoding.
    ''' </summary>
    ''' <param name="format">The binary format, in which the string will be read.</param>
    ''' <param name="encoding">The encoding used for converting the string.</param>
    ''' <returns>The string read from the current stream.</returns>
    Public Overloads Function ReadString(format As BinaryStringFormat, encoding As Encoding) As String
        Select Case format
            Case BinaryStringFormat.ByteLengthPrefix
                Return ReadByteLengthPrefixString(encoding)
            Case BinaryStringFormat.WordLengthPrefix
                Return ReadWordLengthPrefixString(encoding)
            Case BinaryStringFormat.DwordLengthPrefix
                Return ReadDwordLengthPrefixString(encoding)
            Case BinaryStringFormat.ZeroTerminated
                Return ReadZeroTerminatedString(encoding)
            Case BinaryStringFormat.UInt32LengthPrefix
                Return ReadString(ReadUInt32, encoding)
            Case BinaryStringFormat.NoPrefixOrTermination
                Throw New ArgumentException("NoPrefixOrTermination cannot be used for read operations if no length has been specified.", "format")
            Case Else
                Throw New ArgumentOutOfRangeException("format", "The specified binary string format is invalid")
        End Select
    End Function

    ''' <summary>
    ''' Reads a string from the current stream. The string has neither a prefix or postfix, the length has to be
    ''' specified manually.
    ''' </summary>
    ''' <param name="length">The length of the string.</param>
    ''' <returns>The string read from the current stream.</returns>
    Public Overloads Function ReadString(length As Integer) As String
        Return ReadString(length, Encoding)
    End Function

    ''' <summary>
    ''' Reads a string from the current stream. The string has neither a prefix or postfix, the length has to be
    ''' specified manually. The string is available in the specified encoding.
    ''' </summary>
    ''' <param name="length">The length of the string.</param>
    ''' <param name="encoding">The encoding to use for reading the string.</param>
    ''' <returns>The string read from the current stream.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Function ReadString(length%, encoding As Encoding) As String
        Return encoding.GetString(ReadBytes(length))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Function ReadString(length As UInteger, encoding As Encoding) As String
        Return encoding.GetString(ReadBytes(CInt(length)))
    End Function

    ''' <summary>
    ''' Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the
    ''' position of the stream by two bytes.
    ''' </summary>
    ''' <returns>The 2-byte unsigned integer read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.UInt16)>
    Public Overrides Function ReadUInt16() As UInt16
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(2)
            Array.Reverse(bytes)
            Return BitConverter.ToUInt16(bytes, 0)
        Else
            Return MyBase.ReadUInt16()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="UInt16"/> values from the current stream into a
    ''' <see cref="UInt16"/> array and advances the current position by that number of <see cref="UInt16"/> values
    ''' multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="UInt16"/> values to read.</param>
    ''' <returns>The <see cref="UInt16"/> array containing data read from the current stream. This might be less
    ''' than the number of bytes requested if the end of the stream is reached.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ReadUInt16s(count As Integer) As UInt16()
        Return ReadMultiple(count, AddressOf ReadUInt16)
    End Function

    ''' <summary>
    ''' Reads an 4-byte unsigned integer from the current stream and advances the position of the stream by eight
    ''' bytes.
    ''' </summary>
    ''' <returns>The 4-byte unsigned integer read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.UInt32)>
    Public Overrides Function ReadUInt32() As UInt32
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(4)
            Array.Reverse(bytes)
            Return BitConverter.ToUInt32(bytes, 0)
        Else
            Return MyBase.ReadUInt32()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="UInt32"/> values from the current stream into a
    ''' <see cref="UInt32"/> array and advances the current position by that number of <see cref="UInt32"/> values
    ''' multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="UInt32"/> values to read.</param>
    ''' <returns>The <see cref="UInt32"/> array containing data read from the current stream. This might be less
    ''' than the number of bytes requested if the end of the stream is reached.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ReadUInt32s(count As Integer) As UInt32()
        Return ReadMultiple(count, AddressOf ReadUInt32)
    End Function

    ''' <summary>
    ''' Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight
    ''' bytes.
    ''' </summary>
    ''' <returns>The 8-byte unsigned integer read from the current stream.</returns>
    ''' 
    <Bind(TypeCode.UInt64)>
    Public Overrides Function ReadUInt64() As UInt64
        If _needsReversion Then
            Dim bytes As Byte() = MyBase.ReadBytes(8)
            Array.Reverse(bytes)
            Return BitConverter.ToUInt64(bytes, 0)
        Else
            Return MyBase.ReadUInt64()
        End If
    End Function

    ''' <summary>
    ''' Reads the specified number of <see cref="UInt64"/> values from the current stream into a
    ''' <see cref="UInt64"/> array and advances the current position by that number of <see cref="UInt64"/> values
    ''' multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="count">The number of <see cref="UInt64"/> values to read.</param>
    ''' <returns>The <see cref="UInt64"/> array containing data read from the current stream. This might be less
    ''' than the number of bytes requested if the end of the stream is reached.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ReadUInt64s(count As Integer) As UInt64()
        Return ReadMultiple(count, AddressOf ReadUInt64)
    End Function

    ''' <summary>
    ''' Sets the position within the current stream. This is a shortcut to the base stream Seek method.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the current position in the stream.</param>
    ''' <returns>The new position within the current stream.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Seek(offset As Long) As Long
        Return Seek(offset, SeekOrigin.Current)
    End Function

    ''' <summary>
    ''' Sets the position within the current stream. This is a shortcut to the base stream Seek method.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the origin parameter.</param>
    ''' <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
    ''' the new position.</param>
    ''' <returns>The new position within the current stream.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Seek(offset As Long, origin As SeekOrigin) As Long
        Return BaseStream.Seek(offset, origin)
    End Function

    ''' <summary>
    ''' Creates a <see cref="SeekTask"/> to restore the current position after it has been disposed.
    ''' </summary>
    ''' <returns>The <see cref="SeekTask"/> to be disposed to restore to the current position.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function TemporarySeek() As SeekTask
        Return TemporarySeek(0, SeekOrigin.Current)
    End Function

    Public Sub TemporarySeek(offset As Integer, origin As SeekOrigin, action As Action)
        Dim current As Long = Position

        Call Seek(offset, origin)
        Call action()
        Call Seek(current, SeekOrigin.Begin)
    End Sub

    ''' <summary>
    ''' Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
    ''' is disposed, the previous stream position will be restored.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the current position in the stream.</param>
    ''' <returns>The <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function TemporarySeek(offset As Long) As SeekTask
        Return TemporarySeek(offset, SeekOrigin.Current)
    End Function

    ''' <summary>
    ''' Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
    ''' is disposed, the previous stream position will be restored.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the origin parameter.</param>
    ''' <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
    ''' the new position.</param>
    ''' <returns>The <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function TemporarySeek(offset As Long, origin As SeekOrigin) As SeekTask
        Return New SeekTask(BaseStream, offset, origin)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ReadDwordLenString() As String
        Return ReadString(BinaryStringFormat.DwordLengthPrefix)
    End Function

#Region "METHODS (PRIVATE)"

    Private Function ReadMultiple(Of T)(count As Integer, readFunc As Func(Of T)) As T()
        Dim values As T() = New T(count - 1) {}
        For i As Integer = 0 To values.Length - 1
            values(i) = readFunc.Invoke()
        Next
        Return values
    End Function

    Private Function ReadByteLengthPrefixString(encoding As Encoding) As String
        Dim length As Integer = ReadByte()

        ' This will not work for strings with differently sized characters depending on their code.
        Dim charSize As Integer = encoding.GetByteCount("a")

        Return encoding.GetString(ReadBytes(length * charSize))
    End Function

    Private Function ReadWordLengthPrefixString(encoding As Encoding) As String
        Dim length As Integer = ReadInt16()

        ' This will not work for strings with differently sized characters depending on their code.
        Dim charSize As Integer = encoding.GetByteCount("a")

        Return encoding.GetString(ReadBytes(length * charSize))
    End Function

    Private Function ReadDwordLengthPrefixString(encoding As Encoding) As String
        Dim length As Integer = ReadInt32()

        ' This will not work for strings with differently sized characters depending on their code.
        Dim charSize As Integer = encoding.GetByteCount("a")

        Return encoding.GetString(ReadBytes(length * charSize))
    End Function

    Private Function ReadZeroTerminatedString(encoding As Encoding) As String
        ' This will not work for strings with differently sized characters depending on their code.
        Dim charSize As Integer = encoding.GetByteCount("a")

        Dim bytes As New List(Of Byte)()
        If charSize = 1 Then
            ' Read single bytes.
            Dim readByte As Byte = MyBase.ReadByte()
            While readByte <> 0
                bytes.Add(readByte)
                readByte = MyBase.ReadByte()
            End While
        ElseIf charSize = Marshal.SizeOf(GetType(UShort)) Then
            ' Read ushort values with 2 bytes width.
            Dim readUShort As UInteger = ReadUInt16()
            While readUShort <> 0
                Dim ushortBytes As Byte() = BitConverter.GetBytes(readUShort)
                bytes.Add(ushortBytes(0))
                bytes.Add(ushortBytes(1))
                readUShort = ReadUInt16()
            End While
        End If

        ' Convert to string.
        Return encoding.GetString(bytes.ToArray())
    End Function

    Private Function DecimalFromBytes(bytes As Byte()) As Decimal
        If bytes.Length < Marshal.SizeOf(GetType(Decimal)) Then
            Throw New ArgumentException("Not enough bytes to convert decimal from.")
        End If

        ' Create 4 integers from the given bytes.
        Dim intValues As Integer() = New Integer(Marshal.SizeOf(GetType(Decimal)) / Marshal.SizeOf(GetType(Integer)) - 1) {}
        Dim i As Integer = 0
        While i < Marshal.SizeOf(GetType(Decimal))
            intValues(i / Marshal.SizeOf(GetType(Integer))) = BitConverter.ToInt32(bytes, i)
            i += Marshal.SizeOf(GetType(Integer))
        End While
        Return New Decimal(intValues)
    End Function
#End Region

    Public Overrides Function ToString() As String
        Return $"[{Position}/{Length}] {Encoding.ToString} [debug_buffer: {getDebugView(32)}]"
    End Function
End Class
