Imports System.Runtime.CompilerServices

Namespace netCDF.Components

    Public Class chars : Inherits CDFData(Of Char)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.CHAR
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As String) As chars
            Return New chars With {.buffer = data.ToArray}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Char()) As chars
            Return New chars With {.buffer = data}
        End Operator
    End Class
End Namespace