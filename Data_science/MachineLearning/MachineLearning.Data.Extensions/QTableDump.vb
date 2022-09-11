Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.MachineLearning.QLearning.DataModel
Imports Microsoft.VisualBasic.Text

Public Class QTableDump

    ReadOnly __buffer As New Dictionary(Of IndexCurve)

    Public Sub Dump(table As IQTable, iteration As Integer)
        For Each o In table.Table.Values
            For i As Integer = 0 To table.ActionRange - 1
                Dim uid As String = $"[{i}] {o.EnvirState}"
                If Not __buffer.ContainsKey(uid) Then
                    Call __buffer.Add(uid, New IndexCurve(uid))
                End If
                Call __buffer(uid).Properties.Add(iteration, o.Qvalues(i))
            Next
        Next
    End Sub

    Public Sub Save(path As String)
        Call __buffer.Values.SaveTo(path, Encodings.ASCII)
    End Sub
End Class
