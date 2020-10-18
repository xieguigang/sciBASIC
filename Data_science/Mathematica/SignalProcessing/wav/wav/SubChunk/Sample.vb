#Region "Microsoft.VisualBasic::acfaf32a65abbb7796f6f4ab38d1a192, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Sample.vb"

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

' Structure Sample
' 
'     Function: Parse16Bit, Parse32Bit, Parse8Bit, ToString
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure Sample

    ''' <summary>
    ''' 一个sample之中是由好几个声道的数据构成的
    ''' </summary>
    ''' <remarks>
    ''' 在这里使用Short来兼容8bit和16bit的数据
    ''' </remarks>
    Dim channels As Single()

    Public Overrides Function ToString() As String
        Return channels.GetJson
    End Function

    'Friend Shared Iterator Function Parse16Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
    '    Dim sampleSize = channels * 2

    '    Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
    '        Yield New Sample With {
    '            .channels = wav.ReadInt16s(channels).Select(Function(a) CSng(a)).ToArray
    '        }
    '    Loop
    'End Function

    'Friend Shared Iterator Function Parse8Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
    '    Throw New NotImplementedException
    'End Function

    'Friend Shared Iterator Function Parse32Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
    '    Dim sampleSize = channels * 4

    '    Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
    '        Yield New Sample With {
    '            .channels = wav.ReadSingles(channels)
    '        }
    '    Loop
    'End Function

End Structure

Public Structure LazySample

    Dim channels As Integer
    Dim bit As Integer
    Dim start As Long

    Friend Function LoadSample(wav As BinaryDataReader) As Sample
        Call wav.Seek(start, SeekOrigin.Begin)

        Select Case bit
            Case 8
                Throw New NotImplementedException(Me.GetJson)
            Case 16
                Return New Sample With {
                    .channels = wav.ReadInt16s(channels).Select(Function(a) CSng(a)).ToArray
                }
            Case 32
                Return New Sample With {
                    .channels = wav.ReadSingles(channels)
                }
            Case Else
                Throw New NotImplementedException(Me.GetJson)
        End Select
    End Function

    Friend Shared Iterator Function GetLazySamples(position As i32, wavLength As Long, bit As Integer, channels As Integer) As IEnumerable(Of LazySample)
        Dim sampleSize

        Select Case bit
            Case 8 : sampleSize = channels * 1
            Case 16 : sampleSize = channels * 2
            Case 32 : sampleSize = channels * 4
            Case Else
                Throw New NotImplementedException
        End Select

        position = position - sampleSize

        Do While (position = position + sampleSize) <= wavLength
            Yield New LazySample With {
                .channels = channels,
                .bit = bit,
                .start = position
            }
        Loop
    End Function

End Structure