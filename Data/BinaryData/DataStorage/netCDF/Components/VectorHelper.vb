Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace netCDF.Components

    <HideModuleName>
    Public Module VectorHelper

        Public Function FromAny(data As Array, type As CDFDataTypes) As ICDFDataVector
            Select Case type
                Case CDFDataTypes.BYTE
                    Dim bytes As Byte()

                    If TypeOf data Is Byte() Then
                        bytes = data
                    ElseIf data.AsObjectEnumerator.All(Function(obj) TypeOf obj Is Byte()) Then
                        bytes = data.AsObjectEnumerator _
                            .Select(Function(obj)
                                        Return DirectCast(obj, Byte())(Scan0)
                                    End Function) _
                            .ToArray
                    Else
                        bytes = data.As(Of Byte).ToArray
                    End If

                    Return CType(bytes, bytes)
                Case CDFDataTypes.BOOLEAN
                    Return CType(data.vectorAuto(Of Boolean), flags)
                Case CDFDataTypes.CHAR
                    Return CType(data.vectorAuto(Of Char), chars)
                Case CDFDataTypes.DOUBLE
                    Return CType(data.vectorAuto(Of Double), doubles)
                Case CDFDataTypes.FLOAT
                    Return CType(data.vectorAuto(Of Single), floats)
                Case CDFDataTypes.INT
                    Return CType(data.vectorAuto(Of Integer), integers)
                Case CDFDataTypes.SHORT
                    Return CType(data.vectorAuto(Of Short), shorts)
                Case CDFDataTypes.LONG
                    Return CType(data.vectorAuto(Of Long), longs)
                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function vectorAuto(Of T)(data As Array) As T()
            If TypeOf data Is T() Then
                Return DirectCast(data, T())
            Else
                Return data.As(Of T).ToArray
            End If
        End Function
    End Module
End Namespace