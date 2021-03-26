Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace netCDF.Components

    <HideModuleName>
    Public Module VectorHelper

        Public Function FromAny(data As Object(), type As CDFDataTypes) As Object
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

                    Return New bytes With {.buffer = bytes}
                Case CDFDataTypes.BOOLEAN
                    Return New flags With {.buffer = data.As(Of Boolean).ToArray}
                Case CDFDataTypes.CHAR
                    Return New chars With {.buffer = data.As(Of Char).ToArray}
                Case CDFDataTypes.DOUBLE
                    Return New doubles With {.buffer = data.As(Of Double).ToArray}
                Case CDFDataTypes.FLOAT
                    Return New floats With {.buffer = data.As(Of Single).ToArray}
                Case CDFDataTypes.INT
                    Return New integers With {.buffer = data.As(Of Integer).ToArray}
                Case CDFDataTypes.SHORT
                    Return New shorts With {.buffer = data.As(Of Short).ToArray}
                Case CDFDataTypes.LONG
                    Return New longs With {.buffer = data.As(Of Long).ToArray}
                Case Else
                    Throw New NotImplementedException(type.Description)
            End Select
        End Function
    End Module
End Namespace