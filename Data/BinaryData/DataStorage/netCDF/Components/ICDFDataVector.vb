
Imports System.Text

Namespace netCDF.Components

    Public Interface ICDFDataVector

        ReadOnly Property cdfDataType As CDFDataTypes
        ReadOnly Property genericValue As Array
        ReadOnly Property length As Integer

        Function GetBuffer(encoding As Encoding) As Byte()

    End Interface
End Namespace