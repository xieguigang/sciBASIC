Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.JSON.Extensions
Imports Microsoft.VisualBasic.MIME.JSON.ExtendedDictionary

Public Class Test : Inherits Dictionary(Of String, NamedValue(Of Integer()))
    Public Property tt As Double()

End Class

Module Module1

    Sub Main()
        Dim aaa = ParseJsonStr("[{a:[1,2,3,4,5,6,7,[{xxoo:[""233333""]}]], b: ""xxxxxooooo""}]")

        Dim tttt As New Test With {.tt = {1, 2, 3, 4, 5, 6, 7, 8}}
        tttt.Add("1234", New NamedValue(Of Integer())("xxxxx", {1, 2, 3}))
        tttt.Add("2333", New NamedValue(Of Integer())("xx---xxx", {-10, 203, 3}))

        Call GetExtendedJson(Of NamedValue(Of Integer()), Dictionary(Of String, NamedValue(Of Integer())))(tttt).SaveTo("x:\dddd.json")

        Pause()
    End Sub

End Module
