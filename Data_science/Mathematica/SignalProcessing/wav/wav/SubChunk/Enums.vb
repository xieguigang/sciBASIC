#Region "Microsoft.VisualBasic::5c04e1b48b50beabd7c6063ea58e3c0b, Data_science\Mathematica\SignalProcessing\wav\wav\SubChunk\Enums.vb"

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

    '   Total Lines: 33
    '    Code Lines: 16
    ' Comment Lines: 15
    '   Blank Lines: 2
    '     File Size: 652 B


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

Public Enum Channels
    Mono = 1
    Stereo = 2
End Enum

Public Enum ChannelPositions
    None
    Left
    Right
End Enum
