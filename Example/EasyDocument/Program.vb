Imports Microsoft.VisualBasic.Serialization

Module Program

    Sub Main()
        Dim a As TestBin = TestBin.inst
        Call a.Serialize("./test.dat")
    End Sub
End Module

Public Class TestObject

End Class

Public Structure TestBin
    Public Property1 As String
    Public D As Date
    Public n As Integer
    Public f As Double

    Public Shared Function inst() As TestBin
        Return New TestBin With {
            .D = Now,
            .f = RandomDouble(),
            .n = RandomDouble() * 1000,
            .Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
        }
    End Function
End Structure