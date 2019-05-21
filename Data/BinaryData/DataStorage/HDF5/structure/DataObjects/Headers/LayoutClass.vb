Namespace HDF5.[Structure]

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