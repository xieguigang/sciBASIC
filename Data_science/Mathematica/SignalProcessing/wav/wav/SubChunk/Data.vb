#Region "Microsoft.VisualBasic::e0f4abd942fb84040be50a751f4851a4, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Data.vb"

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

    '   Total Lines: 188
    '    Code Lines: 136 (72.34%)
    ' Comment Lines: 11 (5.85%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 41 (21.81%)
    '     File Size: 7.16 KB


    ' Class DataSubChunk
    ' 
    '     Properties: data
    ' 
    '     Function: GenericEnumerator, loadData, LoadSamples, ParseData
    ' 
    ' Class SampleDataChunk
    ' 
    ' 
    ' 
    ' Class LazyDataChunk
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CalculateOffset, LoadSamples, MeasureChunkSize, MoveToDataChunk
    ' 
    '     Sub: Close
    ' 
    ' Module ParserDispatch
    ' 
    '     Function: ResolveParser, ResolveSingleSampleParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class DataSubChunk : Inherits SampleDataChunk
    Implements Enumeration(Of Sample)

    Public Property data As Sample()

    Default Public ReadOnly Property channel(position As Integer) As Single()
        Get
            Return data _
                .Select(Function(sample) sample.channels(position)) _
                .ToArray
        End Get
    End Property

    Public Overrides Function LoadSamples(start As Integer, length As Integer, Optional scan0% = 0) As IEnumerable(Of Sample)
        Dim [sub] As Sample() = New Sample(length - 1) {}
        Call Array.ConstrainedCopy(data, start, [sub], scan0, length)
        Return [sub]
    End Function

    Friend Shared Function ParseData(wav As BinaryDataReader, format As FMTSubChunk) As DataSubChunk
        Call LazyDataChunk.MoveToDataChunk(wav)

        Return New DataSubChunk With {
            .chunkID = wav.ReadString(4),
            .chunkSize = wav.ReadInt32,
            .data = DataSubChunk.loadData(wav, format).ToArray
        }
    End Function

    Private Shared Function loadData(wav As BinaryDataReader, format As FMTSubChunk) As IEnumerable(Of Sample)
        ' little endian
        wav.ByteOrder = ByteOrder.LittleEndian

        Dim parseFunc As Func(Of BinaryDataReader, Integer, IEnumerable(Of Sample)) =
            ResolveParser(format.effectiveAudioFormat, format.BitsPerSample)

        Return parseFunc(wav, format.channels)
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of Sample) Implements Enumeration(Of Sample).GenericEnumerator
        For Each sample In data
            Yield sample
        Next
    End Function
End Class

Public MustInherit Class SampleDataChunk : Inherits SubChunk

    Public MustOverride Iterator Function LoadSamples(start As Integer, length As Integer, Optional scan0% = 0) As IEnumerable(Of Sample)

End Class

Public Class LazyDataChunk : Inherits SampleDataChunk

    ReadOnly wav As BinaryDataReader
    ReadOnly position As Long
    ReadOnly format As FMTSubChunk

    Sub New(wav As BinaryDataReader, format As FMTSubChunk)
        Me.wav = wav
        Me.format = format
        Me.position = MoveToDataChunk(wav).Position
    End Sub

    Public Sub Close()
        Call wav.Dispose()
    End Sub

    Public Overrides Iterator Function LoadSamples(start As Integer, length As Integer, Optional scan0% = 0) As IEnumerable(Of Sample)
        ' little endian
        wav.ByteOrder = ByteOrder.LittleEndian
        wav.Position = CalculateOffset(start, scan0)

        Dim parseSingle As Func(Of BinaryDataReader, Integer, Sample) =
            ResolveSingleSampleParser(format.effectiveAudioFormat, format.BitsPerSample)

        For i As Integer = 0 To length - 1
            Yield parseSingle(wav, format.channels)
        Next
    End Function

    Public Function MeasureChunkSize(length As Integer) As Long
        Return CLng(format.sampleSizeBytes) * length
    End Function

    Public Function CalculateOffset(start As Integer, Optional scan0% = 0) As Long
        Dim offset As Long = CLng(start) * format.sampleSizeBytes
        Return (position + scan0) + offset
    End Function

    Public Shared Function MoveToDataChunk(wav As BinaryDataReader) As BinaryDataReader
        Do While wav.ReadString(4) <> "data"
            wav.Seek(-3, SeekOrigin.Current)
        Loop

        Call wav.Seek(-4, SeekOrigin.Current)

        Return wav
    End Function
