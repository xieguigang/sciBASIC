Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports MBW.Utilities.ManagedSqlite.Core.Helpers
Imports MBW.Utilities.ManagedSqlite.Core.Objects.Enums
Imports MBW.Utilities.ManagedSqlite.Core.Objects.Headers

Namespace MBW.Utilities.ManagedSqlite.Core.Internal
	Public Class ReaderBase
		Implements IDisposable
		Public ReadOnly Property Length() As Long

		Public ReadOnly Property Position() As Long
			Get
				Return _stream.Position
			End Get
		End Property

		Private ReadOnly _stream As Stream
		Private ReadOnly _binaryReader As BinaryReader

		Public Property TextEncoding() As SqliteEncoding
			Get
				Return m_TextEncoding
			End Get
			Private Set
				m_TextEncoding = Value
			End Set
		End Property
		Private m_TextEncoding As SqliteEncoding

		Private _encoding As Encoding

		Public Property PageSize() As UShort
			Get
				Return m_PageSize
			End Get
			Private Set
				m_PageSize = Value
			End Set
		End Property
		Private m_PageSize As UShort

		''' <summary>
		''' Reserved space at the end of every page
		''' </summary>
		Public Property ReservedSpace() As Byte
			Get
				Return m_ReservedSpace
			End Get
			Private Set
				m_ReservedSpace = Value
			End Set
		End Property
		Private m_ReservedSpace As Byte

		Public Sub New(stream As Stream)
			_stream = stream
			Length = _stream.Length

			_binaryReader = New BinaryReader(stream)
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
					Exit Select
				Case SqliteEncoding.UTF16LE
					_encoding = Encoding.Unicode
					Exit Select
				Case SqliteEncoding.UTF16BE
					_encoding = Encoding.BigEndianUnicode
					Exit Select
				Case Else
					Throw New ArgumentOutOfRangeException()
			End Select
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			_stream.Dispose()
		End Sub

		Friend Function CheckMagicBytes(comparison As Byte(), Optional throwException As Boolean = True) As Boolean
			Return CheckMagicBytes(CUInt(comparison.Length), comparison, throwException)
		End Function

		Friend Function CheckMagicBytes(toRead As UInteger, comparison As Byte(), Optional throwException As Boolean = True) As Boolean
			Debug.Assert(toRead >= comparison.Length)
			CheckSize(toRead)

			Dim data As Byte() = _stream.ReadFully(CInt(toRead))

			Dim res As Boolean = data.SequenceEqual(comparison)
			If Not res AndAlso throwException Then
                ' Note: This is the position after read
                Throw New ArgumentException("The requested magic bytes did not match") 'With {
                '.Data = {{NameOf(Stream.Position), _stream.Position}, {NameOf(toRead), toRead}, {NameOf(comparison), comparison.ToHex()}, {NameOf(data), data.ToHex()}}
                '}
            End If

			Return res
		End Function

		Friend Sub CheckSize(sizeWanted As UInteger, Optional throwException As Boolean = True)
			If Not throwException Then
				Return
			End If

			Dim dataLeft As Long = Length - _stream.Position
			If dataLeft < sizeWanted Then
                Throw New ArgumentException("Source stream does not have enough data") 'With {
                ' .Data = {{NameOf(Stream.Position), _stream.Position}, {NameOf(sizeWanted), sizeWanted}, {"SourceLength", Length}}
                ' }
            End If
		End Sub

		Friend Sub SetPositionAndCheckSize(position As ULong, sizeWanted As UInteger, Optional throwException As Boolean = True)
			SetPosition(position)

			CheckSize(sizeWanted, throwException)
		End Sub

		Friend Sub SetPosition(position As ULong)
			Dim newPosition As ULong = CULng(_stream.Seek(CLng(position), SeekOrigin.Begin))

			If newPosition <> position Then
                Throw New ArgumentException($"Unable to seek to position {position}")
            End If
		End Sub

		Friend Sub SeekPage(page As UInteger, Optional offset As UShort = 0)
			If page = 0 Then
				Throw New ArgumentOutOfRangeException(nameof(page))
			End If

			' Note: Pages are 1-indexed
			Dim position As ULong = (page - 1) * PageSize
			position += offset

			SetPositionAndCheckSize(position, CUInt(PageSize - offset))
		End Sub

		Friend Sub Skip(bytes As UInteger)
			_stream.Seek(bytes, SeekOrigin.Current)
		End Sub

		Public Function ReadByte() As Byte
			Return _binaryReader.ReadByte()
		End Function

		Public Function ReadUInt16() As UShort
			Dim res As UShort = _binaryReader.ReadByte()
			res <<= 8
			res += _binaryReader.ReadByte()

			Return res
		End Function

		Public Function ReadUInt32() As UInteger
			Dim res As UInteger = _binaryReader.ReadByte()
			res <<= 8

			res += _binaryReader.ReadByte()
			res <<= 8

			res += _binaryReader.ReadByte()
			res <<= 8

			res += _binaryReader.ReadByte()

			Return res
		End Function

		Public Function ReadInt16() As Short
			Dim res As Short = _binaryReader.ReadByte()
			res <<= 8
			res += _binaryReader.ReadByte()

			Return res
		End Function

		Public Function ReadInt32() As Integer
			Dim res As Integer = _binaryReader.ReadByte()
			res <<= 8

			res += _binaryReader.ReadByte()
			res <<= 8

			res += _binaryReader.ReadByte()
			res <<= 8

			res += _binaryReader.ReadByte()

			Return res
		End Function

		Public Function ReadVarInt() As Long
			Dim NULL As Byte
			Return ReadVarInt(NULL)
		End Function

		Public Function ReadVarInt(ByRef readBytes As Byte) As Long
			Dim res As Long = 0

			' Decode huffman encoding
			'  xyyy yyyy       x = if this is the last byte
			'                  y = data
			' Each byte provides 7 bits of the final data, and one bit to indicate followup bytes
			' The first 8 bytes are like this, the potential 9th byte is all data (8 bits data)

			For readBytes = 1 To 8
				Dim tmp As Byte = ReadByte()

				res <<= 7
				res += tmp And &H7f

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
			ReadByte()
		End Sub

		Public Function Read(count As Integer) As Byte()
			Return _stream.ReadFully(count)
		End Function

		Public Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
			Return _stream.ReadFully(buffer, offset, count)
		End Function

		Public Function ReadInteger(bytes As Byte) As Long
			Dim res As Long = 0

			For i As Integer = 0 To bytes - 1
				Dim tmp As Byte = ReadByte()

				res <<= 8
				res += tmp
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
