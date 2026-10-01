Option Strict On
Option Explicit On

Imports System.IO
Imports std = System.Math

Namespace LZ77Stream

    ''' <summary>
    ''' A read-only, forward-only decompression stream for the classic LZMA
    ''' "alone" container (.lzma files: 13-byte header followed by the raw
    ''' LZMA1 range-coded stream). Handles both known and unknown (0xFFFF...FFFF)
    ''' declared sizes, and verifies the end-of-stream marker when present.
    ''' </summary>
    Public NotInheritable Class LzmaStream
        Inherits Stream

        Private ReadOnly _input As Stream
        Private ReadOnly _leaveOpen As Boolean
        Private ReadOnly _decoder As Lzma1Decoder
        Private ReadOnly _declaredSize As ULong?
        Private _consumed As ULong
        Private _done As Boolean
        Private _disposed As Boolean

        ''' <summary>
        ''' Creates a decompressing stream over an .lzma (alone format) input stream.
        ''' The 13-byte header is consumed immediately.
        ''' </summary>
        ''' <param name="input">The compressed stream; must be readable.</param>
        ''' <param name="leaveOpen">True to leave <paramref name="input"/> open when this stream is disposed.</param>
        Public Sub New(input As Stream, Optional leaveOpen As Boolean = False)
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If Not input.CanRead Then Throw New ArgumentException("The input stream must be readable.", NameOf(input))
            _input = input
            _leaveOpen = leaveOpen

            ' ---- 13-byte alone header: 1 props + 4 dict size (LE) + 8 uncompressed size (LE) ----
            Dim header(12) As Byte
            Try
                input.ReadExactly(header)
            Catch ex As EndOfStreamException
                Throw New InvalidDataException("LZMA: the 13-byte header is truncated.", ex)
            End Try

            Dim dictSize = CUInt(header(1)) Or (CUInt(header(2)) << 8) Or
                       (CUInt(header(3)) << 16) Or (CUInt(header(4)) << 24)
            If dictSize > (1UI << 30) Then
                Throw New InvalidDataException($"LZMA: dictionary size 0x{dictSize:X8} is unreasonably large (over 1 GB).")
            End If

            Dim size As ULong = 0UL
            For i = 0 To 7
                size = size Or (CULng(header(5 + i)) << (8 * i))
            Next
            If size = &HFFFFFFFFFFFFFFFFUL Then
                _declaredSize = Nothing           ' unknown: stream must end with the EOS marker
            Else
                _declaredSize = size
            End If

            _decoder = New Lzma1Decoder(header(0), dictSize, input)
        End Sub

        ''' <summary>Literal context bits (lc) from the properties byte.</summary>
        Public ReadOnly Property Lc As Integer
            Get
                Return _decoder.Lc
            End Get
        End Property

        ''' <summary>Literal position bits (lp) from the properties byte.</summary>
        Public ReadOnly Property Lp As Integer
            Get
                Return _decoder.Lp
            End Get
        End Property

        ''' <summary>Position bits (pb) from the properties byte.</summary>
        Public ReadOnly Property Pb As Integer
            Get
                Return _decoder.Pb
            End Get
        End Property

        ''' <summary>Effective dictionary (ring buffer) size in bytes.</summary>
        Public ReadOnly Property DictionarySize As Integer
            Get
                Return _decoder.DictionarySize
            End Get
        End Property

        ''' <summary>Uncompressed size declared in the header, or Nothing when unknown.</summary>
        Public ReadOnly Property DeclaredSize As ULong?
            Get
                Return _declaredSize
            End Get
        End Property

        ''' <summary>
        ''' Reads decompressed bytes. Data is decoded symbol by symbol on demand;
        ''' a return value smaller than <paramref name="count"/> only happens at
        ''' the end of the stream.
        ''' </summary>
        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            ThrowIfDisposed()
            If buffer Is Nothing Then Throw New ArgumentNullException(NameOf(buffer))
            If offset < 0 OrElse count < 0 OrElse offset + count > buffer.Length Then
                Throw New ArgumentOutOfRangeException("offset/count are outside of buffer.")
            End If
            If count = 0 Then Return 0
            If _done Then Return 0

            Dim available = _decoder.TotalProduced - _consumed

            Do While available = 0UL
                If _decoder.Finished Then
                    If _declaredSize.HasValue AndAlso _decoder.TotalProduced <> _declaredSize.Value Then
                        Throw New InvalidDataException($"LZMA: decoded {_decoder.TotalProduced} bytes but the header declared {_declaredSize.Value}.")
                    End If
                    _done = True
                    Return 0
                End If

                If _declaredSize.HasValue Then
                    If _decoder.TotalProduced > _declaredSize.Value Then
                        Throw New InvalidDataException("LZMA: decoded more bytes than the header declared.")
                    End If
                    If _decoder.TotalProduced = _declaredSize.Value Then
                        _done = True
                        Return 0
                    End If
                End If

                _decoder.DecodeSymbol()
                available = _decoder.TotalProduced - _consumed
            Loop

            Dim n = CInt(std.Min(CULng(count), available))
            _decoder.CopyOut(_consumed, buffer, offset, n)
            _consumed += CULng(n)
            Return n
        End Function

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(LzmaStream))
        End Sub

        ' =====================================================================
        '  Stream boilerplate
        ' =====================================================================

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return Not _disposed
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotSupportedException()
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Throw New NotSupportedException()
            End Get
            Set(value As Long)
                Throw New NotSupportedException()
            End Set
        End Property

        Public Overrides Sub Flush()
        End Sub

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotSupportedException()
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotSupportedException()
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing AndAlso Not _leaveOpen Then
                    _input.Dispose()
                End If
                _disposed = True
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class
End Namespace