End Class

#Region "Parser Dispatch"

''' <summary>
''' Central dispatch for choosing the correct parser based on audio format and bits per sample.
''' </summary>
Friend Module ParserDispatch

    ''' <summary>
    ''' Resolves a streaming (iterator-based) parser for memory loading.
    ''' </summary>
    Friend Function ResolveParser(audioFormat As wFormatTag, bitsPerSample As Integer) _
        As Func(Of BinaryDataReader, Integer, IEnumerable(Of Sample))

        Select Case audioFormat
            Case wFormatTag.WAVE_FORMAT_PCM
                Select Case bitsPerSample
                    Case 8 : Return AddressOf Sample.Parse8Bit
                    Case 16 : Return AddressOf Sample.Parse16Bit
                    Case 24 : Return AddressOf Sample.Parse24Bit
                    Case 32 : Return AddressOf Sample.Parse32BitPCM
                    Case Else
                        Throw New NotSupportedException($"PCM {bitsPerSample}-bit not supported.")
                End Select

            Case wFormatTag.WAVE_FORMAT_IEEE_FLOAT
                Select Case bitsPerSample
                    Case 32 : Return AddressOf Sample.Parse32Bit
                    Case 64 : Return AddressOf Sample.Parse64Bit
                    Case Else
                        Throw New NotSupportedException($"IEEE Float {bitsPerSample}-bit not supported.")
                End Select

            Case wFormatTag.WAVE_FORMAT_ALAW
                Return AddressOf Sample.ParseALaw

            Case wFormatTag.WAVE_FORMAT_MULAW
                Return AddressOf Sample.ParseMuLaw

            Case Else
                Throw New NotSupportedException($"Audio format '{audioFormat}' is not supported.")
        End Select
    End Function

    ''' <summary>
    ''' Resolves a single-sample parser for lazy/streaming loading.
    ''' </summary>
    Friend Function ResolveSingleSampleParser(audioFormat As wFormatTag, bitsPerSample As Integer) _
        As Func(Of BinaryDataReader, Integer, Sample)

        Select Case audioFormat
            Case wFormatTag.WAVE_FORMAT_PCM
                Select Case bitsPerSample
                    Case 8 : Return AddressOf Sample.Parse8BitSample
                    Case 16 : Return AddressOf Sample.Parse16BitSample
                    Case 24 : Return AddressOf Sample.Parse24BitSample
                    Case 32 : Return AddressOf Sample.Parse32BitPCMSample
                    Case Else
                        Throw New NotSupportedException($"PCM {bitsPerSample}-bit not supported for streaming.")
                End Select

            Case wFormatTag.WAVE_FORMAT_IEEE_FLOAT
                Select Case bitsPerSample
                    Case 32 : Return AddressOf Sample.Parse32BitSample
                    Case 64 : Return AddressOf Sample.Parse64BitSample
                    Case Else
                        Throw New NotSupportedException($"IEEE Float {bitsPerSample}-bit not supported for streaming.")
                End Select

            Case wFormatTag.WAVE_FORMAT_ALAW
                Return AddressOf Sample.ParseALawSample

            Case wFormatTag.WAVE_FORMAT_MULAW
                Return AddressOf Sample.ParseMuLawSample

            Case Else
                Throw New NotSupportedException($"Audio format '{audioFormat}' is not supported for streaming.")
        End Select
    End Function

End Module

#End Region
