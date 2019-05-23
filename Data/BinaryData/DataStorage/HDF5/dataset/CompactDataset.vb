'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Namespace HDF5.dataset

    ''' <summary>
    ''' Compact: The array is stored in one contiguous block as part of this object header message.
    ''' </summary>
    Public Class CompactDataset : Inherits Hdf5Dataset

        ''' <summary>
        ''' (2 bytes) This field contains the size of the raw data for the dataset array, in bytes.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
        ''' <summary>
        ''' This field contains the raw data for the dataset array.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property rawData As Byte()


    End Class
End Namespace
