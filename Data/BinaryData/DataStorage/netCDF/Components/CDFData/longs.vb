Namespace netCDF.Components

    Public Class longs : Inherits CDFData(Of Long)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.LONG
            End Get
        End Property
    End Class
End Namespace