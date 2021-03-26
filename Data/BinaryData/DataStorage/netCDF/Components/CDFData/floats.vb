Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class floats : Inherits CDFData(Of Single)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.FLOAT
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Single()) As floats
            Return New floats With {.buffer = data}
        End Operator
    End Class
End Namespace