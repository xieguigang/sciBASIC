#Region "Microsoft.VisualBasic::19fca914c5931c32747c2742a3d6ac4e, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Enums.vb"

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

    '   Total Lines: 52
    '    Code Lines: 20 (38.46%)
    ' Comment Lines: 29 (55.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (5.77%)
    '     File Size: 1.40 KB


    ' Enum wFormatTag
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum Channels
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum ChannelPositions
    ' 
    '     Left, None, Right
    ' 
    '  
    ' 
    ' 
    ' 
    ' Module WavFormatGuids
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Public Enum wFormatTag
    ''' <summary>
    ''' PCM
    ''' </summary>
    WAVE_FORMAT_PCM = &H1
    ''' <summary>
    ''' IEEE float
    ''' </summary>
    WAVE_FORMAT_IEEE_FLOAT = &H3
    ''' <summary>
    ''' 8-bit ITU-T G.711 A-law
    ''' </summary>
    WAVE_FORMAT_ALAW = &H6
    ''' <summary>
    ''' 8-bit ITU-T G.711 µ-law
    ''' </summary>
    WAVE_FORMAT_MULAW = &H7
    ''' <summary>
    ''' Determined by SubFormat
    ''' </summary>
    WAVE_FORMAT_EXTENSIBLE = &HFFFE
End Enum

''' <summary>
''' Legacy enumeration for common channel counts.
''' For multi-channel support, use Integer values directly
''' on <see cref="FMTSubChunk.channels"/>.
''' </summary>
Public Enum Channels
    Mono = 1
    Stereo = 2
End Enum

Public Enum ChannelPositions
    None
    Left
    Right
End Enum

''' <summary>
''' Well-known GUIDs for WAVE_FORMAT_EXTENSIBLE SubFormat field.
''' </summary>
Public Module WavFormatGuids
    ''' <summary>
    ''' KSDATAFORMAT_SUBTYPE_PCM: {00000001-0000-0010-8000-00AA00389B71}
    ''' </summary>
    Public ReadOnly SubTypePcm As Guid = New Guid("00000001-0000-0010-8000-00AA00389B71")
    ''' <summary>
    ''' KSDATAFORMAT_SUBTYPE_IEEE_FLOAT: {00000003-0000-0010-8000-00AA00389B71}
    ''' </summary>
    Public ReadOnly SubTypeIeeeFloat As Guid = New Guid("00000003-0000-0010-8000-00AA00389B71")
End Module
