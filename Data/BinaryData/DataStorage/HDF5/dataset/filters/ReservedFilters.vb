Namespace HDF5.dataset.filters

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