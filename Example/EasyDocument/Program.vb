Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.BinaryDumping.StructFormatter

Module Program

    Sub Main()

        Dim a As TestBin = TestBin.inst
        Call a.Serialize("./test.dat")

        a = Nothing
        a = "./test.dat".Load(Of TestBin)

        Call a.GetJson.__DEBUG_ECHO

        Call New Profiles With {.Test = a}.WriteProfile

        Pause()
    End Sub
End Module

<IniMapIO("#/test.ini")>
Public Class Profiles
    Public Property Test As TestBin
End Class

<ClassName("JSON")>
<Serializable> Public Class TestBin
    <DataFrameColumn> Public Property Property1 As String
    <DataFrameColumn> Public Property D As Date
    <DataFrameColumn> Public Property n As Integer
    <DataFrameColumn> Public Property f As Double

    Public Shared Function inst() As TestBin
        Return New TestBin With {
            .D = Now,
            .f = RandomDouble(),
            .n = RandomDouble() * 1000,
            .Property1 = NetResponse.RFC_UNKNOWN_ERROR.GetJson
        }
    End Function
End Class