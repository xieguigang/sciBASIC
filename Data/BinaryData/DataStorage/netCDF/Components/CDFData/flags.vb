Namespace netCDF.Components

    Public Class flags : Inherits CDFData(Of Boolean)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.BOOLEAN
            End Get
        End Property
    End Class
End Namespace