Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class longs : Inherits CDFData(Of Long)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.LONG
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Long()) As longs
            Return New longs With {.buffer = data}
        End Operator
    End Class
End Namespace