Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim regexs As String() =
            LinqAPI.Exec(Of String) <= From s As String
                                       In tbRegex.Text.lTokens
                                       Where Not String.IsNullOrEmpty(s)
                                       Select s
        Dim LQuery = From expr As String
                     In regexs
                     Select expr,
                         Regex.Matches(tbInputs.Text, expr).ToArray

        Call lbResults.Items.Clear()

        For Each block In LQuery
            For Each result As String In block.ToArray
                Dim s As String = $"[{block.expr}]{vbTab} {result}"
                Call lbResults.Items.Add(s)
            Next
        Next
    End Sub
End Class
