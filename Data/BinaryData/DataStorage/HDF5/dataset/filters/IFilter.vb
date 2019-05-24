Namespace HDF5.dataset.filters

    ''' <summary>
    ''' Interface to be implemented to be a HDF5 filter.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Interface IFilter

        ''' <summary>
        ''' Gets the ID of this filter, this must match the ID in the dataset header.
        ''' </summary>
        ''' <returns> the ID of this filter </returns>
        ReadOnly Property id As Integer

        ''' <summary>
        ''' Gets the name of this filter e.g. 'deflate', 'shuffle'
        ''' </summary>
        ''' <returns> the name of this filter </returns>
        ReadOnly Property name As String

        Function decode(encodedData As Byte(), filterData As Integer()) As Byte()

    End Interface
End Namespace