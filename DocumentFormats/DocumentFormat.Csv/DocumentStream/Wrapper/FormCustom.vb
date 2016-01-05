Friend Class FormCustom

    Public Form As CsvChartDevice

    Private Sub FormCustom_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Form.ShownDialog = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Call EditTitle()

        Close()
    End Sub

#Region "Edit methods"
    Private Sub EditTitle()
        If Not String.IsNullOrEmpty(TextBox1.Text) Then
            Dim T = Form._chart.Titles("Title1")
            T.Text = TextBox1.Text
        End If
    End Sub

#End Region

    Private Sub Button2_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

    Private Sub FormCustom_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each Column In Form.Data.SchemaOridinal
            If Column.Value = 0 Then
                Continue For
            End If
            ComboBox1.Items.Add(Column.Key)
        Next
        ComboBox1.SelectedIndex = 0
        TextBox1.Text = Form._chart.Titles("Title1").Text
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        If ColorDialog1.ShowDialog = System.Windows.Forms.DialogResult.OK Then
            Form._chart.Series(index:=ComboBox1.SelectedIndex).Color = ColorDialog1.Color
        End If
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Dim n As Double = Val(TextBox2.Text)

        If n >= 0 Then
            Form._chart.Series(index:=ComboBox1.SelectedIndex).BorderWidth = CInt(n)
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        TextBox2.Text = CStr(Form._chart.Series(ComboBox1.SelectedIndex).BorderWidth)
    End Sub
End Class