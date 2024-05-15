#Region "Microsoft.VisualBasic::7a36aa2203a9b4cd3d81a6617aa2a6f2, Data\BinaryData\HDF5\dataset\ContiguousDataset.vb"

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

    '   Total Lines: 47
    '    Code Lines: 16
    ' Comment Lines: 25
    '   Blank Lines: 6
    '     File Size: 1.87 KB


    '     Class ContiguousDataset
    ' 
    '         Properties: dataAddress, dimensions, size
    ' 
    '         Function: getBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct

Namespace dataset

    ''' <summary>
    ''' The array is stored in one contiguous area of the file. This layout requires that the size 
    ''' of the array be constant: data manipulations such as chunking, compression, checksums, or 
    ''' encryption are not permitted. The message stores the total storage size of the array. 
    ''' The offset of an element from the beginning of the storage area is computed as in a C array.
    ''' </summary>
    Public Class ContiguousDataset : Inherits Hdf5Dataset

        ''' <summary>
        ''' This is the address of the raw data in the file. The address may have the 
        ''' ��undefined address�� value, to indicate that storage has not yet been allocated 
        ''' for this array.
        ''' </summary>
        ''' <returns></returns>
        Public Property dataAddress As Long
        ''' <summary>
        ''' This field contains the size allocated to store the raw data, in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Long

        Public Overrides ReadOnly Property dimensions As Integer()
            Get
                Return dataSpace.dimensionLength
            End Get
        End Property

        Protected Overrides Function getBuffer(sb As Superblock) As MemoryStream
            Return New MemoryStream(sb.FileReader(dataAddress).readBytes(size))
        End Function
    End Class
End Namespace
