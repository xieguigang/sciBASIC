#Region "Microsoft.VisualBasic::8da96e8fdfca61fa0ed2a1a60e676486, Data\BinaryData\HDF5\structure\DataObjects\Headers\LayoutClass.vb"

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

    '   Total Lines: 41
    '    Code Lines: 8 (19.51%)
    ' Comment Lines: 32 (78.05%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (2.44%)
    '     File Size: 2.10 KB


    '     Enum LayoutClass
    ' 
    '         Virtual
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace struct

    ''' <summary>
    ''' The Data Layout message describes how the elements of a multi-dimensional 
    ''' array are stored in the HDF5 file. Four types of data layout are supported.
    ''' </summary>
    Public Enum LayoutClass As Integer
        ''' <summary>
        ''' Compact: The array is stored in one contiguous block as part of 
        ''' this object header message.
        ''' </summary>
        CompactStorage = 0
        ''' <summary>
        ''' Contiguous: The array is stored in one contiguous area of the file. 
        ''' This layout requires that the size of the array be constant: data 
        ''' manipulations such as chunking, compression, checksums, or encryption 
        ''' are not permitted. The message stores the total storage size of 
        ''' the array. The offset of an element from the beginning of the 
        ''' storage area is computed as in a C array.
        ''' </summary>
        ContiguousStorage = 1
        ''' <summary>
        ''' Chunked: The array domain is regularly decomposed into chunks, and 
        ''' each chunk is allocated and stored separately. This layout supports 
        ''' arbitrary element traversals, compression, encryption, and checksums 
        ''' (these features are described in other messages). The message stores 
        ''' the size of a chunk instead of the size of the entire array; the 
        ''' storage size of the entire array can be calculated by traversing 
        ''' the chunk index that stores the chunk addresses.
        ''' </summary>
        ChunkedStorage = 2
        ''' <summary>
        ''' Virtual: This is only supported for version 4 of the Data Layout message. 
        ''' The message stores information that is used to locate the global heap 
        ''' collection containing the Virtual Dataset (VDS) mapping information. 
        ''' The mapping associates the VDS to the source dataset elements that are 
        ''' stored across a collection of HDF5 files.
        ''' </summary>
        Virtual
    End Enum
End Namespace
