Namespace netCDF.Components

    Public Class floats : Inherits CDFData(Of Single)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.FLOAT
            End Get
        End Property
    End Class
End Namespace