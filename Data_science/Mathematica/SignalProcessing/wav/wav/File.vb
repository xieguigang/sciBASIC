#Region "Microsoft.VisualBasic::0f943b98aa779a4e92d5d3ede1fa19f0, Data_science\Mathematica\SignalProcessing\wav\wav\File.vb"

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

    ' Class File
    ' 
    '     Properties: data, fileSize, fmt, format, magic
    ' 
    '     Function: ParseHeader
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' The wav file model
''' </summary>
Public Class File

    Public Property magic As String
    Public Property fileSize As Integer
    Public Property format As String
    Public Property fmt As FMTSubChunk
    Public Property data As DataSubChunk

    Public Shared Function ParseHeader(wav As BinaryDataReader) As File
        Return New File With {
            .magic = wav.ReadString(4),
            .fileSize = wav.ReadInt32,
            .format = wav.ReadString(4),
            .fmt = New FMTSubChunk With {
                .ChunkID = wav.ReadString(4),
                .ChunkSize = wav.ReadInt32,
                .audioFormat = wav.ReadInt16,
                .Channels = wav.ReadInt16,
                .SampleRate = wav.ReadInt32,
                .ByteRate = wav.ReadInt32,
                .BlockAlign = wav.ReadInt16,
                .BitsPerSample = wav.ReadInt16
            },
            .data = DataSubChunk.ParseData(wav)
        }
    End Function
End Class
