Option Strict On
Option Explicit On

Imports System.IO
Imports std = System.Math

Namespace Snappy

    ''' <summary>
    ''' Snappy 分块流格式（framing format）的解压读取流。
    ''' <para>
    ''' 构造时校验 stream identifier；<see cref="Read"/> 按分块惰性拉取数据，
    ''' 内存中最多驻留一个分块。支持压缩分块、未压缩分块、填充分块与可跳过分块，
    ''' 并对每个数据分块校验 masked CRC32C。
    ''' </para>
    ''' </summary>
    Public NotInheritable Class SnappyInputStream
        Inherits Stream

        Private ReadOnly _input As Stream
        Private ReadOnly _leaveOpen As Boolean
        Private _disposed As Boolean

        Private _chunk As Byte()
        Private _chunkLength As Integer
        Private _chunkPosition As Integer

        ''' <summary>
        ''' 创建一个 Snappy 分块流读取器，并立即校验输入流开头的 stream identifier。
        ''' </summary>
        ''' <param name="input">包含分块流的输入流，必须可读。</param>
        ''' <param name="leaveOpen">释放本流时是否保留 <paramref name="input"/> 打开。</param>
        ''' <exception cref="InvalidDataException">输入流缺少合法的 stream identifier。</exception>
        Public Sub New(input As Stream, Optional leaveOpen As Boolean = False)
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If Not input.CanRead Then Throw New ArgumentException("输入流必须可读。", NameOf(input))

            _input = input
            _leaveOpen = leaveOpen
            ReadStreamIdentifier()
        End Sub

        ''' <summary>读取解压后的数据；到达流末尾时返回 0。</summary>
        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            ThrowIfDisposed()

            If buffer Is Nothing Then Throw New ArgumentNullException(NameOf(buffer))
            If offset < 0 OrElse offset > buffer.Length OrElse count < 0 OrElse count > buffer.Length - offset Then
                Throw New ArgumentOutOfRangeException("offset/count 超出了缓冲区范围。")
            End If
            If count = 0 Then Return 0

            While _chunkPosition >= _chunkLength
                If Not LoadNextChunk() Then Return 0
            End While

            Dim n As Integer = std.Min(count, _chunkLength - _chunkPosition)
            System.Buffer.BlockCopy(_chunk, _chunkPosition, buffer, offset, n)
            _chunkPosition += n

            Return n
        End Function

        Private Sub ReadStreamIdentifier()
            Dim header(3) As Byte

            If Not ReadFully(header, 0, 4, allowEof:=True) Then
                Throw New InvalidDataException("Snappy: 分块流缺少 stream identifier。")
            End If

            If header(0) <> SnappyConstants.ChunkTypeStreamIdentifier Then
                Throw New InvalidDataException($"Snappy: 分块流应以 stream identifier (0xFF) 开头，实际为 0x{header(0):X2}。")
            End If

            Dim length As Integer = ChunkLength(header)

            If length <> SnappyConstants.StreamIdentifier.Length Then
                Throw New InvalidDataException("Snappy: stream identifier 的长度必须为 6。")
            End If

            Dim magic(SnappyConstants.StreamIdentifier.Length - 1) As Byte
            ReadExactlyCore(magic, 0, magic.Length)
            VerifyStreamIdentifier(magic)
        End Sub

        Private Function LoadNextChunk() As Boolean
            Do
                Dim header(3) As Byte

                If Not ReadFully(header, 0, 4, allowEof:=True) Then Return False

                Dim chunkType As Byte = header(0)
                Dim length As Integer = ChunkLength(header)

                Select Case chunkType
                    Case SnappyConstants.ChunkTypeStreamIdentifier
                        If length <> SnappyConstants.StreamIdentifier.Length Then
                            Throw New InvalidDataException("Snappy: stream identifier 的长度必须为 6。")
                        End If

                        Dim magic(SnappyConstants.StreamIdentifier.Length - 1) As Byte
                        ReadExactlyCore(magic, 0, magic.Length)
                        VerifyStreamIdentifier(magic)

                    Case SnappyConstants.ChunkTypePadding
                        Skip(length)

                    Case SnappyConstants.SkippableChunkMin To SnappyConstants.SkippableChunkMax
                        Skip(length)

                    Case SnappyConstants.ChunkTypeCompressed
                        LoadCompressedChunk(length)
                        Return True

                    Case SnappyConstants.ChunkTypeUncompressed
                        LoadUncompressedChunk(length)
                        Return True

                    Case Else
                        Throw New InvalidDataException($"Snappy: 不支持的不可跳过分块类型 0x{chunkType:X2}。")
                End Select
            Loop
        End Function

        Private Sub LoadCompressedChunk(length As Integer)
            If length < 4 Then
                Throw New InvalidDataException("Snappy: 压缩分块长度小于 4 字节（不足以容纳 CRC32C）。")
            End If

            Dim maxLength As Integer = 4 + Snappy.MaxCompressedLength(SnappyConstants.MaxChunkUncompressedData)

            If length > maxLength Then
                Throw New InvalidDataException($"Snappy: 压缩分块长度 {length} 超出允许上限 {maxLength}。")
            End If

            Dim payload(length - 1) As Byte
            ReadExactlyCore(payload, 0, length)

            Dim expected As UInteger = ReadUInt32LE(payload, 0)
            Dim pos As Integer = 4
            Dim decoded As Byte() = SnappyRawDecoder.Decode(payload, pos, length)

            If pos <> length Then
                Throw New InvalidDataException($"Snappy: 压缩分块末尾存在 {length - pos} 字节未消费的数据。")
            End If
            If decoded.Length > SnappyConstants.MaxChunkUncompressedData Then
                Throw New InvalidDataException($"Snappy: 分块解压后长度 {decoded.Length} 超出 65536 字节上限。")
            End If

            VerifyChecksum(expected, decoded)
            SetChunk(decoded)
        End Sub

        Private Sub LoadUncompressedChunk(length As Integer)
            If length < 4 Then
                Throw New InvalidDataException("Snappy: 未压缩分块长度小于 4 字节（不足以容纳 CRC32C）。")
            End If

            Dim dataLength As Integer = length - 4

            If dataLength > SnappyConstants.MaxChunkUncompressedData Then
                Throw New InvalidDataException($"Snappy: 未压缩分块长度 {dataLength} 超出 65536 字节上限。")
            End If

            Dim payload(length - 1) As Byte
            ReadExactlyCore(payload, 0, length)

            Dim expected As UInteger = ReadUInt32LE(payload, 0)
            Dim data As Byte()

            If dataLength = 0 Then
                data = New Byte() {}
            Else
                data = New Byte(dataLength - 1) {}
                Buffer.BlockCopy(payload, 4, data, 0, dataLength)
            End If

            VerifyChecksum(expected, data)
            SetChunk(data)
        End Sub

        Private Sub SetChunk(data As Byte())
            _chunk = data
            _chunkLength = data.Length
            _chunkPosition = 0
        End Sub

        Private Sub VerifyChecksum(expected As UInteger, data As Byte())
            Dim actual As UInteger = Crc32C.Mask(Crc32C.ComputeHash(data))

            If actual <> expected Then
                Throw New InvalidDataException($"Snappy: CRC32C 校验失败（期望 0x{expected:X8}，实际 0x{actual:X8}）。")
            End If
        End Sub

        Private Sub VerifyStreamIdentifier(magic As Byte())
            If magic.Length <> SnappyConstants.StreamIdentifier.Length Then
                Throw New InvalidDataException("Snappy: stream identifier 的长度必须为 6。")
            End If

            For i As Integer = 0 To magic.Length - 1
                If magic(i) <> SnappyConstants.StreamIdentifier(i) Then
                    Throw New InvalidDataException("Snappy: stream identifier 载荷不是 ""sNaPpY""。")
                End If
            Next
        End Sub

        Private Function ChunkLength(header As Byte()) As Integer
            Return CInt(header(1)) Or (CInt(header(2)) << 8) Or (CInt(header(3)) << 16)
        End Function

        Private Function ReadUInt32LE(data As Byte(), offset As Integer) As UInteger
            Return CUInt(data(offset)) Or
                   (CUInt(data(offset + 1)) << 8) Or
                   (CUInt(data(offset + 2)) << 16) Or
                   (CUInt(data(offset + 3)) << 24)
        End Function

        Private Function ReadFully(buffer As Byte(), offset As Integer, count As Integer, allowEof As Boolean) As Boolean
            Dim done As Integer = 0

            While done < count
                Dim n As Integer = _input.Read(buffer, offset + done, count - done)

                If n <= 0 Then
                    If allowEof AndAlso done = 0 Then Return False
                    Throw New InvalidDataException("Snappy: 分块流数据被截断。")
                End If

                done += n
            End While

            Return True
        End Function

        Private Sub ReadExactlyCore(buffer As Byte(), offset As Integer, count As Integer)
            If Not ReadFully(buffer, offset, count, allowEof:=False) Then
                Throw New InvalidDataException("Snappy: 分块流数据被截断。")
            End If
        End Sub

        Private Sub Skip(count As Integer)
            Dim scratch(4095) As Byte
            Dim remaining As Integer = count

            While remaining > 0
                Dim n As Integer = std.Min(remaining, scratch.Length)
                ReadExactlyCore(scratch, 0, n)
                remaining -= n
            End While
        End Sub

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(SnappyInputStream))
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

    End Class
End Namespace
