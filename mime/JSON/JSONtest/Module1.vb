Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.JSON.Extensions
Imports Microsoft.VisualBasic.MIME.JSON.ExtendedDictionary
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Test : Inherits Dictionary(Of String, NamedValue(Of Integer()))
    Public Property tt As Double()
    Public Property sss As String
End Class

Module Module1

    Sub Main()
        Dim aaa = ParseJsonStr("[{a:[1,2,3,4,5,6,7,[{xxoo:[""233333""]}]], b: ""xxxxxooooo""}]")

        Dim tttt As New Test With {.tt = {1, 2, 3, 4, 5, 6, 7, 8}, .sss = "asdasdasdasas" & vbCrLf & "dsfafdsfadasd"}
        tttt.Add("1234", New NamedValue(Of Integer())("xxxxx", {1, 2, 3}))
        tttt.Add("2333", New NamedValue(Of Integer())("xx---xxx", {-10, 203, 3}))

        Call tttt.GetJson(True).__DEBUG_ECHO

        Call GetExtendedJson(Of NamedValue(Of Integer()), Test)(tttt, True).SaveTo("x:\dddd.json")

        Dim dfdsfdsf = GetExtendedJson(Of NamedValue(Of Integer()), Test)(tttt)
        Dim t2 = LoadExtendedJson(Of NamedValue(Of Integer()), Test)(dfdsfdsf)

        Call t2.tt.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

End Module
