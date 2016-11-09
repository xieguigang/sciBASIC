Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.JSON.Extensions
Imports Microsoft.VisualBasic.MIME.JSON.ExtendedDictionary
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class TestDynamicsObject : Inherits Dictionary(Of String, NamedValue(Of Integer()))
    Public Property Tarray As Double()
    Public Property str As String
    Public Property Tarray2 As String()
End Class

Module Module1

    Sub Main()
        Dim aaa = ParseJsonStr("[{a:[1,2,3,4,5,6,7,[{xxoo:[""233333""]}]], b: ""xxxxxooooo""}]")

        Dim t As New TestDynamicsObject With {
            .Tarray = {1, 2, 3, 4, 5, 6, 7, 8},
            .str = "12345" & vbCrLf & "67890",
            .Tarray2 = {
                "xxoo", "1234", "6789", "50"
            }
        }
        t.Add("1234", New NamedValue(Of Integer())("x1", {100, 200, 3}))
        t.Add("2333", New NamedValue(Of Integer())("x2", {-10, 203, 3}))

        Dim json$ = GetExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(t)
        Dim t2 = LoadExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(json)

        Call t.GetJson(True).SaveTo("./test_out.json")
        Call json.SaveTo("./test_out2.json")
        Call t2.Tarray.GetJson.__DEBUG_ECHO
        Call t2.Tarray2.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

End Module
