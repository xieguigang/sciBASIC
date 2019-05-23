
'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Imports Microsoft.VisualBasic.Data.IO.HDF5.struct

Namespace HDF5.dataset

    ''' <summary>
    ''' Chunked: The array domain is regularly decomposed into chunks, and each chunk is allocated and 
    ''' stored separately. This layout supports arbitrary element traversals, compression, encryption, 
    ''' and checksums (these features are described in other messages). The message stores the size of 
    ''' a chunk instead of the size of the entire array; the storage size of the entire array can be 
    ''' calculated by traversing the chunk index that stores the chunk addresses.
    ''' 
    ''' This represents chunked datasets using a b-tree for indexing raw data chunks.
    ''' It supports filters for use when reading the dataset for example to
    ''' decompress.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Class ChunkedDatasetV3 : Inherits Hdf5Dataset

        ''' <summary>
        ''' A chunk has a fixed dimensionality. This field specifies the number of dimension size 
        ''' fields later in the message.
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensionality As Integer
        ''' <summary>
        ''' This is the address of the v1 B-tree that is used to look up the addresses of the chunks 
        ''' that actually store portions of the array data. The address may have the 
        ''' ¡°undefined address¡± value, to indicate that storage has not yet been allocated for this 
        ''' array.
        ''' </summary>
        ''' <returns></returns>
        Public Property BtreeAddress As Long

        ''' <summary>
        ''' These values define the dimension size of a single chunk, in units of array elements 
        ''' (not bytes). The first dimension stored in the list of dimensions is the slowest changing 
        ''' dimension and the last dimension stored is the fastest changing dimension.
        ''' </summary>
        ''' <returns></returns>
        Public Property dimensionSize As Integer()

        ''' <summary>
        ''' ###### Dataset Element Size
        ''' 
        ''' The size of a dataset element, in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public Property byteSize As Integer

        Public Overrides Function data(sb As Superblock) As Object
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace
