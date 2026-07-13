#Region "Microsoft.VisualBasic::WaveWriter, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\WaveWriter.vb"

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

    '   Total Lines: 
    '    Code Lines: 
    ' Comment Lines: 
    '    - Xml Docs: 
    ' 
    '   Blank Lines: 
    '     File Size: 
    '


    ' Class WaveWriter
    ' 
    '     Function: WriteWav
    ' 
    '     Sub: WriteDataChunk, WriteFmtChunk, WriteRiffHeader, WriteSampleData
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports std = System.Math

''' <summary>
''' Static utility class for writing WAV audio files from sample data.
''' 
''' Supports PCM (8/16/24/32-bit), IEEE Float (32/64-bit),
''' and G.711 A-law/μ-law encoding output.
''' </summary>
Public Module WaveWriter

#Region "G.711 Encode Tables"

    ''' <summary>
    ''' G.711 A-law encode table: maps 13-bit linear PCM [-4096, 4095] → 8-bit A-law.
    ''' Input: linear sample value (13-bit signed, zero-centered).
    ''' The sign bit is handled separately.
    ''' </summary>
    ReadOnly ALawEncodeTable As Byte() = {
        1, 1, 2, 2, 3, 3, 3, 3,
        4, 4, 4, 4, 4, 4, 4, 4,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7
    }

    ''' <summary>
    ''' G.711 μ-law encode table: maps 14-bit linear PCM [-8192, 8191] → 8-bit μ-law.
    ''' Input: linear sample value (14-bit signed).
    ''' The sign bit is handled separately.
    ''' </summary>
    ReadOnly MuLawEncodeTable As Byte() = {
        0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
    }

