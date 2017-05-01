Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' unchecked arithmetic
''' </summary>
Public Module UncheckInteger

    Public Function unchecked(u&) As BigInteger
        Return New BigInteger(u)
    End Function

    <Extension>
    Public Function uncheckedULong(bytes As BigInteger) As ULong
        Dim data As Byte() = bytes.ToByteArray
        If data.Length < 8 Then
            data = data.Join({0, 0, 0, 0, 0, 0, 0, 0}).ToArray
        End If
        Return BitConverter.ToUInt64(data, Scan0)
    End Function

    <Extension>
    Public Function uncheckedInteger(bytes As BigInteger) As Integer
        Return BitConverter.ToInt32(bytes.ToByteArray, Scan0)
    End Function
End Module
