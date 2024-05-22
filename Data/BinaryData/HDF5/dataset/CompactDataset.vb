#Region "Microsoft.VisualBasic::c7b31f86dd5cad6acc079a6648b00d5b, Data\BinaryData\HDF5\dataset\CompactDataset.vb"

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

    '   Total Lines: 42
    '    Code Lines: 16 (38.10%)
    ' Comment Lines: 20 (47.62%)
    '    - Xml Docs: 55.00%
    ' 
    '   Blank Lines: 6 (14.29%)
    '     File Size: 1.40 KB


    '     Class CompactDataset
    ' 
    '         Properties: dimensions, rawData, size
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
    ''' Compact: The array is stored in one contiguous block as part of this object header message.
    ''' </summary>
    Public Class CompactDataset : Inherits Hdf5Dataset

        ''' <summary>
        ''' (2 bytes) This field contains the size of the raw data for the dataset array, in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Integer
        ''' <summary>
        ''' This field contains the raw data for the dataset array.
        ''' </summary>
        ''' <returns></returns>
        Public Property rawData As Byte()

        Public Overrides ReadOnly Property dimensions As Integer()
            Get
                Return dataSpace.dimensionLength
            End Get
        End Property

        Protected Overrides Function getBuffer(sb As Superblock) As MemoryStream
            Return New MemoryStream(rawData)
        End Function
    End Class
End Namespace
