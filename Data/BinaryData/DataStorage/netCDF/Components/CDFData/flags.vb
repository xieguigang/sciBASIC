Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class flags : Inherits CDFData(Of Boolean)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.BOOLEAN
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Boolean()) As flags
            Return New flags With {.buffer = data}
        End Operator
    End Class
End Namespace