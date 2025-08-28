#Region "Microsoft.VisualBasic::776cd9ff189646d48d2a4a66f4494d4a, Data\BinaryData\SQLite3\Internal\ReaderBase.vb"

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

    '   Total Lines: 292
    '    Code Lines: 192 (65.75%)
    ' Comment Lines: 30 (10.27%)
    '    - Xml Docs: 23.33%
    ' 
    '   Blank Lines: 70 (23.97%)
    '     File Size: 9.65 KB


    '     Class ReaderBase
    ' 
    '         Properties: EOF, Length, PageSize, Position, ReservedSpace
    '                     TextEncoding
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) CheckMagicBytes, (+2 Overloads) Read, ReadByte, ReadInt16, ReadInt32
    '                   ReadInteger, ReadString, ReadUInt16, ReadUInt32, ReadVarInt
    ' 
    '         Sub: ApplySqliteDatabaseHeader, CheckSize, Dispose, SeekPage, SetPosition
    '              SetPositionAndCheckSize, Skip, SkipVarInt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Helpers
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers

Namespace ManagedSqlite.Core.Internal

    Public Class ReaderBase : Implements IDisposable

        ''' <summary>
        ''' The binary file length
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Long

        Public ReadOnly Property Position As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return stream.Position
            End Get
        End Property

        Public Property TextEncoding As SqliteEncoding
        Public Property PageSize As UShort

        ''' <summary>
        ''' Reserved space at the end of every page
        ''' </summary>
        Public Property ReservedSpace() As Byte

        ReadOnly stream As Stream
        ReadOnly binaryReader As BinaryReader

        Dim _encoding As Encoding

        Public ReadOnly Property EOF As Boolean
            Get
                Return stream.Position = stream.Length
            End Get
        End Property

        Public Sub New(stream As Stream)
            Me.stream = stream
            Me.Length = Me.stream.Length
            Me.binaryReader = New BinaryReader(stream)
        End Sub

        Friend Sub New(stream As Stream, origin As ReaderBase)
            Me.New(stream)
            PageSize = origin.PageSize
            ReservedSpace = origin.ReservedSpace
            TextEncoding = origin.TextEncoding
            _encoding = origin._encoding
        End Sub

        Friend Sub ApplySqliteDatabaseHeader(header As DatabaseHeader)
            PageSize = header.PageSize
            ReservedSpace = header.ReservedSpaceAtEndOfPage
            TextEncoding = header.TextEncoding

            Select Case TextEncoding
                Case SqliteEncoding.UTF8
                    _encoding = Encoding.UTF8

                Case SqliteEncoding.UTF16LE
                    _encoding = Encoding.Unicode

                Case SqliteEncoding.UTF16BE
                    _encoding = Encoding.BigEndianUnicode

                Case Else
                    Throw New ArgumentOutOfRangeException()
            End Select
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Call stream.Dispose()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function CheckMagicBytes(comparison As Byte(), Optional throwException As Boolean = True) As Boolean
            Return CheckMagicBytes(CUInt(comparison.Length), comparison, throwException)
        End Function

        Friend Function CheckMagicBytes(toRead As UInteger, comparison As Byte(), Optional throwException As Boolean = True) As Boolean
            Call System.Diagnostics.Debug.Assert(toRead >= comparison.Length)
            Call CheckSize(toRead)

            Dim data As Byte() = stream.ReadFully(CInt(toRead))
            Dim res As Boolean = data.SequenceEqual(comparison)

            If Not res AndAlso throwException Then
                ' Note: This is the position after read
                Throw New ArgumentException("The requested magic bytes did not match")
            End If

            Return res
        End Function

        Friend Sub CheckSize(sizeWanted As UInteger, Optional throwException As Boolean = True)
            If Not throwException Then
                Return
            End If

            Dim dataLeft As Long = Length - stream.Position

            If dataLeft < sizeWanted Then
                Throw New ArgumentException("Source stream does not have enough data") 'With {
                ' .Data = {{NameOf(Stream.Position), _stream.Position}, {NameOf(sizeWanted), sizeWanted}, {"SourceLength", Length}}
                ' }
            End If
        End Sub

        Friend Sub SetPositionAndCheckSize(position As ULong, sizeWanted As UInteger, Optional throwException As Boolean = True)
            Call SetPosition(position)
            Call CheckSize(sizeWanted, throwException)
        End Sub

        Friend Sub SetPosition(position As ULong)
            Dim newPosition As ULong = CULng(stream.Seek(CLng(position), SeekOrigin.Begin))

            If newPosition <> position Then
                Throw New ArgumentException($"Unable to seek to position {position}")
            End If
        End Sub

        Friend Sub SeekPage(page As UInteger, Optional offset As UShort = 0)
            If page = 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(page))
            End If

            ' Note: Pages are 1-indexed
            Dim position As ULong = (page - 1) * PageSize

            position += offset
            SetPositionAndCheckSize(position, CUInt(PageSize - offset))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Skip(bytes As UInteger)
            stream.Seek(bytes, SeekOrigin.Current)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadByte() As Byte
            Return binaryReader.ReadByte()
        End Function

        Public Function ReadUInt16() As UShort
            Dim res As UShort = binaryReader.ReadByte()
            res <<= 8
            res += binaryReader.ReadByte()

            Return res
        End Function

        Public Function ReadUInt32() As UInteger
            Dim res As UInteger = binaryReader.ReadByte()
            res <<= 8

            res += binaryReader.ReadByte()
            res <<= 8

            res += binaryReader.ReadByte()
            res <<= 8

            res += binaryReader.ReadByte()

            Return res
        End Function

        Public Function ReadInt16() As Short
            Dim res As Short = binaryReader.ReadByte()
            res <<= 8
            res += binaryReader.ReadByte()

            Return res
        End Function

        Public Function ReadInt32() As Integer
            Dim res As Integer = binaryReader.ReadByte()
            res <<= 8

            res += binaryReader.ReadByte()
            res <<= 8

            res += binaryReader.ReadByte()
            res <<= 8

            res += binaryReader.ReadByte()

            Return res
        End Function

        Public Function ReadVarInt(Optional ByRef readBytes As Byte = 0) As Long
            Dim res As Long = 0

            ' Decode huffman encoding
            '  xyyy yyyy       x = if this is the last byte
            '                  y = data
            ' Each byte provides 7 bits of the final data, and one bit to indicate followup bytes
            ' The first 8 bytes are like this, the potential 9th byte is all data (8 bits data)

            For readBytes = 1 To 8
                Dim tmp As Byte = ReadByte()

                res <<= 7
                res += tmp And &H7F

                If (tmp And &H80) = &H0 Then
                    ' Last byte
                    Return res
                End If
            Next

            ' Read final byte
            res <<= 8
            res += ReadByte()

            readBytes += 1

            Return res
        End Function

        Public Sub SkipVarInt()
            ' Decode huffman encoding
            '  xyyy yyyy       x = if this is the last byte
            '                  y = data
            ' Each byte provides 7 bits of the final data, and one bit to indicate followup bytes
            ' The first 8 bytes are like this, the potential 9th byte is all data (8 bits data)

            For readBytes As Byte = 1 To 8
                Dim tmp As Byte = ReadByte()

                If (tmp And &H80) = &H0 Then
                    ' Last byte
                    Return
                End If
            Next

            ' Skip final byte
            Call ReadByte()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Read(count As Integer) As Byte()
            Return stream.ReadFully(count)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Return stream.ReadFully(buffer, offset, count)
        End Function

        Public Function ReadInteger(bytes As Byte) As Long
            Dim res As Long = 0

            If EOF Then
                Return 0
            End If

            For i As Integer = 0 To bytes - 1
                Dim tmp As Byte = ReadByte()

                res <<= 8
                res += tmp

                'If EOF Then
                '    Exit For
                'End If
            Next

            If ((1L << (bytes * 8 - 1)) And res) > 0 Then
                ' Number was negative
                Dim extra As Long = -1L
                ' 0xFFFF FFFF FFFF FFFF in binary
                extra <<= bytes * 8

                res = res Or extra
            End If

            Return res
        End Function

        Public Function ReadString(bytes As UShort) As String
            Dim data As Byte() = Read(bytes)
            Return _encoding.GetString(data, 0, data.Length)
        End Function
    End Class
End Namespace
