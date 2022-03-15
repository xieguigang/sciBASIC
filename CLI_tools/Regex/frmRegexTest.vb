#Region "Microsoft.VisualBasic::e63a81a3afa4e9bf3bebf99b34617b98, sciBASIC#\CLI_tools\Regex\frmRegexTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 80
    '    Code Lines: 65
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.90 KB


    ' Class frmRegexTest
    ' 
    '     Sub: __runRegex, CheckBox9_CheckedChanged, Form1_Load, OpenToolStripMenuItem_Click, tbInputs_TextChanged
    '          tbRegex_TextChanged
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class frmRegexTest

    Private Sub __runRegex()
        Dim regexs As String() =
            LinqAPI.Exec(Of String) <= From s As String
                                       In tbRegex.Text.lTokens
                                       Where Not String.IsNullOrEmpty(s)
                                       Select s

        Try
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
        Catch ex As Exception
            ex = New Exception(tbRegex.Text.lTokens.GetJson, ex)
            Call App.LogException(ex)
        End Try
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
                opt += Scripting.CTypeDynamic(cb.Text, GetType(RegexOptions))
            End If
        Next

        options = opt

        Call __runRegex()
    End Sub

    Private Sub tbRegex_TextChanged(sender As Object, e As EventArgs) Handles tbRegex.TextChanged
        Call __runRegex()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call CheckBox9_CheckedChanged(Nothing, Nothing)
    End Sub

    Private Sub tbInputs_TextChanged(sender As Object, e As EventArgs) Handles tbInputs.TextChanged
        Call __runRegex()
    End Sub
End Class
