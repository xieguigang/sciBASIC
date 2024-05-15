#Region "Microsoft.VisualBasic::dbc8a441a7fe71efbeffd07a8dce50e9, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Sample.vb"

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

    '   Total Lines: 60
    '    Code Lines: 42
    ' Comment Lines: 6
    '   Blank Lines: 12
    '     File Size: 1.98 KB


    ' Structure Sample
    ' 
    '     Properties: left, right
    ' 
    '     Function: Parse16Bit, Parse32Bit, Parse32BitSample, Parse8Bit, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure Sample

    ''' <summary>
    ''' 一个sample之中是由好几个声道的数据构成的
    ''' </summary>
    ''' <remarks>
    ''' 在这里使用Short来兼容8bit和16bit的数据
    ''' </remarks>
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

    Friend Shared Iterator Function Parse16Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 2

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Yield New Sample With {
                .channels = wav.ReadInt16s(channels).Select(Function(a) CSng(a)).ToArray
            }
        Loop
    End Function

    Friend Shared Iterator Function Parse8Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Throw New NotImplementedException
    End Function

    Friend Shared Iterator Function Parse32Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 4

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Yield Parse32BitSample(wav, channels)
        Loop
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function Parse32BitSample(wav As BinaryDataReader, channels As Integer) As Sample
        Return New Sample With {
            .channels = wav.ReadSingles(channels)
        }
    End Function

End Structure
