Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping.StructFormatter

Module Program

    Sub Main()

        Dim a As TestBin = TestBin.inst
        Call a.Serialize("./test.dat")

        a = Nothing
        a = "./test.dat".Load(Of TestBin)

        Call a.GetJson.__DEBUG_ECHO

        Pause()
    End Sub
End Module

Public Class TestObject

End Class

<Serializable> Public Structure TestBin
    Public Property Property1 As String
    Public Property D As Date
    Public Property n As Integer
    Public Property f As Double

    Public Shared Function inst() As TestBin
        Return New TestBin With {
            .D = Now,
            .f = RandomDouble(),
            .n = RandomDouble() * 1000,
            .Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
        }
    End Function
End Structure