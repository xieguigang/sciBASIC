Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class bytes : Inherits CDFData(Of Byte)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.BYTE
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Byte()) As bytes
            Return New bytes With {.buffer = data}
        End Operator
    End Class
End Namespace