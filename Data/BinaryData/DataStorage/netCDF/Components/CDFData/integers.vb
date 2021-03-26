
Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class integers : Inherits CDFData(Of Integer)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.INT
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Integer()) As integers
            Return New integers With {.buffer = data}
        End Operator
    End Class
End Namespace