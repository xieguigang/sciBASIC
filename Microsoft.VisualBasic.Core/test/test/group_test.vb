Imports Microsoft.VisualBasic.Text
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Module group_test

    Sub RunGroup()
        Dim asc_chars = ASCII.AlphaNumericTable.Keys.Select(Function(c) c.ToString).ToArray
        Dim chars = Enumerable.Range(0, 700000).Select(Function(a) randf.Next(asc_chars)).ToArray

        Dim groupBy = chars.GroupBy(Function(s) s).ToDictionary(Function(s) s.Key, Function(s) s.Count)
        Dim dict_group As New Dictionary(Of String, List(Of String))

        For Each c As String In chars
            If Not dict_group.ContainsKey(c) Then
                dict_group.Add(c, New List(Of String))
            End If

            dict_group(c).Add(c)
        Next

        Dim groupByDict = dict_group.ToDictionary(Function(c) c.Key, Function(c) c.Value.Count)


        Pause()
    End Sub

End Module
