Option Strict On
Option Explicit On

Imports System.IO
Imports Microsoft.VisualBasic.Data.Repository
Imports std = System.Math

Namespace LZ77Stream

    ''' <summary>
    ''' A read-only, forward-only decompression stream for the LZ4 frame format
    ''' (magic 0x184D2204). Supports linked (dictionary-dependent) blocks, stored
    ''' blocks, block and content checksum verification, skippable frames, the
    ''' deprecated legacy format (0x184C2102) and concatenated multi-frame streams.
    ''' <para>
    ''' Construct with any readable stream; decompressed data is pulled lazily:
    ''' no more than one frame block is ever buffered in memory.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class Lz4Stream
        Inherits Stream

        Private Const MagicFrame As UInteger = &H184D2204UI
        Private Const MagicLegacy As UInteger = &H184C2102UI
        Private Const MagicSkippableMin As UInteger = &H184D2A50UI
        Private Const MagicSkippableMax As UInteger = &H184D2A5FUI

        ''' <summary>Match window shared between linked blocks.</summary>
        Private Const WindowSize As Integer = 64 * 1024
        Private Const LegacyBlockSize As Integer = 8 * 1024 * 1024

        Private ReadOnly _input As Stream
        Private ReadOnly _leaveOpen As Boolean
        Private _disposed As Boolean

        ' ---- current frame header ----
        Private _frameActive As Boolean
        Private _legacy As Boolean
        Private _linked As Boolean
        Private _blockChecksum As Boolean
        Private _contentChecksum As Boolean
        Private _blockMaxSize As Integer
        Private _hasContentSize As Boolean
        Private _contentSize As ULong
        Private _frameProduced As ULong
        Private _contentHash As XxHash32

        ' ---- current block serving state ----
        Private _out As Byte()
        Private _blockStart As Integer
        Private _blockLen As Integer
        Private _served As Integer

        ' ---- linked-block dictionary tail ----
        Private _prevTail As Byte()
        Private _prevTailLen As Integer
        Private _endAfterServe As Boolean

        ''' <summary>Creates a decompressing stream over an LZ4 compressed input stream.</summary>
        ''' <param name="input">The compressed stream; must be readable.</param>
        ''' <param name="leaveOpen">True to leave <paramref name="input"/> open when this stream is disposed.</param>
        Public Sub New(input As Stream, Optional leaveOpen As Boolean = False)
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If Not input.CanRead Then Throw New ArgumentException("The input stream must be readable.", NameOf(input))
            _input = input
            _leaveOpen = leaveOpen
        End Sub

        ''' <summary>
        ''' Reads decompressed bytes. Blocks are decoded on demand; a return value
        ''' smaller than <paramref name="count"/> only happens at frame / stream ends.
        ''' </summary>
        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            ThrowIfDisposed()
            If buffer Is Nothing Then Throw New ArgumentNullException(NameOf(buffer))
            If offset < 0 OrElse count < 0 OrElse offset + count > buffer.Length Then
                Throw New ArgumentOutOfRangeException("offset/count are outside of buffer.")
            End If
            If count = 0 Then Return 0

            ' Decode until at least one byte of the current block is available.
            Do While _served >= _blockLen
                If _frameActive Then
                    If _legacy Then
                        DecodeNextLegacyBlock()
                    Else
                        DecodeNextBlock()
                    End If
                ElseIf Not BeginFrame() Then
                    Return 0      ' clean end of stream
                End If
            Loop

            Dim n = std.Min(count, _blockLen - _served)
            Array.Copy(_out, _blockStart + _served, buffer, offset, n)
            If _contentChecksum Then _contentHash.Update(buffer, offset, n)
            _served += n

            ' Legacy frames end after any block that produced less than 8 MB.
            If _endAfterServe AndAlso _served >= _blockLen Then
                _frameActive = False
                _endAfterServe = False
            End If

            Return n
        End Function

        ' =====================================================================
        '  Frame parsing
        ' =====================================================================

        ''' <summary>
        ''' Reads the next frame magic. Returns True when a data frame is active;
        ''' skippable frames are consumed transparently; False means clean EOF.
        ''' </summary>
        Private Function BeginFrame() As Boolean
            Do
                Dim magic As UInteger
                If Not TryReadU32LE(magic) Then Return False

                If magic = MagicFrame Then
                    ParseFrameHeader()
                    Return True

                ElseIf magic >= MagicSkippableMin AndAlso magic <= MagicSkippableMax Then
                    Dim size = ReadU32LE()
                    SkipBytes(size)

                ElseIf magic = MagicLegacy Then
                    _legacy = True
                    _linked = False
                    _blockMaxSize = LegacyBlockSize
                    _frameActive = True
                    _prevTailLen = 0
                    _endAfterServe = False
                    EnsureWindow()
                    Return True

                Else
                    Throw New InvalidDataException($"LZ4: unknown magic number 0x{magic:X8}.")
                End If
            Loop
        End Function

        Private Sub ParseFrameHeader()
            Dim flg = ReadByteOrThrow()
            Dim bd = ReadByteOrThrow()

            If (flg >> 6) <> 1 Then
                Throw New InvalidDataException($"LZ4: unsupported frame version {flg >> 6} (expected 01).")
            End If
            If (flg And &H3) <> 0 Then
                Throw New InvalidDataException("LZ4: reserved bits in FLG are set.")
            End If
            ' BD layout: bit7 reserved, bits 6-4 BlockMaxSize (3-bit code 4..7), bits 3-0 reserved.
            If (bd And &H80) <> 0 OrElse (bd And &HF) <> 0 Then
                Throw New InvalidDataException("LZ4: reserved bits in BD are set.")
            End If

            _linked = (flg And &H20) = 0            ' B.Indep bit clear -> linked blocks
            _blockChecksum = (flg And &H10) <> 0    ' B.Checksum
            _hasContentSize = (flg And &H8) <> 0    ' C.Size
            _contentChecksum = (flg And &H4) <> 0   ' C.Checksum

            Dim sizeCode = (bd >> 4) And &H7
            Select Case sizeCode
                Case 4 : _blockMaxSize = 64 * 1024
                Case 5 : _blockMaxSize = 256 * 1024
                Case 6 : _blockMaxSize = 1024 * 1024
                Case 7 : _blockMaxSize = 4 * 1024 * 1024
                Case Else
                    Throw New InvalidDataException($"LZ4: invalid block maximum size code {sizeCode}.")
            End Select

            ' Header checksum covers everything from FLG up to (and including)
            ' the optional content size field.
            Dim header As New List(Of Byte) From {CByte(flg), CByte(bd)}
            If _hasContentSize Then
                _contentSize = ReadU64LE()
                For i = 0 To 7
                    header.Add(CByte((_contentSize >> (8 * i)) And &HFFUL))
                Next
            Else
                _contentSize = 0UL
            End If

            Dim hc = ReadByteOrThrow()
            Dim expected = CByte((XxHash32.ComputeHash(header.ToArray(), 0, header.Count) >> 8) And &HFFUI)
            If hc <> expected Then
                Throw New InvalidDataException("LZ4: frame header checksum (HC) mismatch.")
            End If

            _frameProduced = 0UL
            _contentHash = New XxHash32()
            _prevTailLen = 0
            _endAfterServe = False
            _legacy = False
            _frameActive = True
            EnsureWindow()
        End Sub

        ' =====================================================================
        '  Block decoding
        ' =====================================================================

        Private Sub DecodeNextBlock()
            Dim sizeWord = ReadU32LE()
            If sizeWord = 0UI Then
                FinalizeFrame()
                Return
            End If

            Dim stored = (sizeWord And &H80000000UI) <> 0UI
            Dim dataSize = CInt(sizeWord And &H7FFFFFFFUI)
            If dataSize = 0 Then
                Throw New InvalidDataException("LZ4: block data size is zero (only the EndMark may be zero).")
            End If
            If dataSize > _blockMaxSize Then
                Throw New InvalidDataException($"LZ4: block data size {dataSize} exceeds the frame's maximum block size {_blockMaxSize}.")
            End If

            Dim data(dataSize - 1) As Byte
            ReadExactlyBlock(data)

            If _blockChecksum Then
                Dim sum = ReadU32LE()
                If sum <> XxHash32.ComputeHash(data, 0, dataSize) Then
                    Throw New InvalidDataException("LZ4: block checksum mismatch.")
                End If
            End If

            BeginBlock()

            If stored Then
                Buffer.BlockCopy(data, 0, _out, _blockStart, dataSize)
                _blockLen = dataSize
            Else
                Dim dictStart = If(_linked, 0, _blockStart)
                Dim newEnd = Lz4.DecodeBlock(data, 0, dataSize, _out, dictStart, _blockStart, _blockStart + _blockMaxSize)
                _blockLen = newEnd - _blockStart
            End If

            _frameProduced += CULng(_blockLen)
            _served = 0
            If _linked Then SaveTail()
        End Sub

        Private Sub DecodeNextLegacyBlock()
            Dim sizeWord As UInteger
            If Not TryReadU32LE(sizeWord) Then
                _frameActive = False
                Return
            End If
            If sizeWord = 0UI Then
                _frameActive = False
                Return
            End If

            Dim dataSize = CInt(sizeWord)
            If dataSize > LegacyBlockSize Then
                Throw New InvalidDataException("LZ4: legacy block is too large.")
            End If

            Dim data(dataSize - 1) As Byte
            ReadExactlyBlock(data)

            _blockStart = 0
            _blockLen = Lz4.DecodeBlock(data, 0, dataSize, _out, 0, 0, LegacyBlockSize)
            _served = 0
            ' Legacy frames end after any block that decompressed to less than 8 MB.
            If _blockLen < LegacyBlockSize Then _endAfterServe = True
        End Sub

        ''' <summary>Positions the write cursor for the next block, installing the linked-block dictionary prefix.</summary>
        Private Sub BeginBlock()
            If _linked AndAlso _prevTailLen > 0 Then
                Buffer.BlockCopy(_prevTail, 0, _out, 0, _prevTailLen)
            End If
            _blockStart = If(_linked, _prevTailLen, 0)
        End Sub

        ''' <summary>Remembers the last &lt;= 64 KB of the current block as the dictionary for the next linked block.</summary>
        Private Sub SaveTail()
            If _prevTail Is Nothing OrElse _prevTail.Length < WindowSize Then
                _prevTail = New Byte(WindowSize - 1) {}
            End If
            Dim t = std.Min(WindowSize, _blockLen)
            Buffer.BlockCopy(_out, _blockStart + _blockLen - t, _prevTail, 0, t)
            _prevTailLen = t
        End Sub

        Private Sub FinalizeFrame()
            If _contentChecksum Then
                Dim expected = ReadU32LE()
                If _contentHash.GetValue() <> expected Then
                    Throw New InvalidDataException("LZ4: frame content checksum mismatch.")
                End If
            End If
            If _hasContentSize AndAlso _frameProduced <> _contentSize Then
                Throw New InvalidDataException($"LZ4: decoded {_frameProduced} bytes but the frame header declared {_contentSize}.")
            End If
            _frameActive = False
            _legacy = False
            _prevTailLen = 0
        End Sub

        Private Sub EnsureWindow()
            Dim needed = _blockMaxSize + If(_linked, WindowSize, 0)
            If _out Is Nothing OrElse _out.Length < needed Then
                _out = New Byte(needed - 1) {}
            End If
        End Sub

        ' =====================================================================
        '  Input helpers
        ' =====================================================================

        Private Function ReadByteOrThrow() As Integer
            Dim b = _input.ReadByte()
            If b < 0 Then Throw New InvalidDataException("LZ4: the input stream is truncated.")
            Return b
        End Function

        ''' <summary>Returns False only for a clean EOF before the first byte; any mid-read truncation throws.</summary>
        Private Function TryReadU32LE(ByRef value As UInteger) As Boolean
            value = 0UI
            Dim first = _input.ReadByte()
            If first < 0 Then Return False
            value = CUInt(first)
            For i = 1 To 3
                value = value Or (CUInt(ReadByteOrThrow()) << (8 * i))
            Next
            Return True
        End Function

        Private Function ReadU32LE() As UInteger
            Dim v As UInteger
            If Not TryReadU32LE(v) Then Throw New InvalidDataException("LZ4: the input stream is truncated.")
            Return v
        End Function

        Private Function ReadU64LE() As ULong
            Dim lo = ReadU32LE()
            Dim hi = ReadU32LE()
            Return CULng(lo) Or (CULng(hi) << 32)
        End Function

        Private Sub ReadExactlyBlock(buffer As Byte())
            Dim done = 0
            While done < buffer.Length
                Dim n = _input.Read(buffer, done, buffer.Length - done)
                If n <= 0 Then Throw New InvalidDataException("LZ4: the input stream is truncated inside a block.")
                done += n
            End While
        End Sub

        Private Sub SkipBytes(count As UInteger)
            Dim scratch(4095) As Byte
            Dim remaining As ULong = CULng(count)
            Do While remaining > 0UL
                Dim c = CInt(std.Min(remaining, CULng(scratch.Length)))
                ReadExactlyInto(scratch, c)
                remaining -= CULng(c)
            Loop
        End Sub

        Private Sub ReadExactlyInto(buffer As Byte(), count As Integer)
            Dim done = 0
            While done < count
                Dim n = _input.Read(buffer, done, count - done)
                If n <= 0 Then Throw New InvalidDataException("LZ4: the input stream is truncated.")
                done += n
            End While
        End Sub

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(Lz4Stream))
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