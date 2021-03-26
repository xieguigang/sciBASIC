
Namespace netCDF.Components

    Public Class integers : Inherits CDFData(Of Integer)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.INT
            End Get
        End Property
    End Class
End Namespace