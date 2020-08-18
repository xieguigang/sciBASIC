#Region "Microsoft.VisualBasic::0365b2ced9b4d7fb76c46e71d54cd5f7, Data_science\Mathematica\SignalProcessing\wav\wav\File.vb"

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
    '     Function: Open
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' The wav file model
''' </summary>
Public Class WaveFile

    ''' <summary>
    ''' Contains the letters "RIFF" in ASCII form (0x52494646 big-endian form).
    ''' Resource Interchange File Format.
    ''' </summary>
    ''' <returns></returns>
    Public Property magic As String
    ''' <summary>
    ''' ``36 + SubChunk2Size``, or more precisely:
    ''' 
    ''' ```
    ''' 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
    ''' ```
    ''' 
    ''' This Is the size of the rest of the chunk 
    ''' following this number.  This Is the size Of the 
    ''' entire file In bytes minus 8 bytes For the
    ''' two fields Not included In this count:
    ''' ChunkID And ChunkSize.
    ''' </summary>
    ''' <returns></returns>
    Public Property fileSize As Integer
    ''' <summary>
    ''' Contains the letters "WAVE" (0x57415645 big-endian form).
    ''' </summary>
    ''' <returns></returns>
    Public Property format As String
    ''' <summary>
    ''' Subchunk1
    ''' </summary>
    ''' <returns></returns>
    Public Property fmt As FMTSubChunk
    ''' <summary>
    ''' Subchunk2
    ''' </summary>
    ''' <returns></returns>
    Public Property data As DataSubChunk

    Public Shared Function Open(wav As BinaryDataReader) As WaveFile
        Return New WaveFile With {
            .magic = wav.ReadString(4),
            .fileSize = wav.ReadInt32,
            .format = wav.ReadString(4),
            .fmt = FMTSubChunk.ParseChunk(wav),
            .data = DataSubChunk.ParseData(wav, format:= .fmt)
        }
    End Function
End Class
