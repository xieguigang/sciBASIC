Namespace netCDF.Components

    Public Class chars : Inherits CDFData(Of Char)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.CHAR
            End Get
        End Property
    End Class
End Namespace