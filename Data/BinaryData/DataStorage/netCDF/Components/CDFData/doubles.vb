Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Vectorization

Namespace netCDF.Components

    Public Class doubles : Inherits CDFData(Of Double)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.DOUBLE
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Double()) As doubles
            Return New doubles With {.buffer = data}
        End Operator
    End Class
End Namespace