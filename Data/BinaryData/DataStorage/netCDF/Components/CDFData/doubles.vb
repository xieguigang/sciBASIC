Namespace netCDF.Components

    Public Class doubles : Inherits CDFData(Of Double)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.DOUBLE
            End Get
        End Property
    End Class
End Namespace