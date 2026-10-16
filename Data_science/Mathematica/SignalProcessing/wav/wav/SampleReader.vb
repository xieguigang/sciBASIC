Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' random access reader of the raw sample frames on a wave file.
''' 
''' it bypasses the <see cref="Sample"/> object allocation of the
''' <see cref="WaveFile"/>/<see cref="LazyDataChunk"/> loading path, the byte offset of
''' the sample data section is resolved by the wav parser, and then the raw sample
''' frames are decoded directly from the file stream. this is the fast path which
''' makes the batch decoding of a large wave file (for example a 384kHz 30 seconds
''' analog recording) possible:
''' 
''' | reader | allocation per frame | random access | supported format |
''' |--------|----------------------|---------------|------------------|
''' | <see cref="WaveSampleReader"/> | none, the raw byte block is decoded in place | any frame offset | PCM 8/16/24/32, IEEE float 32/64 |
''' | ``WaveFile`` + <see cref="LazyDataChunk"/> | one <see cref="Sample"/> object per frame | any frame offset | the same, plus G.711 A-law/mu-law |
''' 
''' the class is a forward-only reader on an already opened file stream, and it is
''' thread safe for the concurrent readers on the **different** channel indices only
''' when the same instance is not used concurrently: use one instance per thread.
''' </summary>
Public Class WaveSampleReader : Implements IDisposable

    ''' <summary>
    ''' the file path of current wave file
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Path As String

    ''' <summary>
    ''' the number of the audio channels on current wave file
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Channels As Integer

    ''' <summary>
    ''' samples per second
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property SampleRate As Integer

    ''' <summary>
    ''' 32/64 for the IEEE float wave data, or 8/16/24/32 for the PCM data
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BitsPerSample As Integer

    ''' <summary>
    ''' True for the IEEE float wave data, False for the integer PCM data
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsFloat As Boolean

    ''' <summary>
    ''' the total number of the sample frames on current wave file
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property FrameCount As Long

    ''' <summary>
    ''' the duration of the wave file in seconds
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Duration As Double
        Get
            Return FrameCount / SampleRate
        End Get
    End Property

    ''' <summary>
    ''' the zero based byte offset of the first sample frame on the file stream
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DataOffset As Long

    Dim fs As FileStream
    Dim frameSize As Integer
    Dim _format As SampleFormat

    ''' <summary>
    ''' the sample data encoding which the raw byte block decoder of
    ''' <see cref="WaveSampleReader"/> supports
    ''' </summary>
    Public Enum SampleFormat

        ''' <summary>
        ''' there is no sample data or the encoding is not supported by the raw reader
        ''' </summary>
        Unsupported = 0

        ''' <summary>
        ''' the 8 bit unsigned PCM data, the byte value is [0,255] and it is offset by 128
        ''' </summary>
        Pcm8 = 1

        ''' <summary>
        ''' the 16 bit signed PCM data
        ''' </summary>
        Pcm16

        ''' <summary>
        ''' the 24 bit signed PCM data
        ''' </summary>
        Pcm24

        ''' <summary>
        ''' the 32 bit signed PCM data
        ''' </summary>
        Pcm32

        ''' <summary>
        ''' the 32 bit IEEE float data
        ''' </summary>
        Float32

        ''' <summary>
        ''' the 64 bit IEEE float data
        ''' </summary>
        Float64

        ''' <summary>
        ''' the G.711 A-law/mu-law companded data, it is not supported by the raw byte
        ''' decoder, please use the table driven ``Sample`` parser instead
        ''' </summary>
        G711
    End Enum

    Private Sub New(path As String, channels As Integer, sampleRate As Integer, bits As Integer,
                    isFloat As Boolean, format As SampleFormat, frameCount As Long, baseOffset As Long)

        Me.Path = path
        Me.Channels = channels
        Me.SampleRate = sampleRate
        Me.BitsPerSample = bits
        Me.IsFloat = isFloat
        Me.FrameCount = frameCount
        Me.DataOffset = baseOffset
        Me.frameSize = channels * (bits \ 8)
        Me._format = format
        Me.fs = File.OpenRead(path)
    End Sub

    ''' <summary>
    ''' open a given wave file for the random access of the raw sample frame data
    ''' </summary>
    ''' <param name="path">the file path of the wave file</param>
    ''' <exception cref="NotSupportedException">
    ''' the sample encoding of the wave file is not supported by the raw byte decoder
    ''' (for example the G.711 companded data), the caller should use the
    ''' ``WaveFile``/``Sample`` loading path on such files.
    ''' </exception>
    Public Shared Function Open(path As String) As WaveSampleReader
        Dim baseOffset As Long
        Dim channels As Integer
        Dim sampleRate As Integer
        Dim bits As Integer
        Dim isFloat As Boolean
        Dim fmt As FMTSubChunk

        ' resolve the byte position of the sample data section
        ' via the wav parser library
        Using reader As BinaryDataReader = New BinaryDataReader(File.OpenRead(path))
            Dim wav As WaveFile = WaveFile.Open(reader, lazy:=True)
            fmt = wav.fmt

            ' the lazy data chunk has already moved the reader onto the "data" chunk id,
            ' so the raw sample section starts right after the chunk id (4 bytes) and the
            ' chunk size field (4 bytes). this is the explicit form of the
            ' `LazyDataChunk.CalculateOffset(0, scan0:=8)` contract.
            Call LazyDataChunk.MoveToDataChunk(reader)

            baseOffset = reader.Position + 8

            Call wav.Dispose()
        End Using

        channels = fmt.channels
        sampleRate = fmt.SampleRate
        bits = fmt.BitsPerSample
        isFloat = fmt.effectiveAudioFormat = wFormatTag.WAVE_FORMAT_IEEE_FLOAT

        Dim format As SampleFormat = ResolveFormat(fmt)

        If format = SampleFormat.Unsupported OrElse format = SampleFormat.G711 Then
            Throw New NotSupportedException(
                $"the wave file '{path}' is encoded as {fmt.effectiveAudioFormat} {bits} bit, " &
                "it is not supported by the raw sample frame reader, please use the WaveFile/Sample loading path instead!")
        End If

        Dim fileSize As Long = New FileInfo(path).Length
        Dim frameSize As Integer = channels * (bits \ 8)
        Dim frameCount As Long = (fileSize - baseOffset) \ frameSize

        Return New WaveSampleReader(path, channels, sampleRate, bits, isFloat, format, frameCount, baseOffset)
    End Function

    ''' <summary>
    ''' the raw byte decoder which is used for reading the sample frames
    ''' </summary>
    Private Shared Function ResolveFormat(fmt As FMTSubChunk) As SampleFormat
        Select Case fmt.effectiveAudioFormat
            Case wFormatTag.WAVE_FORMAT_PCM
                Select Case fmt.BitsPerSample
                    Case 8 : Return SampleFormat.Pcm8
                    Case 16 : Return SampleFormat.Pcm16
                    Case 24 : Return SampleFormat.Pcm24
                    Case 32 : Return SampleFormat.Pcm32
                    Case Else : Return SampleFormat.Unsupported
                End Select
            Case wFormatTag.WAVE_FORMAT_IEEE_FLOAT
                Select Case fmt.BitsPerSample
                    Case 32 : Return SampleFormat.Float32
                    Case 64 : Return SampleFormat.Float64
                    Case Else : Return SampleFormat.Unsupported
                End Select
            Case wFormatTag.WAVE_FORMAT_ALAW, wFormatTag.WAVE_FORMAT_MULAW
                Return SampleFormat.G711
            Case Else
                Return SampleFormat.Unsupported
        End Select
    End Function

    ''' <summary>
    ''' read a consecutive block of the sample frames on a given channel
    ''' 
    ''' the requested frame range is clipped into the wave file range, so the length of
    ''' the returned array may be smaller than the requested ``count``, and an empty
    ''' array is returned when the start frame is beyond the end of the wave file.
    ''' </summary>
    ''' <param name="channelIndex">the zero based channel index</param>
    ''' <param name="startFrame">the zero based start frame position</param>
    ''' <param name="count">the number of the sample frames for read</param>
    ''' <returns></returns>
    Public Function ReadChannel(channelIndex As Integer, startFrame As Long, count As Long) As Single()
        Dim n As Long = ClipCount(startFrame, count)
        Dim data As Single() = New Single(n - 1) {}

        Call ReadChannel(channelIndex, startFrame, data, 0, n)

        Return data
    End Function

    ''' <summary>
    ''' read a consecutive block of the sample frames on a given channel into a pre-allocated buffer
    ''' 
    ''' the requested frame range is clipped into the wave file range, so fewer than
    ''' ``count`` frames are written into the buffer when the request runs over the end
    ''' of the wave file.
    ''' </summary>
    ''' <param name="channelIndex">the zero based channel index</param>
    ''' <param name="startFrame">the zero based start frame position</param>
    ''' <param name="buffer">the pre-allocated buffer for the sample data</param>
    ''' <param name="offset">the zero based start position of the buffer for write</param>
    ''' <param name="count">the number of the sample frames for read</param>
    Public Sub ReadChannel(channelIndex As Integer, startFrame As Long, buffer As Single(),
                           offset As Integer, count As Long)

        CheckChannel(channelIndex)

        Dim n As Long = ClipCount(startFrame, count)

        If n <= 0 Then
            Return
        End If

        Dim raw As Byte() = ReadRaw(startFrame, n)
        Dim p As Integer = channelIndex * (BitsPerSample \ 8)

        For i As Integer = 0 To n - 1
            buffer(offset + i) = DecodeSample(raw, p)
            p += frameSize
        Next
    End Sub

    ''' <summary>
    ''' read a consecutive block of the sample frames on multiple channels in one pass
    ''' 
    ''' the requested frame range is clipped into the wave file range, so the length of
    ''' each channel array may be smaller than the requested ``count``.
    ''' </summary>
    ''' <param name="startFrame">the zero based start frame position</param>
    ''' <param name="count">the number of the sample frames for read</param>
    ''' <param name="channels">the zero based channel indices for read</param>
    ''' <returns></returns>
    Public Function ReadChannels(startFrame As Long, count As Long, channels As Integer()) As Single()()
        For c As Integer = 0 To channels.Length - 1
            Call CheckChannel(channels(c))
        Next

        Dim n As Long = ClipCount(startFrame, count)
        Dim result As Single()() = New Single(channels.Length - 1)() {}

        For c As Integer = 0 To channels.Length - 1
            result(c) = New Single(n - 1) {}
        Next

        If n <= 0 Then
            Return result
        End If

        Dim raw As Byte() = ReadRaw(startFrame, n)
        Dim positions As Integer() = New Integer(channels.Length - 1) {}

        For c As Integer = 0 To channels.Length - 1
            positions(c) = channels(c) * (BitsPerSample \ 8)
        Next

        For i As Integer = 0 To n - 1
            For c As Integer = 0 To channels.Length - 1
                result(c)(i) = DecodeSample(raw, positions(c))
                positions(c) += frameSize
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' read the raw byte block of the given sample frame range
    ''' </summary>
    Private Function ReadRaw(startFrame As Long, count As Long) As Byte()
        Dim raw As Byte() = New Byte(count * frameSize - 1) {}

        Call fs.Seek(DataOffset + startFrame * frameSize, SeekOrigin.Begin)
        Call ReadFully(raw)

        Return raw
    End Function

    ''' <summary>
    ''' the number of the frames which are actually available on the requested range
    ''' </summary>
    Private Function ClipCount(startFrame As Long, count As Long) As Long
        If startFrame < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(startFrame), $"the start frame position can not be a negative number: {startFrame}!")
        End If

        If count < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(count), $"the frame count can not be a negative number: {count}!")
        End If

        Dim available As Long = FrameCount - startFrame

        If available <= 0 Then
            Return 0
        End If

        Return System.Math.Min(count, available)
    End Function

    Private Sub CheckChannel(channelIndex As Integer)
        If channelIndex < 0 OrElse channelIndex >= Channels Then
            Throw New ArgumentOutOfRangeException(NameOf(channelIndex),
                $"the channel index {channelIndex} is outside of the channel range [0, {Channels - 1}] of the wave file '{Path}'!")
        End If
    End Sub

    ''' <summary>
    ''' decode a single sample value from the raw byte block.
    ''' 
    ''' the normalization convention is the same thing as the ``Sample`` parsers of the
    ''' wav library (see the ``Parse8BitSample``/``Parse16BitSample``/``Parse24BitSample``/
    ''' ``Parse32BitPCMSample``/``Parse32BitSample``/``Parse64BitSample`` functions), so
    ''' the two loading paths of the library give the same sample values:
    ''' 
    ''' 1. the 8 bit PCM is unsigned and it is offset by 128
    ''' 2. all of the integer PCM data is divided by the half range of its bit depth
    ''' 3. the IEEE float data is used as it is
    ''' 
    ''' the wave sample data is little endian by the RIFF convention, and the bit
    ''' conversion of the raw byte block relies on the little endian host, which is the
    ''' same premise of the ``BinaryDataReader`` of current library.
    ''' </summary>
    Private Function DecodeSample(raw As Byte(), p As Integer) As Single
        Select Case _format
            Case SampleFormat.Pcm8
                Return CSng(raw(p) - 128) / 128.0F
            Case SampleFormat.Pcm16
                Return CSng(BitConverter.ToInt16(raw, p)) / 32768.0F
            Case SampleFormat.Pcm24
                Return CSng(ReadInt24(raw, p)) / 8388608.0F
            Case SampleFormat.Pcm32
                Return CSng(BitConverter.ToInt32(raw, p)) / 2147483648.0F
            Case SampleFormat.Float32
                Return BitConverter.ToSingle(raw, p)
            Case SampleFormat.Float64
                Return CSng(BitConverter.ToDouble(raw, p))
            Case Else
                Throw New NotSupportedException($"the sample format '{_format}' can not be decoded by the raw sample frame reader!")
        End Select
    End Function

    ''' <summary>
    ''' read a little endian 24 bit signed integer with the sign extension
    ''' </summary>
    Private Shared Function ReadInt24(raw As Byte(), p As Integer) As Integer
        ' IMPORTANT: the shift operators of visual basic keeps the type of its left operand,
        ' so a ``Byte`` left operand gives a ``Byte`` result: ``0xFF << 16`` evaluates to zero
        ' instead of 0xFF0000. the bytes are therefore widened into the integer first, this is
        ' also what the ``BinaryDataReader.ReadByte`` based ``ReadInt24Normalized`` does.
        Dim b0 As Integer = raw(p)
        Dim b1 As Integer = raw(p + 1)
        Dim b2 As Integer = raw(p + 2)

        Dim value As Integer = b0 Or (b1 << 8) Or (b2 << 16)

        ' sign-extend from 24-bit to 32-bit
        If (value And &H800000) <> 0 Then
            value = value Or &HFF000000I
        End If

        Return value
    End Function

    Private Sub ReadFully(raw As Byte())
        Dim offset As Integer = 0

        While offset < raw.Length
            Dim n As Integer = fs.Read(raw, offset, raw.Length - offset)

            If n <= 0 Then
                Throw New EndOfStreamException($"unexpected end of the wave file stream [{offset}]!")
            End If

            offset += n
        End While
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If fs IsNot Nothing Then
            Call fs.Dispose()
            fs = Nothing
        End If
    End Sub
End Class
