#Region "Microsoft.VisualBasic::375d059de8b1ab2342c6111fa887c983, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\FMT.vb"

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

    '   Total Lines: 72
    '    Code Lines: 28 (38.89%)
    ' Comment Lines: 38 (52.78%)
    '    - Xml Docs: 92.11%
    ' 
    '   Blank Lines: 6 (8.33%)
    '     File Size: 2.20 KB


    ' Class FMTSubChunk
    ' 
    '     Properties: audioFormat, BitsPerSample, BlockAlign, ByteRate, channels
    '                 isPCM, SampleRate
    ' 
    '     Function: ParseChunk
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' The "fmt " subchunk describes the sound data's format
''' </summary>
Public Class FMTSubChunk : Inherits SubChunk

    ''' <summary>
    ''' PCM = 1 (i.e. Linear quantization) Values other than 1 indicate some form of compression.
    ''' </summary>
    ''' <returns></returns>
    Public Property audioFormat As wFormatTag
    ''' <summary>
    ''' Number of audio channels (1=Mono, 2=Stereo, 6=5.1 surround, etc.)
    ''' </summary>
    ''' <returns></returns>
    Public Property channels As Integer
    ''' <summary>
    ''' 22.05KHz/44.1kHz/48KHz
    ''' </summary>
    ''' <returns></returns>
    Public Property SampleRate As Integer
    ''' <summary>
    ''' (bytes_per sec) 音频的码率，每秒播放的字节数。
    ''' 
    ''' ```
    ''' SampleRate * NumChannels * BitsPerSample / 8
    ''' ```
    ''' </summary>
    ''' <returns></returns>
    Public Property ByteRate As Integer
    ''' <summary>
    ''' ```
    ''' NumChannels * BitsPerSample / 8
    ''' ```
    ''' 
    ''' The number Of bytes For one sample including
    ''' all channels. I wonder what happens When
    ''' this number isn't an integer?
    ''' </summary>
    ''' <returns></returns>
    Public Property BlockAlign As Integer
    ''' <summary>
    ''' 8 bits = 8, 16 bits = 16, etc.
    ''' </summary>
    ''' <returns></returns>
    Public Property BitsPerSample As Integer

    ' ===== EXTENSIBLE format fields =====

    ''' <summary>
    ''' Size of the extension (0 for standard formats, >= 22 for EXTENSIBLE)
    ''' </summary>
    Public Property cbSize As Integer
    ''' <summary>
    ''' Number of valid bits per sample (EXTENSIBLE only, may differ from BitsPerSample)
    ''' </summary>
    Public Property ValidBitsPerSample As Integer
    ''' <summary>
    ''' Speaker position bitmask (EXTENSIBLE only)
    ''' </summary>
    Public Property ChannelMask As Integer
    ''' <summary>
    ''' SubFormat GUID (EXTENSIBLE only, maps to PCM or IEEE Float)
    ''' </summary>
    Public Property SubFormat As Guid

    ' internal backing field for effective audio format
    Friend _effectiveAudioFormat As wFormatTag

    ''' <summary>
    ''' Gets the effective audio format.
    ''' For EXTENSIBLE format, this resolves from the SubFormat GUID.
    ''' For standard formats, this equals <see cref="audioFormat"/>.
    ''' </summary>
    Public ReadOnly Property effectiveAudioFormat As wFormatTag
        Get
            Return _effectiveAudioFormat
        End Get
    End Property

    ''' <summary>
    ''' Pulse Code Modulation
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property isPCM As Boolean
        Get
            Return effectiveAudioFormat = wFormatTag.WAVE_FORMAT_PCM
        End Get
    End Property

    ''' <summary>
    ''' Number of bytes per sample frame (channels * bitsPerSample / 8).
    ''' Handles non-integer byte counts (e.g. 24-bit = 3 bytes/channel).
    ''' </summary>
    Public ReadOnly Property sampleSizeBytes As Integer
        Get
            Select Case BitsPerSample
                Case 8 : Return channels * 1
                Case 16 : Return channels * 2
                Case 24 : Return channels * 3
                Case 32 : Return channels * 4
                Case 64 : Return channels * 8
                Case Else : Return CInt(channels * (BitsPerSample / 8))
            End Select
        End Get
    End Property

    Friend Shared Function ParseChunk(wav As BinaryDataReader) As FMTSubChunk
        Dim subchunk1ID As String = wav.ReadString(4)

        ' number data is in little endian
        wav.ByteOrder = ByteOrder.LittleEndian

        Dim fmt As New FMTSubChunk With {
            .chunkID = subchunk1ID,
            .chunkSize = wav.ReadInt32,
            .audioFormat = CType(wav.ReadInt16, wFormatTag),
            .channels = CInt(wav.ReadInt16),
            .SampleRate = wav.ReadInt32,
            .ByteRate = wav.ReadInt32,
            .BlockAlign = CInt(wav.ReadInt16),
            .BitsPerSample = CInt(wav.ReadInt16)
        }

        ' Handle WAVE_FORMAT_EXTENSIBLE
        If fmt.audioFormat = wFormatTag.WAVE_FORMAT_EXTENSIBLE Then
            fmt.cbSize = CInt(wav.ReadInt16)
            fmt.ValidBitsPerSample = CInt(wav.ReadInt16)
            fmt.ChannelMask = wav.ReadInt32
            fmt.SubFormat = New Guid(wav.ReadBytes(16))

            ' Map SubFormat GUID to standard encoding
            If fmt.SubFormat = WavFormatGuids.SubTypePcm Then
                fmt._effectiveAudioFormat = wFormatTag.WAVE_FORMAT_PCM
            ElseIf fmt.SubFormat = WavFormatGuids.SubTypeIeeeFloat Then
                fmt._effectiveAudioFormat = wFormatTag.WAVE_FORMAT_IEEE_FLOAT
            Else
                ' Unknown SubFormat, keep as EXTENSIBLE (will fall through to error)
                fmt._effectiveAudioFormat = wFormatTag.WAVE_FORMAT_EXTENSIBLE
            End If

            ' Skip any extra bytes beyond the base EXTENSIBLE struct (cbSize - 22)
            Dim extraBytes As Integer = fmt.cbSize - 22
            If extraBytes > 0 Then
                wav.Seek(extraBytes, SeekOrigin.Current)
            End If
        Else
            fmt._effectiveAudioFormat = fmt.audioFormat
            fmt.SubFormat = Guid.Empty
        End If

        Return fmt
    End Function
End Class
