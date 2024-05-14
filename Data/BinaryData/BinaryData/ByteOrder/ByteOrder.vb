#Region "Microsoft.VisualBasic::c827fd1a75721f0943e2ea01ed096e65, Data\BinaryData\BinaryData\ByteOrder\ByteOrder.vb"

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

    '   Total Lines: 20
    '    Code Lines: 5
    ' Comment Lines: 11
    '   Blank Lines: 4
    '     File Size: 484 B


    ' Enum ByteOrder
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript

''' <summary>
''' Represents the possible endianness of binary data.
''' </summary>
Public Enum ByteOrder As UShort

    ''' <summary>
    ''' The binary data is present in big endian.
    ''' 
    ''' (network byte order)
    ''' </summary>
    BigEndian = DataView.BIG_ENDIAN

    ''' <summary>
    ''' The binary data is present in little endian.
    ''' </summary>
    LittleEndian = DataView.LITTLE_ENDIAN

End Enum
