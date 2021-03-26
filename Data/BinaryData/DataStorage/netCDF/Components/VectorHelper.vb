Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace netCDF.Components

    <HideModuleName>
    Public Module VectorHelper

        Public Function FromAny(data As Object(), type As CDFDataTypes) As ICDFDataVector
            Select Case type
                Case CDFDataTypes.BYTE
                    Dim bytes As Byte()

                    If data.All(Function(obj) TypeOf obj Is Byte()) Then
                        bytes = data _
                            .Select(Function(obj)
                                        Return DirectCast(obj, Byte())(Scan0)
                                    End Function) _
                            .ToArray
                    Else
                        bytes = data.As(Of Byte).ToArray
                    End If

                    Return CType(bytes, bytes)
                Case CDFDataTypes.BOOLEAN
                    Return CType(data.As(Of Boolean).ToArray, flags)
                Case CDFDataTypes.CHAR
                    Return CType(data.As(Of Char).ToArray, chars)
                Case CDFDataTypes.DOUBLE
                    Return CType(data.As(Of Double).ToArray, doubles)
                Case CDFDataTypes.FLOAT
                    Return CType(data.As(Of Single).ToArray, floats)
                Case CDFDataTypes.INT
                    Return CType(data.As(Of Integer).ToArray, integers)
                Case CDFDataTypes.SHORT
                    Return CType(data.As(Of Short).ToArray, shorts)
                Case CDFDataTypes.LONG
                    Return CType(data.As(Of Long).ToArray, longs)
                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select
        End Function
    End Module
End Namespace