Imports System.Numerics
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' unchecked arithmetic
''' </summary>
Public Structure UncheckInteger

    Private bytes As BigInteger

    Private Sub New(int As BigInteger)
        bytes = int
    End Sub

    Public Overrides Function ToString() As String
        Return bytes.ToByteArray.GetJson
    End Function

#Region "Conversions"

#Region "To"
    Public Shared Narrowing Operator CType(unchecked As UncheckInteger) As Byte()
        Return unchecked.bytes.ToByteArray
    End Operator

    Public Shared Narrowing Operator CType(unchecked As UncheckInteger) As Long
        Return BitConverter.ToInt64(unchecked.bytes.ToByteArray, Scan0)
    End Operator

    Public Shared Narrowing Operator CType(unchecked As UncheckInteger) As ULong
        Return BitConverter.ToUInt64(unchecked.bytes.ToByteArray, Scan0)
    End Operator

    Public Shared Narrowing Operator CType(unchecked As UncheckInteger) As Short
        Return BitConverter.ToInt16(unchecked.bytes.ToByteArray, Scan0)
    End Operator

    Public Shared Narrowing Operator CType(unchecked As UncheckInteger) As UShort
        Return BitConverter.ToUInt16(unchecked.bytes.ToByteArray, Scan0)
    End Operator

    Public Shared Narrowing Operator CType(unchecked As UncheckInteger) As Integer
        Return BitConverter.ToInt32(unchecked.bytes.ToByteArray, Scan0)
    End Operator
#End Region

#Region "From"
    Public Shared Widening Operator CType(int%) As UncheckInteger
        Return New UncheckInteger With {
            .bytes = New BigInteger(int)
        }
    End Operator

    Public Shared Widening Operator CType(int&) As UncheckInteger
        Return New UncheckInteger With {
            .bytes = New BigInteger(int)
        }
    End Operator

    Public Shared Widening Operator CType(int As Short) As UncheckInteger
        Return New UncheckInteger With {
            .bytes = New BigInteger(int)
        }
    End Operator

    Public Shared Widening Operator CType(int As UShort) As UncheckInteger
        Return New UncheckInteger With {
            .bytes = New BigInteger(int)
        }
    End Operator

    Public Shared Widening Operator CType(int As ULong) As UncheckInteger
        Return New UncheckInteger With {
            .bytes = New BigInteger(int)
        }
    End Operator
#End Region

#End Region

#Region "unchecked arithmetic"

    Public Shared Operator +(unchecked As UncheckInteger, int As BigInteger) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes + int)
    End Operator

    Public Shared Operator +(unchecked As UncheckInteger, int As Integer) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes + int)
    End Operator

    Public Shared Operator +(int As BigInteger, unchecked As UncheckInteger) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes + int)
    End Operator

    Public Shared Operator +(int As Integer, unchecked As UncheckInteger) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes + int)
    End Operator

    Public Shared Operator Xor(unchecked As UncheckInteger, int As BigInteger) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes Xor int)
    End Operator

    Public Shared Operator Xor(unchecked As UncheckInteger, int As UncheckInteger) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes Xor int.bytes)
    End Operator

    Public Shared Operator <<(unchecked As UncheckInteger, int As Integer) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes << int)
    End Operator

    Public Shared Operator +(unchecked As UncheckInteger, int As UncheckInteger) As UncheckInteger
        Return New UncheckInteger(unchecked.bytes + int.bytes)
    End Operator
#End Region
End Structure
