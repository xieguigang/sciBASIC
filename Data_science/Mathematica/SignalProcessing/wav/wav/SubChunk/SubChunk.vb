#Region "Microsoft.VisualBasic::02c1a8fa6bf9d02b4740215c98f586c8, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk.vb"

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

    ' Class SubChunk
    ' 
    '     Properties: ChunkID, ChunkSize
    ' 
    ' Class FMTSubChunk
    ' 
    '     Properties: audioFormat, BitsPerSample, BlockAlign, ByteRate, Channels
    '                 SampleRate
    ' 
    ' Class DataSubChunk
    ' 
    '     Properties: Data
    ' 
    '     Function: loadData, ParseData
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

Public MustInherit Class SubChunk

    Public Property chunkID As String
    Public Property chunkSize As Integer

End Class
