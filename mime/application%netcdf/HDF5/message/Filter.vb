Namespace org.renjin.hdf5.message


    Public Class Filter

        Public Const FILTER_DEFLATE As Integer = 1
        Public Const FILTER_SHUFFLE As Integer = 2
        Public Const FILTER_FLETCHER32 As Integer = 3
        Public Const FILTER_SZIP As Integer = 4
        Public Const FILTER_NBIT As Integer = 5
        Public Const FILTER_SCALE_OFFSET As Integer = 6



        ''' <summary>
        ''' Unique identifier for the filter.
        ''' 
        ''' <p>Values from zero through 32,767 are reserved for filters supported by The HDF Group in the HDF5 Library
        ''' and for filters requested and supported by third parties. Filters supported by The HDF Group are documented
        ''' immediately below. Information on 3rd-party filters can be found at The HDF Group’s Contributions page.
        ''' 
        ''' <p>Values from 32768 to 65535 are reserved for non-distributed uses (for example, internal company usage) or
        ''' for application usage when testing a feature. The HDF Group does not track or document the use of the filters
        ''' with identifiers from this range.</p>
        ''' </summary>
        Public Overridable Property FilterId As Integer


        ''' 
        ''' <returns> the optional name of the filter </returns>
        Public Overridable Property Name As String


        ''' <summary>
        ''' Each filter can store integer values to control how the filter operates.
        ''' </summary>
        Public Overridable Property ClientData As Integer()


        ''' <summary>
        ''' If set then the filter is an optional filter. During output, if an optional filter fails it will
        ''' be silently skipped in the pipeline.
        ''' </summary>
        Public Overridable Property [Optional] As Boolean



        Friend Sub New(filterId As Integer, name As String, clientData() As Integer, [optional] As Boolean)
            Me.filterId = filterId
            Me.name = name
            Me.clientData = clientData
            Me.optional = [optional]
        End Sub
    End Class

End Namespace