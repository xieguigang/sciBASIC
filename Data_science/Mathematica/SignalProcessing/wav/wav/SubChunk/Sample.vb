#Region "Microsoft.VisualBasic::3626eece97e4cfc971850f542b1d2263, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Sample.vb"

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

    '   Total Lines: 357
    '    Code Lines: 247 (69.19%)
    ' Comment Lines: 66 (18.49%)
    '    - Xml Docs: 95.45%
    ' 
    '   Blank Lines: 44 (12.32%)
    '     File Size: 15.33 KB


    ' Structure Sample
    ' 
    '     Properties: left, right
    ' 
    '     Function: Parse16Bit, Parse16BitSample, Parse24Bit, Parse24BitSample, Parse32Bit
    '               Parse32BitPCM, Parse32BitPCMSample, Parse32BitSample, Parse64Bit, Parse64BitSample
    '               Parse8Bit, Parse8BitSample, ParseALaw, ParseALawSample, ParseMuLaw
    '               ParseMuLawSample, ReadInt24Normalized, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure Sample

    ''' <summary>
    ''' Per-channel sample data, normalized to [-1.0, 1.0] for all formats.
    ''' </summary>
    Dim channels As Single()

    Public ReadOnly Property left As Single
        Get
            Return channels(0)
        End Get
    End Property

    Public ReadOnly Property right As Single
        Get
            Return channels(1)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return channels.GetJson
    End Function

#Region "G.711 Decode Tables"

    ''' <summary>
    ''' G.711 A-law to 16-bit linear PCM decode table.
    ''' Decoded values are in the range [-32256, 32256].
    ''' </summary>
    Friend Shared ReadOnly ALawDecodeTable As Short() = {
        -5504, -5248, -6016, -5760, -4480, -4224, -4992, -4736,
        -7552, -7296, -8064, -7808, -6528, -6272, -7040, -6784,
        -2752, -2624, -3008, -2880, -2240, -2112, -2496, -2368,
        -3776, -3648, -4032, -3904, -3264, -3136, -3520, -3392,
        -22016, -20992, -24064, -23040, -17920, -16896, -19968, -18944,
        -30208, -29184, -32256, -31232, -26112, -25088, -28160, -27136,
        -11008, -10496, -12032, -11520, -8960, -8448, -9984, -9472,
        -15104, -14592, -16128, -15616, -13056, -12544, -14080, -13568,
        -344, -328, -376, -360, -280, -264, -312, -296,
        -472, -456, -504, -488, -408, -392, -440, -424,
        -88, -72, -120, -104, -24, -8, -56, -40,
        -216, -200, -248, -232, -152, -136, -184, -168,
        -1376, -1312, -1504, -1440, -1120, -1056, -1248, -1184,
        -1888, -1824, -2016, -1952, -1632, -1568, -1760, -1696,
        -688, -656, -752, -720, -560, -528, -624, -592,
        -944, -912, -1008, -976, -816, -784, -880, -848,
        5504, 5248, 6016, 5760, 4480, 4224, 4992, 4736,
        7552, 7296, 8064, 7808, 6528, 6272, 7040, 6784,
        2752, 2624, 3008, 2880, 2240, 2112, 2496, 2368,
        3776, 3648, 4032, 3904, 3264, 3136, 3520, 3392,
        22016, 20992, 24064, 23040, 17920, 16896, 19968, 18944,
        30208, 29184, 32256, 31232, 26112, 25088, 28160, 27136,
        11008, 10496, 12032, 11520, 8960, 8448, 9984, 9472,
        15104, 14592, 16128, 15616, 13056, 12544, 14080, 13568,
        344, 328, 376, 360, 280, 264, 312, 296,
        472, 456, 504, 488, 408, 392, 440, 424,
        88, 72, 120, 104, 24, 8, 56, 40,
        216, 200, 248, 232, 152, 136, 184, 168,
        1376, 1312, 1504, 1440, 1120, 1056, 1248, 1184,
        1888, 1824, 2016, 1952, 1632, 1568, 1760, 1696,
        688, 656, 752, 720, 560, 528, 624, 592,
        944, 912, 1008, 976, 816, 784, 880, 848
    }

    ''' <summary>
    ''' G.711 μ-law to 16-bit linear PCM decode table.
    ''' Decoded values are in the range [-32124, 32124].
    ''' </summary>
    Friend Shared ReadOnly MuLawDecodeTable As Short() = {
        -32124, -31100, -30076, -29052, -28028, -27004, -25980, -24956,
        -23932, -22908, -21884, -20860, -19836, -18812, -17788, -16764,
        -15996, -15484, -14972, -14460, -13948, -13436, -12924, -12412,
        -11900, -11388, -10876, -10364, -9852, -9340, -8828, -8316,
        -7932, -7676, -7420, -7164, -6908, -6652, -6396, -6140,
        -5884, -5628, -5372, -5116, -4860, -4604, -4348, -4092,
        -3900, -3772, -3644, -3516, -3388, -3260, -3132, -3004,
        -2876, -2748, -2620, -2492, -2364, -2236, -2108, -1980,
        -1884, -1820, -1756, -1692, -1628, -1564, -1500, -1436,
        -1372, -1308, -1244, -1180, -1116, -1052, -988, -924,
        -876, -844, -812, -780, -748, -716, -684, -652,
        -620, -588, -556, -524, -492, -460, -428, -396,
        -372, -356, -340, -324, -308, -292, -276, -260,
        -244, -228, -212, -196, -180, -164, -148, -132,
        -120, -112, -104, -96, -88, -80, -72, -64,
        -56, -48, -40, -32, -24, -16, -8, 0,
        32124, 31100, 30076, 29052, 28028, 27004, 25980, 24956,
        23932, 22908, 21884, 20860, 19836, 18812, 17788, 16764,
        15996, 15484, 14972, 14460, 13948, 13436, 12924, 12412,
        11900, 11388, 10876, 10364, 9852, 9340, 8828, 8316,
        7932, 7676, 7420, 7164, 6908, 6652, 6396, 6140,
        5884, 5628, 5372, 5116, 4860, 4604, 4348, 4092,
        3900, 3772, 3644, 3516, 3388, 3260, 3132, 3004,
        2876, 2748, 2620, 2492, 2364, 2236, 2108, 1980,
        1884, 1820, 1756, 1692, 1628, 1564, 1500, 1436,
        1372, 1308, 1244, 1180, 1116, 1052, 988, 924,
        876, 844, 812, 780, 748, 716, 684, 652,
        620, 588, 556, 524, 492, 460, 428, 396,
        372, 356, 340, 324, 308, 292, 276, 260,
        244, 228, 212, 196, 180, 164, 148, 132,
        120, 112, 104, 96, 88, 80, 72, 64,
        56, 48, 40, 32, 24, 16, 8, 0
    }

#End Region

#Region "Normalization Constants"

    ' Normalization divisors for PCM formats
    Private Const Norm8Bit As Single = 128.0F          ' UInt8 range [0,255] → offset by 128, divide by 128
    Private Const Norm16Bit As Single = 32768.0F       ' Int16 range [-32768,32767]
    Private Const Norm24Bit As Single = 8388608.0F     ' Int24 range [-8388608,8388607]
    Private Const Norm32BitPCM As Single = 2147483648.0F ' Int32 range [-2147483648,2147483647]
    Private Const NormG711 As Single = 32768.0F        ' G.711 decoded to Int16

#End Region

#Region "Memory-mode Iterators (full data load)"

    ''' <summary>
    ''' 8-bit unsigned PCM: value [0,255] → normalized [-1.0, +1.0]
    ''' </summary>
    Friend Shared Iterator Function Parse8Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 1

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Dim channelData As Single() = New Single(channels - 1) {}
            For i As Integer = 0 To channels - 1
                channelData(i) = CSng(wav.ReadByte - 128) / Norm8Bit
            Next
            Yield New Sample With {.channels = channelData}
        Loop
    End Function

    ''' <summary>
    ''' 16-bit signed PCM: value [-32768,32767] → normalized [-1.0, +1.0]
    ''' </summary>
    Friend Shared Iterator Function Parse16Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 2

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Yield New Sample With {
                .channels = wav.ReadInt16s(channels).Select(Function(a) CSng(a) / Norm16Bit).ToArray
            }
        Loop
    End Function

    ''' <summary>
    ''' 24-bit signed PCM (3 bytes per sample, little-endian).
    ''' Value [-8388608, 8388607] → normalized [-1.0, +1.0]
    ''' </summary>
    Friend Shared Iterator Function Parse24Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 3

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Dim channelData As Single() = New Single(channels - 1) {}
            For i As Integer = 0 To channels - 1
                channelData(i) = ReadInt24Normalized(wav)
            Next
            Yield New Sample With {.channels = channelData}
        Loop
    End Function

    ''' <summary>
    ''' 32-bit signed PCM (integer): value [-2147483648, 2147483647] → normalized [-1.0, +1.0]
    ''' </summary>
    Friend Shared Iterator Function Parse32BitPCM(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 4

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Dim channelData As Single() = New Single(channels - 1) {}
            For i As Integer = 0 To channels - 1
                channelData(i) = CSng(wav.ReadInt32) / Norm32BitPCM
            Next
            Yield New Sample With {.channels = channelData}
        Loop
    End Function

    ''' <summary>
    ''' 32-bit IEEE Float: already in range [1.0, -1.0], read directly.
    ''' </summary>
    Friend Shared Iterator Function Parse32Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 4

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Yield Parse32BitSample(wav, channels)
        Loop
    End Function

    ''' <summary>
    ''' 64-bit IEEE Float (double): read as Double, cast to Single.
    ''' </summary>
    Friend Shared Iterator Function Parse64Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 8

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Dim channelData As Single() = New Single(channels - 1) {}
            For i As Integer = 0 To channels - 1
                channelData(i) = CSng(BitConverter.ToDouble(wav.ReadBytes(8), 0))
            Next
            Yield New Sample With {.channels = channelData}
        Loop
    End Function

    ''' <summary>
    ''' G.711 A-law 8-bit encoding.
    ''' </summary>
    Friend Shared Iterator Function ParseALaw(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 1

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Dim channelData As Single() = New Single(channels - 1) {}
            For i As Integer = 0 To channels - 1
                channelData(i) = CSng(ALawDecodeTable(wav.ReadByte)) / NormG711
            Next
            Yield New Sample With {.channels = channelData}
        Loop
    End Function

    ''' <summary>
    ''' G.711 μ-law 8-bit encoding.
    ''' </summary>
    Friend Shared Iterator Function ParseMuLaw(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 1

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Dim channelData As Single() = New Single(channels - 1) {}
            For i As Integer = 0 To channels - 1
                channelData(i) = CSng(MuLawDecodeTable(wav.ReadByte)) / NormG711
            Next
            Yield New Sample With {.channels = channelData}
        Loop
    End Function

#End Region

#Region "Single-Sample Parsers (for lazy/streaming mode)"

    ''' <summary>
    ''' Read a single 8-bit PCM sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse8BitSample(wav As BinaryDataReader, channels As Integer) As Sample
        Dim channelData As Single() = New Single(channels - 1) {}
        For i As Integer = 0 To channels - 1
            channelData(i) = CSng(wav.ReadByte - 128) / Norm8Bit
        Next
        Return New Sample With {.channels = channelData}
    End Function

    ''' <summary>
    ''' Read a single 16-bit PCM sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse16BitSample(wav As BinaryDataReader, channels As Integer) As Sample
        Return New Sample With {
            .channels = wav.ReadInt16s(channels).Select(Function(a) CSng(a) / Norm16Bit).ToArray
        }
    End Function

    ''' <summary>
    ''' Read a single 24-bit PCM sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse24BitSample(wav As BinaryDataReader, channels As Integer) As Sample
        Dim channelData As Single() = New Single(channels - 1) {}
        For i As Integer = 0 To channels - 1
            channelData(i) = ReadInt24Normalized(wav)
        Next
        Return New Sample With {.channels = channelData}
    End Function

    ''' <summary>
    ''' Read a single 32-bit IEEE Float sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse32BitSample(wav As BinaryDataReader, channels As Integer) As Sample
        Return New Sample With {
            .channels = wav.ReadSingles(channels)
        }
    End Function

    ''' <summary>
    ''' Read a single 32-bit PCM (integer) sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse32BitPCMSample(wav As BinaryDataReader, channels As Integer) As Sample
        Dim channelData As Single() = New Single(channels - 1) {}
        For i As Integer = 0 To channels - 1
            channelData(i) = CSng(wav.ReadInt32) / Norm32BitPCM
        Next
        Return New Sample With {.channels = channelData}
    End Function

    ''' <summary>
    ''' Read a single 64-bit IEEE Float sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse64BitSample(wav As BinaryDataReader, channels As Integer) As Sample
        Dim channelData As Single() = New Single(channels - 1) {}
        For i As Integer = 0 To channels - 1
            channelData(i) = CSng(BitConverter.ToDouble(wav.ReadBytes(8), 0))
        Next
        Return New Sample With {.channels = channelData}
    End Function

    ''' <summary>
    ''' Read a single G.711 A-law sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function ParseALawSample(wav As BinaryDataReader, channels As Integer) As Sample
        Dim channelData As Single() = New Single(channels - 1) {}
        For i As Integer = 0 To channels - 1
            channelData(i) = CSng(ALawDecodeTable(wav.ReadByte)) / NormG711
        Next
        Return New Sample With {.channels = channelData}
    End Function

    ''' <summary>
    ''' Read a single G.711 μ-law sample frame.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function ParseMuLawSample(wav As BinaryDataReader, channels As Integer) As Sample
        Dim channelData As Single() = New Single(channels - 1) {}
        For i As Integer = 0 To channels - 1
            channelData(i) = CSng(MuLawDecodeTable(wav.ReadByte)) / NormG711
        Next
        Return New Sample With {.channels = channelData}
    End Function

#End Region

#Region "Helpers"

    ''' <summary>
    ''' Read 3 bytes (little-endian) as a signed 24-bit integer, normalize to [-1.0, +1.0].
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Shared Function ReadInt24Normalized(wav As BinaryDataReader) As Single
        Dim b0 As Integer = wav.ReadByte
        Dim b1 As Integer = wav.ReadByte
        Dim b2 As Integer = wav.ReadByte
        ' Assemble little-endian: b0 | b1<<8 | b2<<16
        Dim value As Integer = b0 Or (b1 << 8) Or (b2 << 16)
        ' Sign-extend from 24-bit to 32-bit
        If (value And &H800000) <> 0 Then
            value = value Or &HFF000000I
        End If
        Return CSng(value) / Norm24Bit
    End Function

#End Region

End Structure
