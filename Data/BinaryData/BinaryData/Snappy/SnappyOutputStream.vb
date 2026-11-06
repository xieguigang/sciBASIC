Option Strict On
Option Explicit On

Imports System.IO
Imports std = System.Math

Namespace Snappy

    ''' <summary>
    ''' Snappy 分块流格式（framing format）的压缩写入流。
    ''' <para>
    ''' 构造时立即写入 stream identifier；每次 <see cref="Write"/> 的数据会先进入
    ''' 一个最多 <c>chunkSize</c> 字节的缓冲，缓冲满后压缩为一个分块。若压缩后
    ''' 没有变小，则写入未压缩分块。每个分块都附带基于未压缩数据的 masked CRC32C。
    ''' </para>
    ''' </summary>
    Public NotInheritable Class SnappyOutputStream
        Inherits Stream

        Private ReadOnly _output As Stream
        Private ReadOnly _leaveOpen As Boolean
        Private ReadOnly _chunkSize As Integer
        Private ReadOnly _buffer As Byte()
        Private _bufferLength As Integer
        Private _disposed As Boolean

        ''' <summary>
        ''' 创建一个 Snappy 分块流写入器，并向 <paramref name="output"/> 写入 stream identifier。
        ''' </summary>
        ''' <param name="output">接收压缩数据的流，必须可写。</param>
        ''' <param name="leaveOpen">释放本流时是否保留 <paramref name="output"/> 打开。</param>
        ''' <param name="chunkSize">每个分块最多容纳的未压缩字节数，取值 1..65536。</param>
        Public Sub New(output As Stream, Optional leaveOpen As Boolean = False, Optional chunkSize As Integer = 65536)
            If output Is Nothing Then Throw New ArgumentNullException(NameOf(output))
            If Not output.CanWrite Then Throw New ArgumentException("输出流必须可写。", NameOf(output))
            If chunkSize < 1 OrElse chunkSize > SnappyConstants.MaxChunkUncompressedData Then
                Throw New ArgumentOutOfRangeException(NameOf(chunkSize), $"分块大小必须位于 1..{SnappyConstants.MaxChunkUncompressedData} 之间。")
            End If

            _output = output
            _leaveOpen = leaveOpen
            _chunkSize = chunkSize
            _buffer = New Byte(chunkSize - 1) {}

            WriteStreamIdentifier()
        End Sub

        ''' <summary>把数据追加到当前分块缓冲，缓冲满时自动写出分块。</summary>
        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            ThrowIfDisposed()

            If buffer Is Nothing Then Throw New ArgumentNullException(NameOf(buffer))
            If offset < 0 OrElse count < 0 OrElse offset > buffer.Length OrElse count > buffer.Length - offset Then
                Throw New ArgumentOutOfRangeException("offset/count 超出了缓冲区范围。")
            End If

            While count > 0
                If _bufferLength = _chunkSize Then FlushChunk()

                Dim n As Integer = std.Min(count, _chunkSize - _bufferLength)
                System.Buffer.BlockCopy(buffer, offset, _buffer, _bufferLength, n)

                _bufferLength += n
                offset += n
                count -= n
            End While
        End Sub

        ''' <summary>写出当前缓冲中的分块，并刷新底层流。</summary>
        Public Overrides Sub Flush()
            ThrowIfDisposed()
            FlushChunk()
            _output.Flush()
        End Sub

        Private Sub WriteStreamIdentifier()
            _output.WriteByte(SnappyConstants.ChunkTypeStreamIdentifier)
            WriteChunkLength(SnappyConstants.StreamIdentifier.Length)
            _output.Write(SnappyConstants.StreamIdentifier, 0, SnappyConstants.StreamIdentifier.Length)
        End Sub

        Private Sub FlushChunk()
            If _bufferLength = 0 Then Return

            Dim compressed As Byte() = SnappyRawEncoder.Encode(_buffer, 0, _bufferLength)
            Dim useCompressed As Boolean = compressed.Length < _bufferLength
            Dim payloadLength As Integer = 4 + If(useCompressed, compressed.Length, _bufferLength)
            Dim crc As UInteger = Crc32C.Mask(Crc32C.ComputeHash(_buffer, 0, _bufferLength))

            _output.WriteByte(If(useCompressed, SnappyConstants.ChunkTypeCompressed, SnappyConstants.ChunkTypeUncompressed))
            WriteChunkLength(payloadLength)
            WriteUInt32LE(crc)

            If useCompressed Then
                _output.Write(compressed, 0, compressed.Length)
            Else
                _output.Write(_buffer, 0, _bufferLength)
            End If

            _bufferLength = 0
        End Sub

        Private Sub WriteChunkLength(length As Integer)
            _output.WriteByte(CByte(length And &HFF))
            _output.WriteByte(CByte((length >> 8) And &HFF))
            _output.WriteByte(CByte((length >> 16) And &HFF))
        End Sub

        Private Sub WriteUInt32LE(value As UInteger)
            _output.WriteByte(CByte(value And &HFFUI))
            _output.WriteByte(CByte((value >> 8) And &HFFUI))
            _output.WriteByte(CByte((value >> 16) And &HFFUI))
            _output.WriteByte(CByte((value >> 24) And &HFFUI))
        End Sub

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(SnappyOutputStream))
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not _disposed Then
                If disposing Then
                    Try
                        FlushChunk()
                    Finally
                        If Not _leaveOpen Then _output.Dispose()
                    End Try
                End If

                _disposed = True
            End If

            MyBase.Dispose(disposing)
        End Sub

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return Not _disposed
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

        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Throw New NotSupportedException()
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotSupportedException()
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

    End Class
End Namespace