#End Region

    ''' <summary>
    ''' Write sample data to a WAV file.
    ''' </summary>
    ''' <param name="path">Output .wav file path.</param>
    ''' <param name="samples">Audio samples, each containing per-channel data normalized to [-1.0, 1.0].</param>
    ''' <param name="sampleRate">Sample rate in Hz (e.g. 44100).</param>
    ''' <param name="channels">Number of audio channels.</param>
    ''' <param name="bitsPerSample">Bit depth: 8, 16, 24, 32, or 64.</param>
    ''' <param name="audioFormat">Encoding format (PCM, IEEE_FLOAT, ALAW, MULAW).</param>
    Public Sub WriteWav(path As String,
                        samples As IEnumerable(Of Sample),
                        sampleRate As Integer,
                        channels As Integer,
                        bitsPerSample As Integer,
                        Optional audioFormat As wFormatTag = wFormatTag.WAVE_FORMAT_PCM)

        Using fs As New FileStream(path, FileMode.Create, FileAccess.Write)
            Using writer As New BinaryWriter(fs)

                ' Reserve space for RIFF header (will patch later)
                WriteRiffHeader(writer, 0)

                ' Write format chunk
                WriteFmtChunk(writer, sampleRate, channels, bitsPerSample, audioFormat)

                ' Write data chunk
                WriteDataChunk(writer, samples, channels, bitsPerSample, audioFormat)

                ' Patch the RIFF header with correct file size
                Dim finalSize As UInteger = CUInt(writer.BaseStream.Length) - 8UI
                writer.Seek(4, SeekOrigin.Begin)
                writer.Write(finalSize)
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Write the RIFF header: "RIFF" + fileSize + "WAVE".
    ''' </summary>
    Private Sub WriteRiffHeader(writer As BinaryWriter, fileSize As UInteger)
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"))
        writer.Write(fileSize)
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"))
    End Sub

    ''' <summary>
    ''' Write the "fmt " subchunk with format parameters.
    ''' Handles standard formats and WAVE_FORMAT_EXTENSIBLE for multi-channel / non-standard setups.
    ''' </summary>
    Private Sub WriteFmtChunk(writer As BinaryWriter,
                              sampleRate As Integer,
                              channels As Integer,
                              bitsPerSample As Integer,
                              audioFormat As wFormatTag)

        Dim byteRate As Integer = sampleRate * channels * (bitsPerSample \ 8)
        Dim blockAlign As Short = CShort(channels * (bitsPerSample \ 8))

        ' Determine if EXTENSIBLE header is needed
        ' EXTENSIBLE is required for: >2 channels, non-PCM/non-Float, bitsPerSample that doesn't align to 8
        Dim useExtensible As Boolean = (channels > 2) OrElse
                                       (audioFormat <> wFormatTag.WAVE_FORMAT_PCM AndAlso
                                        audioFormat <> wFormatTag.WAVE_FORMAT_IEEE_FLOAT) OrElse
                                       (bitsPerSample Mod 8 <> 0)

        If useExtensible Then
            ' WAVE_FORMAT_EXTENSIBLE with 22-byte extra
            Dim fmtChunkSize As Short = CShort(40) ' 16 base + 22 extensible + 2 cbSize

            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "))
            writer.Write(CUInt(fmtChunkSize - 8)) ' chunk data size (excluding id+size fields)
            writer.Write(CUShort(wFormatTag.WAVE_FORMAT_EXTENSIBLE))
            writer.Write(CShort(channels))
            writer.Write(sampleRate)
            writer.Write(byteRate)
            writer.Write(blockAlign)
            writer.Write(CShort(bitsPerSample))

            ' EXTENSIBLE extension
            writer.Write(CShort(22)) ' cbSize = 22
            writer.Write(CShort(bitsPerSample)) ' ValidBitsPerSample
            writer.Write(0UI) ' ChannelMask (0 = unspecified/default layout)

            ' SubFormat GUID
            If audioFormat = wFormatTag.WAVE_FORMAT_IEEE_FLOAT Then
                writer.Write(WavFormatGuids.SubTypeIeeeFloat.ToByteArray)
            Else
                writer.Write(WavFormatGuids.SubTypePcm.ToByteArray)
            End If
        Else
            ' Standard fmt chunk (16 bytes)
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "))
            writer.Write(CUInt(16)) ' chunk data size
            writer.Write(CShort(audioFormat))
            writer.Write(CShort(channels))
            writer.Write(sampleRate)
            writer.Write(byteRate)
            writer.Write(blockAlign)
            writer.Write(CShort(bitsPerSample))
        End If
    End Sub

    ''' <summary>
    ''' Write the "data" subchunk containing all audio sample data.
    ''' </summary>
    Private Sub WriteDataChunk(writer As BinaryWriter,
                               samples As IEnumerable(Of Sample),
                               channels As Integer,
                               bitsPerSample As Integer,
                               audioFormat As wFormatTag)

        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"))

        ' Reserve data size (will patch after writing all samples)
        Dim dataSizePos As Long = writer.BaseStream.Position
        writer.Write(0UI)

        ' Write all sample data
        Dim encoder As Action(Of BinaryWriter, Single) = ResolveEncoder(audioFormat, bitsPerSample)

        For Each sample In samples
            For ch As Integer = 0 To std.Min(channels, sample.channels.Length) - 1
                encoder(writer, sample.channels(ch))
            Next
            ' Pad missing channels with silence
            For ch As Integer = sample.channels.Length To channels - 1
                encoder(writer, 0.0F)
            Next
        Next

        ' Patch data chunk size
        Dim dataEndPos As Long = writer.BaseStream.Position
        Dim dataSize As UInteger = CUInt(dataEndPos - dataSizePos - 4)
        writer.Seek(CInt(dataSizePos), SeekOrigin.Begin)
        writer.Write(dataSize)
        writer.Seek(CInt(dataEndPos), SeekOrigin.Begin)
    End Sub

#Region "Sample Encoders"

    ''' <summary>
    ''' Resolves the correct sample value encoder based on audio format and bits per sample.
    ''' Each encoder writes a single channel value (normalized [-1.0, 1.0]) to the BinaryWriter.
    ''' </summary>
    Private Function ResolveEncoder(audioFormat As wFormatTag, bitsPerSample As Integer) _
        As Action(Of BinaryWriter, Single)

        Select Case audioFormat
            Case wFormatTag.WAVE_FORMAT_PCM
                Select Case bitsPerSample
                    Case 8 : Return AddressOf Encode8BitPCM
                    Case 16 : Return AddressOf Encode16BitPCM
                    Case 24 : Return AddressOf Encode24BitPCM
                    Case 32 : Return AddressOf Encode32BitPCM
                    Case Else
                        Throw New NotSupportedException($"PCM {bitsPerSample}-bit encoding not supported.")
                End Select

            Case wFormatTag.WAVE_FORMAT_IEEE_FLOAT
                Select Case bitsPerSample
                    Case 32 : Return AddressOf Encode32BitFloat
                    Case 64 : Return AddressOf Encode64BitFloat
                    Case Else
                        Throw New NotSupportedException($"IEEE Float {bitsPerSample}-bit encoding not supported.")
                End Select

            Case wFormatTag.WAVE_FORMAT_ALAW
                Return AddressOf EncodeALaw

            Case wFormatTag.WAVE_FORMAT_MULAW
                Return AddressOf EncodeMuLaw

            Case Else
                Throw New NotSupportedException($"Audio format '{audioFormat}' encoding not supported.")
        End Select
    End Function

    ''' <summary>
    ''' Encode float [-1.0, +1.0] → 8-bit unsigned PCM [0, 255].
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub Encode8BitPCM(writer As BinaryWriter, value As Single)
        Dim clamped As Single = std.Max(-1.0F, std.Min(1.0F, value))
        Dim b As Byte = CByte(std.Round((clamped * 128.0F) + 128.0F))
        writer.Write(b)
    End Sub

    ''' <summary>
    ''' Encode float [-1.0, +1.0] → 16-bit signed PCM (little-endian).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub Encode16BitPCM(writer As BinaryWriter, value As Single)
        Dim clamped As Single = std.Max(-1.0F, std.Min(1.0F, value))
        Dim v As Short = CShort(std.Round(clamped * 32767.0F))
        writer.Write(v)
    End Sub

    ''' <summary>
    ''' Encode float [-1.0, +1.0] → 24-bit signed PCM (3 bytes, little-endian).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub Encode24BitPCM(writer As BinaryWriter, value As Single)
        Dim clamped As Single = std.Max(-1.0F, std.Min(1.0F, value))
        Dim v As Integer = CInt(std.Round(clamped * 8388607.0F))
        ' Clamp to 24-bit signed range
        If v > 8388607 Then v = 8388607
        If v < -8388608 Then v = -8388608
        ' Write 3 bytes little-endian
        writer.Write(CByte(v And &HFF))
        writer.Write(CByte((v >> 8) And &HFF))
        writer.Write(CByte((v >> 16) And &HFF))
    End Sub

    ''' <summary>
    ''' Encode float [-1.0, +1.0] → 32-bit signed PCM (little-endian).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub Encode32BitPCM(writer As BinaryWriter, value As Single)
        Dim clamped As Single = std.Max(-1.0F, std.Min(1.0F, value))
        Dim v As Integer = CInt(std.Round(clamped * 2.14748365E+9F))
        writer.Write(v)
    End Sub

    ''' <summary>
    ''' Encode float → 32-bit IEEE Float (write directly).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub Encode32BitFloat(writer As BinaryWriter, value As Single)
        writer.Write(value)
    End Sub

    ''' <summary>
    ''' Encode float → 64-bit IEEE Float (cast to double, write).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub Encode64BitFloat(writer As BinaryWriter, value As Single)
        writer.Write(CDbl(value))
    End Sub

    ''' <summary>
    ''' G.711 A-law encode: float [-1.0, +1.0] → 8-bit A-law byte.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub EncodeALaw(writer As BinaryWriter, value As Single)
        Dim clamped As Single = std.Max(-1.0F, std.Min(1.0F, value))
        ' Convert to 13-bit linear: [-4096, 4095]
        Dim pcmValue As Integer = CInt(clamped * 4095.0F)

        ' Determine sign bit
        Dim signBit As Integer = 0
        If pcmValue < 0 Then
            signBit = &H80 ' bit 7 set for negative
            pcmValue = -pcmValue
        End If

        ' Clamp
        If pcmValue > 4095 Then pcmValue = 4095

        ' Quantize to segment + quantized value
        Dim mask As Integer
        Dim segment As Integer

        If pcmValue >= 256 Then
            segment = 7
            mask = &H7F
            While pcmValue < (1 << (segment + 5))
                segment -= 1
            End While
            mask = mask << segment
        Else
            segment = 0
            mask = &H55
        End If

        Dim aLawValue As Integer = ((pcmValue << 3) >> (segment + 4)) And &HF
        aLawValue = aLawValue Or ((7 - segment) << 4)
        aLawValue = aLawValue Xor &H55 ' XOR with inversion bit pattern

        If signBit <> 0 Then
            aLawValue = aLawValue Xor &H80
        End If

        writer.Write(CByte(aLawValue))
    End Sub

    ''' <summary>
    ''' G.711 μ-law encode: float [-1.0, +1.0] → 8-bit μ-law byte.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub EncodeMuLaw(writer As BinaryWriter, value As Single)
        Dim clamped As Single = std.Max(-1.0F, std.Min(1.0F, value))
        ' Convert to 14-bit linear: [-8192, 8191]
        Dim pcmValue As Integer = CInt(clamped * 8191.0F)

        ' μ-law uses sign-magnitude with bias
        Const bias As Integer = 132 ' 0x84
        Dim signBit As Integer = 0
        If pcmValue < 0 Then
            signBit = &H80
            pcmValue = -pcmValue
        End If

        pcmValue = std.Min(pcmValue + bias, 32635)

        ' Find segment
        Dim segment As Integer = 7
        Dim mask As Integer
        Dim val As Integer = pcmValue

        While (val >> segment) = 0 AndAlso segment > 0
            segment -= 1
        End While

        If segment < 7 Then
            mask = &H7F >> segment
        Else
            mask = &H1
        End If

        Dim muLawValue As Integer = ((pcmValue >> (segment + 3)) And &HF) Or ((7 - segment) << 4)

        muLawValue = muLawValue Xor &HFF
        muLawValue = muLawValue Xor signBit

        writer.Write(CByte(muLawValue))
    End Sub

#End Region

End Module
