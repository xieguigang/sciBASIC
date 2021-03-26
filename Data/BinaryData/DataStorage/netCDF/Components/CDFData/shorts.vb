Namespace netCDF.Components

    Public Class shorts : Inherits CDFData(Of Short)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.SHORT
            End Get
        End Property
    End Class
End Namespace