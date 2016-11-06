Imports Microsoft.VisualBasic.Data.Bootstrapping

Public Class FormLoadModel

    Public Property DllFile As String
    Public ReadOnly Property Model As Type

    Dim models As Type()

    Private Sub FormLoadModel_Load(sender As Object, e As EventArgs) Handles Me.Load
        models = MonteCarlo.DllParser(DllFile).ToArray

        For Each m As Type In models
            Call ListBox1.Items.Add(m.FullName)
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ListBox1.SelectedIndex = -1 Then
            Return
        End If

        For Each m As Type In models
            If Scripting.ToString(ListBox1.SelectedItem) = m.FullName Then
                _Model = m
                Exit For
            End If
        Next

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
End Class