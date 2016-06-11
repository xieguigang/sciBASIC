Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        Dim regexs As String() =
            LinqAPI.Exec(Of String) <= From s As String
                                       In tbRegex.Text.lTokens
                                       Where Not String.IsNullOrEmpty(s)
                                       Select s
        Dim LQuery = From expr As String
                     In regexs
                     Select expr,
                         Regex.Matches(tbInputs.Text, expr, options).ToArray

        Call lbResults.Items.Clear()

        For Each block In LQuery
            For Each result As String In block.ToArray
                Dim s As String = $"[{block.expr}]{vbTab} {result}"
                Call lbResults.Items.Add(s)
            Next
        Next
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "All text files(*.xml, *.txt, *.html, *.svg, *.log)|*.xml;*.txt;*.html;*.svg;*.log|All files(*.*)|*.*"
        }
            If file.ShowDialog = DialogResult.OK Then
                tbInputs.Text = file.FileName.ReadAllText
            End If
        End Using
    End Sub

    Dim options As RegexOptions

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged,
        CheckBox2.CheckedChanged,
        CheckBox3.CheckedChanged,
        CheckBox4.CheckedChanged,
        CheckBox5.CheckedChanged,
        CheckBox6.CheckedChanged,
        CheckBox7.CheckedChanged,
        CheckBox8.CheckedChanged,
        CheckBox9.CheckedChanged

        Dim opt As RegexOptions = RegexOptions.None

        For Each cb As CheckBox In {CheckBox1, CheckBox2, CheckBox3, CheckBox4, CheckBox5, CheckBox6, CheckBox7, CheckBox8, CheckBox9}
            If cb.Checked Then
                opt += Scripting.CastRegexOptions(cb.Text)
            End If
        Next

        options = opt

        Call Button1_Click(Nothing, Nothing)
    End Sub

    Private Sub tbRegex_TextChanged(sender As Object, e As EventArgs) Handles tbRegex.TextChanged
        Call Button1_Click(Nothing, Nothing)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call CheckBox9_CheckedChanged(Nothing, Nothing)
    End Sub
End Class
