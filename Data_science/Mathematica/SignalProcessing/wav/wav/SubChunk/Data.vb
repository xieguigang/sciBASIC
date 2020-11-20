#Region "Microsoft.VisualBasic::fb1963a9c33857e7c4c82114f725bb4c, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Data.vb"

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

    ' Class DataSubChunk
    ' 
    '     Properties: data
    ' 
    '     Function: GenericEnumerator, GetEnumerator, loadData, LoadSamples, ParseData
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

        Select Case format.BitsPerSample
            Case 8
                Return Sample.Parse8Bit(wav, format.channels)
            Case 16
                Return Sample.Parse16Bit(wav, format.channels)
            Case 32
                Return Sample.Parse32Bit(wav, format.channels)
            Case Else
                Throw New NotImplementedException(format.GetJson)
        End Select
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of Sample) Implements Enumeration(Of Sample).GenericEnumerator
        For Each sample In data
            Yield sample
        Next
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of Sample).GetEnumerator
        Yield GetEnumerator()
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

        For i As Integer = 0 To length - 1
            Select Case format.BitsPerSample
                Case 32
                    Yield Sample.Parse32BitSample(wav, format.channels)
                Case Else
                    Throw New NotImplementedException(format.ToString)
            End Select
        Next
    End Function

    Public Function MeasureChunkSize(length As Integer) As Long
        Dim bytes As Integer

        Select Case format.BitsPerSample
            Case 32
                bytes = format.channels * 4
            Case 16
                bytes = format.channels * 2
            Case 8
                bytes = format.channels * 1
            Case Else
                Throw New NotImplementedException(format.ToString)
        End Select

        Return bytes * length
    End Function

    Public Function CalculateOffset(start As Integer, Optional scan0% = 0) As Long
        Dim sampleSize As Integer

        Select Case format.BitsPerSample
            Case 8 : sampleSize = format.channels * 1
            Case 16 : sampleSize = format.channels * 2
            Case 32 : sampleSize = format.channels * 4
            Case Else
                Throw New BadImageFormatException(format.GetJson)
        End Select

        Dim offset As Long = start * sampleSize

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
