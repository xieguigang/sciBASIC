
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

Namespace HDF5.dataset

    ''' <summary>
    ''' The array is stored in one contiguous area of the file. This layout requires that the size 
    ''' of the array be constant: data manipulations such as chunking, compression, checksums, or 
    ''' encryption are not permitted. The message stores the total storage size of the array. 
    ''' The offset of an element from the beginning of the storage area is computed as in a C array.
    ''' </summary>
    Public Class ContiguousDataset : Inherits Hdf5Dataset

        ''' <summary>
        ''' This is the address of the raw data in the file. The address may have the 
        ''' ¡°undefined address¡± value, to indicate that storage has not yet been allocated 
        ''' for this array.
        ''' </summary>
        ''' <returns></returns>
        Public Property dataAddress As Long
        ''' <summary>
        ''' This field contains the size allocated to store the raw data, in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Long

        Protected Overrides Function getBuffer(sb As Superblock) As MemoryStream
            Return New MemoryStream(sb.FileReader(dataAddress).readBytes(size))
        End Function
    End Class
End Namespace
