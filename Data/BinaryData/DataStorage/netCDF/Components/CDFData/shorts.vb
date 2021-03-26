Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class shorts : Inherits CDFData(Of Short)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.SHORT
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Short()) As shorts
            Return New shorts With {.buffer = data}
        End Operator
    End Class
End Namespace