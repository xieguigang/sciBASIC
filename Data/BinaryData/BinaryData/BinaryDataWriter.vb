#Region "Microsoft.VisualBasic::637d25620ecab24f42badd17c20d7703, ..\sciBASIC#\Data\BinaryData\BinaryData\BinaryDataWriter.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Represents an extended <see cref="BinaryWriter"/> supporting special file format data types.
''' </summary>
Public Class BinaryDataWriter
    Inherits BinaryWriter

    Dim _byteOrder As ByteOrder
    Dim _needsReversion As Boolean

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream and
    ''' using UTF-8 encoding.
    ''' </summary>
    ''' <param name="output">The output stream.</param>
    ''' <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
    ''' <exception cref="ArgumentNullException">output is null.</exception>
    Public Sub New(output As Stream)
        Me.New(output, New UTF8Encoding(), False)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream, UTF-8
    ''' encoding and optionally leaves the stream open.
    ''' </summary>
    ''' <param name="output">The output stream.</param>
    ''' <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataWriter"/> object
    ''' is disposed; otherwise <c>false</c>.</param>
    ''' <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
    ''' <exception cref="ArgumentNullException">output is null.</exception>
    Public Sub New(output As Stream, leaveOpen As Boolean)
        Me.New(output, New UTF8Encoding(), leaveOpen)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream and
    ''' character encoding.
    ''' </summary>
    ''' <param name="output">The output stream.</param>
    ''' <param name="encoding">The character encoding to use.</param>
    ''' <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
    ''' <exception cref="ArgumentNullException">output or encoding is null.</exception>
    Public Sub New(output As Stream, encoding As Encoding)
        Me.New(output, encoding, False)
    End Sub

    Public Sub New(output As Stream, Optional encoding As Encodings = Encodings.UTF8)
        Me.New(output, encoding.GetEncodings)
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream and
    ''' character encoding, and optionally leaves the stream open.
    ''' </summary>
    ''' <param name="output">The output stream.</param>
    ''' <param name="encoding__1">The character encoding to use.</param>
    ''' <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataWriter"/> object
    ''' is disposed; otherwise <c>false</c>.</param>
    ''' <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
    ''' <exception cref="ArgumentNullException">output or encoding is null.</exception>
    Public Sub New(output As Stream, encoding__1 As Encoding, leaveOpen As Boolean)
        MyBase.New(output, encoding__1, leaveOpen)
        Encoding = encoding__1
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
            _needsReversion = _byteOrder <> ByteOrderHelper.SystemByteOrder
        End Set
    End Property

    ''' <summary>
    ''' Gets the encoding used for string related operations where no other encoding has been provided. Due to the
    ''' way the underlying <see cref="BinaryWriter"/> is instantiated, it can only be specified at creation time.
    ''' </summary>
    Public Property Encoding() As Encoding
    ''' <summary>
    ''' Gets or sets the position within the current stream. This is a shortcut to the base stream Position
    ''' property.
    ''' </summary>
    Public Property Position() As Long
        Get
            Return BaseStream.Position
        End Get
        Set
            BaseStream.Position = Value
        End Set
    End Property

    ''' <summary>
    ''' Allocates space for an <see cref="Offset"/> which can be satisfied later on.
    ''' </summary>
    ''' <returns>An <see cref="Offset"/> to satisfy later on.</returns>
    Public Function ReserveOffset() As Offset
        Return New Offset(Me)
    End Function

    ''' <summary>
    ''' Aligns the reader to the next given byte multiple..
    ''' </summary>
    ''' <param name="alignment">The byte multiple.</param>
    Public Sub Align(alignment As Integer)
        Seek((-Position Mod alignment + alignment) Mod alignment)
    End Sub

    ''' <summary>
    ''' Sets the position within the current stream. This is a shortcut to the base stream Seek method.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the current position in the stream.</param>
    ''' <returns>The new position within the current stream.</returns>
    Public Overloads Function Seek(offset As Long) As Long
        Return Seek(offset, SeekOrigin.Current)
    End Function

    ''' <summary>
    ''' Sets the position within the current stream. This is a shortcut to the base stream Seek method.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the origin parameter.</param>
    ''' <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
    ''' the new position.</param>
    ''' <returns>The new position within the current stream.</returns>
    Public Overloads Function Seek(offset As Long, origin As SeekOrigin) As Long
        Return BaseStream.Seek(offset, origin)
    End Function

    ''' <summary>
    ''' Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
    ''' is disposed, the previous stream position will be restored.
    ''' </summary>
    ''' <param name="offset">A byte offset relative to the current position in the stream.</param>
    ''' <returns>A <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
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
    ''' <returns>A <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
    Public Function TemporarySeek(offset As Long, origin As SeekOrigin) As SeekTask
        Return New SeekTask(BaseStream, offset, origin)
    End Function

    ''' <summary>
    ''' Writes a <see cref="DateTime"/> to this stream. The <see cref="DateTime"/> will be available in the
    ''' specified binary format.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    ''' <param name="format">The binary format in which the <see cref="DateTime"/> will be written.</param>
    Public Overloads Sub Write(value As DateTime, format As BinaryDateTimeFormat)
        Select Case format
            Case BinaryDateTimeFormat.CTime
                Write(CUInt((New DateTime(1970, 1, 1) - value.ToLocalTime()).TotalSeconds))

            Case BinaryDateTimeFormat.NetTicks
                Write(value.Ticks)

            Case Else
                Throw New ArgumentOutOfRangeException("format", "The specified binary datetime format is invalid.")
        End Select
    End Sub

    ''' <summary>
    ''' Writes an 16-byte floating point value to this stream and advances the current position of the stream by
    ''' sixteen bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As Decimal)
        If _needsReversion Then
            Dim bytes As Byte() = DecimalToBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="Decimal"/> values into the current stream and advances the current
    ''' position by that number of <see cref="Decimal"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="Decimal"/> values to write.</param>
    Public Overloads Sub Write(values As Decimal())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 8-byte floating point value to this stream and advances the current position of the stream by
    ''' eight bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As Double)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="Double"/> values into the current stream and advances the current
    ''' position by that number of <see cref="Double"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="Double"/> values to write.</param>
    Public Overloads Sub Write(values As Double())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 2-byte signed integer to this stream and advances the current position of the stream by two bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As Int16)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="Int16"/> values into the current stream and advances the current
    ''' position by that number of <see cref="Int16"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="Int16"/> values to write.</param>
    Public Overloads Sub Write(values As Int16())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 4-byte signed integer to this stream and advances the current position of the stream by four
    ''' bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As Int32)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="Int32"/> values into the current stream and advances the current
    ''' position by that number of <see cref="Int32"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="Int32"/> values to write.</param>
    Public Overloads Sub Write(values As Int32())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 8-byte signed integer to this stream and advances the current position of the stream by eight
    ''' bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As Int64)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="Int64"/> values into the current stream and advances the current
    ''' position by that number of <see cref="Int64"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="Int64"/> values to write.</param>
    Public Overloads Sub Write(values As Int64())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 4-byte floating point value to this stream and advances the current position of the stream by four
    ''' bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As Single)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="Single"/> values into the current stream and advances the current
    ''' position by that number of <see cref="Single"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="Single"/> values to write.</param>
    Public Overloads Sub Write(values As Single())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes a string to this stream in the current encoding of the <see cref="BinaryDataWriter"/> and advances
    ''' the current position of the stream in accordance with the encoding used and the specific characters being
    ''' written to the stream. The string will be available in the specified binary format.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    ''' <param name="format">The binary format in which the string will be written.</param>
    Public Overloads Sub Write(value As String, format As BinaryStringFormat)
        Write(value, format, Encoding)
    End Sub

    ''' <summary>
    ''' Writes a string to this stream with the given encoding and advances the current position of the stream in
    ''' accordance with the encoding used and the specific characters being written to the stream. The string will
    ''' be available in the specified binary format.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    ''' <param name="format">The binary format in which the string will be written.</param>
    ''' <param name="encoding">The encoding used for converting the string.</param>
    Public Overloads Sub Write(value As String, format As BinaryStringFormat, encoding As Encoding)
        Select Case format
            Case BinaryStringFormat.ByteLengthPrefix
                WriteByteLengthPrefixString(value, encoding)

            Case BinaryStringFormat.WordLengthPrefix
                WriteWordLengthPrefixString(value, encoding)

            Case BinaryStringFormat.DwordLengthPrefix
                WriteDwordLengthPrefixString(value, encoding)

            Case BinaryStringFormat.ZeroTerminated
                WriteZeroTerminatedString(value, encoding)

            Case BinaryStringFormat.NoPrefixOrTermination
                WriteNoPrefixOrTerminationString(value, encoding)

            Case Else
                Throw New ArgumentOutOfRangeException("format", "The specified binary string format is invalid")
        End Select
    End Sub

    ''' <summary>
    ''' Writes an 2-byte unsigned integer value to this stream and advances the current position of the stream by
    ''' two bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As UInt16)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="UInt16"/> values into the current stream and advances the current
    ''' position by that number of <see cref="UInt16"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="UInt16"/> values to write.</param>
    Public Overloads Sub Write(values As UInt16())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 4-byte unsigned integer value to this stream and advances the current position of the stream by
    ''' four bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As UInt32)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="UInt32"/> values into the current stream and advances the current
    ''' position by that number of <see cref="UInt32"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="UInt32"/> values to write.</param>
    Public Overloads Sub Write(values As UInt32())
        WriteMultiple(values, AddressOf Write)
    End Sub

    ''' <summary>
    ''' Writes an 8-byte unsigned integer value to this stream and advances the current position of the stream by
    ''' eight bytes.
    ''' </summary>
    ''' <param name="value">The value to write.</param>
    Public Overrides Sub Write(value As UInt64)
        If _needsReversion Then
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            WriteReversed(bytes)
        Else
            MyBase.Write(value)
        End If
    End Sub

    ''' <summary>
    ''' Writes the specified number of <see cref="UInt64"/> values into the current stream and advances the current
    ''' position by that number of <see cref="UInt64"/> values multiplied with the size of a single value.
    ''' </summary>
    ''' <param name="values">The <see cref="UInt64"/> values to write.</param>
    Public Overloads Sub Write(values As UInt64())
        WriteMultiple(values, AddressOf Write)
    End Sub

    Private Sub WriteMultiple(Of T)(values As T(), writeFunc As Action(Of T))
        For i As Integer = 0 To values.Length - 1
            writeFunc.Invoke(values(i))
        Next
    End Sub

    Private Sub WriteReversed(bytes As Byte())
        Array.Reverse(bytes)
        MyBase.Write(bytes)
    End Sub

    Private Sub WriteByteLengthPrefixString(value As String, encoding As Encoding)
        Write(CByte(value.Length))
        Write(encoding.GetBytes(value))
    End Sub

    Private Sub WriteWordLengthPrefixString(value As String, encoding As Encoding)
        Write(CShort(value.Length))
        Write(encoding.GetBytes(value))
    End Sub

    Private Sub WriteDwordLengthPrefixString(value As String, encoding As Encoding)
        Write(value.Length)
        Write(encoding.GetBytes(value))
    End Sub

    Private Sub WriteZeroTerminatedString(value As String, encoding As Encoding)
        Write(encoding.GetBytes(value))
        Write(CByte(0))
    End Sub

    Private Sub WriteNoPrefixOrTerminationString(value As String, encoding As Encoding)
        Write(encoding.GetBytes(value))
    End Sub

    Private Function DecimalToBytes(value As Decimal) As Byte()
        ' Get the bytes of the decimal.
        Dim bytes As Byte() = New Byte(Marshal.SizeOf(GetType(Decimal)) - 1) {}
        Buffer.BlockCopy(Decimal.GetBits(value), 0, bytes, 0, Marshal.SizeOf(GetType(Decimal)))
        Return bytes
    End Function
End Class
