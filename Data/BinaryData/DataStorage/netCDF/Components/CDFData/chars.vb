Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace netCDF.Components

    Public Class chars : Inherits CDFData(Of Char)

        Public Overrides ReadOnly Property cdfDataType As CDFDataTypes
            Get
                Return CDFDataTypes.CHAR
            End Get
        End Property

        Public Function LoadJSON(Of T)() As T
            Return New String(buffer).LoadJSON(Of T)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As String) As chars
            Return New chars With {.buffer = data.ToArray}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(data As Char()) As chars
            Return New chars With {.buffer = data}
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(chars As chars) As String
            Return New String(chars.buffer)
        End Operator
    End Class
End Namespace