Namespace netCDF.Components

    Public Class bytes : Inherits CDFData(Of Byte)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.BYTE
            End Get
        End Property
    End Class
End Namespace