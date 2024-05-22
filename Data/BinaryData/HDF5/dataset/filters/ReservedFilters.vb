#Region "Microsoft.VisualBasic::4f671619d5208afb6958656979f38f23, Data\BinaryData\HDF5\dataset\filters\ReservedFilters.vb"

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

    '   Total Lines: 36
    '    Code Lines: 11 (30.56%)
    ' Comment Lines: 24 (66.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (2.78%)
    '     File Size: 914 B


    '     Enum ReservedFilters
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace dataset.filters

    ''' <summary>
    ''' The filters currently in library version 1.8.0 are listed below:
    ''' </summary>
    Public Enum ReservedFilters As Short
        ''' <summary>
        ''' Reserved
        ''' </summary>
        NA = 0
        ''' <summary>
        ''' GZIP deflate compression
        ''' </summary>
        deflate = 1
        ''' <summary>
        ''' Data element shuffling
        ''' </summary>
        shuffle = 2
        ''' <summary>
        ''' Fletcher32 checksum
        ''' </summary>
        fletcher32 = 3
        ''' <summary>
        ''' SZIP compression
        ''' </summary>
        szip = 4
        ''' <summary>
        ''' N-bit packing
        ''' </summary>
        nbit = 5
        ''' <summary>
        ''' Scale and offset encoded values
        ''' </summary>
        scaleoffset = 6
    End Enum
End Namespace
